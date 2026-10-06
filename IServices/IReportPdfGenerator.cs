using Backend.DTOs;

namespace Backend.IServices
{
    public interface IReportPdfGenerator
    {
        byte[] Generate(ReportResponseDto report);
    }
}
