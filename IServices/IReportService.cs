using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.IServices
{
    public interface IReportService
    {
        // Patients and doctors only see Released reports (their own / their referrals); lab staff see all.
        Task<IEnumerable<ReportResponseDto>> SearchAsync(string? status);
        Task<ReportResponseDto> GetByIdAsync(int id);
        Task<ReportResponseDto> GetByAppointmentIdAsync(int appointmentId);
        Task<ReportResponseDto> ValidateAsync(int id, ReportReviewDto dto);
        Task<ReportResponseDto> RejectAsync(int id, ReportReviewDto dto);
        Task<ReportResponseDto> ResubmitAsync(int id);
        Task<ReportResponseDto> ReleaseAsync(int id);
        Task<(byte[] Content, string FileName)> GetPdfAsync(int id);
    }
}
