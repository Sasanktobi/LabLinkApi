using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class TestPanelRepository : ITestPanelRepository
    {
        private readonly LabLinkDbContext context;
        public TestPanelRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<IEnumerable<TestPanel>> GetAllAsync(bool includeInactive)
        {
            var query=context.TestPanels
                .Include(p=>p.TestPanelTests).ThenInclude(pt=>pt.Test)
                .AsNoTracking();

            if (!includeInactive)
            {
                query=query.Where(p=>p.IsActive);
            }

            return await query.OrderBy(p=>p.Name).ToListAsync();
        }

        public async Task<TestPanel?> GetByIdAsync(int id)
        {
            return await context.TestPanels
                .Include(p=>p.TestPanelTests).ThenInclude(pt=>pt.Test)
                .FirstOrDefaultAsync(p=>p.TestPanelId==id);
        }

        public async Task<List<TestPanel>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList=ids.Distinct().ToList();
            return await context.TestPanels
                .Include(p=>p.TestPanelTests).ThenInclude(pt=>pt.Test)
                .Where(p=>idList.Contains(p.TestPanelId))
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId)
        {
            return await context.TestPanels.AnyAsync(p=>p.Name==name && p.TestPanelId!=excludeId);
        }

        public async Task<TestPanel> AddAsync(TestPanel panel)
        {
            await context.TestPanels.AddAsync(panel);
            await context.SaveChangesAsync();
            return panel;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
