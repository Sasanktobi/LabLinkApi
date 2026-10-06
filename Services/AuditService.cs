using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.IRepositories;
using Backend.IServices;
using Backend.Models;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository auditLogRepository;
        private readonly ICurrentUserService currentUser;
        private readonly ILogger<AuditService> logger;

        public AuditService(IAuditLogRepository _auditLogRepository, ICurrentUserService _currentUser, ILogger<AuditService> _logger)
        {
            auditLogRepository=_auditLogRepository;
            currentUser=_currentUser;
            logger=_logger;
        }

        public Task LogAsync(string entityName, int entityId, string action, string details)
        {
            return LogAsync(entityName, entityId, action, details, currentUser.UserId);
        }

        public async Task LogAsync(string entityName, int entityId, string action, string details, int? userId)
        {
            try
            {
                await auditLogRepository.AddAsync(new AuditLog
                {
                    EntityName=entityName,
                    EntityId=entityId,
                    Action=action,
                    Details=details,
                    TimeStamp=DateTime.UtcNow,
                    UserId=userId
                });
            }
            catch (Exception ex)
            {
                // The business operation has already been saved; a failed audit write must not turn it into an error response.
                logger.LogError(ex, "Failed to write audit log for {EntityName} {EntityId} {Action}", entityName, entityId, action);
            }
        }

        public async Task<PagedResult<AuditLogResponseDto>> SearchAsync(AuditLogFilterDto filter)
        {
            var (items, total)=await auditLogRepository.SearchAsync(filter);
            return new PagedResult<AuditLogResponseDto>
            {
                Page=filter.Page,
                PageSize=filter.PageSize,
                TotalCount=total,
                Items=items.Select(a=>new AuditLogResponseDto
                {
                    AuditLogId=a.AuditLogId,
                    EntityName=a.EntityName,
                    EntityId=a.EntityId,
                    Action=a.Action,
                    Details=a.Details,
                    TimeStamp=a.TimeStamp,
                    UserId=a.UserId,
                    UserName=a.User == null ? null : $"{a.User.FirstName} {a.User.LastName}"
                }).ToList()
            };
        }
    }
}
