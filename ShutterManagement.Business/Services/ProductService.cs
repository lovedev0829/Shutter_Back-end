using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using Stripe;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace ShutterManagement.Business.Services
{
    public class ProductService : BaseRepository<ShutterDBContext, TblProduct>, IProductService
    {
        private readonly ITenantService tenantService;
        private readonly IExpressionService expressionService;
        private readonly IProductComponentService productComponentService;
        private readonly ClaimsProperty claimProperty;
        private readonly IOption option;
        private readonly IOptionsTypeService optionsType;
        private readonly IComponentService componentService;
        public ProductService(ITenantService tenantService, IExpressionService expressionService, 
            IProductComponentService productComponentService, ClaimsProperty claimsProperty,IComponentService componentService,
            IOption option,IOptionsTypeService optionsType )
        {
            this.tenantService = tenantService;
            this.expressionService = expressionService;
            this.claimProperty = claimsProperty;
            this.option = option;
            this.optionsType = optionsType;
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
                        var IsProductInComponent = productComponentService
                    .GetAllQuerable().Where(x => x.ProductId == ProductId &&
                                         x.IsDelete == false &&
                                         x.TenantId == AppConstant.LoggedInTenantId)
                    .ToList();
                        foreach (var component in IsProductInComponent)
                        {
                            component.IsActive = false;
                            component.IsDelete = true;
                            component.UpdatedAt = DateTime.Now;
                            component.ModifiedBy = AppConstant.LoggedInUserID;
                            await productComponentService.Update(component);
                        }


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
                var IsProductName = await AnyAsync(x => x.ProductName == model.ProductName.Trim()
                    && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId
                    && x.ProductType == model.ProductType);
                if (IsProductName)
                {
                    responseVm.Message = "ProductName " + AppConstant.AlreadyExists;
                    return responseVm;
                }

                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
                    TblProduct product = new TblProduct
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = model.ProductName.Trim(),
                        ProductType = model.ProductType,
                        Markup = model.Markup,
                        Notes = model.Notes,
                        Area = 0,
                        Width = 0,
                        AdditionalWidth = 0,
                        Height = 0,
                        ArchHeight = 0,
                        Length = 0,
                        Color = null,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsComponentAvailable = model.IsComponentAvailable
                    };

                    await Add(product);
                    if (model.IsComponentAvailable != null && (bool)model.IsComponentAvailable)
                    {
                        foreach (var item in model.ProductComponents)
                        {
                            var isComponentExists = await productComponentService.AnyAsync(x => x.ComponentId == item.ComponentId && x.ProductId == product.ProductId
                            && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
                            var isPricingExpression = await expressionService.AnyAsync(x => x.Id.ToString() == item.PricingExpression && x.IsActive == true && x.IsDelete == false
                             && x.TenantId == AppConstant.LoggedInTenantId);
                            if (!isComponentExists && isPricingExpression)
                            {
                                TblProductComponent productcomponent = new TblProductComponent
                                {
                                    ProductComponentId = Guid.NewGuid(),
                                    ProductId = product.ProductId,
                                    ComponentId = item.ComponentId,
                                    PricingExpression = item.PricingExpression,
                                    Quantity = item.Quantity,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID,
                                    IsActive = true,
                                    IsDelete = false,
                                    TenantId = AppConstant.LoggedInTenantId
                                };

                                await productComponentService.Add(productcomponent);
                            }
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

                    bool isProductUsed =
                  dbContext.TblEstimateWindowProducts.Any(p => p.ProductId == ProductId && p.IsDelete == false);



                    if (isProductUsed)
                    {
                        responseVm.Message = "This Product is used in another entity (Estimate) and cannot be deleted.";
                        responseVm.IsSuccess = false;
                        return responseVm;
                    }
                    if (IsProductExists != null)
                    {
                        var IsProductInComponent = productComponentService
                    .GetAllQuerable().Where(x => x.ProductId == ProductId &&
                                         x.IsDelete == false &&
                                         x.TenantId == AppConstant.LoggedInTenantId)
                    .ToList();
                        foreach (var component in IsProductInComponent)
                        {
                            component.IsActive = false;
                            component.IsDelete = true;
                            component.UpdatedAt = DateTime.Now;
                            component.ModifiedBy = AppConstant.LoggedInUserID;
                            await productComponentService.Update(component);
                        }


                        IsProductExists.IsActive = false;
                        IsProductExists.IsDelete = true;
                        IsProductExists.UpdatedAt = DateTime.Now;
                        IsProductExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsProductExists);
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
                  //  x.ParentTenantId == null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (isTenantValid)
                {
                    var productList = (from p in dbContext.TblProducts.OrderByDescending(x => x.CreatedAt)
                                       join pt in dbContext.MstProductTypes on p.ProductType equals pt.ProductId.ToString()
                                       where p.IsDelete == false && (p.TenantId == AppConstant.LoggedInTenantId || p.TenantId.ToString() == AppConstant.LoggedInParentTenantId) 
                                       select new ProductListVm
                                       {
                                           ProductId = p.ProductId,
                                           ProductName = p.ProductName,
                                           Markup = p.Markup,
                                           Notes = p.Notes,
                                           ProductType = pt.ProductId.ToString().ToLower(),
                                           ProductTypeName = pt.ProductName,
                                           TenantId = p.TenantId,
                                           IsActive = p.IsActive,
                                           IsComponentAvailable = p.IsComponentAvailable,
                                       }).ToList();
                    foreach (var item in productList)
                    {
                        var productComponent = (from pm in dbContext.TblProductComponents.OrderByDescending(x => x.CreatedAt)
                                                join c in dbContext.TblComponents on pm.ComponentId equals c.ComponentId
                                                join pe in dbContext.TblExpressions on pm.PricingExpression equals pe.Id.ToString()
                                                where pm.ProductId == item.ProductId && pm.IsDelete == false
                                                select new ProductComponentVmm
                                                {
                                                    ComponentId = pm.ComponentId,
                                                    PricingExpression = pm.PricingExpression,
                                                    PricingExpressionFormula = !string.IsNullOrEmpty(pe.DisplayArray) ? pe.DisplayArray.Split(new[] { ", " }, StringSplitOptions.TrimEntries) : new string[0],
                                                    ComponentName = c.ComponentName,
                                                    Quantity = pm.Quantity,
                                                    ProductComponentId = pm.ProductComponentId,
                                                    IsActive = pm.IsActive,
                                                }).ToList();

                        item.ProductComponents = productComponent;
                    }

                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = productList
                              .Where(x =>
                              (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                              (string.IsNullOrEmpty(filter.ProductType) ? true : (filter.ProductType == x.ProductType?.ToString())) &&
                              (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                 (x.ProductId.ToString() == filter.SearchValue) ||
                                 (x.ProductType?.ToString() == filter.SearchValue) ||
                                 (x.ProductName ?? "").ToLower().Contains(filter.SearchValue) ||
                                 (x.Notes ?? "").ToLower().Contains(filter.SearchValue) ||
                                 (x.Markup.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                 (x.ProductTypeName ?? "").ToLower().Contains(filter.SearchValue))
                             ).ToList();



                    var TakenRecords = Paginate.Pagination(FilterRecords, filter);
                    responseVm.IsSuccess = true;

                    if (TakenRecords != null && TakenRecords.Count > 0)
                    {
                        responseVm.Data = TakenRecords;
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

        public async Task<IResponseVm<string>> Update_Product(UpdateProduct model)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);

                var IsProductName = await AnyAsync(x => x.ProductName == model.ProductName
                    && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId
                    && x.ProductType == model.ProductType && x.ProductId!=model.ProductId);
                if (IsProductName)
                {
                    responseVm.Message = "ProductName " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                if (IsTenant)
                {
                    var IsProductExists = GetAllQuerable()
                        .Where(c => c.ProductId == model.ProductId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();

                    var componentExist =  productComponentService.GetAllQuerable().Where(x=>x.ProductId==model.ProductId &&x.IsDelete==false && x.IsActive==true).Count(); // Fetch the component details
                   if(componentExist>0 && IsProductExists.ProductType != model.ProductType)
                    {
                        responseVm.Message = "You can't edit  Product Component";
                        return responseVm;
                    }
                    if (IsProductExists != null)
                    {
                        IsProductExists.ProductName = model.ProductName;
                        IsProductExists.ProductType = model.ProductType;
                        IsProductExists.Notes = model.Notes;
                        IsProductExists.Markup = model.Markup;
                        IsProductExists.UpdatedAt = DateTime.Now;
                        IsProductExists.ModifiedBy = AppConstant.LoggedInUserID;
                        IsProductExists.IsComponentAvailable = model.IsComponentAvailable;
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

