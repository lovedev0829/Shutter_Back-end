using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShutterManagement.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly IModulePermission _moduleService;

        public ModuleController(IModulePermission moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpGet("modules")]
        public async Task<IActionResult> GetModulesWithPermissions()
        {

            var res = await _moduleService.GetModulesWithPermissionsAsync();
            return Ok(res);
        }
    }
}
