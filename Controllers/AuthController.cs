using System.Threading.Tasks;
using Backend.DTOs;
using Backend.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IUserService userService;
        private readonly ICurrentUserService currentUser;

        public AuthController(IAuthService _authService, IUserService _userService, ICurrentUserService _currentUser)
        {
            authService=_authService;
            userService=_userService;
            currentUser=_currentUser;
        }

        // Patient self-registration. Returns a token so the user is signed in straight away.
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Register(PatientRegisterDto dto)
        {
            var response=await authService.RegisterPatientAsync(dto);
            return CreatedAtAction(nameof(Me), null, response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var response=await authService.LoginAsync(dto);
            if (response == null)
            {
                return Unauthorized(new ProblemDetails { Status=401, Title="Unauthorized", Detail="Invalid email or password." });
            }
            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> Me()
        {
            var user=await userService.GetByIdAsync(currentUser.RequireUserId());
            return user == null ? NotFound() : Ok(user);
        }

        [HttpGet("email-exists")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> EmailExists([FromQuery] string email)
        {
            return Ok(await userService.EmailExistsAsync(email));
        }
    }
}
