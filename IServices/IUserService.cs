using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IUserService
    {
        // Staff accounts (any role except Patient), created by an Admin together with their role profile.
        Task<UserResponseDto> RegisterAsync(StaffCreateDto dto);
        // Patient account + patient profile. Used by self-registration and by reception desk walk-ins.
        Task<UserResponseDto> RegisterPatientAsync(PatientRegisterDto dto);
        Task<UserResponseDto?> GetByIdAsync(int id);
        Task<UserResponseDto?> UpdateAsync(int id, UserUpdateDto dto);
        Task<bool> DeactivateAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<IEnumerable<UserResponseDto>> GetAllActiveAsync();
        Task<IEnumerable<UserResponseDto>> GetByRoleIdAsync(int roleId);
        Task<UserResponseDto?> AuthenticateAsync(LoginDto dto);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto);
        Task<bool> EmailExistsAsync(string email);
    }
}
