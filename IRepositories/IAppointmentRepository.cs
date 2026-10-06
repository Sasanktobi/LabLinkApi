using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface IAppointmentRepository
    {
        // Tracked, with patient, doctor, phlebotomist, ordered tests, specimens and report loaded.
        Task<Appointment?> GetWithDetailsAsync(int id);
        Task<(List<Appointment> Items, int TotalCount)> SearchAsync(AppointmentFilterDto filter);
        Task<Appointment> AddAsync(Appointment appointment);
        Task SaveChangesAsync();
    }
}
