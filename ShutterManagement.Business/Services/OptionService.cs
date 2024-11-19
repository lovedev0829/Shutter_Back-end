using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
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
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;

        public OptionService(ShutterDbContext context, ClaimsProperty claimsProperty, ITenantService tenantService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
        }
        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                       .Where(c => c.OptionId == Id
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsActive == true
                                   && c.IsDelete == false)
                       .FirstOrDefault();
                    if (IsOptionExists != null)
                    {
                        IsOptionExists.IsActive = false;
                        IsOptionExists.UpdatedAt = DateTime.Now;
                        IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = Update(IsOptionExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = $"{AppConstant.UpdateSuccessMsg}";
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoUserFound;
                    }
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
                    foreach (var item in model.Options)
                    {
                        TblOption options = new TblOption
                        {
                            OptionId = Guid.NewGuid(),
                            OptionName = item.OptionName,
                            OptionType = model.OptionType,
                            AdditionalCost = item.AdditionalCost,
                            ProductTypeId = model.ProductTypeId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = AppConstant.LoggedInUserID,
                            TenantId = AppConstant.LoggedInTenantId,
                            IsActive = true,
                            IsDelete = false
                        };
                        await Add(options);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InsertSuccessMsg;
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                       .Where(c => c.OptionId == Id
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsActive == true
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
                        responseVm.Message = AppConstant.NoUserFound;
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }



        public async Task<IResponseVm<dynamic>> GetAll_Option(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    var IsOptionExists = (await GetAll())
                        .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                        .ToList();

                    var FilterRecords = IsOptionExists
                    .Where(x => string.IsNullOrEmpty(filter.SearchValue.Trim()) ||
                     (x.OptionName ?? "").Contains(filter.SearchValue.Trim())
                    ||
                    (x.OptionType ?? "").Contains(filter.SearchValue.Trim()))
                    .ToList();


                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    if (TakenRecords != null && TakenRecords.Count > 0)
                    {
                        responseVm.Data = TakenRecords.Select(x => new
                        {
                            x.OptionId,
                            x.OptionName,
                            x.OptionType,
                            x.AdditionalCost,
                            //x.IsActive
                        });
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                    responseVm.TotalRecords = FilterRecords.Count;  
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }
            return responseVm;
        }


        public async Task<IResponseVm<string>> Update_Option(UpdateOption model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                        .Where(c => c.OptionId == model.OptionId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();
                    foreach (var item in model.Options)
                    {
                        if (IsOptionExists != null)
                        {
                            IsOptionExists.OptionName = item.OptionName;
                            IsOptionExists.OptionType = model.OptionType;
                            IsOptionExists.AdditionalCost = item.AdditionalCost;
                            IsOptionExists.ProductTypeId = model.ProductTypeId;
                            IsOptionExists.UpdatedAt = DateTime.Now;
                            IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
                            var res = Update(IsOptionExists);
                            responseVm.IsSuccess = true;
                            responseVm.Message = $"{AppConstant.UpdateSuccessMsg}";
                        }

                        else
                        {
                            responseVm.Message = AppConstant.NoUserFound;
                        }
                    }
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
