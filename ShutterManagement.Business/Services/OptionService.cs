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

        public OptionService(ShutterDBContext context, IOptionsTypeService optionsTypeService,
            ClaimsProperty claimsProperty, ITenantService tenantService)
        {
            _context = context;
            this.optionsTypeService = optionsTypeService;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId  && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                       .Where(c => c.OptionId == Id
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                       .FirstOrDefault();
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
                                        OptionType = item.OptionType.Trim(),
                                        AdditionalCost = item.AdditionalCost,
                                       // Unit = item.Unit.Trim(),
                                        IsDefault = item.IsDefault,
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = AppConstant.LoggedInUserID,
                                        IsActive = true,
                                        IsDelete = false
                                    };
                                    await optionsTypeService.Add(optionsType);
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
                        responseVm.Message = "Option Name"+" "+AppConstant.AlreadyExists;
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId  && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                       .Where(c => c.OptionId == Id
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                       .FirstOrDefault();
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

        public async Task<IResponseVm<List<OptionsList>>> GetAll_Option(Pagination filter)
        {
            IResponseVm<List<OptionsList>> responseVm = new ResponseVm<List<OptionsList>>();
            try
            {
                var tenant = await tenantService.FirstOrDefaultAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.IsActive == true &&
                    x.IsDelete == false);

                if (tenant != null)
                {
                    bool isSubTenant = tenant.ParentTenantId != null;

                    var allOptions = (await GetAll())
                        .Where(x => x.IsDelete == false &&
                                    (x.TenantId == AppConstant.LoggedInTenantId ))//||
                                    // x.TenantId.ToString().ToLower() == AppConstant.LoggedInParentTenantId.ToLower()))
                        .OrderByDescending(x => x.CreatedAt)
                        .ToList();

                    
                    //if (isSubTenant)
                    //{
                    //    var assignedOptionIds = dbContext.TblManagePartnersProductsOptions
                    //        .Where(m => m.TenantId == AppConstant.LoggedInTenantId && m.IsAvailable == true && m.IsDelete==false && m.IsActive==true)
                    //        .Select(m => m.OptionId)
                    //        .ToList();

                        allOptions = allOptions
                            .Where(x => assignedOptionIds.Contains(x.OptionId) || x.TenantId == AppConstant.LoggedInTenantId)
                            .ToList();
                    }

                    var IsOptionExists = allOptions
                        .Select(x => new OptionsList
                        {
                            OptionId = x.OptionId,
                            IsActive = x.IsActive,
                            OptionName = x.OptionName??"",
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

                        var optionsType = optionsTypeService.GetAllQuerable().
                            Where(x => x.IsDelete == false && x.OptionId == item.OptionId).OrderBy(x => x.CreatedAt)
                            .Select(x => new OptionsType
                            {
                                AdditionalCost = x.AdditionalCost,
                                IsDefault = x.IsDefault,
                                OptionType = x.OptionType,
                                OptionTypeId = x.OptionTypeId,
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId  && x.IsActive == true && x.IsDelete == false);
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


                        var InsertRecords = model.Options.Where(a1 => !OptionsTypes.Any(a2 => a1.OptionType == a2.OptionType && a1.AdditionalCost == a2.AdditionalCost && a1.IsDefault == a2.IsDefault)).ToList();
                        var DeleteRecords = OptionsTypes.Where(a1 => !model.Options.Any(a2 => a1.OptionType == a2.OptionType && a1.AdditionalCost == a2.AdditionalCost && a1.IsDefault == a2.IsDefault)).ToList();

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
                                && x.IsDelete == false && x.OptionId == IsOptionExists.OptionId && x.OptionType == item.OptionType);
                            if (!IsOptionTypeExists)
                            {
                                TblOptionsType optionsType = new TblOptionsType
                                {
                                    OptionId = IsOptionExists.OptionId,
                                    OptionType = item.OptionType,
                                    AdditionalCost = item.AdditionalCost,
                                    //Unit = item.Unit,
                                    IsDefault = item.IsDefault,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID,
                                    IsActive = true,
                                    IsDelete = false
                                };
                                await optionsTypeService.Add(optionsType);
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
