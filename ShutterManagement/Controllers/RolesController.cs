using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService roleService;

        public RolesController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "RolesAdd")]
        public async Task<IActionResult> Create([FromBody] AddRole model)
        {
            var res = await roleService.Create(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize(Roles = "RolesEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateRole model)
        {
            var res = await roleService.Update(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "RolesDelete")]
        public async Task<IActionResult> Delete(string RoleId)
        {
            var res = await roleService.Delete(RoleId);
            return Ok(res);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "RolesEdit")]
        public async Task<IActionResult> ActiveInactive(string RoleId)
        {
            var res = await roleService.ActiveInactive(RoleId);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "RolesView")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var res = await roleService.GetAll(filter);
            return Ok(res);
        }
    }
}
