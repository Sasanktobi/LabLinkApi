using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    // Admin creates staff accounts. Which profile fields are required depends on the role;
    // UserService validates that.
    public class StaffCreateDto : UserCreateDto
    {
        [MaxLength(150)]
        public string? Specialization{get;set;}

        [MaxLength(150)]
        public string? Qualification{get;set;}

        [MaxLength(50)]
        public string? LicenseNumber{get;set;}

        [Range(0, 80)]
        public int? YearsOfExperience{get;set;}

        public string? ClinicAddress{get;set;}

        [MaxLength(100)]
        public string? Department{get;set;}

        [MaxLength(50)]
        public string? ShiftTiming{get;set;}

        [MaxLength(100)]
        public string? ServiceZone{get;set;}

        [MaxLength(100)]
        public string? BranchLocation{get;set;}
    }

    // Only the fields that apply to the user's role are used; the rest are ignored.
    public class StaffProfileUpdateDto
    {
        [MaxLength(150)]
        public string? Specialization{get;set;}

        [MaxLength(150)]
        public string? Qualification{get;set;}

        [MaxLength(50)]
        public string? LicenseNumber{get;set;}

        [Range(0, 80)]
        public int? YearsOfExperience{get;set;}

        public string? ClinicAddress{get;set;}

        [MaxLength(100)]
        public string? Department{get;set;}

        [MaxLength(50)]
        public string? ShiftTiming{get;set;}

        [MaxLength(100)]
        public string? ServiceZone{get;set;}

        public bool? IsAvailable{get;set;}

        [MaxLength(100)]
        public string? BranchLocation{get;set;}
    }

    public class StaffResponseDto
    {
        public int UserId{get;set;}
        public int ProfileId{get;set;}
        public string RoleName{get;set;}=string.Empty;
        public string FirstName{get;set;}=string.Empty;
        public string LastName{get;set;}=string.Empty;
        public string Email{get;set;}=string.Empty;
        public string PhoneNo{get;set;}=string.Empty;
        public bool IsActive{get;set;}

        public string? Specialization{get;set;}
        public string? Qualification{get;set;}
        public string? LicenseNumber{get;set;}
        public int? YearsOfExperience{get;set;}
        public string? ClinicAddress{get;set;}
        public string? Department{get;set;}
        public string? ShiftTiming{get;set;}
        public string? ServiceZone{get;set;}
        public bool? IsAvailable{get;set;}
        public string? BranchLocation{get;set;}
    }

    public class PhlebotomistAvailabilityDto
    {
        [Required]
        public bool IsAvailable{get;set;}
    }
}
