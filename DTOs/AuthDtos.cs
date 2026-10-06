using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    // Public self-registration always creates a Patient; staff accounts are created by an Admin.
    public class PatientRegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName{get;set;}=string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName{get;set;}=string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email{get;set;}=string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNo{get;set;}=string.Empty;

        [Required]
        [MinLength(8)]
        public string Password{get;set;}=string.Empty;

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

    public class AuthResponseDto
    {
        public string Token{get;set;}=string.Empty;
        public DateTime ExpiresAt{get;set;}
        public UserResponseDto User{get;set;}=null!;
    }
}
