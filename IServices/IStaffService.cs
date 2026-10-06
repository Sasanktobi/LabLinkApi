using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffResponseDto>> GetStaffAsync(string? roleName, string? search, bool activeOnly);
        Task<StaffResponseDto> GetByUserIdAsync(int userId);
        Task<StaffResponseDto> UpdateProfileAsync(int userId, StaffProfileUpdateDto dto);
        Task<StaffResponseDto> SetMyAvailabilityAsync(bool isAvailable);
    }
}
