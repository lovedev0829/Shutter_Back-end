using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService vendorService;
        public VendorController(IVendorService vendorService)
        {
            this.vendorService = vendorService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateVendor createVendor)
        {
            var result = await vendorService.Create_Vendor(createVendor);
            return Ok(result);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateVendor updateVendor)
        {
            var result = await vendorService.Update_Vendor(updateVendor);
            return Ok(result);
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(Guid VendorId)
        {
            var result = await vendorService.Delete(VendorId);
            return Ok(result);
        }

        [HttpPost("ActiveInActive")]
        public async Task<IActionResult> ActiveInactive(Guid VendorId)
        {
            var result = await vendorService.ActiveInActive(VendorId);
            return Ok(result);
        }


        [HttpPost]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var res = await vendorService.GetAll_Vendor(filter);
            return Ok(res);
        }
    }
}
