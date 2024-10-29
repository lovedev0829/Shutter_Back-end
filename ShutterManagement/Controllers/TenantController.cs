using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
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
            IResponseVm<List<Sp_Tenant>> res = new ResponseVm<List<Sp_Tenant>>();
            if (model == null)
            {
                res.Message = "Tenant"+$"{AppConstant.ModelError}";
            }

            try
            {
                await tenantService.TenantCreate(model);
                res.IsSuccess = true;
                res.Message = $"{AppConstant.InsertSuccessMsg}";   
            }
            catch (Exception ex)
            {
                res.Message = $"{AppConstant.ExternalError} {ex.Message}";
            }
            return Ok(res);

        }

        [HttpPut]
        [Route("TenantUpdate")]
        public async Task<IActionResult> TenantUpdate([FromBody] Sp_Tenant model)
        {
            IResponseVm<List<Sp_Tenant>> res = new ResponseVm<List<Sp_Tenant>>();
            if (model == null || model.TenantId == Guid.Empty)
            {
                res.Message = "Tenant" + $"{AppConstant.ModelError}";
            }

            try
            {
 await tenantService.TenantUpdate(model);
                res.IsSuccess= true;    
                res.Message= $"{AppConstant.UpdateSuccessMsg}"; 
            }
            catch (Exception ex)
            {
                res.Message = $"{AppConstant.ExternalError} {ex.Message}";
            }
            return Ok(res);
        }

        [HttpDelete]
        [Route("TenantDelete/{id}")]
        public async Task<IActionResult> TenantDelete(Guid id)
        {
            IResponseVm<List<Sp_Tenant>> res = new ResponseVm<List<Sp_Tenant>>();
            if (id == Guid.Empty)
            {
                res.Message = $"{AppConstant.InvalidGuid}"; 
            }

            try
            {
                await tenantService.DeleteTenant(id);
                res.IsSuccess = true;
                res.Message = $"{AppConstant.DeletedSuccessMsg}";
            }
            catch (Exception ex)
            {
                res.Message = $"{AppConstant.ExternalError} {ex.Message}";
            }
            return Ok(res );
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
                res.Message= $"{AppConstant.FetchSuccessMsg}";  
                res.Data = response;
            }
            catch (Exception ex)
            {
                res.Message = $"{AppConstant.ExternalError} {ex.Message}";

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
                res.Message = $"{AppConstant.InvalidGuid}"; 
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
                    res.Message = $"{AppConstant.FetchSuccessMsg}" ;    
                    res.Data = tenant;
                }
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                res.Message = $"{AppConstant.ExternalError} {ex.Message}";
            }
            return Ok(res);
        }
    }
}
