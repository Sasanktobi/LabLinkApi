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
    [Route("api/test-results")]
    [Authorize]
    public class TestResultsController : ControllerBase
    {
        private const string ReaderRoles=RoleNames.LabTechnician + "," + RoleNames.Pathologist + "," + RoleNames.Admin;

        private readonly ITestResultService testResultService;

        public TestResultsController(ITestResultService _testResultService)
        {
            testResultService=_testResultService;
        }

        [HttpPost]
        [Authorize(Roles=RoleNames.LabTechnician)]
        public async Task<ActionResult<TestResultResponseDto>> Create(TestResultCreateDto dto)
        {
            var created=await testResultService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id=created.TestResultId }, created);
        }

        // Enter several results for one specimen in a single call.
        [HttpPost("bulk")]
        [Authorize(Roles=RoleNames.LabTechnician)]
        public async Task<ActionResult<IEnumerable<TestResultResponseDto>>> CreateBulk(TestResultBulkCreateDto dto)
        {
            return Ok(await testResultService.CreateBulkAsync(dto));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles=RoleNames.LabTechnician)]
        public async Task<ActionResult<TestResultResponseDto>> Update(int id, TestResultUpdateDto dto)
        {
            return Ok(await testResultService.UpdateAsync(id, dto));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles=ReaderRoles)]
        public async Task<ActionResult<TestResultResponseDto>> GetById(int id)
        {
            return Ok(await testResultService.GetByIdAsync(id));
        }

        [HttpGet("by-appointment/{appointmentId:int}")]
        [Authorize(Roles=ReaderRoles)]
        public async Task<ActionResult<IEnumerable<TestResultResponseDto>>> GetByAppointment(int appointmentId)
        {
            return Ok(await testResultService.GetByAppointmentAsync(appointmentId));
        }

        [HttpGet("by-specimen/{specimenId:int}")]
        [Authorize(Roles=ReaderRoles)]
        public async Task<ActionResult<IEnumerable<TestResultResponseDto>>> GetBySpecimen(int specimenId)
        {
            return Ok(await testResultService.GetBySpecimenAsync(specimenId));
        }
    }
}
