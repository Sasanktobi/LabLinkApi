using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class UserUpdateDto
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
    }
}
