using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OptionController : ControllerBase
    {
        private readonly IOption _option;

        public OptionController(IOption option)
        {
            _option = option;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "OptionsAdd")]
        public async Task<IActionResult> Create([FromBody] CreateOption optionVm)
        {
            var result = await _option.Create_Option(optionVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize(Roles = "OptionsEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateOption optionVm)
        {
            var result = await _option.Update_Option(optionVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "OptionsDelete")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var result = await _option.Delete(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "OptionsEdit")]
        public async Task<IActionResult> ActiveInactive(Guid Id)
        {
            var result = await _option.ActiveInActive(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "OptionsView")]
        public async Task<IActionResult> GetAll(Pagination filter, bool? IsManageProductOption)
        {
            var result = await _option.GetAll_Option(filter, IsManageProductOption);
            return Ok(result);
        }

    }
}
