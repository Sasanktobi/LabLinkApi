using System;
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
    [Route("api/slots")]
    [Authorize]
    public class SlotsController : ControllerBase
    {
        private const string SlotManagerRoles=RoleNames.Admin + "," + RoleNames.Receptionist + "," + RoleNames.Phlebotomist + "," + RoleNames.LabTechnician;

        private readonly IAvailabilitySlotService slotService;

        public SlotsController(IAvailabilitySlotService _slotService)
        {
            slotService=_slotService;
        }

        // providerType: "Lab" (for LabVisit) or "Phlebotomist" (for HomeCollection).
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SlotResponseDto>>> Search(
            [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string? providerType,
            [FromQuery] int? userId, [FromQuery] bool availableOnly=true)
        {
            return Ok(await slotService.SearchAsync(fromDate ?? DateTime.Today, toDate, providerType, userId, availableOnly));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SlotResponseDto>> GetById(int id)
        {
            return Ok(await slotService.GetByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles=SlotManagerRoles)]
        public async Task<ActionResult<SlotResponseDto>> Create(SlotCreateDto dto)
        {
            var created=await slotService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id=created.AvailabilitySlotId }, created);
        }

        [HttpPost("bulk")]
        [Authorize(Roles=SlotManagerRoles)]
        public async Task<ActionResult<IEnumerable<SlotResponseDto>>> CreateBulk(SlotBulkCreateDto dto)
        {
            return Ok(await slotService.CreateBulkAsync(dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles=SlotManagerRoles)]
        public async Task<IActionResult> Delete(int id)
        {
            await slotService.DeleteAsync(id);
            return NoContent();
        }
    }
}
