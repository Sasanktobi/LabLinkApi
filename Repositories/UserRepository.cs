using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LabLinkDbContext context;
        public UserRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var duplicate=await context.Users.FirstOrDefaultAsync(u=>u.Email==user.Email);
            if (duplicate != null)
            {
                throw new InvalidOperationException($"A user with email '{user.Email}' already exists.");
            }

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await context.Users
                .Include(u=>u.Role)
                .FirstOrDefaultAsync(u=>u.UserId==id);
        }

        public async Task<User?> GetUserWithProfilesAsync(int id)
        {
            return await WithProfiles()
                .FirstOrDefaultAsync(u=>u.UserId==id);
        }

        public async Task<User?> UpdateUserAsync(int id, User user)
        {
            var existing=await context.Users.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            var emailTaken=await context.Users.AnyAsync(u=>u.Email==user.Email && u.UserId!=id);
            if (emailTaken)
            {
                throw new InvalidOperationException($"A user with email '{user.Email}' already exists.");
            }

            existing.FirstName=user.FirstName;
            existing.LastName=user.LastName;
            existing.Email=user.Email;
            existing.PhoneNo=user.PhoneNo;

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<User?> DeleteUserAsync(int id)
        {
            var existing=await context.Users.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.IsActive=false;
            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<User?> ActivateUserAsync(int id)
        {
            var existing=await context.Users.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.IsActive=true;
            await context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> UpdatePasswordAsync(int id, string passwordHash)
        {
            var existing=await context.Users.FindAsync(id);
            if (existing == null)
            {
                return false;
            }

            existing.PasswordHash=passwordHash;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u=>u.Email==email);
        }

        public async Task<User?> GetByEmailWithRoleAsync(string email)
        {
            // Profiles are loaded so the login response can carry the ProfileId.
            return await WithProfiles()
                .FirstOrDefaultAsync(u=>u.Email==email);
        }

        public async Task<IEnumerable<User>> GetByRoleIdAsync(int roleId)
        {
            return await context.Users
                .Include(u=>u.Role)
                .Where(u=>u.RoleId==roleId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await context.Users.AnyAsync(u=>u.Email==email);
        }

        public async Task<bool> IsActiveAsync(int id)
        {
            return await context.Users.AnyAsync(u=>u.UserId==id && u.IsActive);
        }

        public async Task<IEnumerable<User>> GetAllActiveAsync()
        {
            return await context.Users
                .Include(u=>u.Role)
                .Where(u=>u.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        private IQueryable<User> WithProfiles()
        {
            return context.Users
                .Include(u=>u.Role)
                .Include(u=>u.Admin)
                .Include(u=>u.Patient)
                .Include(u=>u.Doctor)
                .Include(u=>u.Pathologist)
                .Include(u=>u.LabTechnician)
                .Include(u=>u.Phlebotomist)
                .Include(u=>u.Receptionist);
        }
    }
}
