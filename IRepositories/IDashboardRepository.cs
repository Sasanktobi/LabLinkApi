using System;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IRepositories
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryDto> GetSummaryAsync(DateTime today);
    }
}
