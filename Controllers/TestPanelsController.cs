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
    [Route("api/test-panels")]
    [Authorize(Roles=RoleNames.Admin)]
    public class TestPanelsController : ControllerBase
    {
        private readonly ITestCatalogueService catalogueService;

        public TestPanelsController(ITestCatalogueService _catalogueService)
        {
            catalogueService=_catalogueService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TestPanelResponseDto>>> GetAll([FromQuery] bool includeInactive=false)
        {
            return Ok(await catalogueService.GetPanelsAsync(includeInactive));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<TestPanelResponseDto>> GetById(int id)
        {
            return Ok(await catalogueService.GetPanelAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<TestPanelResponseDto>> Create(TestPanelUpsertDto dto)
        {
            var created=await catalogueService.CreatePanelAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id=created.TestPanelId }, created);
        }

        // Replaces the panel's details and its full list of tests.
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TestPanelResponseDto>> Update(int id, TestPanelUpsertDto dto)
        {
            return Ok(await catalogueService.UpdatePanelAsync(id, dto));
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await catalogueService.SetPanelActiveAsync(id, false);
            return NoContent();
        }

        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await catalogueService.SetPanelActiveAsync(id, true);
            return NoContent();
        }
    }
}
