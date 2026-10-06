using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly LabLinkDbContext context;
        public TestResultRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<TestResult?> GetByIdAsync(int id)
        {
            return await context.TestResults
                .Include(r=>r.Test)
                .Include(r=>r.EnteredByLabTechnician).ThenInclude(l=>l!.User)
                .Include(r=>r.Specimen).ThenInclude(s=>s.Appointment).ThenInclude(a=>a.Report)
                .FirstOrDefaultAsync(r=>r.TestResultId==id);
        }

        public async Task<List<TestResult>> GetByAppointmentIdAsync(int appointmentId)
        {
            return await context.TestResults
                .Include(r=>r.Test)
                .Include(r=>r.Specimen)
                .Include(r=>r.EnteredByLabTechnician).ThenInclude(l=>l!.User)
                .Where(r=>r.Specimen.AppointmentId==appointmentId)
                .OrderBy(r=>r.Test.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<TestResult>> GetBySpecimenIdAsync(int specimenId)
        {
            return await context.TestResults
                .Include(r=>r.Test)
                .Include(r=>r.Specimen)
                .Include(r=>r.EnteredByLabTechnician).ThenInclude(l=>l!.User)
                .Where(r=>r.SpecimenId==specimenId)
                .OrderBy(r=>r.Test.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<TestResult> results)
        {
            await context.TestResults.AddRangeAsync(results);
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
