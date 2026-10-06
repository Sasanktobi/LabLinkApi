using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(int patientId);
        Task<Patient?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Patient>> SearchAsync(string? search);
        Task SaveChangesAsync();
    }
}
