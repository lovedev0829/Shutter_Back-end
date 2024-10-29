using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
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
        [Route("TenantCreate")]
        public async Task<IActionResult> TenantCreate([FromBody] Sp_Tenant model)
        {
            if (model == null)
            {
                return BadRequest("Tenant model is required.");
            }

            try
            {
                await tenantService.TenantCreate(model);
                return Ok("Tenant created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("TenantUpdate")]
        public async Task<IActionResult> TenantUpdate([FromBody] Sp_Tenant model)
        {
            if (model == null || model.TenantId == Guid.Empty)
            {
                return BadRequest("Valid Tenant model with TenantId is required.");
            }

            try
            {
                await tenantService.TenantUpdate(model);
                return Ok("Tenant updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("TenantDelete/{id}")]
        public async Task<IActionResult> TenantDelete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid Tenant ID specified.");
            }

            try
            {
                await tenantService.DeleteTenant(id);
                return Ok("Tenant deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetAllTenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            IResponseVm<List<Sp_Tenant>> res = new ResponseVm<List<Sp_Tenant>>();
            try
            {
                var response = await tenantService.GetAll();
                res.IsSuccess = true;
                res.Message = "Data fetched successfully";
                res.Data = response;
            }
            catch (Exception ex)
            {
                res.Message = $"Internal server error: {ex.Message}";
            }
            return Ok(res);

        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetTenantById(Guid id)
        {
            IResponseVm<Sp_Tenant> res = new ResponseVm<Sp_Tenant>();
            if (id == Guid.Empty)
            {
                res.Message = "Invalid Tenant ID specified.";
            }

            try
            {
                var tenant = await tenantService.GetById(id);
                if (tenant == null)
                {
                    res.Message = $"Tenant with ID {id} not found.";
                }
                else
                {
                    res.IsSuccess = true;
                    res.Message = "Data fetched successfully";
                    res.Data = tenant;
                }
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                res.Message = $"Internal server error: {ex.Message}";
            }
            return Ok(res);
        }
    }
}
