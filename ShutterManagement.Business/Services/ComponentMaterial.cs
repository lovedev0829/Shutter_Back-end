using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class ComponentMaterial : BaseRepository<ShutterDBContext, TblComponentMaterial>, IComponentMaterial
    {
        private readonly ShutterDBContext dbContext;
        private readonly ITenantService tenantService;

        public ComponentMaterial(ShutterDBContext dbContext, ITenantService tenantService)
        {
            this.dbContext = dbContext;
            this.tenantService = tenantService;
        }

        public async Task<IResponseVm<string>> AddComponentMaterial(CreateComponentMaterialVm model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            string MaterialExistsMessage = "";
            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                        x.TenantId == AppConstant.LoggedInTenantId &&
                        x.ParentTenantId == null &&
                        x.IsDelete == false &&
                        x.IsActive == true);
                if (isTenantValid)
                {
                    var IsComponentExist = dbContext.TblComponents
                            .Any(x => x.ComponentId == model.ComponentId && x.TenantId == AppConstant.LoggedInTenantId
                                           && x.IsDelete == false);

                    if (IsComponentExist)
                    {
                        foreach (var material in model.Materials)
                        {
                            var existingMaterial = await AnyAsync(x => x.RawMaterialId == material.MaterialId
                                               && x.ComponentId == model.ComponentId
                                               && x.IsActive == true && x.IsDelete == false);

                            if (!existingMaterial)
                            {
                                TblComponentMaterial newComponentMaterial = new TblComponentMaterial
                                {
                                    ComponentId = model.ComponentId,
                                    RawMaterialId = material.MaterialId,
                                    Quantity = material.Qty,
                                    WasteFactor = material.WasteFactor,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID,
                                    IsActive = true,
                                    IsDelete = false,
                                    TenantId = AppConstant.LoggedInTenantId,
                                    MaterialPricingExpression = model.MaterialPricingExpression
                                };
                                await Add(newComponentMaterial);

                            }
                            else
                            {
                                MaterialExistsMessage += "but some " + AppConstant.MaterialAlreadyExists;
                            }
                        }
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg + MaterialExistsMessage;

                    }
                    else
                    {
                        responseVm.Message = AppConstant.ComponentNotExists;
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

        public async Task<IResponseVm<string>> Delete(Guid Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.ParentTenantId == null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (isTenantValid)
                {
                    var componentMaterial = GetAllQuerable()
                        .Where(x => x.ComponentMaterialId == Id &&
                                    x.TenantId == AppConstant.LoggedInTenantId &&
                                    x.IsDelete == false)
                        .FirstOrDefault();



                    if (componentMaterial != null)
                    {
                        componentMaterial.IsActive = false;
                        componentMaterial.IsDelete = true;
                        componentMaterial.UpdatedAt = DateTime.Now;
                        componentMaterial.ModifiedBy = AppConstant.LoggedInUserID;

                        await Update(componentMaterial);

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.DeleteSuccessMsg;
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

        public async Task<IResponseVm<string>> UpdateComponentMaterial(UpdateComponentMaterialVm model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.ParentTenantId == null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (isTenantValid)
                { 
                    
                    var IsComponentMaterialExists = GetAllQuerable()
                        .Where(x => x.ComponentMaterialId == model.ComponentMaterialId && x.ComponentId == model.ComponentId
                         && x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true
                         && x.IsDelete == false).FirstOrDefault();

                    if (IsComponentMaterialExists != null)
                    {
                        var IsMaterialAlreadyExists = await AnyAsync(x => x.ComponentMaterialId != model.ComponentMaterialId &&
                        x.ComponentId == model.ComponentId &&
                        x.RawMaterialId == model.RawMaterialId && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
                        if (IsMaterialAlreadyExists)
                        {
                            responseVm.Message = AppConstant.MaterialAlreadyExists;
                            return responseVm;
                        }

                        IsComponentMaterialExists.RawMaterialId = model.RawMaterialId;
                        IsComponentMaterialExists.Quantity = model.Quantity;
                        IsComponentMaterialExists.MaterialPricingExpression = model.MaterialPricingExpression;
                        IsComponentMaterialExists.WasteFactor = model.WasteFactor;
                        IsComponentMaterialExists.UpdatedAt = DateTime.Now;
                        IsComponentMaterialExists.ModifiedBy = AppConstant.LoggedInUserID;

                        await Update(IsComponentMaterialExists);

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
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }



    }
}
