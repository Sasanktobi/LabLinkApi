using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly LabLinkDbContext context;
        public StaffRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<IEnumerable<User>> GetStaffUsersAsync(string? roleName, string? search, bool activeOnly)
        {
            var query=context.Users
                .Include(u=>u.Role)
                .Include(u=>u.Admin)
                .Include(u=>u.Doctor)
                .Include(u=>u.Pathologist)
                .Include(u=>u.LabTechnician)
                .Include(u=>u.Phlebotomist)
                .Include(u=>u.Receptionist)
                .Where(u=>u.Role.Name!=RoleNames.Patient)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                query=query.Where(u=>u.Role.Name==roleName);
            }

            if (activeOnly)
            {
                query=query.Where(u=>u.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term=search.Trim();
                query=query.Where(u=>
                    u.FirstName.Contains(term) ||
                    u.LastName.Contains(term) ||
                    u.Email.Contains(term));
            }

            return await query.OrderBy(u=>u.FirstName).ThenBy(u=>u.LastName).ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int doctorId)
        {
            return await context.Doctors.Include(d=>d.User).FirstOrDefaultAsync(d=>d.DoctorId==doctorId);
        }

        public async Task<Doctor?> GetDoctorByUserIdAsync(int userId)
        {
            return await context.Doctors.Include(d=>d.User).FirstOrDefaultAsync(d=>d.UserId==userId);
        }

        public async Task<Pathologist?> GetPathologistByUserIdAsync(int userId)
        {
            return await context.Pathologists.Include(p=>p.User).FirstOrDefaultAsync(p=>p.UserId==userId);
        }

        public async Task<LabTechnician?> GetLabTechnicianByUserIdAsync(int userId)
        {
            return await context.LabTechnicians.Include(l=>l.User).FirstOrDefaultAsync(l=>l.UserId==userId);
        }

        public async Task<Phlebotomist?> GetPhlebotomistByIdAsync(int phlebotomistId)
        {
            return await context.Phlebotomists.Include(p=>p.User).FirstOrDefaultAsync(p=>p.PhlebotomistId==phlebotomistId);
        }

        public async Task<Phlebotomist?> GetPhlebotomistByUserIdAsync(int userId)
        {
            return await context.Phlebotomists.Include(p=>p.User).FirstOrDefaultAsync(p=>p.UserId==userId);
        }

        public async Task<bool> LicenseNumberExistsAsync(string roleName, string licenseNumber, int? excludeUserId)
        {
            if (roleName == RoleNames.Doctor)
            {
                return await context.Doctors.AnyAsync(d=>d.LicenseNumber==licenseNumber && d.UserId!=excludeUserId);
            }

            if (roleName == RoleNames.Pathologist)
            {
                return await context.Pathologists.AnyAsync(p=>p.LicenseNumber==licenseNumber && p.UserId!=excludeUserId);
            }

            return false;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
