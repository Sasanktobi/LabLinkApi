using System;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.IRepositories;
using Backend.IServices;

namespace Backend.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository dashboardRepository;

        public DashboardService(IDashboardRepository _dashboardRepository)
        {
            dashboardRepository=_dashboardRepository;
        }

        public Task<DashboardSummaryDto> GetSummaryAsync()
        {
            return dashboardRepository.GetSummaryAsync(DateTime.Today);
        }
    }
}
