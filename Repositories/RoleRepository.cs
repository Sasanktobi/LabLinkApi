using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly LabLinkDbContext context;
        public RoleRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await context.Roles.AsNoTracking().OrderBy(r=>r.RoleId).ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await context.Roles.FindAsync(id);
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await context.Roles.FirstOrDefaultAsync(r=>r.Name==name);
        }
    }
}
