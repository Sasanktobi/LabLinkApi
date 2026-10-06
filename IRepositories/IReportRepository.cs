using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface IReportRepository
    {
        Task<Report?> GetByIdAsync(int id);
        Task<Report?> GetByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<Report>> SearchAsync(string? status, int? patientId, int? doctorId);
        Task SaveChangesAsync();
    }
}
