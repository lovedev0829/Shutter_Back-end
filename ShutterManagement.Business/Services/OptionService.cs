using Microsoft.Extensions.Options;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class OptionService : BaseRepository<ShutterDBContext, TblOption>, IOption
    {
        private readonly ShutterDBContext _context;
        private readonly IOptionsTypeService optionsTypeService;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;
        private readonly IManagePartnersProductsOptions managePartnersProductsOptions;

        public OptionService(ShutterDBContext context, IOptionsTypeService optionsTypeService,
            ClaimsProperty claimsProperty, ITenantService tenantService,
            IManagePartnersProductsOptions managePartnersProductsOptions
            )
        {
            _context = context;
            this.optionsTypeService = optionsTypeService;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
            this.managePartnersProductsOptions = managePartnersProductsOptions;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                       .Where(c => c.OptionId == Id
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                       .FirstOrDefault();
                    var isManageproductoption = managePartnersProductsOptions.GetAllQuerable().Where
                        (x => x.OptionId == Id && x.IsDelete == false && x.IsActive == true && x.TenantId == AppConstant.LoggedInTenantId).
                        FirstOrDefault();
                    if(isManageproductoption != null)
                    {
                        isManageproductoption.IsActive = !isManageproductoption.IsActive;
                        isManageproductoption.UpdatedAt = DateTime.Now;
                        isManageproductoption.ModifiedBy = AppConstant.LoggedInTenantId;
                        dbContext.TblManagePartnersProductsOptions.Update(isManageproductoption);
                        await dbContext.SaveChangesAsync();
                    }
                    if (IsOptionExists != null)
                    {
                        IsOptionExists.IsActive = !IsOptionExists.IsActive;
                        IsOptionExists.UpdatedAt = DateTime.Now;
                        IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = Update(IsOptionExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = $"{AppConstant.UpdateSuccessMsg}";
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

        public async Task<IResponseVm<string>> Create_Option(CreateOption model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId);
                if (IsTenant)
                {
                    var message = "";
                    bool alloptiontypeadded = true;
                    var IsOptionNameExists = await AnyAsync(x => x.OptionName == model.OptionName.Trim()
                    && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId
                    && x.ProductId == model.ProductId && x.ProductTypeId == model.ProductTypeId);
                    if (!IsOptionNameExists)
                    {
                        TblOption options = new TblOption
                        {
                            OptionId = Guid.NewGuid(),
                            OptionName = model.OptionName.Trim(),
                            ProductTypeId = model.ProductTypeId,
                            ProductId = model.ProductId,
                            Markup = model.Markup,
                            OptionExpression = model.OptionExpression,
                            Unit = model.Unit.Trim(),
                            CreatedAt = DateTime.Now,
                            CreatedBy = AppConstant.LoggedInUserID,
                            TenantId = AppConstant.LoggedInTenantId,
                            IsTaxApplied = model.IsTaxAvailable,
                            TaxId = model.TaxId,
                            IsActive = true,
                            IsDelete = false
                        };
                        await Add(options);
                        var manageproduct = dbContext.TblManagePartnersProducts.Where(x=> x.ProductId == options.ProductId
                        && x.TenantId == AppConstant.LoggedInTenantId && x.IsAvailable == true && x.IsActive == true && x.IsDelete == false)
                            .FirstOrDefault();  
                        if (options.OptionId != Guid.Empty)
                        {
                            foreach (var item in model.Options)
                            {
                                var IsOptionTypeExists = await optionsTypeService.AnyAsync(x => x.IsActive == true
                                && x.IsDelete == false && x.OptionId == options.OptionId && x.OptionType == item.OptionType);
                                if (!IsOptionTypeExists)
                                {
                                    TblOptionsType optionsType = new TblOptionsType
                                    {
                                        OptionId = options.OptionId,
                                        OptionTypeId = Guid.NewGuid(),
                                        OptionType = item.OptionType.Trim(),
                                        AdditionalCost = item.AdditionalCost,
                                        // Unit = item.Unit.Trim(),
                                        IsDefault = item.IsDefault,
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = AppConstant.LoggedInUserID,
                                        SortOrder=item.SortOrder,
                                        IsActive = true,
                                        IsDelete = false
                                    };
                                    await optionsTypeService.Add(optionsType);
                                    if (!AppConstant.IsTenant)
                                    {
                                        if (manageproduct != null)
                                        {
                                            TblManagePartnersProductsOption managePartnerProductOption = new TblManagePartnersProductsOption
                                            {
                                                ManageProductId = manageproduct.ManageProductId,
                                                OptionId = options.OptionId,
                                                OptionTypeId = optionsType.OptionTypeId,
                                                TaxId = options.TaxId,
                                                TenantId = AppConstant.LoggedInTenantId,
                                                IsActive = true,
                                                IsDelete = false,
                                                IsAvailable = true,
                                                CreatedAt = DateTime.Now,
                                                CreatedBy = AppConstant.LoggedInUserID,
                                            };
                                            await managePartnersProductsOptions.Add(managePartnerProductOption);
                                        }
                                        //dbContext.Add(managePartnerProductOption);
                                        //await dbContext.SaveChangesAsync();
                                    }
                                }
                                else
                                {
                                    alloptiontypeadded = false;
                                    message += $"{item.OptionType} OptionType " + AppConstant.AlreadyExists;
                                }
                            }
                            if (!alloptiontypeadded)
                            {
                                responseVm.IsSuccess = true;
                                responseVm.Message = message;
                            }
                            else
                            {
                                responseVm.IsSuccess = true;
                                responseVm.Message = AppConstant.InsertSuccessMsg;
                            }
                        }
                        else
                        {
                            responseVm.Message = AppConstant.ExternalError;
                        }
                    }
                    else
                    {
                        responseVm.Message = "Option Name" + " " + AppConstant.AlreadyExists;
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                       .Where(c => c.OptionId == Id
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                       .FirstOrDefault();
                    var isManageproductoption = managePartnersProductsOptions.GetAllQuerable().Where
                     (x => x.OptionId == Id && x.IsDelete == false && x.IsActive == true && x.TenantId == AppConstant.LoggedInTenantId).
                     FirstOrDefault();
                    if (isManageproductoption != null)
                    {
                        isManageproductoption.IsActive = false;
                        isManageproductoption.IsDelete = true;
                        isManageproductoption.UpdatedAt = DateTime.Now;
                        isManageproductoption.ModifiedBy = AppConstant.LoggedInTenantId;
                        dbContext.TblManagePartnersProductsOptions.Update(isManageproductoption);
                        await dbContext.SaveChangesAsync();
                    }
                    if (IsOptionExists != null)
                    {
                        IsOptionExists.IsDelete = true;
                        IsOptionExists.IsActive = false;
                        IsOptionExists.UpdatedAt = DateTime.Now;
                        IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = Update(IsOptionExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = $"{AppConstant.DeleteSuccessMsg}";
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

        public async Task<IResponseVm<List<OptionsList>>> GetAll_Option(Pagination filter,bool? IsManageProductOption)
        {
            IResponseVm<List<OptionsList>> responseVm = new ResponseVm<List<OptionsList>>();
            try
            {
                var tenant = await tenantService.FirstOrDefaultAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.IsActive == true &&
                    x.IsDelete == false);
                var IsOptionExists = new List<OptionsList>();
                if (tenant != null)
                {
                    bool isSubTenant = tenant.ParentTenantId != null;

                    var allOptions = (await GetAll())
                        .Where(x => x.IsDelete == false &&
                                    (x.TenantId == AppConstant.LoggedInTenantId))
                        //||
                        // x.TenantId.ToString().ToLower() == AppConstant.LoggedInParentTenantId.ToLower())
                        .OrderByDescending(x => x.CreatedAt)
                        .ToList();
                    //if (issubtenant)
                    //{
                    //    isoptionexists = (
                    //            from op in dbcontext.tbloptions
                    //            join p in dbcontext.tblmanagepartnersproducts
                    //              on op.productid equals p.productid
                    //            //join opp in dbcontext.tblmanagepartnersproductsoptions
                    //            //on p.manageproductid equals opp.manageproductid
                    //            //into optiontype from
                    //            //opp in optiontype.defaultifempty()

                    //            join tax in dbcontext.tbltaxrates
                    //             on op.taxid equals tax.taxrateid into taxgroup
                    //            from tax in taxgroup.defaultifempty()

                    //            join prod in dbcontext.tblproducts
                    //            on op.productid equals prod.productid into productgroup
                    //            from product in productgroup.defaultifempty()

                    //            join pt in dbcontext.mstproducttypes
                    //             on op.producttypeid equals pt.productid into ptgroup
                    //            from producttype in ptgroup.defaultifempty()

                    //            where op.isdelete == false
                    //       && op.tenantid == appconstant.loggedintenantid
                    //       && p.tenantid == op.tenantid
                    //      // && (opp == null || (opp.isdelete == false && opp.isavailable == true))
                    //      && p.isavailable == true
                    //      orderby op.createdat descending

                    //    select new optionslist
                    //    {
                    //    optionid = op.optionid,
                    //    isactive = op.isactive,
                    //    optionname = op.optionname ?? "",
                    //    taxid = op.taxid,
                    //    taxname = tax != null ? tax.taxname : "",
                    //   productid = op.productid,
                    //    unit = op.unit,
                    //   markup = op.markup ?? 0.0m,
                    //   optionexpression = op.optionexpression != null ? op.optionexpression.tostring() : "",
                    //   productname = product != null ? product.productname : "",
                    //   producttypeid = op.producttypeid,
                    //   producttypename = producttype != null ? producttype.productname : "",
                    //   istaxavailable = op.istaxapplied,

                    //   }).tolist();
                    //}

                    if ((bool)IsManageProductOption)
                    {
                        var assignedOptionIds = dbContext.TblManagePartnersProductsOptions
                            .Where(m => m.TenantId == AppConstant.LoggedInTenantId && m.IsAvailable == true && m.IsDelete == false && m.IsActive == true)
                            .Select(m => m.OptionId)
                            .ToList();

                        allOptions = allOptions
                        .Where(x => assignedOptionIds.Contains(x.OptionId) || x.TenantId == AppConstant.LoggedInTenantId)
                        .ToList();
                    }
                    
                        IsOptionExists = allOptions
                           .Select(x => new OptionsList
                           {
                               OptionId = x.OptionId,
                               IsActive = x.IsActive,
                               OptionName = x.OptionName ?? "",
                               TaxId = x.TaxId ?? null,
                               TaxName = dbContext.TblTaxRates.Where(y => y.TaxRateId == x.TaxId).Select(z => z.TaxName).FirstOrDefault() ?? "",
                               ProductId = x.ProductId,
                               Unit = x.Unit,
                               Markup = x.Markup ?? 0.0m,
                               OptionExpression = x.OptionExpression.ToString() ?? "",
                               ProductName = dbContext.TblProducts.Where(y => y.ProductId == x.ProductId).Select(z => z.ProductName).FirstOrDefault(),
                               ProductTypeId = x.ProductTypeId,
                               ProductTypeName = dbContext.MstProductTypes.Where(y => y.ProductId == x.ProductTypeId).Select(z => z.ProductName).FirstOrDefault(),
                               IsTaxAvailable = x.IsTaxApplied,


                           })
                           .ToList();
                   
                    
                    foreach (var item in IsOptionExists)
                    {

                        var optionsType = optionsTypeService.GetAllQuerable().OrderBy(x=>x.SortOrder).
                            Where(x => x.IsDelete == false && x.OptionId == item.OptionId).OrderBy(x => x.CreatedAt)
                            .Select(x => new OptionsType
                            {
                                AdditionalCost = x.AdditionalCost,
                                IsDefault = x.IsDefault,
                                OptionType = x.OptionType,
                                OptionTypeId = x.OptionTypeId,
                                SortOrder=x.SortOrder,
                                //Unit = x.Unit
                            })
                            .ToList();
                        item.Options = optionsType;
                    }

                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();

                    var FilterRecords = IsOptionExists
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                     (string.IsNullOrEmpty(filter.ProductType) ? true : (filter.ProductType == x.ProductTypeId.ToString())) &&
                                    (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                    (x.OptionId.ToString() == filter.SearchValue)
                                    || (x.ProductName?.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.OptionName?.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.Markup?.ToString().ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.OptionExpression?.ToString().ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.TaxName?.ToLower() ?? "").Contains(filter?.SearchValue)
                                    || (x.ProductTypeName?.ToLower() ?? "").Contains(filter?.SearchValue)))
                                    .ToList();


                    var queryable = FilterRecords.AsQueryable();

                    queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                    queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                    var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                    responseVm.IsSuccess = true;
                    responseVm.Data = TakenRecords;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count();
                }

                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = $"Error: {ex.Message}";
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_Option(UpdateOption model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                        .Where(c => c.OptionId == model.OptionId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();
                    var IsOptionNameExists = await AnyAsync(x => x.OptionName == model.OptionName.Trim()
                  && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId
                  && x.ProductId == model.ProductId && x.ProductTypeId == model.ProductTypeId && x.OptionId != model.OptionId);
                    if (IsOptionExists == null)
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                        return responseVm;
                    }
                    if (IsOptionNameExists)
                    {
                        responseVm.Message = "Option Name " + AppConstant.AlreadyExists;
                        return responseVm;
                    }


                    //TblOption options = new TblOption
                    //{
                    //    OptionId = Guid.NewGuid(),
                    //    OptionName = model.OptionName,
                    //    ProductId = model.ProductId,
                    //    ProductTypeId = model.ProductTypeId,
                    //    CreatedAt = DateTime.Now,
                    //    CreatedBy = AppConstant.LoggedInUserID,
                    //    TenantId = AppConstant.LoggedInTenantId,
                    //    IsTaxApplied = model.IsTaxAvailable,
                    //    TaxId = model.TaxId,
                    //    IsActive = true,
                    //    IsDelete = false
                    //};

                    if (IsOptionExists != null)
                    {
                        IsOptionExists.OptionName = model.OptionName;
                        IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
                        IsOptionExists.UpdatedAt = DateTime.Now;
                        IsOptionExists.ProductId = model.ProductId;
                        IsOptionExists.Markup = model.Markup;
                        IsOptionExists.ProductTypeId = model.ProductTypeId;
                        IsOptionExists.Unit = model.Unit.Trim();
                        IsOptionExists.OptionExpression = model.OptionExpression;
                        IsOptionExists.IsTaxApplied = model.IsTaxAvailable;
                        IsOptionExists.TaxId = model.TaxId;


                        await Update(IsOptionExists);

                        var OptionsTypes = optionsTypeService.GetAllQuerable().Where(x => x.IsActive == true
                        && x.IsDelete == false && x.OptionId == IsOptionExists.OptionId).Select(x => new OptionsType
                        {
                            AdditionalCost = x.AdditionalCost,
                            IsDefault = x.IsDefault,
                            OptionType = x.OptionType,
                            OptionTypeId = x.OptionTypeId,
                            // Unit = x.Unit
                        }).ToArray();
                        //var Ismanageproductexist = await dbContext.TblManagePartnersProductsOptions.Where
                        //    (x => x.OptionId == IsOptionExists.OptionId && x.TenantId == AppConstant.LoggedInTenantId
                        //    && x.IsActive == true && x.IsAvailable == true && x.IsDelete == false).FirstOrDefaultAsync();
                        //if(Ismanageproductexist != null)
                        //{ 
                        //    Ismanageproductexist.TaxId = AppConstant.LoggedInTenantId;
                        //}


                        var InsertRecords = model.Options.Where(a1 => !OptionsTypes.Any(a2 => a1.OptionType == a2.OptionType && a1.AdditionalCost == a2.AdditionalCost && a1.IsDefault == a2.IsDefault)).ToList();
                        var DeleteRecords = OptionsTypes.Where(a1 => !model.Options.Any(a2 => a1.OptionType == a2.OptionType && a1.AdditionalCost == a2.AdditionalCost && a1.IsDefault == a2.IsDefault)).ToList();

                        if (!AppConstant.IsTenant)
                        {

                            foreach (var item in DeleteRecords)
                            {
                                var Ismanageproductexist = managePartnersProductsOptions.GetAllQuerable().Where
                               (x => x.TenantId == AppConstant.LoggedInTenantId
                               && x.IsActive == true && x.IsAvailable == true && x.IsDelete == false && x.OptionTypeId == item.OptionTypeId).FirstOrDefault();
                                if (Ismanageproductexist != null)
                                {
                                    Ismanageproductexist.IsActive = false;
                                    Ismanageproductexist.IsDelete = true;
                                    Ismanageproductexist.IsAvailable = false;

                                    await managePartnersProductsOptions.Update(Ismanageproductexist);
                                    //dbContext.Update(Ismanageproductexist);
                                    //await dbContext.SaveChangesAsync();
                                }
                            }
                        }
                        foreach (var item in DeleteRecords)
                        {
                            var data = optionsTypeService.GetAllQuerable().Where(x => x.OptionTypeId == item.OptionTypeId
                             && x.IsDelete == false).FirstOrDefault();
                            data.IsDelete = true;
                            data.IsActive = false;
                            await optionsTypeService.Update(data);
                        }

                        foreach (var item in InsertRecords)
                        {
                            
                            var IsOptionTypeExists = await optionsTypeService.AnyAsync(x => x.IsActive == true
                                && x.IsDelete == false && x.OptionId == IsOptionExists.OptionId && x.OptionType ==
                                item.OptionType);
                            if (!IsOptionTypeExists)
                            {
                                TblOptionsType optionsType = new TblOptionsType
                                {
                                    OptionId = IsOptionExists.OptionId,
                                    OptionTypeId = Guid.NewGuid(),
                                    OptionType = item.OptionType,
                                    AdditionalCost = item.AdditionalCost,
                                    //Unit = item.Unit,
                                    IsDefault = item.IsDefault,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID,
                                    SortOrder = item.SortOrder,
                                    IsActive = true,
                                    IsDelete = false
                                };
                                await optionsTypeService.Add(optionsType);

                                if (!AppConstant.IsTenant)
                                {
                                    var manageproduct = dbContext.TblManagePartnersProducts.Where(x => x.ProductId == IsOptionExists.ProductId
                                   && x.TenantId == AppConstant.LoggedInTenantId && x.IsAvailable == true && x.IsActive == true && x.IsDelete == false)
                            .FirstOrDefault();
                                    if (manageproduct != null)
                                    {
                                        TblManagePartnersProductsOption managePartnerProductOption = new TblManagePartnersProductsOption
                                        {
                                            ManageProductId = manageproduct.ManageProductId,
                                            OptionId = IsOptionExists.OptionId,
                                            OptionTypeId = optionsType.OptionTypeId,
                                            TaxId = IsOptionExists.TaxId,
                                            TenantId = AppConstant.LoggedInTenantId,
                                            IsActive = true,
                                            IsDelete = false,
                                            IsAvailable = true,
                                            CreatedAt = DateTime.Now,
                                            CreatedBy = AppConstant.LoggedInUserID,
                                        };
                                        await managePartnersProductsOptions.Add(managePartnerProductOption);
                                        //dbContext.TblManagePartnersProductsOptions.Add(managePartnerProductOption);
                                        //await dbContext.SaveChangesAsync();
                                    }
                                }
                            }
                        }
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
