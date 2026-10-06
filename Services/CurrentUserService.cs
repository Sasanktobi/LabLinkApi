using System;
using System.Linq;
using System.Security.Claims;
using Backend.Exceptions;
using Backend.IServices;
using Microsoft.AspNetCore.Http;

namespace Backend.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor _httpContextAccessor)
        {
            httpContextAccessor=_httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

        public int? UserId
        {
            get
            {
                var value=Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : null;
            }
        }

        public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

        public bool IsInRole(params string[] roles)
        {
            var role=Role;
            return role != null && roles.Contains(role, StringComparer.Ordinal);
        }

        public int RequireUserId()
        {
            return UserId ?? throw new ForbiddenException("Authentication is required.");
        }
    }
}
