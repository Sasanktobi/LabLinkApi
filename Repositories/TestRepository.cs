using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class TestRepository : ITestRepository
    {
        private readonly LabLinkDbContext context;
        public TestRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<IEnumerable<Test>> GetAllAsync(string? search, bool includeInactive)
        {
            var query=context.Tests.AsNoTracking();

            if (!includeInactive)
            {
                query=query.Where(t=>t.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term=search.Trim();
                query=query.Where(t=>t.Name.Contains(term) || t.Code.Contains(term) || t.SampleType.Contains(term));
            }

            return await query.OrderBy(t=>t.Name).ToListAsync();
        }

        public async Task<Test?> GetByIdAsync(int id)
        {
            return await context.Tests.FindAsync(id);
        }

        public async Task<List<Test>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList=ids.Distinct().ToList();
            return await context.Tests.Where(t=>idList.Contains(t.TestId)).ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId)
        {
            return await context.Tests.AnyAsync(t=>t.Name==name && t.TestId!=excludeId);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId)
        {
            return await context.Tests.AnyAsync(t=>t.Code==code && t.TestId!=excludeId);
        }

        public async Task<Test> AddAsync(Test test)
        {
            await context.Tests.AddAsync(test);
            await context.SaveChangesAsync();
            return test;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
