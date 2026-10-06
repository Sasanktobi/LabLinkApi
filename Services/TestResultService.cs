using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.IRepositories;
using Backend.IServices;
using Backend.Models;

namespace Backend.Services
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepository testResultRepository;
        private readonly ISpecimenRepository specimenRepository;
        private readonly IStaffRepository staffRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IAuditService auditService;

        public TestResultService(ITestResultRepository _testResultRepository, ISpecimenRepository _specimenRepository,
            IStaffRepository _staffRepository, ICurrentUserService _currentUser, IAuditService _auditService)
        {
            testResultRepository=_testResultRepository;
            specimenRepository=_specimenRepository;
            staffRepository=_staffRepository;
            currentUser=_currentUser;
            auditService=_auditService;
        }

        public async Task<TestResultResponseDto> CreateAsync(TestResultCreateDto dto)
        {
            var created=await CreateBulkAsync(new TestResultBulkCreateDto
            {
                SpecimenId=dto.SpecimenId,
                Results=new List<TestResultItemDto>
                {
                    new TestResultItemDto { TestId=dto.TestId, ResultValue=dto.ResultValue, Remarks=dto.Remarks }
                }
            });
            return created.Single();
        }

        public async Task<IEnumerable<TestResultResponseDto>> CreateBulkAsync(TestResultBulkCreateDto dto)
        {
            var technician=await RequireLabTechnicianAsync();
            var specimen=await specimenRepository.GetByIdAsync(dto.SpecimenId)
                ?? throw new NotFoundException($"Specimen {dto.SpecimenId} was not found.");

            if (specimen.Status != SpecimenStatuses.Received)
            {
                throw new ConflictException($"Results can only be entered for Received specimens (current status: {specimen.Status}).");
            }

            var appointment=specimen.Appointment;
            if (appointment.Status == AppointmentStatuses.Cancelled || appointment.Status == AppointmentStatuses.Completed)
            {
                throw new ConflictException($"Appointment {appointment.AppointmentId} is {appointment.Status}.");
            }
            EnsureReportEditable(appointment.Report);

            var duplicatesInRequest=dto.Results.GroupBy(r=>r.TestId).Where(g=>g.Count() > 1).Select(g=>g.Key).ToList();
            if (duplicatesInRequest.Count > 0)
            {
                throw new BadRequestException($"Test id(s) appear more than once: {string.Join(", ", duplicatesInRequest)}.");
            }

            var existing=await testResultRepository.GetByAppointmentIdAsync(appointment.AppointmentId);
            var alreadyEntered=existing.Select(r=>r.TestId).Intersect(dto.Results.Select(r=>r.TestId)).ToList();
            if (alreadyEntered.Count > 0)
            {
                throw new ConflictException($"Results already exist for test id(s) {string.Join(", ", alreadyEntered)}; update them instead.");
            }

            var results=new List<TestResult>();
            foreach (var item in dto.Results)
            {
                var ordered=appointment.AppointmentTests
                    .FirstOrDefault(at=>at.TestId == item.TestId && at.Status != AppointmentTestStatuses.Cancelled)
                    ?? throw new BadRequestException($"Test {item.TestId} was not ordered on appointment {appointment.AppointmentId}.");

                var value=item.ResultValue.Trim();
                results.Add(new TestResult
                {
                    SpecimenId=specimen.SpecimenId,
                    Specimen=specimen,
                    TestId=ordered.TestId,
                    Test=ordered.Test,
                    ResultValue=value,
                    Remarks=item.Remarks.Trim(),
                    Flag=ResultFlagCalculator.Calculate(value, ordered.Test.ReferenceRangeLow, ordered.Test.ReferenceRangeHigh),
                    EnteredAt=DateTime.UtcNow,
                    EnteredByLabTechnicianId=technician.LabTechnicianId,
                    EnteredByLabTechnician=technician
                });

                ordered.Status=AppointmentTestStatuses.ResultEntered;
            }

            await testResultRepository.AddRangeAsync(results);
            appointment.Status=AppointmentStatuses.InProgress;

            // Once every ordered test has a result, the report goes to the pathologist queue.
            var allEntered=appointment.AppointmentTests
                .Where(at=>at.Status != AppointmentTestStatuses.Cancelled)
                .All(at=>at.Status == AppointmentTestStatuses.ResultEntered);

            var reportCreated=false;
            if (allEntered && appointment.Report == null)
            {
                appointment.Report=new Report
                {
                    AppointmentId=appointment.AppointmentId,
                    Status=ReportStatuses.PendingValidation,
                    CreatedAt=DateTime.UtcNow
                };
                reportCreated=true;
            }

            await testResultRepository.SaveChangesAsync();

            foreach (var result in results)
            {
                await auditService.LogAsync(nameof(TestResult), result.TestResultId, AuditActions.Create,
                    $"Entered {result.Test.Code} = {result.ResultValue} ({result.Flag}) on specimen {specimen.SpecimenId}.");
            }

            if (reportCreated)
            {
                await auditService.LogAsync(nameof(Report), appointment.Report!.ReportId, AuditActions.Create,
                    $"All results entered for appointment {appointment.AppointmentId}; report sent for validation.");
            }

            return results.Select(DtoMapper.ToDto).ToList();
        }

        public async Task<TestResultResponseDto> UpdateAsync(int id, TestResultUpdateDto dto)
        {
            var technician=await RequireLabTechnicianAsync();
            var result=await testResultRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Test result {id} was not found.");

            EnsureReportEditable(result.Specimen.Appointment.Report);

            var previous=result.ResultValue;
            result.ResultValue=dto.ResultValue.Trim();
            result.Remarks=dto.Remarks.Trim();
            result.Flag=ResultFlagCalculator.Calculate(result.ResultValue, result.Test.ReferenceRangeLow, result.Test.ReferenceRangeHigh);
            result.EnteredAt=DateTime.UtcNow;
            result.EnteredByLabTechnicianId=technician.LabTechnicianId;
            result.EnteredByLabTechnician=technician;

            await testResultRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(TestResult), id, AuditActions.Update,
                $"Changed {result.Test.Code} from '{previous}' to '{result.ResultValue}' ({result.Flag}).");

            return DtoMapper.ToDto(result);
        }

        public async Task<TestResultResponseDto> GetByIdAsync(int id)
        {
            var result=await testResultRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Test result {id} was not found.");
            return DtoMapper.ToDto(result);
        }

        public async Task<IEnumerable<TestResultResponseDto>> GetByAppointmentAsync(int appointmentId)
        {
            var results=await testResultRepository.GetByAppointmentIdAsync(appointmentId);
            return results.Select(DtoMapper.ToDto).ToList();
        }

        public async Task<IEnumerable<TestResultResponseDto>> GetBySpecimenAsync(int specimenId)
        {
            var results=await testResultRepository.GetBySpecimenIdAsync(specimenId);
            return results.Select(DtoMapper.ToDto).ToList();
        }

        private static void EnsureReportEditable(Report? report)
        {
            if (report != null && (report.Status == ReportStatuses.Validated || report.Status == ReportStatuses.Released))
            {
                throw new ConflictException($"The report is already {report.Status}; results can no longer be changed.");
            }
        }

        private async Task<LabTechnician> RequireLabTechnicianAsync()
        {
            var userId=currentUser.RequireUserId();
            return await staffRepository.GetLabTechnicianByUserIdAsync(userId)
                ?? throw new ForbiddenException("Only lab technicians can enter results.");
        }
    }
}
