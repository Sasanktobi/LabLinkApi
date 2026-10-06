using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly LabLinkDbContext context;
        public PatientRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<Patient?> GetByIdAsync(int patientId)
        {
            return await context.Patients
                .Include(p=>p.User)
                .FirstOrDefaultAsync(p=>p.PatientId==patientId);
        }

        public async Task<Patient?> GetByUserIdAsync(int userId)
        {
            return await context.Patients
                .Include(p=>p.User)
                .FirstOrDefaultAsync(p=>p.UserId==userId);
        }

        public async Task<IEnumerable<Patient>> SearchAsync(string? search)
        {
            var query=context.Patients.Include(p=>p.User).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term=search.Trim();
                query=query.Where(p=>
                    p.User.FirstName.Contains(term) ||
                    p.User.LastName.Contains(term) ||
                    p.User.Email.Contains(term) ||
                    p.User.PhoneNo.Contains(term));
            }

            return await query
                .OrderBy(p=>p.User.FirstName).ThenBy(p=>p.User.LastName)
                .Take(200)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
