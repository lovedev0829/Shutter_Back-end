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
    public class ProductService : BaseRepository<ShutterDbContext, TblProduct>, IProductService
    {
        private readonly ITenantService tenantService;
        private readonly IProductComponentService productComponentService;
        private readonly ClaimsProperty claimProperty;
        public ProductService(ITenantService tenantService,IProductComponentService productComponentService,ClaimsProperty claimsProperty)
        {
             this.tenantService = tenantService;
             this.claimProperty = claimsProperty;
             this.productComponentService = productComponentService;
        }
        public async Task<IResponseVm<string>> ActiveInActiveProduct(Guid ProductId)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {
                    var IsProductExists = GetAllQuerable()
                        .Where(c => c.ProductId == ProductId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();

                    if (IsProductExists != null)
                    {
                       IsProductExists.IsActive = !IsProductExists.IsActive;
                       IsProductExists.UpdatedAt = DateTime.Now;
                       IsProductExists.ModifiedBy = AppConstant.LoggedInUserID;
                       await Update(IsProductExists);
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
        

        public async Task<IResponseVm<string>> Create_Product(CreateProduct model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
                    TblProduct product = new TblProduct
                    {
                        ProductId = new Guid(),
                        ProductName = model.ProductName,
                        ProductType = model.ProductType,
                        Markup = model.Markup,
                        Notes = model.Notes,
                        Area = 0,
                        Width = 0,
                        AdditionalWidth= 0,
                        Height = 0,
                        ArchHeight = 0,
                        Length = 0,
                        Color=null,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false,
                        TenantId = AppConstant.LoggedInTenantId
                    };

                    await Add(product);

                    foreach (var item in model.ProductComponents)
                    {
                        TblProductComponent productcomponent = new TblProductComponent
                        {
                            ProductId=product.ProductId,
                            ComponentId = item.ComponentId,
                            Quantity = item.Quantity,
                            CreatedAt = DateTime.Now,
                            CreatedBy = AppConstant.LoggedInUserID,
                            IsActive = true,
                            IsDelete = false,
                            TenantId = AppConstant.LoggedInTenantId
                        };

                        await productComponentService.Add(productcomponent);
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

        public async Task<IResponseVm<string>> DeleteProduct(Guid ProductId)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {
                    var IsProductExists = GetAllQuerable()
                        .Where(c => c.ProductId == ProductId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();

                    if (IsProductExists != null)
                    {
                        IsProductExists.IsActive = false;
                        IsProductExists.IsDelete = true;
                        IsProductExists.UpdatedAt = DateTime.Now;
                        IsProductExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsProductExists);
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
        

        public async Task<IResponseVm<List<ProductListVm>>> GetAll_Product(Pagination filter)
        {
            IResponseVm<List<ProductListVm>> responseVm = new ResponseVm<List<ProductListVm>>();
            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.ParentTenantId == null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (isTenantValid)
                {
                    var product = (await GetAll())
                        .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                        .ToList();

                    var productComponents = productComponentService.GetAllQuerable()
                        .Where(cm => cm.IsDelete == false && cm.TenantId == AppConstant.LoggedInTenantId)
                        .ToList();

                    var filteredComponents = product
                        .Where(x =>
                            string.IsNullOrEmpty(filter.SearchValue) ||
                            (x.ProductName ?? "").ToLower().Contains(filter.SearchValue.Trim().ToLower()) ||
                            (x.ProductType ?? "").ToLower().Contains(filter.SearchValue.Trim().ToLower()) ||
                            (x.Markup.ToString() ?? "").ToLower().Contains(filter.SearchValue.Trim().ToLower()) ||
                            productComponents
                                .Any(cm => cm.ProductId == x.ProductId &&
                                           (cm.ComponentId.ToString().Contains(filter.SearchValue.Trim()) ||
                                            cm.Quantity.ToString().Contains(filter.SearchValue.Trim()))) 
                                            
                        )
                        .ToList();

                    var paginatedRecords = filteredComponents
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.Data = paginatedRecords.Select(x => new ProductListVm
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        IsActive = x.IsActive,
                        TenantId = x.TenantId,
                        Notes = x.Notes,
                        Markup = x.Markup,
                        ProductComponents = productComponents
                            .Where(cm => cm.ProductId == x.ProductId)
                            .Select(cm => new ProductComponents
                            {
                                ProductComponentId = cm.ProductComponentId,
                                ComponentId = cm.ComponentId,
                                Quantity = cm.Quantity
                                //CreatedAt = cm.CreatedAt
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
        

        public async Task<IResponseVm<string>> Update_Product(UpdateProduct model)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {
                    var IsProductExists = GetAllQuerable()
                        .Where(c => c.ProductId == model.ProductId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();

                    if (IsProductExists != null)
                    {
                        IsProductExists.ProductName = model.ProductName;
                        IsProductExists.ProductType = model.ProductType;
                        IsProductExists.Notes = model.Notes;
                        IsProductExists.Markup = model.Markup;
                        IsProductExists.UpdatedAt = DateTime.Now;
                        IsProductExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsProductExists);
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
    }
    }

