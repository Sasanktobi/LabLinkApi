using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface ITestResultRepository
    {
        Task<TestResult?> GetByIdAsync(int id);
        Task<List<TestResult>> GetByAppointmentIdAsync(int appointmentId);
        Task<List<TestResult>> GetBySpecimenIdAsync(int specimenId);
        Task AddRangeAsync(IEnumerable<TestResult> results);
        Task SaveChangesAsync();
    }
}
