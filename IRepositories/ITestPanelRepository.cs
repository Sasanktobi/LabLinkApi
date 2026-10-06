using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface ITestPanelRepository
    {
        Task<IEnumerable<TestPanel>> GetAllAsync(bool includeInactive);
        Task<TestPanel?> GetByIdAsync(int id);
        Task<List<TestPanel>> GetByIdsAsync(IEnumerable<int> ids);
        Task<bool> NameExistsAsync(string name, int? excludeId);
        Task<TestPanel> AddAsync(TestPanel panel);
        Task SaveChangesAsync();
    }
}
