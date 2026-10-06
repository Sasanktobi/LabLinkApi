using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private const string ReleaseRoles=RoleNames.Pathologist + "," + RoleNames.Receptionist + "," + RoleNames.Admin;

        private readonly IReportService reportService;

        public ReportsController(IReportService _reportService)
        {
            reportService=_reportService;
        }

        // Patients and doctors only ever receive Released reports; the status filter is ignored for them.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportResponseDto>>> Search([FromQuery] string? status)
        {
            return Ok(await reportService.SearchAsync(status));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReportResponseDto>> GetById(int id)
        {
            return Ok(await reportService.GetByIdAsync(id));
        }

        [HttpGet("by-appointment/{appointmentId:int}")]
        public async Task<ActionResult<ReportResponseDto>> GetByAppointment(int appointmentId)
        {
            return Ok(await reportService.GetByAppointmentIdAsync(appointmentId));
        }

        [HttpPatch("{id:int}/validate")]
        [Authorize(Roles=RoleNames.Pathologist)]
        public async Task<ActionResult<ReportResponseDto>> Validate(int id, ReportReviewDto dto)
        {
            return Ok(await reportService.ValidateAsync(id, dto));
        }

        // Sends the report back to the lab for corrections; remarks are required.
        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles=RoleNames.Pathologist)]
        public async Task<ActionResult<ReportResponseDto>> Reject(int id, ReportReviewDto dto)
        {
            return Ok(await reportService.RejectAsync(id, dto));
        }

        [HttpPatch("{id:int}/resubmit")]
        [Authorize(Roles=RoleNames.LabTechnician)]
        public async Task<ActionResult<ReportResponseDto>> Resubmit(int id)
        {
            return Ok(await reportService.ResubmitAsync(id));
        }

        [HttpPatch("{id:int}/release")]
        [Authorize(Roles=ReleaseRoles)]
        public async Task<ActionResult<ReportResponseDto>> Release(int id)
        {
            return Ok(await reportService.ReleaseAsync(id));
        }

        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var (content, fileName)=await reportService.GetPdfAsync(id);
            return File(content, "application/pdf", fileName);
        }
    }
}
