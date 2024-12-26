using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentsService paymentsService;

        public PaymentController(IPaymentsService paymentsService)
        {
            this.paymentsService = paymentsService;
        }

        [HttpPost("GetAll")]
        //[Authorize(Roles = "PaymentView")]
        public async Task<IActionResult> GetAllPayment(Pagination filter)
        {
            var res = await paymentsService.GetAllPayment(filter);
            return Ok(res);

        }

        [HttpPost("Create")]
        //[Authorize(Roles = "PaymentAdd")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePayments model)
        {

            var res = await paymentsService.CreatePayment(model);
            return Ok(res);
        }

        [HttpPost("Update")]
        //[Authorize(Roles = "PaymentEdit")]
        public async Task<IActionResult> UpdatePayment([FromBody] UpdatePayments model)
        {

            var res = await paymentsService.UpdatePayment(model);
            return Ok(res);
        }

        [HttpPost("ActiveInactive")]
        //[Authorize(Roles = "PaymentEdit")]
        public async Task<IActionResult> ActiveInActive(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await paymentsService.ActiveInActive(guid);
            return Ok(res);
        }

        [HttpPost("Delete")]
        //[Authorize(Roles = "PaymentDelete")]
        public async Task<IActionResult> DeleteContact(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await paymentsService.DeletePayment(guid);

            return Ok(res);
        }
    }

}
