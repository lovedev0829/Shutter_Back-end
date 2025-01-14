using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportServices reportServices;

        public ReportController(IReportServices reportServices)
        {
            this.reportServices = reportServices;
        }

        [HttpPost]
        [Route("GetOrderConfirmed")]
        //[Authorize(Roles = "AddressView")]
        public async Task<IActionResult> GetOrderConfirmed()
        {
            var result = await reportServices.GetOrderConfirmed();
            return Ok(result);
        }
    }
}
