using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class UserCreateDto
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
        public int RoleId{get;set;}
    }
}
