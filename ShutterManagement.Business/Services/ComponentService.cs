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
    public class ComponentService : BaseRepository<BtprojecShuttermanagementDevContext, TblComponent>, IComponentService
    {
        private readonly BtprojecShuttermanagementDevContext _dbContext;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;
        private readonly IComponentMaterial _componentMaterial;
        private readonly IProductComponentService productService;
        private readonly IMaterial material;
        private readonly IMstProductType mstProductType;

        public ComponentService(BtprojecShuttermanagementDevContext dbContext, ClaimsProperty claimsProperty, ITenantService tenantService,
            IComponentMaterial componentMaterial, IProductComponentService productService, IMaterial material, IMstProductType mstProductType)
        {
            _dbContext = dbContext;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
            _componentMaterial = componentMaterial;
            this.productService = productService;
            this.material = material;
            this.mstProductType = mstProductType;
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
                        PricingExpression = model.ExpressionId.ToString(),
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
                                TenantId = AppConstant.LoggedInTenantId
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
                        IsComponentExists.PricingExpression = model.ExpressionId.ToString();
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
                    var IsComponentInProduct = await productService.AnyAsync(x => x.ComponentId == Id && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
                    if (!IsComponentInProduct)
                    {
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
                        responseVm.Message = "Component exists in Product";
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
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.ParentTenantId == null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (isTenantValid)
                {
                    var componentList = (
      from c in dbContext.TblComponents.OrderByDescending(x => x.CreatedAt)
      join pt in dbContext.MstProductTypes on Guid.Parse(c.ProductType) equals pt.ProductId
      where c.IsDelete == false && c.TenantId == AppConstant.LoggedInTenantId
      select new ComponentListVm
      {
          ComponentId = c.ComponentId,
          ComponentName = c.ComponentName,
          IsActive = c.IsActive,
          MarkupPercentage = c.MarkupPercentage,
          Notes = c.Notes,
          PricingExpression = c.PricingExpression,
         
          ExpressionFormulae = dbContext.TblExpressions
              .Where(x => x.Id == Guid.Parse(c.PricingExpression) && x.IsDelete == false)
              .Select(x => x.ExpressionFormulae)
              .FirstOrDefault(),
          ExpressionName = dbContext.TblExpressions
              .Where(x => x.Id == Guid.Parse(c.PricingExpression) && x.IsDelete == false)
              .Select(x => x.ExpressionName)
              .FirstOrDefault(),
          ProductType = pt.ProductId.ToString().ToLower(),
          ProductTypeName = pt.ProductName,
          TenantId = c.TenantId,
      }).ToList();

                    foreach (var item in componentList)
                    {
                        var ComponentMaterial = (from cm in dbContext.TblComponentMaterials.OrderByDescending(x => x.CreatedAt)
                                                 join m in dbContext.TblRawMaterials on cm.RawMaterialId equals m.RawMaterialId
                                                 where cm.ComponentId == item.ComponentId && cm.IsDelete == false && cm.TenantId == AppConstant.LoggedInTenantId
                                                 select new ComponentMaterialVm
                                                 {
                                                     ComponentMaterialId = cm.ComponentMaterialId,
                                                     RawMaterialId = cm.RawMaterialId,
                                                     WasteFactor = cm.WasteFactor,
                                                     Quantity = cm.Quantity,
                                                     MaterialName = m.MaterialName,
                                                     IsDelete = cm.IsDelete,
                                                     IsActive = cm.IsActive
                                                 }).ToList();

                        item.ComponentMaterials = ComponentMaterial;
                    }



                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = componentList
                              .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) && 
                                    (string.IsNullOrEmpty(filter.ProductType) ? true : (filter.ProductType == x.ProductType.ToString())) &&
                                  (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                 (x.ComponentId.ToString() == filter.SearchValue) ||
                                 (x.ProductType.ToString() == filter.SearchValue) ||
                                 (x.ComponentName ?? "").ToLower().Contains(filter.SearchValue) ||
                                 (x.ProductType.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                 (x.PricingExpression ?? "").ToLower().Contains(filter.SearchValue) ||
                                 (x.Notes ?? "").ToLower().Contains(filter.SearchValue) ||
                                 (x.MarkupPercentage.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                 (x.ProductTypeName ?? "").ToLower().Contains(filter.SearchValue))
                             ).ToList();



                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();
                    responseVm.IsSuccess = true;

                    if (TakenRecords != null && TakenRecords.Count > 0)
                    {
                        responseVm.Data = TakenRecords.ToList();
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                        responseVm.TotalRecords = FilterRecords.Count;
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
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
    }
}
