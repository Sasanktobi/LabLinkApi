using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface ITestResultService
    {
        Task<TestResultResponseDto> CreateAsync(TestResultCreateDto dto);
        Task<IEnumerable<TestResultResponseDto>> CreateBulkAsync(TestResultBulkCreateDto dto);
        Task<TestResultResponseDto> UpdateAsync(int id, TestResultUpdateDto dto);
        Task<TestResultResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<TestResultResponseDto>> GetByAppointmentAsync(int appointmentId);
        Task<IEnumerable<TestResultResponseDto>> GetBySpecimenAsync(int specimenId);
    }
}
