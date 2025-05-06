using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using Microsoft.Extensions.Options;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Business.Services;
using Org.BouncyCastle.Utilities;
using ShutterManagement.Core.Common;

namespace ShutterManagement.Business.Services
{
    public class ManagePartnersProductsService : BaseRepository<ShutterDBContext, TblManagePartnersProduct>, IManagePartnersProducts
    {
        private readonly ITenantService tenantService;
        private readonly IProductService productService;
        private readonly IManagePartnersProductsOptions managePartnersProductsOptions;

        public ManagePartnersProductsService(ITenantService tenantService, IProductService productService, IManagePartnersProductsOptions managePartnersProductsOptions)
        {
            this.tenantService = tenantService;
            this.productService = productService;
            this.managePartnersProductsOptions = managePartnersProductsOptions;
        }

        public async Task<IResponseVm<string>> CreateManagePartnerProduct(CreateManagePartnerProduct model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsSuccess = false;
                var message = "";
                bool IsSubtenant = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId != null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (!IsSubtenant)
                {
                    if (string.IsNullOrEmpty(model.TenantId))
                    {
                        responseVm.Message = $"First Select Partner. ";
                        return responseVm;
                    }
                    var IsProductExist = await productService.AnyAsync(x => x.ProductId == model.ProductId && x.IsActive == true
                       && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
                    if (IsProductExist)
                    {
                       await UpdateManagePartnerProduct(model);
                    }
                    else
                    {
                        responseVm.Message += $" Some Products does not exist in the Tenant. ";
                        return responseVm;
                    }
                   
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Manage Product for Partner successfully.";
                }

                else
                {
                    bool IsProductExist = await productService.AnyAsync(x => x.ProductId == model.ProductId && x.IsActive == true
                        && x.IsDelete == false && x.TenantId.ToString() == AppConstant.LoggedInParentTenantId);
                    if (IsProductExist)
                    {
                        var IsManagePartnerProductExist = GetAllQuerable().Where(x => x.ProductId == model.ProductId &&
                        x.IsActive == true && x.IsDelete == false && x.IsAvailable == true && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                        if (IsManagePartnerProductExist != null)
                        {
                            IsManagePartnerProductExist.PartnerSellingPrice = model.PartnerSellingPrice;
                            IsManagePartnerProductExist.MarkupType = model.MarkupType;
                            IsManagePartnerProductExist.MarkupPer = model.MarkupPer;
                            await Update(IsManagePartnerProductExist);
                            foreach (var item in model.ManagePartnerProductOptions)
                            {
                                var IsManagepartneroptiontype = managePartnersProductsOptions.GetAllQuerable().
                                    Where(x => x.OptionId == item.OptionId && x.OptionTypeId == item.OptionTypeId && x.ManageProductId.ToString().ToLower()
                                    == IsManagePartnerProductExist.ManageProductId.ToString().ToLower() &&
                                    x.IsActive == true && x.IsDelete == false && x.IsAvailable == true).FirstOrDefault();
                                if (IsManagepartneroptiontype != null)
                                {
                                    IsManagepartneroptiontype.PartnerSellingPrice = item.PartnerSellingPrice;
                                    await managePartnersProductsOptions.Update(IsManagepartneroptiontype);
                                }
                            }
                        }
                    }
                    else
                    {
                        IsSuccess = true;
                        message = message += $"Some Products does not exist in the Parent Tenant.";
                    }
                    if (IsSuccess)
                    {
                        responseVm.Message = message;
                        return responseVm;
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Manage Product successfully."; ;

                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        private async Task UpdateManagePartnerProduct(CreateManagePartnerProduct items)
        {
            var IsManagePartnerProductExist = GetAllQuerable().Where(x => x.ProductId == items.ProductId &&
                x.IsActive == true && x.IsDelete == false && x.TenantId.ToString().ToLower() == items.TenantId.ToLower() && x.IsAvailable == true).FirstOrDefault();

            if (IsManagePartnerProductExist != null)
            {
                IsManagePartnerProductExist.IsAvailable = items.IsAvailable;
                IsManagePartnerProductExist.WholeSalePrice = items.WholeSalePrice;
                IsManagePartnerProductExist.MaxPartnerPrice = items.MaxPartnerPrice;
                IsManagePartnerProductExist.TenantId = Guid.Parse(items.TenantId);
                IsManagePartnerProductExist.ModifiedBy = AppConstant.LoggedInUserID;
                IsManagePartnerProductExist.UpdatedAt = DateTime.Now;
                await Update(IsManagePartnerProductExist);

                foreach (var item in items.ManagePartnerProductOptions)
                {
                    await UpdatePartnerProductOptions(item, IsManagePartnerProductExist.ManageProductId);
                }
            }
            else
            {
                await CreateNewManagePartnerProduct(items);
            }
        }

        private async Task UpdatePartnerProductOptions(ManagePartnerProductOptions item, Guid manageProductId)
        {
            var IsManagepartneroptiontype = managePartnersProductsOptions.GetAllQuerable()
                .Where(x => x.OptionId == item.OptionId && x.OptionTypeId == item.OptionTypeId && x.ManageProductId.ToString().ToLower()
                == manageProductId.ToString().ToLower() &&
                x.IsActive == true && x.IsDelete == false).FirstOrDefault();

            if (IsManagepartneroptiontype != null)
            {
                IsManagepartneroptiontype.IsAvailable = item.IsAvailable;
                IsManagepartneroptiontype.WholeSalePrice = item.WholeSalePrice;
                IsManagepartneroptiontype.MaxPrice = item.MaxPrice;
                IsManagepartneroptiontype.ModifiedBy = AppConstant.LoggedInUserID;
                IsManagepartneroptiontype.UpdatedAt = DateTime.Now;
                await managePartnersProductsOptions.Update(IsManagepartneroptiontype);
            }
            else
            {
                await CreateNewManagePartnerProductOption(item, manageProductId);
            }
        }

        private async Task CreateNewManagePartnerProduct(CreateManagePartnerProduct model)
        {
            TblManagePartnersProduct managePartnersProduct = new TblManagePartnersProduct
            {
                ManageProductId = Guid.NewGuid(),
                ProductId = model.ProductId,
                IsAvailable = model.IsAvailable,
                WholeSalePrice = model.WholeSalePrice,
                MaxPartnerPrice = model.MaxPartnerPrice,
                PartnerSellingPrice = model.PartnerSellingPrice,
                CreatedAt = DateTime.Now,
                CreatedBy = AppConstant.LoggedInUserID,
                TenantId = Guid.Parse(model.TenantId),
                IsActive = true,
                IsDelete = false,
            };

            await Add(managePartnersProduct);

            foreach (var item in model.ManagePartnerProductOptions)
            {
                if (item.IsAvailable != null && (bool)item.IsAvailable)
                {
                    await CreateNewManagePartnerProductOption(item, managePartnersProduct.ManageProductId);
                }
            }
        }

        private async Task CreateNewManagePartnerProductOption(ManagePartnerProductOptions model, Guid manageProductId)
        {
            TblManagePartnersProductsOption optionsType = new TblManagePartnersProductsOption
            {
                ManageProductId = manageProductId,
                OptionId = model.OptionId,
                OptionTypeId = model.OptionTypeId,
                IsAvailable = model.IsAvailable,
                WholeSalePrice = model.WholeSalePrice,
                MaxPrice = model.MaxPrice,
                CreatedAt = DateTime.Now,
                CreatedBy = AppConstant.LoggedInUserID,
                IsActive = true,
                IsDelete = false,
                TenantId = Guid.Parse(model.TenantId)
            };
            await managePartnersProductsOptions.Add(optionsType);
        }

        public async Task<IResponseVm<List<ProductOptionsVm>>> GetAllProductOptions(Pagination filter)
        {
            var responseVm = new ResponseVm<List<ProductOptionsVm>>();

            try
            {
                List<ProductOptionsVm> productlist = new List<ProductOptionsVm>();
                bool IsTenantorSubtenant = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId != null &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (!IsTenantorSubtenant)
                {
                    bool hasPartnerData = false;
                    if (!string.IsNullOrEmpty(filter.TenantId))
                    {
                        hasPartnerData = GetAllQuerable()
                            .Any(x => x.TenantId.ToString() == filter.TenantId && x.IsDelete == false && x.IsActive == true);
                    }
                    if (!string.IsNullOrEmpty(filter.TenantId))
                    {
                        productlist = (hasPartnerData)
                                ? GetManagePartnerProductData(filter)
                                : GetDefaultProductData(filter);

                        productlist.DistinctBy(x => x.ProductName);
                        responseVm.IsSuccess = true;
                        responseVm.Data = productlist;
                        responseVm.Message = AppConstant.FetchSuccessMsg;
                        responseVm.TotalRecords = productlist.Count();
                    }

                    //else
                    //{
                    //    productlist = GetDefaultProductData(filter);
                    //    responseVm.IsSuccess = true;
                    //    responseVm.Data = productlist;
                    //    responseVm.Message = AppConstant.FetchSuccessMsg;
                    //    responseVm.TotalRecords = productlist.Count();
                    //}
                }
            
                else
                {
                    var products = (from p1 in dbContext.TblManagePartnersProducts
                                    join p2 in dbContext.TblProducts on p1.ProductId equals p2.ProductId
                                    where p1.IsActive == true && p1.IsDelete == false && p1.TenantId == AppConstant.LoggedInTenantId &&
                                    p1.IsAvailable == true && p2.ProductType == filter.ProductType
                                    select new ProductOptionsVm
                                    {
                                        ProductId = p1.ProductId,
                                        ProductName = p2.ProductName,
                                        PartnerSellingPrice = p1.PartnerSellingPrice ?? 0.0m,
                                        ManageProductId = p1.ManageProductId.ToString() ?? null,
                                        WholeSalePrice = p1.WholeSalePrice ?? 0.0m,
                                        MarkupPer = p1.MarkupPer ?? 0.0m,
                                        MarkupType = p1.MarkupType ?? "",
                                    }).ToList();

                    var productIds = products.Select(p => p.ProductId).ToList();

                    var allOptions = (from o1 in dbContext.TblManagePartnersProductsOptions
                                      join o2 in dbContext.TblOptions on
                                      o1.OptionId equals o2.OptionId
                                      join
                                      o3 in dbContext.TblManagePartnersProducts on o1.ManageProductId equals o3.ManageProductId
                                      where o1.IsDelete == false && o1.IsActive == true && productIds.Contains((Guid)o2.ProductId)
                                      && o1.TenantId == AppConstant.LoggedInTenantId
                                      select new ProductOptionsList
                                      {
                                          OptionId = o1.OptionId,
                                          ProductId = o2.ProductId,
                                          OptionName = o2.OptionName
                                      }).Distinct().ToList();

                    var optionIds = allOptions.Select(o => o.OptionId).ToList();

                    var allOptionTypes = (from o1 in dbContext.TblManagePartnersProductsOptions
                                          join o2 in dbContext.TblOptionsTypes
                                              on o1.OptionTypeId equals o2.OptionTypeId
                                          join o3 in dbContext.TblManagePartnersProducts on o1.ManageProductId equals o3.ManageProductId
                                          where o1.IsDelete == false && o1.IsActive == true && optionIds.Contains(o1.OptionId)
                                          && o1.TenantId == AppConstant.LoggedInTenantId
                                          select new ProductOptionTypeList
                                          {
                                              OptionTypeId = o1.OptionTypeId,
                                              OptionTypeName = o2.OptionType,
                                              OptionId = o1.OptionId,
                                              Price = o2.AdditionalCost,
                                              PartnerSellingPrice = o1.PartnerSellingPrice ?? 0.0m,
                                              WholeSalePrice = o1.WholeSalePrice ?? 0.0m
                                          }).ToList();

                    var productList = products.Select(product => new ProductOptionsVm
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        IsAvailable = product.IsAvailable,
                        WholeSalePrice = product.WholeSalePrice,
                        MaxPartnerPrice = product.MaxPartnerPrice,
                        PartnerSellingPrice = product.PartnerSellingPrice,
                        MarkupPer = product.MarkupPer,
                        MarkupType = product.MarkupType,
                        Options = allOptions
                            .Where(opt => opt.ProductId == product.ProductId)
                            .Select(opt => new ProductOptionsList
                            {
                                OptionId = opt.OptionId,
                                OptionName = opt.OptionName,
                                optionTypes = allOptionTypes
                                    .Where(ot => ot.OptionId == opt.OptionId)
                                    .ToList()
                            }).ToList()
                    }).ToList();

                    responseVm.IsSuccess = true;
                    responseVm.Data = productList;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = productList.Count();
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }

        private List<ProductOptionsVm> GetManagePartnerProductData(Pagination filter)
        {
            Guid? filterTenantId = null;

            if (!string.IsNullOrEmpty(filter.TenantId))
            {
                filterTenantId = Guid.Parse(filter.TenantId);
            }

            var products = (from p1 in dbContext.TblProducts
                            join p2 in dbContext.TblManagePartnersProducts on p1.ProductId  equals p2.ProductId into joined
                            from p2 in joined.Where(x => string.IsNullOrEmpty(filter.TenantId) || x.TenantId.ToString() == filter.TenantId).DefaultIfEmpty()
                            where p1.IsActive == true &&
                                  p1.IsDelete == false &&
                                  p1.TenantId == AppConstant.LoggedInTenantId &&
                                  p1.ProductType.ToString() == filter.ProductType
                            select new ProductOptionsVm
                            {
                                ProductId = p1.ProductId,
                                ProductName = p1.ProductName,
                                WholeSalePrice = (p2 != null && p2.TenantId.ToString() == filter.TenantId) ? p2.WholeSalePrice ?? 0.0m : 0.0m,
                                MaxPartnerPrice = (p2 != null && p2.TenantId.ToString() == filter.TenantId) ? p2.MaxPartnerPrice ?? 0.0m : 0.0m,
                                IsAvailable = (p2 != null && p2.TenantId.ToString() == filter.TenantId) ? p2.IsAvailable ?? false : false,
                                ManageProductId = (p2 != null && p2.TenantId.ToString() == filter.TenantId) ? p2.ManageProductId.ToString() : null,
                                TenantId = ((p2 != null && p2.TenantId.ToString() == filter.TenantId) ? p2.TenantId.ToString() : null) ?? ""
                            }).ToList();



            var productIds = products.Select(p => p.ProductId).ToList();

            var options = dbContext.TblOptions
                .Where(o => o.IsActive == true && o.IsDelete == false && productIds.Contains((Guid)o.ProductId))
                .Select(o => new ProductOptionsList
                {
                    OptionId = o.OptionId,
                    ProductId = o.ProductId,
                    OptionName = o.OptionName
                }).ToList();

            var optionIds = options.Select(o => o.OptionId).ToList();

            var optionTypes = (from o1 in dbContext.TblOptionsTypes
                               join o2 in dbContext.TblManagePartnersProductsOptions on o1.OptionTypeId equals o2.OptionTypeId into joined
                               from o2 in joined.Where(x => string.IsNullOrEmpty(filter.TenantId) || 
                               x.TenantId.ToString() == filter.TenantId).DefaultIfEmpty()
                               //from o2 in joined.DefaultIfEmpty() 
                               join o3 in dbContext.TblManagePartnersProducts on o2.ManageProductId equals o3.ManageProductId 
                               into manageproductId
                               from o3 in manageproductId.DefaultIfEmpty()
                               where o1.IsActive == true && o1.IsDelete == false && optionIds.Contains(o1.OptionId)
                               select new ProductOptionTypeList
                               {
                                   OptionTypeId = o1.OptionTypeId,
                                   OptionTypeName = o1.OptionType,
                                   OptionId = o1.OptionId,
                                   Price = o1.AdditionalCost ?? 0.0m,
                                   WholeSalePrice = (o2 != null && o2.TenantId.ToString() == filter.TenantId) ? o2.WholeSalePrice ?? 0.0m : 0.0m,
                                   MaxPrice = (o2 != null && o2.TenantId.ToString() == filter.TenantId) ? o2.MaxPrice ?? 0.0m : 0.0m,
                                   IsAvailable = (o2 != null && o2.TenantId.ToString() == filter.TenantId) ? o2.IsAvailable : false,
                               }).Distinct().ToList();

            foreach (var product in products)
            {
                product.Options = options
                    .Where(opt => opt.ProductId == product.ProductId)
                    .Select(opt => new ProductOptionsList
                    {
                        OptionId = opt.OptionId,
                        OptionName = opt.OptionName,
                        optionTypes = optionTypes.Where(ot => ot.OptionId == opt.OptionId).ToList()
                    }).ToList();
            }
            return products;
        }

        private List<ProductOptionsVm> GetDefaultProductData(Pagination filter)
        {
            var products = dbContext.TblProducts
                .Where(p => p.IsActive == true && p.IsDelete == false && p.TenantId == AppConstant.LoggedInTenantId &&
                            p.ProductType.ToString() == filter.ProductType)
                .Select(p => new ProductOptionsVm
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    MaxPartnerPrice = 0.0m,
                    WholeSalePrice = 0.0m,
                    IsAvailable = false,
                    TenantId = ""
                }).ToList();

            var productIds = products.Select(p => p.ProductId).ToList();

            var options = dbContext.TblOptions
                .Where(o => o.IsActive == true && o.IsDelete == false && productIds.Contains((Guid)o.ProductId))
                .Select(o => new ProductOptionsList
                {
                    OptionId = o.OptionId,
                    ProductId = o.ProductId,
                    OptionName = o.OptionName
                }).ToList();

            var optionIds = options.Select(o => o.OptionId).ToList();

            var optionTypes = dbContext.TblOptionsTypes
                .Where(ot => ot.IsActive == true && ot.IsDelete == false && optionIds.Contains(ot.OptionId))
                .Select(ot => new ProductOptionTypeList
                {
                    OptionTypeId = ot.OptionTypeId,
                    OptionTypeName = ot.OptionType,
                    OptionId = ot.OptionId,
                    Price = ot.AdditionalCost ?? 0.0m
                }).ToList();

            foreach (var product in products)
            {
                product.Options = options
                    .Where(opt => opt.ProductId == product.ProductId)
                    .Select(opt => new ProductOptionsList
                    {
                        OptionId = opt.OptionId,
                        OptionName = opt.OptionName,
                        optionTypes = optionTypes.Where(ot => ot.OptionId == opt.OptionId).ToList()
                    }).ToList();
            }
            return products;
        }

    }
}

