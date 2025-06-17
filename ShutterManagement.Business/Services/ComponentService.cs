using Microsoft.EntityFrameworkCore;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace ShutterManagement.Business.Services
{
    public class ComponentService : BaseRepository<ShutterDBContext, TblComponent>, IComponentService
    {
        private readonly ShutterDBContext _dbContext;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;
        private readonly IComponentMaterial _componentMaterial;
        private readonly IProductComponentService productService;
        private readonly IMaterial material;
        private readonly IMstProductType mstProductType;
        private readonly IExpressionService expressionService;

        public ComponentService(ShutterDBContext dbContext, ClaimsProperty claimsProperty, ITenantService tenantService,
            IComponentMaterial componentMaterial, IProductComponentService productService, IMaterial material, 
            IMstProductType mstProductType,IExpressionService expressionService)
        {
            _dbContext = dbContext;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
            _componentMaterial = componentMaterial;
            this.productService = productService;
            this.material = material;
            this.mstProductType = mstProductType;
            this.expressionService = expressionService;
        }


        public async Task<IResponseVm<string>> Create_Component(CreateComponent model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {

                    bool IsComponentExists = await AnyAsync(x => x.IsDelete == false &&
                      (x.ComponentName == model.ComponentName.Trim())
                      && x.TenantId==AppConstant.LoggedInTenantId);
                      
                    if (IsComponentExists)
                    {
                        responseVm.Message = "Component Name " + AppConstant.AlreadyExists;
                        return responseVm;
                    }

                    TblComponent component = new TblComponent
                    {
                        ComponentId = Guid.NewGuid(),
                        ComponentName = model.ComponentName.Trim(),
                        ProductType = model.ProductType,
                        //PricingExpression = model.PricingExpression,
                        MarkupPercentage = model.MarkupPercentage,
                        Notes = model.Notes,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false,
                        TenantId = AppConstant.LoggedInTenantId
                    };

                    await Add(component);

                    foreach (var material in model.Materials)
                    {
                        var isMaterialExists = await _componentMaterial.AnyAsync(x => x.RawMaterialId == material.MaterialId && x.ComponentId == component.ComponentId
                        && x.IsDelete == false);
                        if (!isMaterialExists)
                        {
                            TblComponentMaterial componentmaterial = new TblComponentMaterial
                            {
                                ComponentId = component.ComponentId,
                                RawMaterialId = material.MaterialId,
                                Quantity = material.Qty,
                                WasteFactor = material.WasteFactor,
                                CreatedAt = DateTime.Now,
                                CreatedBy = AppConstant.LoggedInUserID,
                                IsActive = true,
                                IsDelete = false,
                                TenantId = AppConstant.LoggedInTenantId,
                                MaterialPricingExpression = material.MaterialPricingExpression ?? Guid.Empty
                            };

                            await _componentMaterial.Add(componentmaterial);
                        }
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = $"{AppConstant.InsertSuccessMsg}";
                }
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<dynamic>> Update_Component(UpdateComponent model)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {
                    var IsComponentExists = GetAllQuerable()
                        .Where(c => c.ComponentId == Guid.Parse(model.ComponentId)
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();
                    bool IsComponentNameExists = await AnyAsync(x => x.IsDelete == false &&
                    (x.ComponentName == model.ComponentName.Trim())
                    && x.TenantId == AppConstant.LoggedInTenantId && x.ComponentId.ToString()!=model.ComponentId);

                    if (IsComponentNameExists)
                    {
                        responseVm.Message = "Component Name " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (IsComponentExists != null)
                    {
                        IsComponentExists.ComponentName = model.ComponentName?.Trim();
                        IsComponentExists.ProductType = model.ProductType;
                        //IsComponentExists.PricingExpression = model.PricingExpression;
                        IsComponentExists.MarkupPercentage = model.MarkupPercentage;
                        IsComponentExists.Notes = model.Notes;
                        IsComponentExists.UpdatedAt = DateTime.Now;
                        IsComponentExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsComponentExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                var IsComponentExists = GetAllQuerable()
                      .Where(c => c.ComponentId == Id
                                  && c.TenantId == AppConstant.LoggedInTenantId
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                bool isComponenteUsed =
              dbContext.TblEstimateWindowProductComponents.Any(p => p.ComponentId == Id && p.IsDelete == false);

                if (isComponenteUsed)
                {
                    responseVm.Message = "This Component is used in another entity (Estimate,) and cannot be deactivate.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                var IsComponentInProduct = await productService.AnyAsync(x => x.ComponentId == Id && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
                if (IsComponentInProduct)
                {
                    responseVm.Message = "Component exists in Product";
                    return responseVm;
                }
                if (IsComponentExists != null)
                {
                    var IsMaterialInComponent = _componentMaterial.GetAllQuerable().Where
                      (x => x.ComponentId == Id && x.IsDelete == false
                      && x.TenantId == AppConstant.LoggedInTenantId).ToList();
                    if (IsMaterialInComponent != null && IsMaterialInComponent.Count > 0)
                    {
                        foreach (var item in IsMaterialInComponent)
                        {
                            item.IsActive = !item.IsActive;
                            item.UpdatedAt = DateTime.Now;
                            item.ModifiedBy = AppConstant.LoggedInUserID;
                            await _componentMaterial.Update(item);
                        }
                    }

                    IsComponentExists.IsActive = !IsComponentExists.IsActive;
                    IsComponentExists.UpdatedAt = DateTime.Now;
                    IsComponentExists.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsComponentExists);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    responseVm.Message = $"{AppConstant.NoDataFound}";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Delete(Guid Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsComponentExists = GetAllQuerable()
                          .Where(c => c.ComponentId == Id
                                      && c.TenantId == AppConstant.LoggedInTenantId
                                      && c.IsDelete == false)
                          .FirstOrDefault();

                    bool isComponenteUsed =
                  dbContext.TblEstimateWindowProductComponents.Any(p => p.ComponentId == Id && p.IsDelete == false);

                    if (isComponenteUsed)
                    {
                        responseVm.Message = "This Component is used in another entity (Estimate,) and cannot be deleted.";
                        responseVm.IsSuccess = false;
                        return responseVm;
                    }
                    var IsComponentInProduct = await productService.AnyAsync(x => x.ComponentId == Id && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
                    if (IsComponentInProduct)
                    {
                        responseVm.Message = "Component exists in Product";
                        return responseVm;
                    }
                    if (IsComponentExists != null)
                        {
                            var IsMaterialInComponent = _componentMaterial.GetAllQuerable().Where
                            (x => x.ComponentId == Id && x.IsDelete == false
                            && x.TenantId == AppConstant.LoggedInTenantId).ToList();

                            if (IsMaterialInComponent != null && IsMaterialInComponent.Count > 0)
                            {
                                foreach (var item in IsMaterialInComponent)
                                {
                                    item.IsActive = false;
                                    item.IsDelete = true;
                                    item.UpdatedAt = DateTime.Now;
                                    item.ModifiedBy = AppConstant.LoggedInUserID;
                                    await _componentMaterial.Update(item);
                                }
                            }

                            IsComponentExists.IsActive = false;
                            IsComponentExists.IsDelete = true;
                            IsComponentExists.UpdatedAt = DateTime.Now;
                            IsComponentExists.ModifiedBy = AppConstant.LoggedInUserID;

                            var res = await Update(IsComponentExists);

                            responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.DeleteSuccessMsg;
                        }
                        else
                        {
                            responseVm.Message = $"{AppConstant.NoDataFound}";
                        }
                   

                }
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<ComponentListVm>>> GetAll_Component(Pagination filter)
        {
            IResponseVm<List<ComponentListVm>> responseVm = new ResponseVm<List<ComponentListVm>>();

            try
            {
                var isTenantValid = await IsTenantValid();
                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.AccessDenied;
                    return responseVm;
                }
                var componentList = GetComponentList();

                var filteredRecords = ApplyFilters(componentList, filter);

               // var pagedRecords = Paginate(filteredRecords, filter);


                var queryable = filteredRecords.AsQueryable();

                queryable = ShutterManagement.Core.Common.Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = ShutterManagement.Core.Common.Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                var TakenRecords = ShutterManagement.Core.Common.Paginate.Pagination(queryable.ToList(), filter);

                responseVm.IsSuccess = true;
                if (TakenRecords.Any())
                {
                    responseVm.Data = TakenRecords;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = filteredRecords.Count();
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        private async Task<bool> IsTenantValid()
        {
            return await tenantService.AnyAsync(x =>
                x.TenantId == AppConstant.LoggedInTenantId &&
                x.ParentTenantId == null &&
                x.IsDelete == false &&
                x.IsActive == true);
        }

        private TblComponent GetComponent()
        {
            return GetAllQuerable()
                .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false)
                .FirstOrDefault();
        }

        private MstProductType GetProductTypeDetail(string ProductType)
        {
            return mstProductType.GetAllQuerable()
                .Where(x => x.ProductId.ToString().ToLower() == ProductType.ToLower() && x.IsDelete == false)
                .FirstOrDefault();
        }

        private TblExpression GetExpression(string PricingExpression)
        {
            return expressionService.GetAllQuerable()
                .Where(x => x.Id.ToString() == PricingExpression && x.IsDelete == false)
                .FirstOrDefault();
        }

        private List<ComponentListVm> GetComponentList()
        {
            var componentList = GetAllQuerable()
                .Where(c => c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).OrderByDescending(x=> x.CreatedAt)
                .Select(c => new ComponentListVm
                {
                    ComponentId = c.ComponentId,
                    ComponentName = c.ComponentName,
                    IsActive = c.IsActive,
                    MarkupPercentage = c.MarkupPercentage,
                    Notes = c.Notes,
                    ProductType = c.ProductType,
                    //PricingExpression = c.PricingExpression,
                    TenantId = c.TenantId,
                }).ToList();

            foreach (var item in componentList)
            {
                //var component = GetComponent();
                var productTypeDetail = GetProductTypeDetail(item.ProductType ?? "");
                var expression = GetExpression(item.PricingExpression ?? "");
                item.ExpressionName = expression?.ExpressionName;
                item.ExpressionFormulae = expression?.DisplayArray;
                //item.displayArray = expression?.DisplayArray;
                item.ProductType = productTypeDetail?.ProductId.ToString().ToLower();
                item.ProductTypeName = productTypeDetail?.ProductName;

                item.ComponentMaterials = GetComponentMaterials((Guid)item.ComponentId);
            }

            return componentList;
        }

        private List<ComponentMaterialVm> GetComponentMaterials(Guid componentId)
        {
            var materialCompoenent = (from cm in dbContext.TblComponentMaterials.OrderByDescending(x => x.CreatedAt)
                                      join m in dbContext.TblRawMaterials on cm.RawMaterialId equals m.RawMaterialId
                                      where cm.ComponentId == componentId && cm.IsDelete == false && cm.TenantId == AppConstant.LoggedInTenantId
                                      select new
                                      {
                                          cm.ComponentMaterialId,
                                          cm.RawMaterialId,
                                          cm.WasteFactor,
                                          cm.Quantity,
                                          m.MaterialName,
                                          cm.IsDelete,
                                          cm.IsActive,
                                          cm.MaterialPricingExpression,
                                      }).ToList();

            var result = new List<ComponentMaterialVm>();


            foreach (var item in materialCompoenent)
            {
                var exp = GetExpression(item.MaterialPricingExpression?.ToString());

                var matcomp = new ComponentMaterialVm
                {
                    ComponentMaterialId = item.ComponentMaterialId,
                    RawMaterialId = item.RawMaterialId ?? Guid.Empty,
                    WasteFactor = item.WasteFactor ?? 0.0m,
                    Quantity = item.Quantity ?? 0.0m,
                    MaterialName = item.MaterialName ?? "",
                    IsActive = item.IsActive,
                    IsDelete = item.IsDelete,
                    MaterialPricingExpression = item.MaterialPricingExpression ?? Guid.Empty,
                    displayArray = exp?.DisplayArray ?? "",
                    ExpressionFormulae = exp?.ExpressionFormulae ?? "",
                    ExpressionName = exp?.ExpressionName ?? "",
                };

                result.Add(matcomp);
            }

            return result;
        }


        private IEnumerable<ComponentListVm> ApplyFilters(IEnumerable<ComponentListVm> componentList, Pagination filter)
        {
            filter.SearchValue = filter.SearchValue?.Trim().ToLower();

            return componentList
                .Where(x =>
                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                    (string.IsNullOrEmpty(filter.ProductType) ? true : (filter.ProductType == x.ProductType)) &&
                    (string.IsNullOrEmpty(filter.SearchValue) ? true :
                        (x.ComponentId.ToString() == filter.SearchValue) ||
                        (x.ProductType == filter.SearchValue) ||
                        (x.ComponentName ?? "").ToLower().Contains(filter.SearchValue) ||
                        (x.ProductType ?? "").ToLower().Contains(filter.SearchValue) ||
                        (x.PricingExpression ?? "").ToLower().Contains(filter.SearchValue) ||
                        (x.Notes ?? "").ToLower().Contains(filter.SearchValue) ||
                        (x.MarkupPercentage.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                        (x.ProductTypeName ?? "").ToLower().Contains(filter.SearchValue))
                );
        }

        private List<ComponentListVm> Paginate(IEnumerable<ComponentListVm> filteredRecords, Pagination filter)
        {
            return filteredRecords
                .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                .Take(filter.RecordPerPage)
                .ToList();
        }

    }
}
