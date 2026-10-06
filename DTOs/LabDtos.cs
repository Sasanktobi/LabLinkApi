using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class SpecimenCreateDto
    {
        [Required]
        public int AppointmentId{get;set;}

        // e.g. "Blood - EDTA", "Urine".
        [Required]
        [MaxLength(100)]
        public string SpecimenName{get;set;}=string.Empty;

        // Defaults to now.
        public DateTime? CollectedAt{get;set;}
    }

    public class SpecimenRejectDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason{get;set;}=string.Empty;
    }

    public class SpecimenResponseDto
    {
        public int SpecimenId{get;set;}
        public int AppointmentId{get;set;}
        public int PatientId{get;set;}
        public string PatientName{get;set;}=string.Empty;
        public string SpecimenName{get;set;}=string.Empty;
        public string Status{get;set;}=string.Empty;
        public DateTime CollectedAt{get;set;}
        public int? CollectedByUserId{get;set;}
        public string? CollectedByName{get;set;}
        public string RejectionReason{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}
    }

    public class TestResultCreateDto
    {
        [Required]
        public int SpecimenId{get;set;}

        [Required]
        public int TestId{get;set;}

        [Required]
        [MaxLength(100)]
        public string ResultValue{get;set;}=string.Empty;

        [MaxLength(500)]
        public string Remarks{get;set;}=string.Empty;
    }

    public class TestResultItemDto
    {
        [Required]
        public int TestId{get;set;}

        [Required]
        [MaxLength(100)]
        public string ResultValue{get;set;}=string.Empty;

        [MaxLength(500)]
        public string Remarks{get;set;}=string.Empty;
    }

    public class TestResultBulkCreateDto
    {
        [Required]
        public int SpecimenId{get;set;}

        [Required]
        [MinLength(1)]
        public List<TestResultItemDto> Results{get;set;}=new List<TestResultItemDto>();
    }

    public class TestResultUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string ResultValue{get;set;}=string.Empty;

        [MaxLength(500)]
        public string Remarks{get;set;}=string.Empty;
    }

    public class TestResultResponseDto
    {
        public int TestResultId{get;set;}
        public int SpecimenId{get;set;}
        public int AppointmentId{get;set;}
        public int TestId{get;set;}
        public string TestName{get;set;}=string.Empty;
        public string TestCode{get;set;}=string.Empty;
        public string Unit{get;set;}=string.Empty;
        public string ReferenceRangeLow{get;set;}=string.Empty;
        public string ReferenceRangeHigh{get;set;}=string.Empty;
        public string ResultValue{get;set;}=string.Empty;
        public string Flag{get;set;}=string.Empty;
        public string Remarks{get;set;}=string.Empty;
        public DateTime EnteredAt{get;set;}
        public int? EnteredByLabTechnicianId{get;set;}
        public string? EnteredByName{get;set;}
    }

    public class ReportReviewDto
    {
        [MaxLength(1000)]
        public string Remarks{get;set;}=string.Empty;
    }

    public class ReportResponseDto
    {
        public int ReportId{get;set;}
        public int AppointmentId{get;set;}
        public string Status{get;set;}=string.Empty;

        public int PatientId{get;set;}
        public string PatientName{get;set;}=string.Empty;
        public int PatientAge{get;set;}
        public string PatientGender{get;set;}=string.Empty;
        public DateTime ScheduleDate{get;set;}
        public string? DoctorName{get;set;}

        public string PathologistRemarks{get;set;}=string.Empty;
        public int? ValidatedByPathologistId{get;set;}
        public string? ValidatedByName{get;set;}
        public DateTime? ValidatedAt{get;set;}
        public DateTime? ReleasedAt{get;set;}
        public DateTime CreatedAt{get;set;}
        public bool HasPdf{get;set;}

        public List<TestResultResponseDto> Results{get;set;}=new List<TestResultResponseDto>();
    }

    public class AuditLogResponseDto
    {
        public int AuditLogId{get;set;}
        public string EntityName{get;set;}=string.Empty;
        public int EntityId{get;set;}
        public string Action{get;set;}=string.Empty;
        public string Details{get;set;}=string.Empty;
        public DateTime TimeStamp{get;set;}
        public int? UserId{get;set;}
        public string? UserName{get;set;}
    }

    public class AuditLogFilterDto
    {
        public string? EntityName{get;set;}
        public int? EntityId{get;set;}
        public int? UserId{get;set;}
        public DateTime? From{get;set;}
        public DateTime? To{get;set;}

        [Range(1, int.MaxValue)]
        public int Page{get;set;}=1;

        [Range(1, 200)]
        public int PageSize{get;set;}=50;
    }

    public class DashboardSummaryDto
    {
        public int AppointmentsToday{get;set;}
        public int PendingCollection{get;set;}
        public int SpecimensAwaitingReceipt{get;set;}
        public int AppointmentsInProgress{get;set;}
        public int ReportsPendingValidation{get;set;}
        public int ReportsReadyToRelease{get;set;}
        public int ReportsReleasedToday{get;set;}
        public int TotalPatients{get;set;}
        public decimal RevenueToday{get;set;}
    }
}
