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
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ICurrentUserService currentUser;

        public UsersController(IUserService _userService, ICurrentUserService _currentUser)
        {
            userService=_userService;
            currentUser=_currentUser;
        }

        [HttpGet]
        [Authorize(Roles=RoleNames.Admin)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllActive()
        {
            return Ok(await userService.GetAllActiveAsync());
        }

        [HttpGet("by-role/{roleId:int}")]
        [Authorize(Roles=RoleNames.Admin)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetByRole(int roleId)
        {
            return Ok(await userService.GetByRoleIdAsync(roleId));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetById(int id)
        {
            if (!IsSelfOrAdmin(id))
            {
                return Forbid();
            }

            var user=await userService.GetByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        // Creates a staff account (Admin, Doctor, Pathologist, LabTechnician, Phlebotomist, Receptionist) with its profile.
        [HttpPost]
        [Authorize(Roles=RoleNames.Admin)]
        public async Task<ActionResult<UserResponseDto>> Create(StaffCreateDto dto)
        {
            var created=await userService.RegisterAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id=created.UserId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> Update(int id, UserUpdateDto dto)
        {
            if (!IsSelfOrAdmin(id))
            {
                return Forbid();
            }

            var updated=await userService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles=RoleNames.Admin)]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (id == currentUser.UserId)
            {
                return BadRequest(new ProblemDetails { Status=400, Title="Bad Request", Detail="You cannot deactivate your own account." });
            }

            return await userService.DeactivateAsync(id) ? NoContent() : NotFound();
        }

        [HttpPatch("{id:int}/activate")]
        [Authorize(Roles=RoleNames.Admin)]
        public async Task<IActionResult> Activate(int id)
        {
            return await userService.ActivateAsync(id) ? NoContent() : NotFound();
        }

        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var changed=await userService.ChangePasswordAsync(currentUser.RequireUserId(), dto);
            if (!changed)
            {
                return BadRequest(new ProblemDetails { Status=400, Title="Bad Request", Detail="Current password is incorrect." });
            }
            return NoContent();
        }

        private bool IsSelfOrAdmin(int id)
        {
            return currentUser.UserId == id || currentUser.IsInRole(RoleNames.Admin);
        }
    }
}
