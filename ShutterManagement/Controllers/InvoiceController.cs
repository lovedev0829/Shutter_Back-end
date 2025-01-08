using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            this.invoiceService = invoiceService;
        }

        [HttpPost("GetAll")]
        [Authorize(Roles = "InvoiceView")]
        public async Task<IActionResult> GetAllInvoice(Pagination filter)
        {
            var res = await invoiceService.GetAllInvoice(filter);
            return Ok(res);

        }

        [HttpPost("Create")]
        [Authorize(Roles = "InvoiceAdd")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoice model)
        {

            var res = await invoiceService.CreateInvoice(model);
            return Ok(res);
        }

        [HttpPost("Update")]
        [Authorize(Roles = "InvoiceEdit")]
        public async Task<IActionResult> UpdateInvoice([FromBody] UpdateInvoice model)
        {

            var res = await invoiceService.UpdateInvoice(model);
            return Ok(res);
        }

        [HttpPost("ActiveInactive")]
        [Authorize(Roles = "InvoiceEdit")]
        public async Task<IActionResult> ActiveInActiveInvoice(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await invoiceService.ActiveInActive(guid);
            return Ok(res);
        }

        [HttpPost("Delete")]
        [Authorize(Roles = "InvoiceDelete")]
        public async Task<IActionResult> DeleteContact(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await invoiceService.DeleteInvoice(guid);

            return Ok(res);
        }

        [HttpPost("SendInvoice")]
        public async Task<IActionResult> SendInvoice(GenerateInvoiceModel model)
        {
            var res = await invoiceService.SendInvoiceAttachment(model);

            return Ok(res);
        }

    }
}
