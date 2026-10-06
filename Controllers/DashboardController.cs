using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles=RoleNames.Admin + "," + RoleNames.Receptionist + "," + RoleNames.Pathologist + "," + RoleNames.LabTechnician)]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService dashboardService;

        public DashboardController(IDashboardService _dashboardService)
        {
            dashboardService=_dashboardService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            return Ok(await dashboardService.GetSummaryAsync());
        }
    }
}
