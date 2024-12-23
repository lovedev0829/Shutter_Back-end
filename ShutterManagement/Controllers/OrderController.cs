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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        //[HttpPost("GetAll")]
        //[Authorize(Roles = "OrderView")]
        //public async Task<IActionResult> GetAllEstimates(Pagination filter)
        //{
        //    var res = await orderService.GetAll_Estimate(filter);
        //    return Ok(res);

        //}

        [HttpPost("Create")]
       // [Authorize(Roles = "OrderAdd")]
        public async Task<IActionResult> Create(Guid EstimateId)
        {

            var res = await orderService.Create_Order(EstimateId);
            return Ok(res);
        }

        //[HttpPost("Update")]
        //[Authorize(Roles = "OrderEdit")]
        //public async Task<IActionResult> Update([FromBody] UpdateOrder model)
        //{

        //    var res = await orderService.Update_Order(model);
        //    return Ok(res);
        //}
    }
}
