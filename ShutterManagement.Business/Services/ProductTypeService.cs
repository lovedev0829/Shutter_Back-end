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
    public class ProductTypeService : BaseRepository<ShutterDBContext, MstProductType>, IMstProductType
    {
        private readonly ShutterDBContext _context;
        private readonly ITenantService tenantService;
        private readonly IMaterial material;

        public ProductTypeService(ShutterDBContext context, ITenantService tenantService ,IMaterial material )
        {
            _context = context;
            this.tenantService = tenantService;
            this.material = material;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid? ProductId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsProductTypeExists = GetAllQuerable()
                          .Where(c => c.ProductId == ProductId
                                      && c.IsDelete == false)
                          .FirstOrDefault();
                var ProductTypeExistInProduct = dbContext.TblRawMaterials.Where(x => x.ProductType == IsProductTypeExists.ProductId).FirstOrDefault();
                if (ProductTypeExistInProduct != null)
                {
                    responseVm.Message = "It can't be deleted. ProductType Exist in Product.";
                    return responseVm;
                }
                if (IsProductTypeExists != null)
                    {
                        IsProductTypeExists.IsActive = !IsProductTypeExists.IsActive;
                        IsProductTypeExists.UpdatedAt = DateTime.Now;
                        IsProductTypeExists.ModifiedBy = AppConstant.LoggedInUserID;

                        var res =  Update(IsProductTypeExists);

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = $"{AppConstant.NoDataFound}";
                        responseVm.IsSuccess=false;
                        return responseVm;
                    }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_ProductType(CreateProductType model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var existingProductType = GetAllQuerable()
                    .FirstOrDefault(x => x.ProductName == model.ProductName && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);

                if (existingProductType != null)
                {
                    responseVm.Message = $"ProductTypeName {AppConstant.AlreadyExists}";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                if (model.IsDefault == true)
                {
                    // Remove the previous default
                    var previousDefault = GetAllQuerable()
                        .FirstOrDefault(x => x.IsDefault == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);

                    if (previousDefault != null)
                    {
                        previousDefault.IsDefault = false;
                        previousDefault.UpdatedAt = DateTime.Now;
                        previousDefault.ModifiedBy = AppConstant.LoggedInUserID;
                        Update(previousDefault);
                    }
                }

                var productData = new MstProductType
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = model.ProductName,
                    IsDefault = model.IsDefault,
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    TenantId = AppConstant.LoggedInTenantId,
                    CreatedBy = AppConstant.LoggedInUserID,
                };

                var result = Add(productData);
                responseVm.Message = AppConstant.InsertSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }


        public async Task<IResponseVm<string>> DeleteProductType(Guid? ProductTypeId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                if (ProductTypeId == null)
                {
                    responseVm.Message = "Invalid Product Type ID.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                // Validate Tenant
                var IsTenant = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.ParentTenantId == null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                // Check if Product Type is used in another entity (Material)
                var isProductTypeUsedInMaterial = material.GetAllQuerable()
                    .FirstOrDefault(c => c.ProductType == ProductTypeId && c.IsDelete == false);

                if (isProductTypeUsedInMaterial != null)
                {
                    responseVm.Message = "This Product Type is used in another material and cannot be deleted.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }


                bool isProductTypeUsed =
                    dbContext.TblProducts.Any(p => p.ProductType == ProductTypeId.ToString() && p.IsDelete == false) ||
                    material.Any(m => m.ProductType == ProductTypeId && m.IsDelete == false);
                   

                if (isProductTypeUsed)
                {
                    responseVm.Message = "This Product Type is used in another entity (Product, Material) and cannot be deleted.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                // Find the Product Type
                var productType = GetAllQuerable()
                    .FirstOrDefault(c => c.ProductId == ProductTypeId && c.IsDelete == false);

                if (productType == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                // Soft Delete the Product Type
                productType.IsDelete = true;
                productType.IsActive = false;
                productType.UpdatedAt = DateTime.Now;
                productType.ModifiedBy = AppConstant.LoggedInUserID;

                await Update(productType);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<ProductTypeList>>> GetAll_ProductType(Pagination filter)
        {
            IResponseVm<List<ProductTypeList>> responseVm = new ResponseVm<List<ProductTypeList>>();
            try
            {
                var productTypeQuery = (await GetAll()).Where(p => p.IsDelete == false  && p.TenantId != null  && (p.TenantId == AppConstant.LoggedInTenantId || p.TenantId.ToString() == AppConstant.LoggedInParentTenantId)).OrderByDescending(x=>x.CreatedAt);

                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var filteredProductTypes = productTypeQuery
                    .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                        (string.IsNullOrEmpty(filter.SearchValue) 
                        ||x.ProductName.ToLower().Contains(filter.SearchValue) 
                        ||x.ProductId.ToString().ToLower().Contains(filter.SearchValue))
                    )
                    .Select(x => new ProductTypeList
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                         IsDefault= x.IsDefault,
                        IsActive = x.IsActive,
                    })
                    .ToList();

                var paginatedProductTypes = filteredProductTypes
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = paginatedProductTypes;
                responseVm.TotalRecords = filteredProductTypes.Count;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }


        public async Task<IResponseVm<string>> Update_ProductType(UpdateProductType model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var existingProductType = GetAllQuerable()
                    .FirstOrDefault(x => x.ProductName == model.ProductName && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.ProductId != model.ProductId);

                if (existingProductType != null)
                {
                    responseVm.Message = $"ProductTypeName {AppConstant.AlreadyExists}";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsProductTypeExists = GetAllQuerable()
                    .FirstOrDefault(c => c.ProductId == model.ProductId && c.IsDelete == false && c.TenantId == AppConstant.LoggedInTenantId);

                if (IsProductTypeExists == null)
                {
                    responseVm.Message = $"{AppConstant.NoDataFound}";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                if (model.IsDefault == true)
                {
                    // Remove the previous default
                    var previousDefault = GetAllQuerable()
                        .FirstOrDefault(x => x.IsDefault == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.ProductId != model.ProductId);

                    if (previousDefault != null)
                    {
                        previousDefault.IsDefault = false;
                        previousDefault.UpdatedAt = DateTime.Now;
                        previousDefault.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(previousDefault);
                    }
                }

                IsProductTypeExists.ProductName = model.ProductName;
                IsProductTypeExists.IsDefault = model.IsDefault;
                IsProductTypeExists.UpdatedAt = DateTime.Now;
                IsProductTypeExists.ModifiedBy = AppConstant.LoggedInUserID;

                var res = Update(IsProductTypeExists);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

    }
}
