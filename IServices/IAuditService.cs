using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IAuditService
    {
        // Records an action by the current user (or anonymous when there is none).
        Task LogAsync(string entityName, int entityId, string action, string details);
        Task LogAsync(string entityName, int entityId, string action, string details, int? userId);
        Task<PagedResult<AuditLogResponseDto>> SearchAsync(AuditLogFilterDto filter);
    }
}
