using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.Business.Services
{
    public class ExpressionService : BaseRepository<BtprojecShuttermanagementDevContext, TblExpression>, IExpressionService
    {
        private readonly BtprojecShuttermanagementDevContext dbContext;
        private readonly ITenantService tenantService;

        public ExpressionService(BtprojecShuttermanagementDevContext dbContext,ITenantService tenantService)
        {
            this.dbContext = dbContext;
            this.tenantService = tenantService;
        }
        public async Task<IResponseVm<string>> ActiveInActive(Guid ExpressionId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (!IsTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsExpressionExist = GetAllQuerable().Where(e => e.Id == ExpressionId && e.TenantId == AppConstant.LoggedInTenantId && e.IsDelete == false).FirstOrDefault();
                if (IsExpressionExist != null)
                {
                    IsExpressionExist.IsActive = !IsExpressionExist.IsDelete;
                    IsExpressionExist.UpdateAt = DateTime.Now;
                    IsExpressionExist.ModifiedBy = AppConstant.LoggedInUserID;
                }
                var res = await Update(IsExpressionExist);
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Expression(CreateExpression model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (!IsTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                bool IsExpressionNameExists = await AnyAsync(e => e.ExpressionName == model.ExpressionName && e.IsDelete == false && e.TenantId == AppConstant.LoggedInTenantId);
                if (!IsExpressionNameExists)
                {
                    responseVm.Message = "Expression Name" + AppConstant.AlreadyExists;
                    return responseVm;
                }
                TblExpression expression = new TblExpression()
                {
                    ExpressionFormulae = model.ExpressionFormulae,  
                    ExpressionName = model.ExpressionName,
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    TenantId = AppConstant.LoggedInTenantId
                };
                var res = await Add(expression);
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_Expression(UpdateExpression model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (!IsTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsExpressionNameExist = await AnyAsync(e => e.ExpressionName == model.ExpressionName && e.Id != model.ExpressionId
                && e.TenantId == AppConstant.LoggedInTenantId && e.IsDelete == false);
                if (!IsExpressionNameExist)
                {
                    responseVm.Message = "Expression Name" + AppConstant.AlreadyExists;
                    return responseVm;
                }
                var IsExpressionExist = GetAllQuerable().Where(e => e.Id == model.ExpressionId && e.TenantId == AppConstant.LoggedInTenantId && e.IsDelete == false).FirstOrDefault();
                if (IsExpressionNameExist)
                {
                    IsExpressionExist.ExpressionName = model.ExpressionName;    
                    IsExpressionExist.ExpressionFormulae = model.ExpressionFormulae;    
                    IsExpressionExist.UpdateAt = DateTime.Now;
                    IsExpressionExist.ModifiedBy = AppConstant.LoggedInUserID;
                }
                var res = await Update(IsExpressionExist);
                responseVm.IsSuccess = true;    
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
                return responseVm;
            }
        public async Task<IResponseVm<string>> Delete(Guid ExpressionId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (!IsTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsExpressionExist = GetAllQuerable().Where(e => e.Id == ExpressionId && e.TenantId == AppConstant.LoggedInTenantId && e.IsDelete == false).FirstOrDefault();
                if (IsExpressionExist != null)
                {
                    IsExpressionExist.IsActive = false;
                    IsExpressionExist.IsDelete = true;
                    IsExpressionExist.UpdateAt = DateTime.Now;
                    IsExpressionExist.ModifiedBy = AppConstant.LoggedInUserID;
                }
                var res = await Update(IsExpressionExist);
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<GetExpression>>> Get_AllExpression(Pagination filter)
        {
            IResponseVm<List<GetExpression>> responseVm = new ResponseVm<List<GetExpression>>();
            try
            {
             bool IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
               if (!IsTenantValid)
               {
                  responseVm.Message = AppConstant.TenantNotFound;
                  return responseVm;
               }
                var Expressionlist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).ToList();
                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                var FilterRecords = Expressionlist
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (string.IsNullOrEmpty(filter.TenantId) ? true : (filter.TenantId == x.TenantId.ToString())) &&
                                    (filter.SearchValue != "" ?
                                    (x.Id.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.ExpressionFormulae ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.ExpressionName ?? "").ToLower().Contains(filter.SearchValue)
                                    : true)).ToList();



                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();



                responseVm.Data = TakenRecords.Select(x => new GetExpression
                {
                    ExpressionId = x.Id,
                    ExpressionName = x.ExpressionName,
                    ExpressionFormulae = x.ExpressionFormulae,
                    IsActive = x.IsActive
                }).ToList();
                responseVm.IsSuccess = true;
                responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                responseVm.TotalRecords = TakenRecords.Count;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
      }   
    }

