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
    public class AppointmentService : IAppointmentService
    {
        private static readonly string[] ModifiableStatuses={AppointmentStatuses.Booked, AppointmentStatuses.Confirmed};

        private readonly IAppointmentRepository appointmentRepository;
        private readonly IPatientRepository patientRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IAvailabilitySlotRepository slotRepository;
        private readonly ITestRepository testRepository;
        private readonly ITestPanelRepository panelRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IAuditService auditService;

        public AppointmentService(IAppointmentRepository _appointmentRepository, IPatientRepository _patientRepository,
            IStaffRepository _staffRepository, IAvailabilitySlotRepository _slotRepository, ITestRepository _testRepository,
            ITestPanelRepository _panelRepository, ICurrentUserService _currentUser, IAuditService _auditService)
        {
            appointmentRepository=_appointmentRepository;
            patientRepository=_patientRepository;
            staffRepository=_staffRepository;
            slotRepository=_slotRepository;
            testRepository=_testRepository;
            panelRepository=_panelRepository;
            currentUser=_currentUser;
            auditService=_auditService;
        }

        public async Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto dto)
        {
            if (!AppointmentTypes.All.Contains(dto.AppointmentType))
            {
                throw new BadRequestException($"AppointmentType must be one of: {string.Join(", ", AppointmentTypes.All)}.");
            }

            if (dto.TestIds.Count == 0 && dto.TestPanelIds.Count == 0)
            {
                throw new BadRequestException("Select at least one test or test panel.");
            }

            var patient=await ResolvePatientAsync(dto.PatientId);
            if (!patient.User.IsActive)
            {
                throw new BadRequestException("Appointments cannot be booked for an inactive patient.");
            }

            var appointment=new Appointment
            {
                PatientId=patient.PatientId,
                AppointmentType=dto.AppointmentType,
                Status=AppointmentStatuses.Booked,
                BookedBy=currentUser.Role ?? string.Empty,
                BookedByUserId=currentUser.UserId,
                CreatedAt=DateTime.UtcNow
            };

            await ApplyScheduleAsync(appointment, dto.AvailabilitySlotId, dto.ScheduleDate, dto.ScheduleTimeSlot);

            if (appointment.AppointmentType == AppointmentTypes.HomeCollection)
            {
                var address=string.IsNullOrWhiteSpace(dto.HomeAddress) ? FormatAddress(patient) : dto.HomeAddress.Trim();
                if (string.IsNullOrWhiteSpace(address))
                {
                    throw new BadRequestException("HomeAddress is required for home collection.");
                }
                appointment.HomeAddress=address;
            }

            if (dto.DoctorId.HasValue)
            {
                var doctor=await staffRepository.GetDoctorByIdAsync(dto.DoctorId.Value);
                if (doctor == null || !doctor.User.IsActive)
                {
                    throw new BadRequestException($"Doctor {dto.DoctorId.Value} was not found.");
                }
                appointment.DoctorId=doctor.DoctorId;
            }

            appointment.AppointmentTests=await BuildOrderedTestsAsync(dto.TestIds, dto.TestPanelIds);

            // Slot (if any) is tracked, so IsBooked is saved in the same transaction; its RowVersion guards double booking.
            await appointmentRepository.AddAsync(appointment);
            await auditService.LogAsync(nameof(Appointment), appointment.AppointmentId, AuditActions.Create,
                $"Booked {appointment.AppointmentType} for patient {patient.PatientId} on {appointment.ScheduleDate:yyyy-MM-dd} {appointment.ScheduleTimeSlot} with {appointment.AppointmentTests.Count} test(s).");

            return await GetByIdAsync(appointment.AppointmentId);
        }

        public async Task<AppointmentResponseDto> GetByIdAsync(int id)
        {
            var appointment=await LoadAsync(id);
            await EnsureCanViewAsync(appointment);
            return DtoMapper.ToDto(appointment);
        }

        public async Task<PagedResult<AppointmentResponseDto>> SearchAsync(AppointmentFilterDto filter)
        {
            var userId=currentUser.RequireUserId();

            if (currentUser.IsInRole(RoleNames.Patient))
            {
                var patient=await patientRepository.GetByUserIdAsync(userId)
                    ?? throw new NotFoundException("No patient profile is linked to this account.");
                filter.PatientId=patient.PatientId;
            }
            else if (currentUser.IsInRole(RoleNames.Doctor))
            {
                var doctor=await staffRepository.GetDoctorByUserIdAsync(userId)
                    ?? throw new NotFoundException("No doctor profile is linked to this account.");
                filter.DoctorId=doctor.DoctorId;
            }
            else if (currentUser.IsInRole(RoleNames.Phlebotomist))
            {
                var phlebotomist=await staffRepository.GetPhlebotomistByUserIdAsync(userId)
                    ?? throw new NotFoundException("No phlebotomist profile is linked to this account.");
                filter.PhlebotomistId=phlebotomist.PhlebotomistId;
            }

            var (items, total)=await appointmentRepository.SearchAsync(filter);
            return new PagedResult<AppointmentResponseDto>
            {
                Items=items.Select(DtoMapper.ToDto).ToList(),
                Page=filter.Page,
                PageSize=filter.PageSize,
                TotalCount=total
            };
        }

        public async Task<AppointmentResponseDto> RescheduleAsync(int id, AppointmentRescheduleDto dto)
        {
            var appointment=await LoadAsync(id);
            EnsureCanModify(appointment);

            var previous=$"{appointment.ScheduleDate:yyyy-MM-dd} {appointment.ScheduleTimeSlot}";
            ReleaseSlot(appointment);
            await ApplyScheduleAsync(appointment, dto.AvailabilitySlotId, dto.ScheduleDate, dto.ScheduleTimeSlot);

            // A new time needs to be confirmed again by the front desk.
            appointment.Status=AppointmentStatuses.Booked;

            await appointmentRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Appointment), id, AuditActions.Update,
                $"Rescheduled from {previous} to {appointment.ScheduleDate:yyyy-MM-dd} {appointment.ScheduleTimeSlot}.");

            return DtoMapper.ToDto(appointment);
        }

        public async Task<AppointmentResponseDto> ConfirmAsync(int id)
        {
            var appointment=await LoadAsync(id);

            if (appointment.Status != AppointmentStatuses.Booked)
            {
                throw new ConflictException($"Only Booked appointments can be confirmed (current status: {appointment.Status}).");
            }

            if (appointment.AppointmentType == AppointmentTypes.HomeCollection && appointment.PhlebotomistId == null)
            {
                throw new ConflictException("Assign a phlebotomist before confirming a home collection.");
            }

            appointment.Status=AppointmentStatuses.Confirmed;
            await appointmentRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Appointment), id, AuditActions.StatusChange, "Booked -> Confirmed.");

            return DtoMapper.ToDto(appointment);
        }

        public async Task<AppointmentResponseDto> AssignPhlebotomistAsync(int id, AssignPhlebotomistDto dto)
        {
            var appointment=await LoadAsync(id);

            if (appointment.AppointmentType != AppointmentTypes.HomeCollection)
            {
                throw new BadRequestException("Phlebotomists are only assigned to home collections.");
            }

            if (!ModifiableStatuses.Contains(appointment.Status))
            {
                throw new ConflictException($"Cannot reassign an appointment in status {appointment.Status}.");
            }

            var phlebotomist=await staffRepository.GetPhlebotomistByIdAsync(dto.PhlebotomistId);
            if (phlebotomist == null || !phlebotomist.User.IsActive)
            {
                throw new BadRequestException($"Phlebotomist {dto.PhlebotomistId} was not found.");
            }

            if (!phlebotomist.IsAvailable)
            {
                throw new ConflictException($"{DtoMapper.FullName(phlebotomist.User)} is currently marked unavailable.");
            }

            // A slot booked on another phlebotomist's calendar no longer applies.
            if (appointment.AvailabilitySlot != null && appointment.AvailabilitySlot.UserId != phlebotomist.UserId)
            {
                ReleaseSlot(appointment);
            }

            appointment.PhlebotomistId=phlebotomist.PhlebotomistId;
            await appointmentRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Appointment), id, AuditActions.Update, $"Assigned phlebotomist {phlebotomist.PhlebotomistId}.");

            return await GetByIdAsync(id);
        }

        public async Task<AppointmentResponseDto> CancelAsync(int id, AppointmentCancelDto dto)
        {
            var appointment=await LoadAsync(id);
            EnsureCanModify(appointment);

            ReleaseSlot(appointment);
            appointment.Status=AppointmentStatuses.Cancelled;
            foreach (var test in appointment.AppointmentTests)
            {
                test.Status=AppointmentTestStatuses.Cancelled;
            }

            await appointmentRepository.SaveChangesAsync();
            var reason=string.IsNullOrWhiteSpace(dto.Reason) ? "No reason given." : dto.Reason.Trim();
            await auditService.LogAsync(nameof(Appointment), id, AuditActions.StatusChange, $"Cancelled. Reason: {reason}");

            return DtoMapper.ToDto(appointment);
        }

        private async Task<Appointment> LoadAsync(int id)
        {
            return await appointmentRepository.GetWithDetailsAsync(id)
                ?? throw new NotFoundException($"Appointment {id} was not found.");
        }

        private async Task<Patient> ResolvePatientAsync(int? requestedPatientId)
        {
            if (currentUser.IsInRole(RoleNames.Patient))
            {
                var userId=currentUser.RequireUserId();
                return await patientRepository.GetByUserIdAsync(userId)
                    ?? throw new NotFoundException("No patient profile is linked to this account.");
            }

            if (!requestedPatientId.HasValue)
            {
                throw new BadRequestException("PatientId is required when booking on behalf of a patient.");
            }

            return await patientRepository.GetByIdAsync(requestedPatientId.Value)
                ?? throw new NotFoundException($"Patient {requestedPatientId.Value} was not found.");
        }

        // Sets date/time either from a slot (booking it) or from the explicit values.
        private async Task ApplyScheduleAsync(Appointment appointment, int? slotId, DateTime? date, string? timeSlot)
        {
            if (slotId.HasValue)
            {
                var slot=await slotRepository.GetByIdAsync(slotId.Value)
                    ?? throw new NotFoundException($"Slot {slotId.Value} was not found.");

                if (slot.IsBooked)
                {
                    throw new ConflictException("That slot has already been booked.");
                }

                if (slot.SlotDate.Date < DateTime.Today)
                {
                    throw new BadRequestException("That slot is in the past.");
                }

                if (!slot.User.IsActive)
                {
                    throw new BadRequestException("That slot's provider is no longer active.");
                }

                var expectedProvider=appointment.AppointmentType == AppointmentTypes.HomeCollection ? ProviderTypes.Phlebotomist : ProviderTypes.Lab;
                if (slot.ProviderType != expectedProvider)
                {
                    throw new BadRequestException($"A {appointment.AppointmentType} appointment needs a {expectedProvider} slot.");
                }

                if (slot.ProviderType == ProviderTypes.Phlebotomist)
                {
                    var phlebotomist=await staffRepository.GetPhlebotomistByUserIdAsync(slot.UserId)
                        ?? throw new BadRequestException("The slot owner has no phlebotomist profile.");
                    appointment.PhlebotomistId=phlebotomist.PhlebotomistId;
                }

                slot.IsBooked=true;
                appointment.AvailabilitySlotId=slot.AvailabilitySlotId;
                appointment.AvailabilitySlot=slot;
                appointment.ScheduleDate=slot.SlotDate.Date;
                appointment.ScheduleTimeSlot=$"{slot.StartTime}-{slot.EndTime}";
                return;
            }

            if (!date.HasValue || string.IsNullOrWhiteSpace(timeSlot))
            {
                throw new BadRequestException("Provide either AvailabilitySlotId, or ScheduleDate and ScheduleTimeSlot.");
            }

            if (date.Value.Date < DateTime.Today)
            {
                throw new BadRequestException("ScheduleDate cannot be in the past.");
            }

            appointment.AvailabilitySlotId=null;
            appointment.AvailabilitySlot=null;
            appointment.ScheduleDate=date.Value.Date;
            appointment.ScheduleTimeSlot=timeSlot.Trim();
        }

        private static void ReleaseSlot(Appointment appointment)
        {
            if (appointment.AvailabilitySlot != null)
            {
                appointment.AvailabilitySlot.IsBooked=false;
            }
            appointment.AvailabilitySlotId=null;
            appointment.AvailabilitySlot=null;
        }

        // Expands panels into their tests and spreads each panel's price across them (weighted by list price),
        // so the sum of PriceAtOrder always equals what the patient is charged. A test that is already covered
        // by an earlier panel is not ordered (or charged) twice.
        private async Task<List<AppointmentTest>> BuildOrderedTestsAsync(List<int> testIds, List<int> panelIds)
        {
            var ordered=new List<AppointmentTest>();
            var covered=new HashSet<int>();

            if (panelIds.Count > 0)
            {
                var panels=await panelRepository.GetByIdsAsync(panelIds);
                var missing=panelIds.Distinct().Except(panels.Select(p=>p.TestPanelId)).ToList();
                if (missing.Count > 0)
                {
                    throw new BadRequestException($"Unknown test panel id(s): {string.Join(", ", missing)}.");
                }

                foreach (var panel in panels.OrderBy(p=>panelIds.IndexOf(p.TestPanelId)))
                {
                    if (!panel.IsActive)
                    {
                        throw new BadRequestException($"Panel '{panel.Name}' is no longer offered.");
                    }

                    var tests=panel.TestPanelTests
                        .Select(pt=>pt.Test)
                        .Where(t=>t.IsActive && !covered.Contains(t.TestId))
                        .OrderBy(t=>t.TestId)
                        .ToList();

                    if (tests.Count == 0)
                    {
                        throw new BadRequestException($"Every test in panel '{panel.Name}' is already included in another selected panel.");
                    }

                    var prices=AllocatePrice(panel.Price, tests.Select(t=>t.Price).ToList());
                    for (var i=0; i < tests.Count; i++)
                    {
                        covered.Add(tests[i].TestId);
                        ordered.Add(new AppointmentTest
                        {
                            TestId=tests[i].TestId,
                            Test=tests[i],
                            TestPanelId=panel.TestPanelId,
                            TestPanel=panel,
                            PriceAtOrder=prices[i],
                            Status=AppointmentTestStatuses.Ordered
                        });
                    }
                }
            }

            var individualIds=testIds.Distinct().Where(id=>!covered.Contains(id)).ToList();
            if (individualIds.Count > 0)
            {
                var tests=await testRepository.GetByIdsAsync(individualIds);
                var missing=individualIds.Except(tests.Select(t=>t.TestId)).ToList();
                if (missing.Count > 0)
                {
                    throw new BadRequestException($"Unknown test id(s): {string.Join(", ", missing)}.");
                }

                foreach (var test in tests)
                {
                    if (!test.IsActive)
                    {
                        throw new BadRequestException($"Test '{test.Name}' is no longer offered.");
                    }

                    ordered.Add(new AppointmentTest
                    {
                        TestId=test.TestId,
                        Test=test,
                        PriceAtOrder=test.Price,
                        Status=AppointmentTestStatuses.Ordered
                    });
                }
            }

            return ordered;
        }

        private static List<decimal> AllocatePrice(decimal total, List<decimal> weights)
        {
            var weightSum=weights.Sum();
            var shares=new List<decimal>();
            decimal allocated=0;

            for (var i=0; i < weights.Count; i++)
            {
                decimal share;
                if (i == weights.Count - 1)
                {
                    share=total - allocated;
                }
                else
                {
                    share=weightSum > 0
                        ? Math.Round(total * weights[i] / weightSum, 2, MidpointRounding.AwayFromZero)
                        : Math.Round(total / weights.Count, 2, MidpointRounding.AwayFromZero);
                }

                shares.Add(share);
                allocated+=share;
            }

            return shares;
        }

        private static string FormatAddress(Patient patient)
        {
            var parts=new[] { patient.Address, patient.City, patient.State, patient.PostalCode }
                .Where(p=>!string.IsNullOrWhiteSpace(p))
                .Select(p=>p.Trim());
            return string.Join(", ", parts);
        }

        private async Task EnsureCanViewAsync(Appointment appointment)
        {
            var userId=currentUser.RequireUserId();

            if (currentUser.IsInRole(RoleNames.Patient) && appointment.Patient.UserId != userId)
            {
                throw new ForbiddenException("You can only view your own appointments.");
            }

            if (currentUser.IsInRole(RoleNames.Doctor))
            {
                var doctor=await staffRepository.GetDoctorByUserIdAsync(userId);
                if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                {
                    throw new ForbiddenException("You can only view appointments referred by you.");
                }
            }

            if (currentUser.IsInRole(RoleNames.Phlebotomist))
            {
                var phlebotomist=await staffRepository.GetPhlebotomistByUserIdAsync(userId);
                if (phlebotomist == null || appointment.PhlebotomistId != phlebotomist.PhlebotomistId)
                {
                    throw new ForbiddenException("You can only view appointments assigned to you.");
                }
            }
        }

        // Patients may change their own bookings; front desk and admins may change any. Only before collection.
        private void EnsureCanModify(Appointment appointment)
        {
            if (currentUser.IsInRole(RoleNames.Patient) && appointment.Patient.UserId != currentUser.UserId)
            {
                throw new ForbiddenException("You can only change your own appointments.");
            }

            if (!ModifiableStatuses.Contains(appointment.Status))
            {
                throw new ConflictException($"Appointments in status {appointment.Status} can no longer be changed.");
            }
        }
    }
}
