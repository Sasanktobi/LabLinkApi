using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class SpecimenRepository : ISpecimenRepository
    {
        private readonly LabLinkDbContext context;
        public SpecimenRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<Specimen?> GetByIdAsync(int id)
        {
            return await context.Specimens
                .Include(s=>s.CollectedByUser)
                .Include(s=>s.Appointment).ThenInclude(a=>a.Patient).ThenInclude(p=>p.User)
                .Include(s=>s.Appointment).ThenInclude(a=>a.AppointmentTests).ThenInclude(at=>at.Test)
                .Include(s=>s.Appointment).ThenInclude(a=>a.Specimens)
                .Include(s=>s.Appointment).ThenInclude(a=>a.Report)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s=>s.SpecimenId==id);
        }

        public async Task<IEnumerable<Specimen>> SearchAsync(int? appointmentId, string? status)
        {
            var query=context.Specimens
                .Include(s=>s.CollectedByUser)
                .Include(s=>s.Appointment).ThenInclude(a=>a.Patient).ThenInclude(p=>p.User)
                .AsNoTracking();

            if (appointmentId.HasValue)
            {
                query=query.Where(s=>s.AppointmentId==appointmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query=query.Where(s=>s.Status==status);
            }

            return await query.OrderByDescending(s=>s.CollectedAt).Take(500).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
