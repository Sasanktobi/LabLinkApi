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
    public class SpecimenService : ISpecimenService
    {
        // Collection is possible from booking until results are complete (a rejected sample can be re-drawn).
        private static readonly string[] CollectableStatuses=
        {
            AppointmentStatuses.Booked, AppointmentStatuses.Confirmed, AppointmentStatuses.SampleCollected, AppointmentStatuses.InProgress
        };

        private readonly ISpecimenRepository specimenRepository;
        private readonly IAppointmentRepository appointmentRepository;
        private readonly ITestResultRepository testResultRepository;
        private readonly IStaffRepository staffRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IAuditService auditService;

        public SpecimenService(ISpecimenRepository _specimenRepository, IAppointmentRepository _appointmentRepository,
            ITestResultRepository _testResultRepository, IStaffRepository _staffRepository, ICurrentUserService _currentUser,
            IAuditService _auditService)
        {
            specimenRepository=_specimenRepository;
            appointmentRepository=_appointmentRepository;
            testResultRepository=_testResultRepository;
            staffRepository=_staffRepository;
            currentUser=_currentUser;
            auditService=_auditService;
        }

        public async Task<SpecimenResponseDto> CollectAsync(SpecimenCreateDto dto)
        {
            var userId=currentUser.RequireUserId();
            var appointment=await appointmentRepository.GetWithDetailsAsync(dto.AppointmentId)
                ?? throw new NotFoundException($"Appointment {dto.AppointmentId} was not found.");

            if (!CollectableStatuses.Contains(appointment.Status))
            {
                throw new ConflictException($"Samples cannot be collected for an appointment in status {appointment.Status}.");
            }

            if (currentUser.IsInRole(RoleNames.Phlebotomist))
            {
                var phlebotomist=await staffRepository.GetPhlebotomistByUserIdAsync(userId);
                if (phlebotomist == null || appointment.PhlebotomistId != phlebotomist.PhlebotomistId)
                {
                    throw new ForbiddenException("You can only collect samples for home collections assigned to you.");
                }
            }

            var collectedAt=dto.CollectedAt?.ToUniversalTime() ?? DateTime.UtcNow;
            if (collectedAt > DateTime.UtcNow.AddMinutes(5))
            {
                throw new BadRequestException("CollectedAt cannot be in the future.");
            }

            var specimen=new Specimen
            {
                AppointmentId=appointment.AppointmentId,
                Appointment=appointment,
                SpecimenName=dto.SpecimenName.Trim(),
                CollectedAt=collectedAt,
                CollectedByUserId=userId,
                Status=SpecimenStatuses.Collected,
                CreatedAt=DateTime.UtcNow
            };
            appointment.Specimens.Add(specimen);

            foreach (var test in appointment.AppointmentTests.Where(t=>t.Status == AppointmentTestStatuses.Ordered))
            {
                test.Status=AppointmentTestStatuses.SampleCollected;
            }

            if (appointment.Status == AppointmentStatuses.Booked || appointment.Status == AppointmentStatuses.Confirmed)
            {
                appointment.Status=AppointmentStatuses.SampleCollected;
            }

            await appointmentRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Specimen), specimen.SpecimenId, AuditActions.Create,
                $"Collected '{specimen.SpecimenName}' for appointment {appointment.AppointmentId}.");

            return await GetByIdAsync(specimen.SpecimenId);
        }

        public async Task<SpecimenResponseDto> GetByIdAsync(int id)
        {
            var specimen=await LoadAsync(id);
            return DtoMapper.ToDto(specimen);
        }

        public async Task<IEnumerable<SpecimenResponseDto>> SearchAsync(int? appointmentId, string? status)
        {
            var specimens=await specimenRepository.SearchAsync(appointmentId, status);
            return specimens.Select(DtoMapper.ToDto).ToList();
        }

        public async Task<SpecimenResponseDto> ReceiveAsync(int id)
        {
            var specimen=await LoadAsync(id);

            if (specimen.Status != SpecimenStatuses.Collected)
            {
                throw new ConflictException($"Only Collected specimens can be received (current status: {specimen.Status}).");
            }

            specimen.Status=SpecimenStatuses.Received;
            await specimenRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Specimen), id, AuditActions.StatusChange, "Collected -> Received.");

            return DtoMapper.ToDto(specimen);
        }

        public async Task<SpecimenResponseDto> RejectAsync(int id, SpecimenRejectDto dto)
        {
            var specimen=await LoadAsync(id);

            if (specimen.Status == SpecimenStatuses.Rejected)
            {
                throw new ConflictException("Specimen is already rejected.");
            }

            var results=await testResultRepository.GetBySpecimenIdAsync(id);
            if (results.Count > 0)
            {
                throw new ConflictException("Results have already been entered against this specimen.");
            }

            specimen.Status=SpecimenStatuses.Rejected;
            specimen.RejectionReason=dto.Reason.Trim();

            // With no usable sample left, the tests go back to awaiting collection.
            var appointment=specimen.Appointment;
            var hasUsableSpecimen=appointment.Specimens.Any(s=>s.SpecimenId != id && s.Status != SpecimenStatuses.Rejected);
            if (!hasUsableSpecimen)
            {
                foreach (var test in appointment.AppointmentTests.Where(t=>t.Status == AppointmentTestStatuses.SampleCollected))
                {
                    test.Status=AppointmentTestStatuses.Ordered;
                }

                if (appointment.Status == AppointmentStatuses.SampleCollected)
                {
                    appointment.Status=AppointmentStatuses.Confirmed;
                }
            }

            await specimenRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Specimen), id, AuditActions.StatusChange, $"Rejected. Reason: {specimen.RejectionReason}");

            return DtoMapper.ToDto(specimen);
        }

        private async Task<Specimen> LoadAsync(int id)
        {
            return await specimenRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Specimen {id} was not found.");
        }
    }
}
