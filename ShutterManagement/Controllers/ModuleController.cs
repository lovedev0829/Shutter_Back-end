using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShutterManagement.WebApi.Controllers
{
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
            IResponseVm<List<ModuleResponse>> res = new ResponseVm<List<ModuleResponse>>(); 
            try
            {
                var modules = await _moduleService.GetModulesWithPermissionsAsync();
                res.IsSuccess = true;
                res.Message = "Modules fetched";
                res.Data = modules;
                res.TotalRecords = modules.Count;

            }
            catch (Exception ex)
            {
                res.Message = $"Internal server error: {ex.Message}";
            }
            return Ok(res);
        }
    }
}
