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
    public class VendorOrderController : ControllerBase
    {
        private readonly IVendorOrderService vendorOrderService;

        public VendorOrderController(IVendorOrderService vendorOrderService)
        {
            this.vendorOrderService = vendorOrderService;
        }

        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateVendorOrder createVendorOrder)
        {
            var result = await vendorOrderService.Create_VendorOrder(createVendorOrder);
            return Ok(result);
        }

        [HttpPost("Update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateVendorOrder updateVendorOrder)
        {
            var result = await vendorOrderService.Update_VendorOrder(updateVendorOrder);
            return Ok(result);
        }

        [HttpPost("Delete")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid VendorOrderId)
        {
            var result = await vendorOrderService.Delete_VendorOrder(VendorOrderId);
            return Ok(result);
        }
        [HttpPost("ActiveInActive")]
        [Authorize]
        public async Task<IActionResult> ActiveInactive(Guid VendorOrderId)
        {
            var result = await vendorOrderService.ActiveInActive(VendorOrderId);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var res = await vendorOrderService.GetAll_VendorOrder(filter);
            return Ok(res);
        }

        [HttpPost]
        [Route("update_VendorOrderItem")]
        [Authorize]
        public async Task<IActionResult> update_VendorOrderItem(UpdateVendorOrderItem mode)
        {
            var res = await vendorOrderService.Update_VendorOrderItem(mode);
            return Ok(res);
        }

        [HttpPost]
        [Route("Delete_VendorOrderItem")]
        [Authorize]
        public async Task<IActionResult> Delete_VendorOrderItem(Guid? VendorOrderItemId)
        {
            var res = await vendorOrderService.Delete_VendorOrderItem(VendorOrderItemId);
            return Ok(res);
        }


    }
}
