using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.Common;
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

      

        [HttpPost("Create")]
        [Authorize(Roles = "OrderAdd")]
        public async Task<IActionResult> Create(Guid EstimateId)
        {

            var res = await orderService.Create_Order_From_Estimate(EstimateId,true);
            return Ok(res);
        }

        [HttpPost("GetAll")]
        [Authorize(Roles = "OrderView")]
        public async Task<IActionResult> GetAllOrders(Pagination filter)
        {
            var res = await orderService.GetLatestVersionOrders(filter);
            return Ok(res);

        }


        [HttpPost("ViewHistory")]
        [Authorize(Roles = "OrderView")]
        public async Task<IActionResult> ViewHistory(Guid EstimationId)
        {
            var res = await orderService.ViewHistory(EstimationId);
            return Ok(res);

        }

        [HttpPost("Update")]
        [Authorize(Roles = "OrderEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateOrder model)
        {

            var res = await orderService.Update_Order(model);
            return Ok(res);
        }

        [HttpPost("ActiveInactive")]
        [Authorize(Roles = "OrderEdit")]
        public async Task<IActionResult> ActiveInActiveOrder(string OrderId)
        {
            Guid guid = Guid.Parse(OrderId);
            var res = await orderService.ActiveInActiveOrder(guid);
            return Ok(res);
        }

        [HttpPost("Delete")]
        [Authorize(Roles = "OrderDelete")]
        public async Task<IActionResult> DeleteOrder(string OrderId)
        {
            Guid guid = Guid.Parse(OrderId);
            var res = await orderService.DeleteOrder(guid);

            return Ok(res);
        }

        [HttpPost("SendForApproval")]
        [Authorize(Roles = "OrderEdit")]
        public async Task<IActionResult> SendForApproval(Guid OrderId)
        {
            var res = await orderService.SendForApproval(OrderId);
            return Ok(res);
        }

        [HttpPost("ViewOrder")]
        //[Authorize(Roles = "EstimateView")]
        [AllowAnonymous]
        public async Task<IActionResult> ViewOrder(Guid OrderId)
        {
            var res = await orderService.ViewOrder(OrderId);
            return Ok(res);

        }

        [HttpPost("OrderApproved")]
        [AllowAnonymous]
        public async Task<IActionResult> OrderApproved(Guid OrderId, Estimation Status)
        {
            var res = await orderService.ChangeStatus(OrderId, Status);
            return Ok(res);

        }
    }
}
