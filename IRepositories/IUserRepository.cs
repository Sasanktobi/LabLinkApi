using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserWithProfilesAsync(int id);
        Task<User?> UpdateUserAsync(int id, User user);
        Task<User?> DeleteUserAsync(int id);
        Task<User?> ActivateUserAsync(int id);
        Task<bool> UpdatePasswordAsync(int id, string passwordHash);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailWithRoleAsync(string email);
        Task<IEnumerable<User>> GetByRoleIdAsync(int roleId);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> IsActiveAsync(int id);
        Task<IEnumerable<User>> GetAllActiveAsync();
    }
}
