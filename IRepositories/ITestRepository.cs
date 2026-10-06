using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface ITestRepository
    {
        Task<IEnumerable<Test>> GetAllAsync(string? search, bool includeInactive);
        Task<Test?> GetByIdAsync(int id);
        Task<List<Test>> GetByIdsAsync(IEnumerable<int> ids);
        Task<bool> NameExistsAsync(string name, int? excludeId);
        Task<bool> CodeExistsAsync(string code, int? excludeId);
        Task<Test> AddAsync(Test test);
        Task SaveChangesAsync();
    }
}
