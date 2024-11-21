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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace ShutterManagement.Business.Services
{
    public class ComponentService : BaseRepository<ShutterDbContext, TblComponent>, IComponentService
    {
        private readonly ShutterDbContext _dbContext;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;
        private readonly IComponentMaterial _componentMaterial;

        public ComponentService(ShutterDbContext dbContext, ClaimsProperty claimsProperty,ITenantService tenantService,IComponentMaterial componentMaterial)
        {
            _dbContext = dbContext;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
            _componentMaterial = componentMaterial;
        }


        public Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant =  tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                var IsComponentExists = GetAllQuerable()
                      .Where(c => c.ComponentId == Id
                                  && c.TenantId == AppConstant.LoggedInTenantId
                                  && c.IsActive == true
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

            return Task.FromResult(responseVm);
        }

        public async Task<IResponseVm<string>> Create_Component(CreateComponent model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
                    TblComponent component = new TblComponent
                    {
                        ComponentId = new Guid(),
                        ComponentName = model.ComponentName,
                        ProductType = model.ProductType,
                        PricingExpression = model.PricingExpression,
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

        public async Task<IResponseVm<string>> Delete(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null &&x.IsActive== true && x.IsDelete == false );
                if (IsTenant)
                {
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
                     
                    if(IsMaterialInComponent != null && IsMaterialInComponent.Count > 0)
                    {
                        foreach (var item in IsMaterialInComponent)
                        {
                            item.IsActive = false;
                            item.IsDelete = true;
                            item.UpdatedAt = DateTime.Now;
                            item.ModifiedBy = AppConstant.LoggedInUserID;
                            }
                    }
                    
                        IsComponentExists.IsActive = false;
                        IsComponentExists.IsDelete = true;
                        IsComponentExists.UpdatedAt = DateTime.Now;
                        IsComponentExists.ModifiedBy = AppConstant.LoggedInUserID;

                        var res = Update(IsComponentExists);

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
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.ParentTenantId == null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (isTenantValid)
                {
                    var components = (await GetAll())
                        .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                        .ToList();

                    var componentMaterials = _componentMaterial.GetAllQuerable()
                        .Where(cm => cm.IsDelete == false && cm.TenantId == AppConstant.LoggedInTenantId)
                        .ToList();

                    var filteredComponents = components
                        .Where(x =>
                            string.IsNullOrEmpty(filter.SearchValue) ||
                            (x.ComponentName ?? "").ToLower().Contains(filter.SearchValue.Trim().ToLower()) ||
                            (x.ProductType ?? "").ToLower().Contains(filter.SearchValue.Trim().ToLower()) ||
                            (x.PricingExpression ?? "").ToLower().Contains(filter.SearchValue.Trim().ToLower()) ||
                            componentMaterials
                                .Any(cm => cm.ComponentId == x.ComponentId &&
                                           (cm.RawMaterialId.ToString().Contains(filter.SearchValue.Trim()) ||
                                            cm.Quantity.ToString().Contains(filter.SearchValue.Trim()) ||
                                            cm.WasteFactor.ToString().Contains(filter.SearchValue.Trim())))
                        )
                        .ToList();

                    var paginatedRecords = filteredComponents
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.Data = paginatedRecords.Select(x => new ComponentListVm
                    {
                        ComponentId = x.ComponentId,
                        ComponentName = x.ComponentName,
                        IsActive = x.IsActive,
                        TenantId = x.TenantId,  
                        MarkupPercentage = x.MarkupPercentage,
                        PricingExpression = x.PricingExpression,
                        Notes = x.Notes,
                        ComponentMaterials = componentMaterials
                            .Where(cm => cm.ComponentId == x.ComponentId)
                            .Select(cm => new ComponentMaterialVm
                            {
                                RawMaterialId = cm.RawMaterialId,
                                Quantity = cm.Quantity,
                                WasteFactor = cm.WasteFactor,
                                CreatedAt = cm.CreatedAt
                            }).ToList()
                    }).ToList();

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = filteredComponents.Count;  
                }
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return await Task.FromResult(responseVm);
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

                    if (IsComponentExists != null)
                    {
                        IsComponentExists.ComponentName = model.ComponentName;
                        IsComponentExists.ProductType = model.ProductType;
                        IsComponentExists.PricingExpression = model.PricingExpression;
                        IsComponentExists.MarkupPercentage = model.MarkupPercentage;
                        IsComponentExists.Notes = model.Notes;
                        IsComponentExists.UpdatedAt = DateTime.Now;
                        IsComponentExists.ModifiedBy = AppConstant.LoggedInUserID;

                        await Update(IsComponentExists);

                        if (model.Materials != null && model.Materials.Any())
                        {
                            foreach (var material in model.Materials)
                            {
                                var componentMaterial = _dbContext.TblComponentMaterials
                                    .Where(cm => cm.ComponentId == IsComponentExists.ComponentId && cm.RawMaterialId == material.MaterialId)
                                    .FirstOrDefault();

                                if (componentMaterial != null)
                                {
                                    componentMaterial.Quantity = material.Qty;
                                    componentMaterial.WasteFactor = material.WasteFactor;
                                    componentMaterial.UpdatedAt = DateTime.Now;
                                    componentMaterial.ModifiedBy = AppConstant.LoggedInUserID;

                                    await _dbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    TblComponentMaterial newComponentMaterial = new TblComponentMaterial
                                    {
                                        ComponentId = IsComponentExists.ComponentId,
                                        RawMaterialId = material.MaterialId,
                                        Quantity = material.Qty,
                                        WasteFactor = material.WasteFactor,
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = AppConstant.LoggedInUserID,
                                        IsActive = true,
                                        IsDelete = false,
                                        TenantId = AppConstant.LoggedInTenantId
                                    };

                                    _dbContext.TblComponentMaterials.Add(newComponentMaterial);
                                    await _dbContext.SaveChangesAsync();
                                }
                            }
                        }

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        responseVm.IsSuccess = false;
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
    }
}
