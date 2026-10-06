using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Configuration;
using Backend.DTOs;
using Backend.IServices;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings settings;

        public TokenService(IOptions<JwtSettings> _settings)
        {
            settings=_settings.Value;
        }

        public (string Token, DateTime ExpiresAt) CreateToken(UserResponseDto user)
        {
            var expiresAt=DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes);

            var claims=new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.RoleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.ProfileId.HasValue)
            {
                claims.Add(new Claim("profileId", user.ProfileId.Value.ToString()));
            }

            var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
            var token=new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
