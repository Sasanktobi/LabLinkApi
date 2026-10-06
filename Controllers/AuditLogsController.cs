using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize(Roles=RoleNames.Admin)]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService auditService;

        public AuditLogsController(IAuditService _auditService)
        {
            auditService=_auditService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<AuditLogResponseDto>>> Search([FromQuery] AuditLogFilterDto filter)
        {
            return Ok(await auditService.SearchAsync(filter));
        }
    }
}
