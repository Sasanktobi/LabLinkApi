using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface ITestCatalogueService
    {
        Task<IEnumerable<TestResponseDto>> GetTestsAsync(string? search, bool includeInactive);
        Task<TestResponseDto> GetTestAsync(int id);
        Task<TestResponseDto> CreateTestAsync(TestUpsertDto dto);
        Task<TestResponseDto> UpdateTestAsync(int id, TestUpsertDto dto);
        Task SetTestActiveAsync(int id, bool isActive);

        Task<IEnumerable<TestPanelResponseDto>> GetPanelsAsync(bool includeInactive);
        Task<TestPanelResponseDto> GetPanelAsync(int id);
        Task<TestPanelResponseDto> CreatePanelAsync(TestPanelUpsertDto dto);
        Task<TestPanelResponseDto> UpdatePanelAsync(int id, TestPanelUpsertDto dto);
        Task SetPanelActiveAsync(int id, bool isActive);
    }
}
