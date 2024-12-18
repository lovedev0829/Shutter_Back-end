using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService discountService;

        public DiscountController(IDiscountService discountService)
        {
            this.discountService = discountService;
        }
        [HttpPost("Create")]
        public async Task<IActionResult> CreateCDiscoount([FromBody]  CreateDiscount model)
        {
            var res = await discountService.CreateDiscountCoupon(model);
            return Ok(res);
        }
        [HttpPost("Update")]
        public async Task<IActionResult> UpdateDiscount(UpdateDiscount model)
        {
            var res = await discountService.UpdateDiscountCoupon(model);
            return Ok(res);
        }
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteDiscount(Guid Discountid)
        {
            var res = await discountService.DeleteDiscountCoupon(Discountid);
            return Ok(res);
        }

        [HttpPost("ActiveInActive")]
        public async Task<IActionResult> ActiveInActive(Guid Discountid)
        {
            var res = await discountService.ActiveInActive(Discountid);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var res = await discountService.GetAll_Coupons(filter);
            return Ok(res);
        }

    }
}
