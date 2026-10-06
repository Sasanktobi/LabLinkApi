using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.IServices;
using Backend.Models;

namespace Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService userService;
        private readonly ITokenService tokenService;
        private readonly IAuditService auditService;

        public AuthService(IUserService _userService, ITokenService _tokenService, IAuditService _auditService)
        {
            userService=_userService;
            tokenService=_tokenService;
            auditService=_auditService;
        }

        public async Task<AuthResponseDto> RegisterPatientAsync(PatientRegisterDto dto)
        {
            var user=await userService.RegisterPatientAsync(dto);
            return BuildResponse(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user=await userService.AuthenticateAsync(dto);
            if (user == null)
            {
                return null;
            }

            await auditService.LogAsync(nameof(User), user.UserId, AuditActions.Login, "Signed in.", user.UserId);
            return BuildResponse(user);
        }

        private AuthResponseDto BuildResponse(UserResponseDto user)
        {
            var (token, expiresAt)=tokenService.CreateToken(user);
            return new AuthResponseDto
            {
                Token=token,
                ExpiresAt=expiresAt,
                User=user
            };
        }
    }
}
