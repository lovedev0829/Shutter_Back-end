using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class ProductComponentService : BaseRepository<ShutterDbContext, TblProductComponent>, IProductComponentService
    {
        private readonly ITenantService tenantService;
        public ProductComponentService(ITenantService tenantService)
        {
            this.tenantService = tenantService;
        }
        public async Task<IResponseVm<string>> AddProductComponent(CreateProductComponentVm model)
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
                    bool allProductAddedSuccessfully = true;

                    foreach (var productcomp in model.ProductComponents)
                    {
                        try
                        {
                            TblProductComponent newproductcomponent = new TblProductComponent
                            {
                                ProductId = model.ProductId,
                                ComponentId = productcomp.ComponentId,
                                Quantity = productcomp.Quantity,
                                CreatedAt = DateTime.Now,
                                CreatedBy = AppConstant.LoggedInUserID,
                                IsActive = true,
                                IsDelete = false,
                                TenantId = AppConstant.LoggedInTenantId
                            };

                            dbContext.TblProductComponents.Add(newproductcomponent);
                        }
                        catch (Exception ex)
                        {
                            responseVm.Message = $"Failed to add material {productcomp.ComponentId}: {ex.Message}";
                        }
                    }

                    if (allProductAddedSuccessfully)
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
                    var productcomponent = GetAllQuerable()
                        .Where(x => x.ProductComponentId == Id &&
                                    x.TenantId == AppConstant.LoggedInTenantId &&
                                    x.IsDelete == false)
                        .FirstOrDefault();

                    if (productcomponent != null)
                    {
                        productcomponent.IsActive = false;
                        productcomponent.IsDelete = true;
                        productcomponent.UpdatedAt = DateTime.Now;
                        productcomponent.ModifiedBy = AppConstant.LoggedInUserID;

                        await Update(productcomponent);

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
        

        public async Task<IResponseVm<string>> UpdateProductComponent(UpdateProductComponentVm model)
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
                    var productComponent = GetAllQuerable()
                        .Where(x => x.ProductComponentId == model.ProductComponentId &&
                                    x.TenantId == AppConstant.LoggedInTenantId &&
                                    x.IsDelete == false)
                        .FirstOrDefault();

                    if (productComponent != null)
                    {
                        productComponent.Quantity = model.Quantity;
                        productComponent.UpdatedAt = DateTime.Now;
                        productComponent.ModifiedBy = AppConstant.LoggedInUserID;

                        await Update(productComponent);

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
