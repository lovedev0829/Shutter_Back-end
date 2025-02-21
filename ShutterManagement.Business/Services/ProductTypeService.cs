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

        public ProductTypeService(ShutterDBContext context, ITenantService tenantService)
        {
            _context = context;
            this.tenantService = tenantService;
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
                    .FirstOrDefault(x => x.ProductName == model.ProductName && x.IsDelete==false && x.TenantId==AppConstant.LoggedInTenantId);

                if (existingProductType != null)
                {
                    responseVm.Message = $"ProductTypeName {AppConstant.AlreadyExists}";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                bool defaultExists = GetAllQuerable().Any(x => (x.IsDefault ?? false) && x.IsDelete==false && x.TenantId == AppConstant.LoggedInTenantId);

                if ((model.IsDefault ?? false) && defaultExists)
                {
                    responseVm.Message = "Only one default product type is allowed.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
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
                else
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

               
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteProductType(Guid? ProductId)
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
                    IsProductTypeExists.IsDelete = true;
                    IsProductTypeExists.IsActive = false;
                    IsProductTypeExists.UpdatedAt = DateTime.Now;
                    IsProductTypeExists.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsProductTypeExists);

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

                var IsProductTypeExists = GetAllQuerable()
                      .Where(c => c.ProductId == model.ProductId
                                  && c.IsDelete == false && c.TenantId == AppConstant.LoggedInTenantId)
                      .FirstOrDefault();

                bool defaultExists = GetAllQuerable().Any(x => (x.IsDefault ?? false) && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);

                if ((model.IsDefault ?? false) && defaultExists)
                {
                    responseVm.Message = "Only one default product type is allowed.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                if (IsProductTypeExists != null)
                {
                    IsProductTypeExists.ProductName = model.ProductName;
                    IsProductTypeExists.IsDefault = model.IsDefault;
                    IsProductTypeExists.UpdatedAt = DateTime.Now;
                    IsProductTypeExists.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsProductTypeExists);

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
    }
}
