using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using ShutterManagement.Core;

namespace ShutterManagement.Business.Services
{
    public class TaxService : BaseRepository<BtprojecShuttermanagementDevContext, TblTaxRate>, ITaxRate
    {
        private readonly BtprojecShuttermanagementDevContext dbContext;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;

        public TaxService(BtprojecShuttermanagementDevContext dbContext,ClaimsProperty claimsProperty,ITenantService tenantService)
        {
            this.dbContext = dbContext;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
        }

        public Task<IResponseVm<string>> ActiveInActiveTax(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExist = tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExist == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return Task.FromResult(responseVm);
                }
                var IsTaxExist = GetAllQuerable().Where(x => x.TaxRateId == Id && x.IsDelete == false).FirstOrDefault();

                if (IsTaxExist != null)
                {
                    IsTaxExist.IsActive = !IsTaxExist.IsActive;
                    IsTaxExist.UpdatedAt = DateTime.Now;
                    IsTaxExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = Update(IsTaxExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }

                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return Task.FromResult(responseVm);
        }
        

        public Task<IResponseVm<string>> CreateTax(CreateTax model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExist = tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExist != null)
                {
                    var tax = new TblTaxRate
                    {
                        Region = model.Region,
                        Rate = model.Rate,
                        EffectiveDate = model.EffectiveDate,
                        ExpiryDate = model.ExpiryDate,
                        IsActive = true,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        TenantId = AppConstant.LoggedInTenantId
                    };

                    var res =  Add(tax);
                  
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InsertSuccessMsg;
                  
                }
                else
                {
                    responseVm.Message = AppConstant.NotRelatesToTenant;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return Task.FromResult(responseVm);
        }

        public Task<IResponseVm<string>> DeleteTax(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExist = tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExist == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return Task.FromResult(responseVm);
                }
                var IsTaxExist = GetAllQuerable().Where(x => x.TaxRateId == Id && x.IsDelete == false).FirstOrDefault();

                if (IsTaxExist != null)
                {
                    IsTaxExist.IsActive = !IsTaxExist.IsActive;
                    IsTaxExist.IsDelete = true;
                    IsTaxExist.UpdatedAt = DateTime.Now;
                    IsTaxExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = Update(IsTaxExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
                }

                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return Task.FromResult(responseVm);
        }
        
        public async Task<IResponseVm<List<GetAllTax>>> GetAll_Tax(Pagination filter)
        {
            IResponseVm<List<GetAllTax>> responseVm = new ResponseVm<List<GetAllTax>>();
            try
            {
                var taxlist = (from t2 in dbContext.TblTaxRates.OrderByDescending(x => x.CreatedAt)
                              join t1 in dbContext.MstRegions on t2.Region equals t1.Id
                              where t2.TenantId == AppConstant.LoggedInTenantId && t2.IsDelete == false
                              select new GetAllTax
                              {
                                  TaxRateId = t2.TaxRateId,
                                  Rate = t2.Rate,
                                  EffectiveDate = t2.EffectiveDate,
                                  ExpiryDate = t2.ExpiryDate,
                                  IsActive = t2.IsActive,
                                  Region = t2.Region,
                                  RegionName = t1.RegionName,
                                  TenantId = t2.TenantId.ToString(),

                              }).ToList();
                
                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                var FilterRecords = taxlist
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (string.IsNullOrEmpty(filter.Region) ? true : (filter.Region == x.Region.ToString())) &&
                                    (string.IsNullOrEmpty(filter.TenantId) ? true : (filter.TenantId == x.TenantId.ToString())) &&
                                    (filter.SearchValue != "" ?
                                    (x.TaxRateId.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.Region.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.Rate.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.RegionName ?? "" ).ToLower().Contains(filter.SearchValue)
                                    : true)).ToList();



                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();



                responseVm.Data = TakenRecords.Select(x => new GetAllTax
                {
                    TaxRateId = x.TaxRateId,
                    Rate = x.Rate,
                    Region = x.Region,
                    RegionName = x.RegionName,
                    EffectiveDate = x.EffectiveDate,
                    ExpiryDate = x.ExpiryDate,
                    IsActive = x.IsActive,
                    TenantId = x.TenantId,
                }).ToList();
                responseVm.IsSuccess = true;
                responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                responseVm.TotalRecords = TakenRecords.Count;


            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public Task<IResponseVm<string>> UpdateTax(UpdateTax model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExist = tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExist == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return Task.FromResult(responseVm);
                }
                var IsTaxExist = GetAllQuerable().Where(x => x.TaxRateId == model.TaxRateId && x.IsDelete == false).FirstOrDefault();
            
                if (IsTaxExist != null)
                {
                    IsTaxExist.Rate = model.Rate;
                    IsTaxExist.EffectiveDate = model.EffectiveDate;
                    IsTaxExist.ExpiryDate = model.ExpiryDate;
                    IsTaxExist.UpdatedAt = DateTime.Now;
                    IsTaxExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = Update(IsTaxExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }

                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return Task.FromResult(responseVm);
        }
    }
}
