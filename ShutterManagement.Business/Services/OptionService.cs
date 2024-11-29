using Microsoft.Extensions.Options;
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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class OptionService : BaseRepository<ShutterDbContext, TblOption>, IOption
    {
        private readonly ShutterDbContext _context;
        private readonly IOptionsTypeService optionsTypeService;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;

        public OptionService(ShutterDbContext context, IOptionsTypeService optionsTypeService,
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    var IsOptionNameExists = await AnyAsync(x => x.OptionName == model.OptionName.Trim()
                    && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId
                    && x.ProductTypeId == model.ProductTypeId);
                    if (!IsOptionNameExists)
                    {
                        TblOption options = new TblOption
                        {
                            OptionId = Guid.NewGuid(),
                            OptionName = model.OptionName.Trim(),
                            ProductTypeId = model.ProductTypeId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = AppConstant.LoggedInUserID,
                            TenantId = AppConstant.LoggedInTenantId,
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
                                        OptionType = item.OptionType,
                                        AdditionalCost = item.AdditionalCost,
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
                            responseVm.Message = AppConstant.InsertSuccessMsg;
                        }
                        else
                        {
                            responseVm.Message = AppConstant.ExternalError;
                        }
                    }
                    else
                    {
                        responseVm.Message = AppConstant.AlreadyExists;
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
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

        public async Task<IResponseVm<List<OptionsList>>> GetAll_Option(Pagination filter)
        {
            IResponseVm<List<OptionsList>> responseVm = new ResponseVm<List<OptionsList>>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsOptionExists = (await GetAll())
                        .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt)
                        .Select(x => new OptionsList
                        {
                            OptionId = x.OptionId,
                            IsActive = x.IsActive,
                            OptionName = x.OptionName,
                            ProductTypeId = x.ProductTypeId,
                            ProductTypeName = dbContext.MstProductTypes.Where(y => y.ProductId == x.ProductTypeId).Select(z => z.ProductName).FirstOrDefault()
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
                                OptionTypeId = x.OptionTypeId
                            })
                            .ToList();
                        item.Options = optionsType;
                    }

                    filter.SearchValue = filter.SearchValue.Trim().ToLower();

                    var FilterRecords = IsOptionExists
                                    .Where(x => (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                    (x.OptionId.ToString() == filter.SearchValue)||
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true))
                                    || (x.ProductTypeName ?? "").Contains(filter.SearchValue)
                                    || (x.OptionName ?? "").Contains(filter.SearchValue)))
                                    .ToList();


                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.IsSuccess = true;
                    responseVm.Data = TakenRecords;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count;
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                        .Where(c => c.OptionId == model.OptionId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();
                    var IsOptionNameExists = await AnyAsync(x => x.OptionName == model.OptionName.Trim()
                  && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId
                  && x.ProductTypeId == model.ProductTypeId && x.OptionId != model.OptionId);
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
                    TblOption options = new TblOption
                    {
                        OptionId = Guid.NewGuid(),
                        OptionName = model.OptionName,
                        ProductTypeId = model.ProductTypeId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
                        IsDelete = false
                    };

                    if (IsOptionExists != null)
                    {
                        IsOptionExists.OptionName = model.OptionName;
                        IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
                        IsOptionExists.UpdatedAt = DateTime.Now;
                        IsOptionExists.ProductTypeId = model.ProductTypeId;

                        await Update(IsOptionExists);

                        var OptionsTypes = optionsTypeService.GetAllQuerable().Where(x => x.IsActive == true
                        && x.IsDelete == false && x.OptionId == IsOptionExists.OptionId).Select(x => new OptionsType
                        {
                            AdditionalCost = x.AdditionalCost,
                            IsDefault = x.IsDefault,
                            OptionType = x.OptionType,
                            OptionTypeId = x.OptionTypeId
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
