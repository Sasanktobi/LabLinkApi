using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientResponseDto>> SearchAsync(string? search);
        Task<PatientResponseDto> GetByIdAsync(int patientId);
        Task<PatientResponseDto> GetMineAsync();
        Task<PatientResponseDto> UpdateAsync(int patientId, PatientUpdateDto dto);
        Task<PatientResponseDto> UpdateMineAsync(PatientUpdateDto dto);
    }
}
