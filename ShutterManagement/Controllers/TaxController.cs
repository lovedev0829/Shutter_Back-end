using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxController : ControllerBase
    {

        private readonly ITaxRate _taxrate;

        public TaxController(ITaxRate taxrate)
        {
            _taxrate = taxrate;
        }


        [HttpPost]
        [Route("Create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateTax model)
        {
            var result = await _taxrate.CreateTax(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateTax model)
        {
            var result = await _taxrate.UpdateTax(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var result = await _taxrate.DeleteTax(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize]
        public async Task<IActionResult> ActiveInactive(Guid Id)
        {
            var result = await _taxrate.ActiveInActiveTax(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize]
        public async Task<IActionResult> GetAllAddresses(Pagination filter)
        {
            var result = await _taxrate.GetAll_Tax(filter);
            return Ok(result);
        }
    }
}
