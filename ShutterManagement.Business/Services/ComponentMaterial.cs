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
    public class ComponentMaterial : BaseRepository<ShutterDbContext, TblComponentMaterial>, IComponentMaterial
    {
        private readonly ShutterDbContext dbContext;
        private readonly ITenantService tenantService;

        public ComponentMaterial(ShutterDbContext dbContext,ITenantService tenantService)
        {
            this.dbContext = dbContext;
            this.tenantService = tenantService;
        }

        public async Task<IResponseVm<string>> AddComponentMaterial(CreateComponentMaterialVm model)
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
                    bool allMaterialsAddedSuccessfully = true;
                    var IsComponentExist =  await dbContext.TblComponents
                            .AnyAsync(x => x.ComponentId == model.ComponentId && x.TenantId == AppConstant.LoggedInTenantId                                         
                                           && x.IsDelete == true);

                    if (IsComponentExist) {
                        foreach (var material in model.Materials)
                        {
                            var existingMaterial = await dbContext.TblComponentMaterials
                                .AnyAsync(x => x.RawMaterialId == material.MaterialId
                                               && x.ComponentId == model.ComponentId
                                               && x.IsActive == true);

                            if (!existingMaterial)
                            {
                                try
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
                                        TenantId = AppConstant.LoggedInTenantId
                                    };

                                }
                                catch (Exception ex)
                                {
                                    responseVm.Message = $"Failed to add material {material.MaterialId}: {ex.Message}";
                                }

                            }
                            else
                            {
                                responseVm.Message = "Material already added.";
                            }

                            if (allMaterialsAddedSuccessfully)
                            {
                                await dbContext.SaveChangesAsync();
                                responseVm.IsSuccess = true;
                                responseVm.Message = AppConstant.InsertSuccessMsg;
                            }
                            else
                            {
                                responseVm.Message = "Data not added.";
                            }
                        }
                     }
                    else
                    {
                        responseVm.Message = "Component Does not exist.";
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
                    var componentMaterial = GetAllQuerable()
                        .Where(x => x.ComponentMaterialId == model.ComponentMaterialId &&
                                    x.TenantId == AppConstant.LoggedInTenantId &&
                                    x.IsDelete == false)
                        .FirstOrDefault();

                    if (componentMaterial != null)
                    {
                        componentMaterial.Quantity = model.Quantity;
                        componentMaterial.WasteFactor = model.WasteFactor;
                        componentMaterial.UpdatedAt = DateTime.Now;
                        componentMaterial.ModifiedBy = AppConstant.LoggedInUserID;

                        await Update(componentMaterial);

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
