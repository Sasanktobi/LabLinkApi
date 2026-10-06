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
    [Route("api/patients")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private const string StaffRoles=RoleNames.Admin + "," + RoleNames.Receptionist + "," + RoleNames.LabTechnician + ","
            + RoleNames.Pathologist + "," + RoleNames.Phlebotomist + "," + RoleNames.Doctor;
        private const string FrontDeskRoles=RoleNames.Admin + "," + RoleNames.Receptionist;

        private readonly IPatientService patientService;
        private readonly IUserService userService;

        public PatientsController(IPatientService _patientService, IUserService _userService)
        {
            patientService=_patientService;
            userService=_userService;
        }

        [HttpGet]
        [Authorize(Roles=FrontDeskRoles + "," + RoleNames.LabTechnician + "," + RoleNames.Pathologist)]
        public async Task<ActionResult<IEnumerable<PatientResponseDto>>> Search([FromQuery] string? search)
        {
            return Ok(await patientService.SearchAsync(search));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles=StaffRoles + "," + RoleNames.Patient)]
        public async Task<ActionResult<PatientResponseDto>> GetById(int id)
        {
            return Ok(await patientService.GetByIdAsync(id));
        }

        // Walk-in registration at the front desk.
        [HttpPost]
        [Authorize(Roles=FrontDeskRoles)]
        public async Task<ActionResult<UserResponseDto>> Register(PatientRegisterDto dto)
        {
            var created=await userService.RegisterPatientAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id=created.ProfileId }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles=FrontDeskRoles)]
        public async Task<ActionResult<PatientResponseDto>> Update(int id, PatientUpdateDto dto)
        {
            return Ok(await patientService.UpdateAsync(id, dto));
        }

        [HttpGet("me")]
        [Authorize(Roles=RoleNames.Patient)]
        public async Task<ActionResult<PatientResponseDto>> GetMine()
        {
            return Ok(await patientService.GetMineAsync());
        }

        [HttpPut("me")]
        [Authorize(Roles=RoleNames.Patient)]
        public async Task<ActionResult<PatientResponseDto>> UpdateMine(PatientUpdateDto dto)
        {
            return Ok(await patientService.UpdateMineAsync(dto));
        }
    }
}
