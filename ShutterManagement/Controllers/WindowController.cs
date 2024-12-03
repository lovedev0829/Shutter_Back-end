using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WindowController : ControllerBase
    {
        private readonly IWindowService windowservice;
        public WindowController(IWindowService windowService)
        {
            this.windowservice = windowService;
        }
        [HttpPost("GetAll")]
        [Authorize]
        public async Task<IActionResult> GetAllWindow(Pagination filter)
        {
            var res = await windowservice.GetAll_Window(filter);
            return Ok(res);

        }

        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> CreateWindow([FromBody] List<CreateWindow> model)
        {

            var res = await windowservice.Create_Window(model);
            return Ok(res);
        }

        [HttpPost("Update")]
        [Authorize]
        public async Task<IActionResult> UpdateWindow([FromBody] List<UpdateWindow> model)
        {

            var res = await windowservice.Update_Window(model);
            return Ok(res);
        }

        [HttpPost("ActiveInactive")]
        [Authorize]
        public async Task<IActionResult> ActiveInActiveWindow(string WindowId)
        {
            Guid guid = Guid.Parse(WindowId);
            var res = await windowservice.ActiveInActiveWindow(guid);
            return Ok(res);
        }

        [HttpPost("Delete")]
        [Authorize]
        public async Task<IActionResult> DeleteContact(string WindowId)
        {
            Guid guid = Guid.Parse(WindowId);
            var res = await windowservice.DeleteWindow(guid);

            return Ok(res);
        }
    }
}
