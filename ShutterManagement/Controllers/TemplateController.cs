using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly ITemplatesService templatesService;

        public TemplateController(ITemplatesService templatesService)
        {
            this.templatesService = templatesService;
        }

        [HttpPost]
        [Route("Create")]
      //  [Authorize(Roles = "AddressAdd")]
        public async Task<IActionResult> Create([FromBody] CreateTemplate addressVm)
        {
            var result = await templatesService.Create_Temaplate(addressVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        //[Authorize(Roles = "AddressEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateTemplate address)
        {
            var result = await templatesService.Update_Template(address);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        //[Authorize(Roles = "AddressDelete")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var result = await templatesService.Delete(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
       // [Authorize(Roles = "AddressEdit")]
        public async Task<IActionResult> ActiveInactive(Guid Id)
        {
            var result = await templatesService.ActiveInActive(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        //[Authorize(Roles = "AddressView")]
        public async Task<IActionResult> GetAllTemplate(Pagination filter)
        {
            var result = await templatesService.GetAll_Template(filter);
            return Ok(result);
        }

    }
    }
