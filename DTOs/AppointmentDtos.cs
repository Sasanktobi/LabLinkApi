using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class AppointmentCreateDto
    {
        // Required when staff book on behalf of a patient; ignored when a patient books for themselves.
        public int? PatientId{get;set;}

        // Either pick a slot, or give ScheduleDate + ScheduleTimeSlot directly.
        public int? AvailabilitySlotId{get;set;}
        public DateTime? ScheduleDate{get;set;}

        [MaxLength(30)]
        public string? ScheduleTimeSlot{get;set;}

        // LabVisit or HomeCollection.
        [Required]
        public string AppointmentType{get;set;}=string.Empty;

        // Defaults to the patient's address for home collection when empty.
        public string? HomeAddress{get;set;}

        // Referring doctor (optional).
        public int? DoctorId{get;set;}

        public List<int> TestIds{get;set;}=new List<int>();
        public List<int> TestPanelIds{get;set;}=new List<int>();
    }

    public class AppointmentRescheduleDto
    {
        public int? AvailabilitySlotId{get;set;}
        public DateTime? ScheduleDate{get;set;}

        [MaxLength(30)]
        public string? ScheduleTimeSlot{get;set;}
    }

    public class AssignPhlebotomistDto
    {
        [Required]
        public int PhlebotomistId{get;set;}
    }

    public class AppointmentCancelDto
    {
        [MaxLength(500)]
        public string Reason{get;set;}=string.Empty;
    }

    public class AppointmentFilterDto
    {
        public int? PatientId{get;set;}
        public int? DoctorId{get;set;}
        public int? PhlebotomistId{get;set;}
        public string? Status{get;set;}
        public string? AppointmentType{get;set;}
        public DateTime? FromDate{get;set;}
        public DateTime? ToDate{get;set;}

        [Range(1, int.MaxValue)]
        public int Page{get;set;}=1;

        [Range(1, 100)]
        public int PageSize{get;set;}=20;
    }

    public class AppointmentTestResponseDto
    {
        public int AppointmentTestId{get;set;}
        public int TestId{get;set;}
        public string TestName{get;set;}=string.Empty;
        public string TestCode{get;set;}=string.Empty;
        public string SampleType{get;set;}=string.Empty;
        public int? TestPanelId{get;set;}
        public string? TestPanelName{get;set;}
        public decimal PriceAtOrder{get;set;}
        public string Status{get;set;}=string.Empty;
    }

    public class AppointmentResponseDto
    {
        public int AppointmentId{get;set;}
        public DateTime ScheduleDate{get;set;}
        public string ScheduleTimeSlot{get;set;}=string.Empty;
        public string AppointmentType{get;set;}=string.Empty;
        public string Status{get;set;}=string.Empty;
        public string HomeAddress{get;set;}=string.Empty;
        public string BookedBy{get;set;}=string.Empty;
        public int? BookedByUserId{get;set;}
        public DateTime CreatedAt{get;set;}

        public int PatientId{get;set;}
        public string PatientName{get;set;}=string.Empty;
        public string PatientPhoneNo{get;set;}=string.Empty;

        public int? DoctorId{get;set;}
        public string? DoctorName{get;set;}

        public int? PhlebotomistId{get;set;}
        public string? PhlebotomistName{get;set;}

        public int? AvailabilitySlotId{get;set;}

        public decimal TotalAmount{get;set;}
        public List<AppointmentTestResponseDto> Tests{get;set;}=new List<AppointmentTestResponseDto>();

        public int? ReportId{get;set;}
        public string? ReportStatus{get;set;}
    }

    public class PagedResult<T>
    {
        public List<T> Items{get;set;}=new List<T>();
        public int Page{get;set;}
        public int PageSize{get;set;}
        public int TotalCount{get;set;}
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
