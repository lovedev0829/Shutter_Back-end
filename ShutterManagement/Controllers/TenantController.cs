using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using System;
using System.Threading.Tasks;

namespace ShutterManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService tenantService;

        public TenantController(ITenantService tenantService)
        {
            this.tenantService = tenantService;
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> TenantCreate([FromBody] Tenant model)
        {
            var res = await tenantService.TenantCreate(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> TenantUpdate([FromBody] UpdateTenant model)
        {
            var res = await tenantService.TenantUpdate(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> TenantDelete([FromBody] string TenantId)
        {
            var res = await tenantService.TenantDelete(TenantId);
            return Ok(res);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        public async Task<IActionResult> TenantActiveInactive([FromBody] string TenantId)
        {
            var res = await tenantService.TenantActiveInactive(TenantId);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var res = await tenantService.TenantGetAll(filter);
            return Ok(res);
        }


    }
}
