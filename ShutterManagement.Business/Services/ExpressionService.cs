using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NCalc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.Business.Services
{
    public class ExpressionService : BaseRepository<ShutterDBContext, TblExpression>, IExpressionService
    {
        private readonly ShutterDBContext dbContext;
        private readonly IComponentPriceService componentPriceService;
        private readonly IMaterial materialService;
        private readonly ITenantService tenantService;

        public ExpressionService(ShutterDBContext dbContext, IComponentPriceService componentPriceService,
            IMaterial materialService, ITenantService tenantService)
        {
            this.dbContext = dbContext;
            this.componentPriceService = componentPriceService;
            this.materialService = materialService;
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
                    ProductType = model.ProductTypeId,
                    FormulaeArray = string.Join(", ", model.FormulaeArray),
                    DisplayArray = string.Join(", ", model.displayArray),
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    TenantId = AppConstant.LoggedInTenantId,
                    IsOptionExpression = model.IsOptionExpression,
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
                    IsExpressionExist.IsOptionExpression = model.IsOptionExpression;
                    IsExpressionExist.UpdateAt = DateTime.Now;
                  //  IsExpressionExist.OptionId = string.IsNullOrEmpty(model.OptionId) ? null : Guid.Parse(model.OptionId);
                    IsExpressionExist.ModifiedBy = AppConstant.LoggedInUserID;
                    IsExpressionExist.FormulaeArray = string.Join(", ", model.FormulaeArray);
                    IsExpressionExist.DisplayArray = string.Join(", ", model.displayArray);
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
                bool IsProductExpressionExist = dbContext.TblProductComponents.Any(x => x.PricingExpression == ExpressionId.ToString() && x.IsActive ==
                true && x.IsDelete == false);
                var IsOptionExpressionExist = dbContext.TblOptions.Any(x => x.OptionExpression.ToString() == ExpressionId.ToString() && x.IsActive ==
               true && x.IsDelete == false);
                if (IsProductExpressionExist || IsOptionExpressionExist)
                {
                    responseVm.Message = "Expression can be Deleted. It's already is in use.";
                    return responseVm;
                }
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


                var Expressionlist = (from t1 in dbContext.TblExpressions.OrderByDescending(x => x.CreatedAt)
                                      join t2 in dbContext.MstProductTypes on t1.ProductType equals t2.ProductId
                                      where t1.TenantId == AppConstant.LoggedInTenantId && t1.IsDelete == false
                                      select new GetExpression
                                      {
                                          ExpressionId = t1.Id,
                                          ExpressionName = t1.ExpressionName ?? "",
                                          ExpressionFormulae = t1.ExpressionFormulae,
                                          formulaeArray = !string.IsNullOrEmpty(t1.FormulaeArray) ? t1.FormulaeArray.Split(new[] { ", " }, StringSplitOptions.TrimEntries) : new string[0],
                                          displayArray = !string.IsNullOrEmpty(t1.DisplayArray) ? t1.DisplayArray.Split(new[] { ", " }, StringSplitOptions.TrimEntries) : new string[0],
                                          ProductTypeId = t1.ProductType ?? Guid.Empty,
                                          ProductTypeName = t2.ProductName ?? "",
                                          IsOptionExpression = t1.IsOptionExpression ?? false,
                                          IsActive = t1.IsActive,
                                          TenantId = t1.TenantId,
                                          
                                      }).ToList();

                filter.SearchValue = filter.SearchValue?.Trim().ToLower();

                var FilterRecords = Expressionlist
                    .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                        (string.IsNullOrEmpty(filter.TenantId) ? true : (filter.TenantId == x.TenantId.ToString())) &&
                        (string.IsNullOrEmpty(filter.ProductType) ? true : (filter.ProductType == x.ProductTypeId.ToString())) &&
                        (filter.IsOptionExpression == null ? true : x.IsOptionExpression == filter.IsOptionExpression) &&
                        (filter.SearchValue != "" ?
                            (x.ExpressionId.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                            (x.ProductId.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                            (x.ProductTypeId.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                            (x.ProductName ?? "").ToLower().Contains(filter.SearchValue) ||
                            (x.ProductTypeName ?? "").ToLower().Contains(filter.SearchValue) ||
                            (x.ExpressionFormulae ?? "").ToLower().Contains(filter.SearchValue) ||
                            (x.ExpressionName ?? "").ToLower().Contains(filter.SearchValue)
                            : true)).ToList();

                var queryable = FilterRecords.AsQueryable();

                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                responseVm.Data = TakenRecords;
                responseVm.IsSuccess = true;
                responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                responseVm.TotalRecords = queryable.Count();
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
    }
}

