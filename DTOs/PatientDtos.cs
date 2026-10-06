using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class PatientResponseDto
    {
        public int PatientId{get;set;}
        public int UserId{get;set;}
        public string FirstName{get;set;}=string.Empty;
        public string LastName{get;set;}=string.Empty;
        public string Email{get;set;}=string.Empty;
        public string PhoneNo{get;set;}=string.Empty;
        public bool IsActive{get;set;}
        public DateTime DateOfBirth{get;set;}
        public int Age{get;set;}
        public string Gender{get;set;}=string.Empty;
        public string BloodGroup{get;set;}=string.Empty;
        public string Address{get;set;}=string.Empty;
        public string City{get;set;}=string.Empty;
        public string State{get;set;}=string.Empty;
        public string PostalCode{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}
    }

    public class PatientUpdateDto
    {
        [Required]
        public DateTime DateOfBirth{get;set;}

        [Required]
        [MaxLength(20)]
        public string Gender{get;set;}=string.Empty;

        [MaxLength(10)]
        public string BloodGroup{get;set;}=string.Empty;

        public string Address{get;set;}=string.Empty;

        [MaxLength(100)]
        public string City{get;set;}=string.Empty;

        [MaxLength(100)]
        public string State{get;set;}=string.Empty;

        [MaxLength(20)]
        public string PostalCode{get;set;}=string.Empty;
    }
}
