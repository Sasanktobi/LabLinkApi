using System;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateToken(UserResponseDto user);
    }
}
