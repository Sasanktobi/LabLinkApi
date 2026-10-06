using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IAvailabilitySlotService
    {
        Task<IEnumerable<SlotResponseDto>> SearchAsync(DateTime? fromDate, DateTime? toDate, string? providerType, int? userId, bool availableOnly);
        Task<SlotResponseDto> GetByIdAsync(int id);
        Task<SlotResponseDto> CreateAsync(SlotCreateDto dto);
        Task<IEnumerable<SlotResponseDto>> CreateBulkAsync(SlotBulkCreateDto dto);
        Task DeleteAsync(int id);
    }
}
