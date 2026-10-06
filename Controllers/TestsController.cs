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
    [Route("api/tests")]
    [Authorize(Roles=RoleNames.Admin)]
    public class TestsController : ControllerBase
    {
        private readonly ITestCatalogueService catalogueService;

        public TestsController(ITestCatalogueService _catalogueService)
        {
            catalogueService=_catalogueService;
        }

        // The catalogue is public so it can be browsed before signing up.
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TestResponseDto>>> GetAll([FromQuery] string? search, [FromQuery] bool includeInactive=false)
        {
            return Ok(await catalogueService.GetTestsAsync(search, includeInactive));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<TestResponseDto>> GetById(int id)
        {
            return Ok(await catalogueService.GetTestAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<TestResponseDto>> Create(TestUpsertDto dto)
        {
            var created=await catalogueService.CreateTestAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id=created.TestId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TestResponseDto>> Update(int id, TestUpsertDto dto)
        {
            return Ok(await catalogueService.UpdateTestAsync(id, dto));
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await catalogueService.SetTestActiveAsync(id, false);
            return NoContent();
        }

        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await catalogueService.SetTestActiveAsync(id, true);
            return NoContent();
        }
    }
}
