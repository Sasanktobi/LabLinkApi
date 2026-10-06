using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private const string BookingRoles=RoleNames.Patient + "," + RoleNames.Receptionist + "," + RoleNames.Admin;
        private const string FrontDeskRoles=RoleNames.Receptionist + "," + RoleNames.Admin;

        private readonly IAppointmentService appointmentService;

        public AppointmentsController(IAppointmentService _appointmentService)
        {
            appointmentService=_appointmentService;
        }

        [HttpPost]
        [Authorize(Roles=BookingRoles)]
        public async Task<ActionResult<AppointmentResponseDto>> Create(AppointmentCreateDto dto)
        {
            var created=await appointmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id=created.AppointmentId }, created);
        }

        // Scoped by role: patients get their own, doctors their referrals, phlebotomists their assignments.
        [HttpGet]
        public async Task<ActionResult<PagedResult<AppointmentResponseDto>>> Search([FromQuery] AppointmentFilterDto filter)
        {
            return Ok(await appointmentService.SearchAsync(filter));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentResponseDto>> GetById(int id)
        {
            return Ok(await appointmentService.GetByIdAsync(id));
        }

        [HttpPut("{id:int}/reschedule")]
        [Authorize(Roles=BookingRoles)]
        public async Task<ActionResult<AppointmentResponseDto>> Reschedule(int id, AppointmentRescheduleDto dto)
        {
            return Ok(await appointmentService.RescheduleAsync(id, dto));
        }

        [HttpPatch("{id:int}/confirm")]
        [Authorize(Roles=FrontDeskRoles)]
        public async Task<ActionResult<AppointmentResponseDto>> Confirm(int id)
        {
            return Ok(await appointmentService.ConfirmAsync(id));
        }

        [HttpPatch("{id:int}/assign-phlebotomist")]
        [Authorize(Roles=FrontDeskRoles)]
        public async Task<ActionResult<AppointmentResponseDto>> AssignPhlebotomist(int id, AssignPhlebotomistDto dto)
        {
            return Ok(await appointmentService.AssignPhlebotomistAsync(id, dto));
        }

        [HttpPatch("{id:int}/cancel")]
        [Authorize(Roles=BookingRoles)]
        public async Task<ActionResult<AppointmentResponseDto>> Cancel(int id, AppointmentCancelDto dto)
        {
            return Ok(await appointmentService.CancelAsync(id, dto));
        }
    }
}
