using System;

namespace Backend.DTOs
{
    public class UserResponseDto
    {
        public int UserId{get;set;}
        public string FirstName{get;set;}=string.Empty;
        public string LastName{get;set;}=string.Empty;
        public string Email{get;set;}=string.Empty;
        public string PhoneNo{get;set;}=string.Empty;
        public bool IsActive{get;set;}
        public DateTime CreatedAt{get;set;}
        public int RoleId{get;set;}
        public string RoleName{get;set;}=string.Empty;

        // Id of the role profile row (PatientId, DoctorId, ...). Null when the profile was not loaded.
        public int? ProfileId{get;set;}
    }
}
