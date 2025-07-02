using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PowderCoatColorController : ControllerBase
    {
        private readonly IPowderCoatColorService powderCoatColorService;

        public PowderCoatColorController(IPowderCoatColorService powderCoatColorService)
        {
            this.powderCoatColorService = powderCoatColorService;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "PowderCoatColorAdd")]
        public async Task<IActionResult> Create_PowderCoatColor([FromBody] CreatePowderCoatColor model)
        {
            var res = await powderCoatColorService.Create_PowderCoatColor(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize(Roles = "PowderCoatColorEdit")]
        public async Task<IActionResult> Update_PowderCoatColor(UpdatePowderCoatColor model)
        {
            var res = await powderCoatColorService.Update_PowderCoatColor(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "PowderCoatColorEdit")]
        public async Task<IActionResult> ActiveInActive(Guid powderCoatColorId)
        {
            var res = await powderCoatColorService.ActiveInActive(powderCoatColorId);
            return Ok(res);
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "PowderCoatColorDelete")]
        public async Task<IActionResult> DeletePowderCoatColor(Guid? powderCoatColorId)
        {
            var res = await powderCoatColorService.DeletePowderCoatColor(powderCoatColorId);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "PowderCoatColorView")]
        public async Task<IActionResult> GetAllPowderCoatColor(Pagination filter)
        {
            var res = await powderCoatColorService.GetAllPowderCoatColor(filter);
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllForDropdown")]
        [Authorize(Roles = "JobView")]
        public async Task<IActionResult> GetPowderCoatColorsForDropdown()
        {
            var res = await powderCoatColorService.GetPowderCoatColorsForDropdown();
            return Ok(res);
        }
    }
}