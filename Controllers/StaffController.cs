using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/staff")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private const string FrontDeskRoles=RoleNames.Admin + "," + RoleNames.Receptionist;

        private readonly IStaffService staffService;
        private readonly ICurrentUserService currentUser;

        public StaffController(IStaffService _staffService, ICurrentUserService _currentUser)
        {
            staffService=_staffService;
            currentUser=_currentUser;
        }

        [HttpGet]
        [Authorize(Roles=FrontDeskRoles)]
        public async Task<ActionResult<IEnumerable<StaffResponseDto>>> GetAll([FromQuery] string? role, [FromQuery] string? search, [FromQuery] bool includeInactive=false)
        {
            return Ok(await staffService.GetStaffAsync(role, search, !includeInactive));
        }

        // Any signed-in user may pick a referring doctor while booking.
        [HttpGet("doctors")]
        public async Task<ActionResult<IEnumerable<StaffResponseDto>>> GetDoctors([FromQuery] string? search)
        {
            return Ok(await staffService.GetStaffAsync(RoleNames.Doctor, search, true));
        }

        [HttpGet("phlebotomists")]
        [Authorize(Roles=FrontDeskRoles)]
        public async Task<ActionResult<IEnumerable<StaffResponseDto>>> GetPhlebotomists([FromQuery] string? search)
        {
            return Ok(await staffService.GetStaffAsync(RoleNames.Phlebotomist, search, true));
        }

        [HttpGet("{userId:int}")]
        public async Task<ActionResult<StaffResponseDto>> GetById(int userId)
        {
            if (currentUser.UserId != userId && !currentUser.IsInRole(RoleNames.Admin, RoleNames.Receptionist))
            {
                return Forbid();
            }
            return Ok(await staffService.GetByUserIdAsync(userId));
        }

        [HttpPut("{userId:int}/profile")]
        public async Task<ActionResult<StaffResponseDto>> UpdateProfile(int userId, StaffProfileUpdateDto dto)
        {
            return Ok(await staffService.UpdateProfileAsync(userId, dto));
        }

        [HttpPatch("me/availability")]
        [Authorize(Roles=RoleNames.Phlebotomist)]
        public async Task<ActionResult<StaffResponseDto>> SetMyAvailability(PhlebotomistAvailabilityDto dto)
        {
            return Ok(await staffService.SetMyAvailabilityAsync(dto.IsAvailable));
        }
    }
}
