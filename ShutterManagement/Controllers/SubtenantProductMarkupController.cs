using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubtenantProductMarkupController : ControllerBase
    {
        private readonly ISubtenantProductMarkup markup;

        public SubtenantProductMarkupController(ISubtenantProductMarkup markup)
        {
            this.markup = markup;
        }

        [HttpPost]
        [Route("Create")]
       // [Authorize(Roles = "AddressAdd")]
        public async Task<IActionResult> Create([FromBody] CreateSubtenantProductmarkup model)
        {
            var result = await markup.Create_SubtenatProductMarkup(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        //[Authorize(Roles = "AddressEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateSubtenantProductMarkup model)
        {
            var result = await markup.Update_SubtenantProductMarkup(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        //[Authorize(Roles = "AddressDelete")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var result = await markup.Delete(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        //[Authorize(Roles = "AddressEdit")]
        public async Task<IActionResult> ActiveInactive(Guid Id)
        {
            var result = await markup.ActiveInActive(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        //[Authorize(Roles = "AddressView")]
        public async Task<IActionResult> GetAllProductMarkup(Pagination filter)
        {
            var result = await markup.GetAllSubtenantProductMarkup(filter);
            return Ok(result);
        }

    }
}
