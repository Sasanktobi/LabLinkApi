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
    [Route("api/specimens")]
    [Authorize]
    public class SpecimensController : ControllerBase
    {
        private const string CollectorRoles=RoleNames.Phlebotomist + "," + RoleNames.LabTechnician;
        private const string LabRoles=RoleNames.LabTechnician + "," + RoleNames.Pathologist + "," + RoleNames.Admin + "," + RoleNames.Receptionist;

        private readonly ISpecimenService specimenService;

        public SpecimensController(ISpecimenService _specimenService)
        {
            specimenService=_specimenService;
        }

        [HttpPost]
        [Authorize(Roles=CollectorRoles)]
        public async Task<ActionResult<SpecimenResponseDto>> Collect(SpecimenCreateDto dto)
        {
            var created=await specimenService.CollectAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id=created.SpecimenId }, created);
        }

        [HttpGet]
        [Authorize(Roles=LabRoles)]
        public async Task<ActionResult<IEnumerable<SpecimenResponseDto>>> Search([FromQuery] int? appointmentId, [FromQuery] string? status)
        {
            return Ok(await specimenService.SearchAsync(appointmentId, status));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles=LabRoles)]
        public async Task<ActionResult<SpecimenResponseDto>> GetById(int id)
        {
            return Ok(await specimenService.GetByIdAsync(id));
        }

        [HttpPatch("{id:int}/receive")]
        [Authorize(Roles=RoleNames.LabTechnician)]
        public async Task<ActionResult<SpecimenResponseDto>> Receive(int id)
        {
            return Ok(await specimenService.ReceiveAsync(id));
        }

        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles=RoleNames.LabTechnician)]
        public async Task<ActionResult<SpecimenResponseDto>> Reject(int id, SpecimenRejectDto dto)
        {
            return Ok(await specimenService.RejectAsync(id, dto));
        }
    }
}
