using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.Common;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportServices reportServices;
        private readonly IInvoiceService invoiceService;

        public ReportController(IReportServices reportServices ,IInvoiceService invoiceService)
        {
            this.reportServices = reportServices;
            this.invoiceService = invoiceService;
        }

        
        [HttpPost]
        [Route("GetOrderConfirmed")]
        [Authorize(Roles = "ReportsView")]
        public async Task<IActionResult> GetOrderConfirmed(Pagination filter)
        {
            var result = await reportServices.GetOrderConfirmed(filter);
            return Ok(result);
        }


        [HttpPost]
        [Route("GetTaxReport")]
        [Authorize(Roles = "ReportsView")]
        public async Task<IActionResult> GetTaxReport(Pagination filter)
        {
            var result = await reportServices.GetTaxReport(filter);
            result.Data = result?.Data.Where(x => x.jobDetails.JobIsActive == true).ToList();
            return Ok(result);
        }


        [HttpPost]
        [Route("GetInvoiceReport")]
        [Authorize(Roles = "ReportsView")]
        public async Task<IActionResult> GetInvoiceReport(Pagination filter,ReportFilterType Type)
        {
            var result = await invoiceService.InvoiceReport(filter,Type);
            return Ok(result);
        }
    }
}
