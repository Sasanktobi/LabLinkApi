using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class SlotCreateDto
    {
        // Provider who owns the slot. Admin/Receptionist may set it; other staff always create their own slots.
        public int? UserId{get;set;}

        [Required]
        public DateTime SlotDate{get;set;}

        // 24-hour "HH:mm".
        [Required]
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage="Use HH:mm (24-hour).")]
        public string StartTime{get;set;}=string.Empty;

        [Required]
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage="Use HH:mm (24-hour).")]
        public string EndTime{get;set;}=string.Empty;
    }

    // Splits StartTime-EndTime into back-to-back slots of DurationMinutes.
    public class SlotBulkCreateDto : SlotCreateDto
    {
        [Range(5, 240)]
        public int DurationMinutes{get;set;}=30;
    }

    public class SlotResponseDto
    {
        public int AvailabilitySlotId{get;set;}
        public int UserId{get;set;}
        public string ProviderName{get;set;}=string.Empty;
        public string ProviderType{get;set;}=string.Empty;
        public DateTime SlotDate{get;set;}
        public string StartTime{get;set;}=string.Empty;
        public string EndTime{get;set;}=string.Empty;
        public bool IsBooked{get;set;}
    }
}
