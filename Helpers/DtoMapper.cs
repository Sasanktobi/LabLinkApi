using System;
using System.Linq;
using Backend.Constants;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Helpers
{
    // Entity -> DTO mapping shared by the services. Callers must load the navigation properties used here.
    public static class DtoMapper
    {
        public static string FullName(User? user)
        {
            return user == null ? string.Empty : $"{user.FirstName} {user.LastName}".Trim();
        }

        public static int AgeOn(DateTime dateOfBirth, DateTime onDate)
        {
            var age=onDate.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > onDate.Date.AddYears(-age))
            {
                age--;
            }
            return Math.Max(age, 0);
        }

        public static int? ProfileIdOf(User user)
        {
            return user.Role?.Name switch
            {
                RoleNames.Admin => user.Admin?.AdminId,
                RoleNames.Patient => user.Patient?.PatientId,
                RoleNames.Doctor => user.Doctor?.DoctorId,
                RoleNames.Pathologist => user.Pathologist?.PathologistId,
                RoleNames.LabTechnician => user.LabTechnician?.LabTechnicianId,
                RoleNames.Phlebotomist => user.Phlebotomist?.PhlebotomistId,
                RoleNames.Receptionist => user.Receptionist?.ReceptionistId,
                _ => null
            };
        }

        public static PatientResponseDto ToDto(Patient patient)
        {
            return new PatientResponseDto
            {
                PatientId=patient.PatientId,
                UserId=patient.UserId,
                FirstName=patient.User.FirstName,
                LastName=patient.User.LastName,
                Email=patient.User.Email,
                PhoneNo=patient.User.PhoneNo,
                IsActive=patient.User.IsActive,
                DateOfBirth=patient.DateOfBirth,
                Age=AgeOn(patient.DateOfBirth, DateTime.Today),
                Gender=patient.Gender,
                BloodGroup=patient.BloodGroup,
                Address=patient.Address,
                City=patient.City,
                State=patient.State,
                PostalCode=patient.PostalCode,
                CreatedAt=patient.CreatedAt
            };
        }

        public static StaffResponseDto ToStaffDto(User user)
        {
            var dto=new StaffResponseDto
            {
                UserId=user.UserId,
                ProfileId=ProfileIdOf(user) ?? 0,
                RoleName=user.Role?.Name ?? string.Empty,
                FirstName=user.FirstName,
                LastName=user.LastName,
                Email=user.Email,
                PhoneNo=user.PhoneNo,
                IsActive=user.IsActive
            };

            if (user.Doctor != null)
            {
                dto.Specialization=user.Doctor.Specialization;
                dto.Qualification=user.Doctor.Qualification;
                dto.LicenseNumber=user.Doctor.LicenseNumber;
                dto.YearsOfExperience=user.Doctor.YearsOfExperience;
                dto.ClinicAddress=user.Doctor.ClinicAddress;
            }
            else if (user.Pathologist != null)
            {
                dto.Specialization=user.Pathologist.Specialization;
                dto.Qualification=user.Pathologist.Qualification;
                dto.LicenseNumber=user.Pathologist.LicenseNumber;
            }
            else if (user.LabTechnician != null)
            {
                dto.Department=user.LabTechnician.Department;
                dto.ShiftTiming=user.LabTechnician.ShiftTiming;
            }
            else if (user.Phlebotomist != null)
            {
                dto.ServiceZone=user.Phlebotomist.ServiceZone;
                dto.IsAvailable=user.Phlebotomist.IsAvailable;
            }
            else if (user.Receptionist != null)
            {
                dto.ShiftTiming=user.Receptionist.ShiftTiming;
                dto.BranchLocation=user.Receptionist.BranchLocation;
            }

            return dto;
        }

        public static SlotResponseDto ToDto(AvailabilitySlot slot)
        {
            return new SlotResponseDto
            {
                AvailabilitySlotId=slot.AvailabilitySlotId,
                UserId=slot.UserId,
                ProviderName=FullName(slot.User),
                ProviderType=slot.ProviderType,
                SlotDate=slot.SlotDate,
                StartTime=slot.StartTime,
                EndTime=slot.EndTime,
                IsBooked=slot.IsBooked
            };
        }

        public static TestResponseDto ToDto(Test test)
        {
            return new TestResponseDto
            {
                TestId=test.TestId,
                Name=test.Name,
                Code=test.Code,
                Description=test.Description,
                SampleType=test.SampleType,
                Price=test.Price,
                ReferenceRangeLow=test.ReferenceRangeLow,
                ReferenceRangeHigh=test.ReferenceRangeHigh,
                Unit=test.Unit,
                TurnaroundHours=test.TurnaroundHours,
                IsActive=test.IsActive
            };
        }

        public static TestPanelResponseDto ToDto(TestPanel panel)
        {
            var tests=panel.TestPanelTests
                .Select(pt=>pt.Test)
                .Where(t=>t != null)
                .OrderBy(t=>t.Name)
                .ToList();

            return new TestPanelResponseDto
            {
                TestPanelId=panel.TestPanelId,
                Name=panel.Name,
                Description=panel.Description,
                Price=panel.Price,
                IsActive=panel.IsActive,
                IndividualTestsTotal=tests.Sum(t=>t.Price),
                Tests=tests.Select(ToDto).ToList()
            };
        }

        public static AppointmentResponseDto ToDto(Appointment appointment)
        {
            var tests=appointment.AppointmentTests
                .OrderBy(at=>at.TestPanelId.HasValue ? 0 : 1)
                .ThenBy(at=>at.Test?.Name)
                .ToList();

            return new AppointmentResponseDto
            {
                AppointmentId=appointment.AppointmentId,
                ScheduleDate=appointment.ScheduleDate,
                ScheduleTimeSlot=appointment.ScheduleTimeSlot,
                AppointmentType=appointment.AppointmentType,
                Status=appointment.Status,
                HomeAddress=appointment.HomeAddress,
                BookedBy=appointment.BookedBy,
                BookedByUserId=appointment.BookedByUserId,
                CreatedAt=appointment.CreatedAt,
                PatientId=appointment.PatientId,
                PatientName=FullName(appointment.Patient?.User),
                PatientPhoneNo=appointment.Patient?.User?.PhoneNo ?? string.Empty,
                DoctorId=appointment.DoctorId,
                DoctorName=appointment.Doctor == null ? null : FullName(appointment.Doctor.User),
                PhlebotomistId=appointment.PhlebotomistId,
                PhlebotomistName=appointment.Phlebotomist == null ? null : FullName(appointment.Phlebotomist.User),
                AvailabilitySlotId=appointment.AvailabilitySlotId,
                TotalAmount=tests.Where(at=>at.Status!=AppointmentTestStatuses.Cancelled).Sum(at=>at.PriceAtOrder),
                Tests=tests.Select(at=>new AppointmentTestResponseDto
                {
                    AppointmentTestId=at.AppointmentTestId,
                    TestId=at.TestId,
                    TestName=at.Test?.Name ?? string.Empty,
                    TestCode=at.Test?.Code ?? string.Empty,
                    SampleType=at.Test?.SampleType ?? string.Empty,
                    TestPanelId=at.TestPanelId,
                    TestPanelName=at.TestPanel?.Name,
                    PriceAtOrder=at.PriceAtOrder,
                    Status=at.Status
                }).ToList(),
                ReportId=appointment.Report?.ReportId,
                ReportStatus=appointment.Report?.Status
            };
        }

        public static SpecimenResponseDto ToDto(Specimen specimen)
        {
            return new SpecimenResponseDto
            {
                SpecimenId=specimen.SpecimenId,
                AppointmentId=specimen.AppointmentId,
                PatientId=specimen.Appointment?.PatientId ?? 0,
                PatientName=FullName(specimen.Appointment?.Patient?.User),
                SpecimenName=specimen.SpecimenName,
                Status=specimen.Status,
                CollectedAt=specimen.CollectedAt,
                CollectedByUserId=specimen.CollectedByUserId,
                CollectedByName=specimen.CollectedByUser == null ? null : FullName(specimen.CollectedByUser),
                RejectionReason=specimen.RejectionReason,
                CreatedAt=specimen.CreatedAt
            };
        }

        public static TestResultResponseDto ToDto(TestResult result)
        {
            return new TestResultResponseDto
            {
                TestResultId=result.TestResultId,
                SpecimenId=result.SpecimenId,
                AppointmentId=result.Specimen?.AppointmentId ?? 0,
                TestId=result.TestId,
                TestName=result.Test?.Name ?? string.Empty,
                TestCode=result.Test?.Code ?? string.Empty,
                Unit=result.Test?.Unit ?? string.Empty,
                ReferenceRangeLow=result.Test?.ReferenceRangeLow ?? string.Empty,
                ReferenceRangeHigh=result.Test?.ReferenceRangeHigh ?? string.Empty,
                ResultValue=result.ResultValue,
                Flag=result.Flag,
                Remarks=result.Remarks,
                EnteredAt=result.EnteredAt,
                EnteredByLabTechnicianId=result.EnteredByLabTechnicianId,
                EnteredByName=result.EnteredByLabTechnician == null ? null : FullName(result.EnteredByLabTechnician.User)
            };
        }
    }
}
