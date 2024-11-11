using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        public RolesController()
        {
                
        }
        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "RoleEdit")]
        public async Task<IActionResult> ActiveInactive(string RoleId)
        {
            var res = await tenantService.TenantActiveInactive(TenantId);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "RoleView")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var res = await tenantService.TenantGetAll(filter);
            return Ok(res);
        }
    }
}
