using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class AvailabilitySlotRepository : IAvailabilitySlotRepository
    {
        private readonly LabLinkDbContext context;
        public AvailabilitySlotRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<AvailabilitySlot?> GetByIdAsync(int id)
        {
            return await context.AvailabilitySlots
                .Include(s=>s.User).ThenInclude(u=>u.Role)
                .FirstOrDefaultAsync(s=>s.AvailabilitySlotId==id);
        }

        public async Task<IEnumerable<AvailabilitySlot>> SearchAsync(DateTime? fromDate, DateTime? toDate, string? providerType, int? userId, bool availableOnly)
        {
            var query=context.AvailabilitySlots
                .Include(s=>s.User)
                .Where(s=>s.User.IsActive)
                .AsNoTracking();

            if (fromDate.HasValue)
            {
                query=query.Where(s=>s.SlotDate>=fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query=query.Where(s=>s.SlotDate<=toDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(providerType))
            {
                query=query.Where(s=>s.ProviderType==providerType);
            }

            if (userId.HasValue)
            {
                query=query.Where(s=>s.UserId==userId.Value);
            }

            if (availableOnly)
            {
                query=query.Where(s=>!s.IsBooked);
            }

            // "HH:mm" strings sort chronologically.
            return await query
                .OrderBy(s=>s.SlotDate).ThenBy(s=>s.StartTime)
                .Take(500)
                .ToListAsync();
        }

        public async Task<IEnumerable<AvailabilitySlot>> GetForUserOnDateAsync(int userId, DateTime date)
        {
            return await context.AvailabilitySlots
                .Where(s=>s.UserId==userId && s.SlotDate==date.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<AvailabilitySlot> slots)
        {
            await context.AvailabilitySlots.AddRangeAsync(slots);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AvailabilitySlot slot)
        {
            context.AvailabilitySlots.Remove(slot);
            await context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
