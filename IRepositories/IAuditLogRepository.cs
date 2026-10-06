using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<(List<AuditLog> Items, int TotalCount)> SearchAsync(AuditLogFilterDto filter);
    }
}
