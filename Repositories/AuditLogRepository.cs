using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly LabLinkDbContext context;
        public AuditLogRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task AddAsync(AuditLog log)
        {
            await context.AuditLogs.AddAsync(log);
            await context.SaveChangesAsync();
        }

        public async Task<(List<AuditLog> Items, int TotalCount)> SearchAsync(AuditLogFilterDto filter)
        {
            var query=context.AuditLogs.Include(a=>a.User).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.EntityName))
            {
                query=query.Where(a=>a.EntityName==filter.EntityName);
            }

            if (filter.EntityId.HasValue)
            {
                query=query.Where(a=>a.EntityId==filter.EntityId.Value);
            }

            if (filter.UserId.HasValue)
            {
                query=query.Where(a=>a.UserId==filter.UserId.Value);
            }

            if (filter.From.HasValue)
            {
                query=query.Where(a=>a.TimeStamp>=filter.From.Value);
            }

            if (filter.To.HasValue)
            {
                query=query.Where(a=>a.TimeStamp<=filter.To.Value);
            }

            var total=await query.CountAsync();
            var items=await query
                .OrderByDescending(a=>a.TimeStamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
