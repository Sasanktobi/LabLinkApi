using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface ISpecimenService
    {
        Task<SpecimenResponseDto> CollectAsync(SpecimenCreateDto dto);
        Task<SpecimenResponseDto> GetByIdAsync(int id);
        Task<IEnumerable<SpecimenResponseDto>> SearchAsync(int? appointmentId, string? status);
        Task<SpecimenResponseDto> ReceiveAsync(int id);
        Task<SpecimenResponseDto> RejectAsync(int id, SpecimenRejectDto dto);
    }
}
