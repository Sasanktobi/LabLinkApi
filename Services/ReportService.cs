using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.Constants;
using Backend.DTOs;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.IRepositories;
using Backend.IServices;
using Backend.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class ReportService : IReportService
    {
        private static readonly string[] LabStaffRoles={RoleNames.Admin, RoleNames.Pathologist, RoleNames.LabTechnician, RoleNames.Receptionist};

        private readonly IReportRepository reportRepository;
        private readonly ITestResultRepository testResultRepository;
        private readonly IPatientRepository patientRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IReportPdfGenerator pdfGenerator;
        private readonly ICurrentUserService currentUser;
        private readonly IAuditService auditService;
        private readonly string reportsDirectory;

        public ReportService(IReportRepository _reportRepository, ITestResultRepository _testResultRepository,
            IPatientRepository _patientRepository, IStaffRepository _staffRepository, IReportPdfGenerator _pdfGenerator,
            ICurrentUserService _currentUser, IAuditService _auditService, IOptions<StorageSettings> storage, IWebHostEnvironment environment)
        {
            reportRepository=_reportRepository;
            testResultRepository=_testResultRepository;
            patientRepository=_patientRepository;
            staffRepository=_staffRepository;
            pdfGenerator=_pdfGenerator;
            currentUser=_currentUser;
            auditService=_auditService;

            var path=storage.Value.ReportsPath;
            reportsDirectory=Path.IsPathRooted(path) ? path : Path.Combine(environment.ContentRootPath, path);
        }

        public async Task<IEnumerable<ReportResponseDto>> SearchAsync(string? status)
        {
            var userId=currentUser.RequireUserId();
            int? patientId=null;
            int? doctorId=null;

            if (currentUser.IsInRole(RoleNames.Patient))
            {
                var patient=await patientRepository.GetByUserIdAsync(userId)
                    ?? throw new NotFoundException("No patient profile is linked to this account.");
                patientId=patient.PatientId;
                status=ReportStatuses.Released;
            }
            else if (currentUser.IsInRole(RoleNames.Doctor))
            {
                var doctor=await staffRepository.GetDoctorByUserIdAsync(userId)
                    ?? throw new NotFoundException("No doctor profile is linked to this account.");
                doctorId=doctor.DoctorId;
                status=ReportStatuses.Released;
            }
            else if (!currentUser.IsInRole(LabStaffRoles))
            {
                throw new ForbiddenException("You do not have access to reports.");
            }

            // List view: results are omitted; fetch a single report for the full detail.
            var reports=await reportRepository.SearchAsync(status, patientId, doctorId);
            return reports.Select(r=>ToDto(r, new List<TestResult>())).ToList();
        }

        public async Task<ReportResponseDto> GetByIdAsync(int id)
        {
            var report=await LoadAsync(id);
            EnsureCanView(report);
            return await ToDetailedDtoAsync(report);
        }

        public async Task<ReportResponseDto> GetByAppointmentIdAsync(int appointmentId)
        {
            var report=await reportRepository.GetByAppointmentIdAsync(appointmentId)
                ?? throw new NotFoundException($"No report exists yet for appointment {appointmentId}.");
            EnsureCanView(report);
            return await ToDetailedDtoAsync(report);
        }

        public async Task<ReportResponseDto> ValidateAsync(int id, ReportReviewDto dto)
        {
            var pathologist=await RequirePathologistAsync();
            var report=await LoadAsync(id);
            RequireStatus(report, ReportStatuses.PendingValidation, "validated");

            report.Status=ReportStatuses.Validated;
            report.PathologistRemarks=dto.Remarks.Trim();
            report.ValidatedAt=DateTime.UtcNow;
            report.ValidatedByPathologistId=pathologist.PathologistId;
            report.ValidatedByPathologist=pathologist;

            await reportRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Report), id, AuditActions.StatusChange, "PendingValidation -> Validated.");
            return await ToDetailedDtoAsync(report);
        }

        public async Task<ReportResponseDto> RejectAsync(int id, ReportReviewDto dto)
        {
            await RequirePathologistAsync();
            var report=await LoadAsync(id);
            RequireStatus(report, ReportStatuses.PendingValidation, "rejected");

            if (string.IsNullOrWhiteSpace(dto.Remarks))
            {
                throw new BadRequestException("Remarks are required when sending a report back to the lab.");
            }

            report.Status=ReportStatuses.Rejected;
            report.PathologistRemarks=dto.Remarks.Trim();

            await reportRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Report), id, AuditActions.StatusChange, $"PendingValidation -> Rejected. Remarks: {report.PathologistRemarks}");
            return await ToDetailedDtoAsync(report);
        }

        public async Task<ReportResponseDto> ResubmitAsync(int id)
        {
            var report=await LoadAsync(id);
            RequireStatus(report, ReportStatuses.Rejected, "resubmitted");

            report.Status=ReportStatuses.PendingValidation;
            await reportRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Report), id, AuditActions.StatusChange, "Rejected -> PendingValidation (corrected by lab).");
            return await ToDetailedDtoAsync(report);
        }

        public async Task<ReportResponseDto> ReleaseAsync(int id)
        {
            var report=await LoadAsync(id);
            RequireStatus(report, ReportStatuses.Validated, "released");

            report.Status=ReportStatuses.Released;
            report.ReleasedAt=DateTime.UtcNow;
            report.Appointment.Status=AppointmentStatuses.Completed;

            // The PDF is a frozen snapshot of the released report.
            var dto=await ToDetailedDtoAsync(report);
            report.PdfFilepath=await WritePdfAsync(dto);
            dto.HasPdf=true;

            await reportRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Report), id, AuditActions.StatusChange, "Validated -> Released; appointment completed.");
            return dto;
        }

        public async Task<(byte[] Content, string FileName)> GetPdfAsync(int id)
        {
            var report=await LoadAsync(id);
            EnsureCanView(report);

            if (report.Status != ReportStatuses.Released)
            {
                throw new ConflictException("The PDF is available once the report has been released.");
            }

            var fileName=$"LabLink_Report_{report.ReportId}.pdf";
            var fullPath=string.IsNullOrWhiteSpace(report.PdfFilepath) ? null : Path.Combine(reportsDirectory, report.PdfFilepath);

            if (fullPath == null || !File.Exists(fullPath))
            {
                // File storage was cleared or moved: regenerate from the stored (immutable) data.
                report.PdfFilepath=await WritePdfAsync(await ToDetailedDtoAsync(report));
                await reportRepository.SaveChangesAsync();
                fullPath=Path.Combine(reportsDirectory, report.PdfFilepath);
            }

            return (await File.ReadAllBytesAsync(fullPath), fileName);
        }

        private async Task<string> WritePdfAsync(ReportResponseDto dto)
        {
            Directory.CreateDirectory(reportsDirectory);
            var relative=$"report_{dto.ReportId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            await File.WriteAllBytesAsync(Path.Combine(reportsDirectory, relative), pdfGenerator.Generate(dto));
            return relative;
        }

        private async Task<Report> LoadAsync(int id)
        {
            return await reportRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Report {id} was not found.");
        }

        private async Task<Pathologist> RequirePathologistAsync()
        {
            var userId=currentUser.RequireUserId();
            return await staffRepository.GetPathologistByUserIdAsync(userId)
                ?? throw new ForbiddenException("Only pathologists can review reports.");
        }

        private static void RequireStatus(Report report, string expected, string action)
        {
            if (report.Status != expected)
            {
                throw new ConflictException($"Only {expected} reports can be {action} (current status: {report.Status}).");
            }
        }

        private void EnsureCanView(Report report)
        {
            var userId=currentUser.RequireUserId();

            if (currentUser.IsInRole(LabStaffRoles))
            {
                return;
            }

            var released=report.Status == ReportStatuses.Released;

            if (currentUser.IsInRole(RoleNames.Patient) && released && report.Appointment.Patient.UserId == userId)
            {
                return;
            }

            if (currentUser.IsInRole(RoleNames.Doctor) && released && report.Appointment.Doctor?.UserId == userId)
            {
                return;
            }

            // Hide unreleased reports entirely rather than revealing that they exist.
            throw new NotFoundException($"Report {report.ReportId} was not found.");
        }

        private async Task<ReportResponseDto> ToDetailedDtoAsync(Report report)
        {
            var results=await testResultRepository.GetByAppointmentIdAsync(report.AppointmentId);
            return ToDto(report, results);
        }

        private ReportResponseDto ToDto(Report report, List<TestResult> results)
        {
            var appointment=report.Appointment;
            var patient=appointment.Patient;

            return new ReportResponseDto
            {
                ReportId=report.ReportId,
                AppointmentId=report.AppointmentId,
                Status=report.Status,
                PatientId=patient.PatientId,
                PatientName=DtoMapper.FullName(patient.User),
                PatientAge=DtoMapper.AgeOn(patient.DateOfBirth, appointment.ScheduleDate),
                PatientGender=patient.Gender,
                ScheduleDate=appointment.ScheduleDate,
                DoctorName=appointment.Doctor == null ? null : DtoMapper.FullName(appointment.Doctor.User),
                PathologistRemarks=report.PathologistRemarks,
                ValidatedByPathologistId=report.ValidatedByPathologistId,
                ValidatedByName=report.ValidatedByPathologist == null ? null : DtoMapper.FullName(report.ValidatedByPathologist.User),
                ValidatedAt=report.ValidatedAt,
                ReleasedAt=report.ReleasedAt,
                CreatedAt=report.CreatedAt,
                HasPdf=!string.IsNullOrWhiteSpace(report.PdfFilepath),
                Results=results.Select(DtoMapper.ToDto).ToList()
            };
        }
    }
}
