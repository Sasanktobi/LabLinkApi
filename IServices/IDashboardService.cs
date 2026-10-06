using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
