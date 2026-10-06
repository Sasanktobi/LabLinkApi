using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterPatientAsync(PatientRegisterDto dto);
        // Null when the credentials are wrong or the account is inactive.
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }
}
