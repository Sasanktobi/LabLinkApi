using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface IStaffRepository
    {
        Task<IEnumerable<User>> GetStaffUsersAsync(string? roleName, string? search, bool activeOnly);
        Task<Doctor?> GetDoctorByIdAsync(int doctorId);
        Task<Doctor?> GetDoctorByUserIdAsync(int userId);
        Task<Pathologist?> GetPathologistByUserIdAsync(int userId);
        Task<LabTechnician?> GetLabTechnicianByUserIdAsync(int userId);
        Task<Phlebotomist?> GetPhlebotomistByIdAsync(int phlebotomistId);
        Task<Phlebotomist?> GetPhlebotomistByUserIdAsync(int userId);
        Task<bool> LicenseNumberExistsAsync(string roleName, string licenseNumber, int? excludeUserId);
        Task SaveChangesAsync();
    }
}
