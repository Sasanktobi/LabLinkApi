using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.IRepositories
{
    public interface IAvailabilitySlotRepository
    {
        Task<AvailabilitySlot?> GetByIdAsync(int id);
        Task<IEnumerable<AvailabilitySlot>> SearchAsync(DateTime? fromDate, DateTime? toDate, string? providerType, int? userId, bool availableOnly);
        Task<IEnumerable<AvailabilitySlot>> GetForUserOnDateAsync(int userId, DateTime date);
        Task AddRangeAsync(IEnumerable<AvailabilitySlot> slots);
        Task DeleteAsync(AvailabilitySlot slot);
        Task SaveChangesAsync();
    }
}
