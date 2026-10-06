using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Roles=RoleNames.Admin)]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository roleRepository;

        public RolesController(IRoleRepository _roleRepository)
        {
            roleRepository=_roleRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var roles=await roleRepository.GetAllAsync();
            return Ok(roles.Select(r=>new { r.RoleId, r.Name }));
        }
    }
}
