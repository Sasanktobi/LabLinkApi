using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto dto);
        Task<AppointmentResponseDto> GetByIdAsync(int id);
        // Results are scoped to the caller: patients see their own, doctors their referrals, phlebotomists their assignments.
        Task<PagedResult<AppointmentResponseDto>> SearchAsync(AppointmentFilterDto filter);
        Task<AppointmentResponseDto> RescheduleAsync(int id, AppointmentRescheduleDto dto);
        Task<AppointmentResponseDto> ConfirmAsync(int id);
        Task<AppointmentResponseDto> AssignPhlebotomistAsync(int id, AssignPhlebotomistDto dto);
        Task<AppointmentResponseDto> CancelAsync(int id, AppointmentCancelDto dto);
    }
}
