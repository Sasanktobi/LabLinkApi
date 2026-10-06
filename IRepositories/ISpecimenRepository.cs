using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface ISpecimenRepository
    {
        // Tracked, with the parent appointment (tests, specimens, report, patient) loaded.
        Task<Specimen?> GetByIdAsync(int id);
        Task<IEnumerable<Specimen>> SearchAsync(int? appointmentId, string? status);
        Task SaveChangesAsync();
    }
}
