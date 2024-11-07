using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
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
    public class SubTenantController : ControllerBase
    {
        private readonly ISubtenantService tenantService;

        public SubTenantController(ISubtenantService tenantService)
        {
            this.tenantService = tenantService;
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] CreateSubTenant model)
        {
            var res = await tenantService.SubTenantCreate(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateSubTenant model)
        {
            var res = await tenantService.SubTenantUpdate(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete([FromBody] string TenantId)
        {
            var res = await tenantService.SubTenantDelete(TenantId);
            return Ok(res);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        public async Task<IActionResult> ActiveInactive([FromBody] string TenantId)
        {
            var res = await tenantService.SubTenantActiveInactive(TenantId);
            return Ok(res);
        }


    }
}
