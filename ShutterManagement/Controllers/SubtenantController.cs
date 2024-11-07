//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using ShutterManagement.Business.Interfaces;
//using ShutterManagement.Core;
//using ShutterManagement.Core.SPModel;
//using ShutterManagement.Core.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace ShutterManagement.Api.Controllers
//{

//    [Authorize]
//    [Route("api/[controller]")]
//    [ApiController]
//    public class SubtenantController : ControllerBase
//    {
//        private readonly ISubtenantService _subtenantService;
//        private readonly ITenantService _tenantService;

//        public SubtenantController(ISubtenantService subtenantService,ITenantService tenantService)
//        {
//            _subtenantService = subtenantService;
//            _tenantService = tenantService;
//        }

//        [HttpPost]
//        [Route("SubTenantCreate")]
//        public async Task<IActionResult> CreateSubtenant([FromBody] Sp_Tenant model)
//        {
//            IResponseVm<Sp_Tenant> res = new ResponseVm<Sp_Tenant>();
//            if (model == null || string.IsNullOrEmpty(model.TenantName))
//            {
//                res.Message = "Tenant" + $"{AppConstant.ModelError}";
//                return BadRequest(res);
//            }

//            await _subtenantService.CreateSubtenant(model);
//            res.IsSuccess = true;
//            res.Message = $"{AppConstant.InsertSuccessMsg}";
//            return Ok(res);
//        }

//        [HttpPut]
//        [Route("SubTenantUpdate/{id}")]
//        public async Task<IActionResult> UpdateSubtenant(Guid id, [FromBody] Sp_Tenant model)
//        {
//            IResponseVm<Sp_Tenant> res = new ResponseVm<Sp_Tenant>();

//            if (model == null || id == Guid.Empty)
//            {
//                res.Message = "Invalid tenant information.";
//            }

//            if (model.ParentTenantId.HasValue && model.ParentTenantId != Guid.Empty)
//            {
//                var parentTenant = await _tenantService.GetById(model.ParentTenantId.Value);
//                if (parentTenant == null)
//                {
//                    res.Message = "Invalid Parent Tenant ID.";
//                }
//            }
        
//                model.TenantId = id;
//                await _subtenantService.UpdateSubtenant(model); 

//                res.IsSuccess = true;
//                res.Message = $"{AppConstant.UpdateSuccessMsg}";
//                return Ok(res); 
            
//        }


//        [HttpGet]
//        [Route("GetAllSubTenants")]
//        public async Task<IActionResult> GetAllSubtenants()
//        {
//            IResponseVm<List<Sp_Tenant>> res = new ResponseVm<List<Sp_Tenant>>();
//            var subtenants = await _subtenantService.GetAllSubtenants();
//            res.IsSuccess = true;
//            res.Message = $"{AppConstant.FetchSuccessMsg}";
//            res.Data = subtenants;
//            return Ok(res);
//        }

//        [HttpGet]
//        [Route("Get_SubtenantByID/{id}")]
//        public async Task<IActionResult> GetSubtenantById(Guid id)
//        {
//            IResponseVm<Sp_Tenant> res = new ResponseVm<Sp_Tenant>();

//            if (id == Guid.Empty)
//            {
//                res.Message = $"{AppConstant.InvalidGuid}";
//            }

//            var subtenant = await _subtenantService.GetSubtenantById(id);
//            res.IsSuccess = true;
//            res.Message = $"{AppConstant.FetchSuccessMsg}";
//            res.Data = subtenant;

//            if (subtenant == null)
//            {
//                res.Message = "Subtenant not found.";
//            }

//            return Ok(res);
//        }

//        [HttpDelete]
//        [Route("SubTenantDelete/{id}")]
//        public async Task<IActionResult> DeleteSubtenant(Guid id)
//        {
//            IResponseVm<List<Sp_Tenant>> res = new ResponseVm<List<Sp_Tenant>>();

//            if (id == Guid.Empty)
//            {
//                res.Message = $"{AppConstant.InvalidGuid}";
//            }

//            await _subtenantService.DeleteSubtenant(id);
//            res.IsSuccess = true;
//            res.Message = $"{AppConstant.DeletedSuccessMsg}";
//            return Ok(res);
//        }
//    }
//}
