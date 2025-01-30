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
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly ITenantService tenantService;

        public ProductTypeService(BtprojecShuttermanagementDevContext context, ITenantService tenantService)
        {
            _context = context;
            this.tenantService = tenantService;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid? ProductId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                
                    var IsProductTypeExists = GetAllQuerable()
                          .Where(c => c.ProductId == ProductId
                                      && c.IsDelete == false)
                          .FirstOrDefault();
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
                    .FirstOrDefault(x => x.ProductName == model.ProductName && x.IsDelete==false);

                if (existingProductType != null)
                {
                    responseVm.Message = $"ProductTypeName {AppConstant.AlreadyExists}";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                var productData = new MstProductType
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = model.ProductName,
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInTenantId,
                };

                var result =  Add(productData);
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

        public async Task<IResponseVm<string>> DeleteProductType(Guid? ProductId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                var IsProductTypeExists = GetAllQuerable()
                      .Where(c => c.ProductId == ProductId
                                  && c.IsDelete == false)
                      .FirstOrDefault();
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

        public async Task<IResponseVm<List<ProductTypeList>>> GetAll_ProductType()
        {
            IResponseVm<List<ProductTypeList>> responseVm = new ResponseVm<List<ProductTypeList>>();
            try
            {
                var ProductTypeList = (await GetAll()).Where(x => x.IsDelete == false).ToList();

                    responseVm.Data = ProductTypeList.Select(x => new ProductTypeList
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        IsActive = x.IsActive,
                    }).ToList();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = ProductTypeList.Count;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
         }

        public async Task<IResponseVm<string>> Update_ProductType(UpdateProductType model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                var IsProductTypeExists = GetAllQuerable()
                      .Where(c => c.ProductId == model.ProductId
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                if (IsProductTypeExists != null)
                {
                    IsProductTypeExists.ProductName = model.ProductName;
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
