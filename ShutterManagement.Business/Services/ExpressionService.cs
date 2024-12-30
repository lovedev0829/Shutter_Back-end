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
                    IsExpressionExist.IsActive = !IsExpressionExist.IsActive;
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
                if (IsExpressionNameExists)
                {
                    responseVm.Message = "Expression Name" + AppConstant.AlreadyExists;
                    return responseVm;
                }
                TblExpression expression = new TblExpression()
                {
                    ExpressionFormulae = model.ExpressionFormulae,  
                    ExpressionName = model.ExpressionName,
                    ProductId = model.ProductId,
                    ProductType = model.ProductTypeId,
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
                if (IsExpressionNameExist)
                {
                    responseVm.Message = "Expression Name" + AppConstant.AlreadyExists;
                    return responseVm;
                }
                var IsExpressionExist = GetAllQuerable().Where(e => e.Id == model.ExpressionId && e.TenantId == AppConstant.LoggedInTenantId && e.IsDelete == false).FirstOrDefault();
                if (IsExpressionNameExist != null)
                {
                    IsExpressionExist.ExpressionName = model.ExpressionName;    
                    IsExpressionExist.ExpressionFormulae = model.ExpressionFormulae;
                    IsExpressionExist.ProductType = model.ProductTypeId;
                    IsExpressionExist.ProductId = model.ProductId;
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
               var Expressionlist = (from t1 in  dbContext.TblExpressions join
                                     t2 in dbContext.MstProductTypes on t1.ProductType equals t2.ProductId join
                                     t3 in dbContext.TblProducts on t1.ProductId equals t3.ProductId
                                     where t1.TenantId == AppConstant.LoggedInTenantId && t1.IsDelete == false
                                     select new GetExpression {
                                         ExpressionId = t1.Id,
                                         ExpressionName = t1.ExpressionName,
                                         ExpressionFormulae = t1.ExpressionFormulae,
                                         IsActive = t1.IsActive,
                                         ProductId = t1.ProductId,
                                         ProductName = t3.ProductName,
                                         ProductTypeId = t1.ProductType,
                                         ProductTypeName = t2.ProductName,
                                         TenantId = t1.TenantId,

                                     }).ToList();

                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                var FilterRecords = Expressionlist
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (string.IsNullOrEmpty(filter.TenantId) ? true : (filter.TenantId == x.TenantId.ToString())) &&
                                    (filter.SearchValue != "" ?
                                    (x.ExpressionId.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.ProductId.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.ProductTypeId.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.ProductName ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.ProductTypeName ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.ExpressionFormulae ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.ExpressionName ?? "").ToLower().Contains(filter.SearchValue)
                                    : true)).ToList();



                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();



                responseVm.Data = TakenRecords.Select(x => new GetExpression
                {
                    ExpressionId = x.ExpressionId,
                    ExpressionName = x.ExpressionName,
                    ExpressionFormulae = x.ExpressionFormulae,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ProductTypeId = x.ProductTypeId,
                    ProductTypeName = x.ProductTypeName,
                    IsActive = x.IsActive,
                    TenantId = x.TenantId
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

