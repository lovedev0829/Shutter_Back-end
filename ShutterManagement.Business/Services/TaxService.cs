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
using ShutterManagement.Core.Common;

namespace ShutterManagement.Business.Services
{
    public class TaxService : BaseRepository<ShutterDBContext, TblTaxRate>, ITaxRate
    {
        private readonly ShutterDBContext dbContext;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;

        public TaxService(ShutterDBContext dbContext,ClaimsProperty claimsProperty,ITenantService tenantService)
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
                var IsTenantExist = tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
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
        

        public async Task<IResponseVm<string>> CreateTax(CreateTax model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExist = tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExist != null)
                {
                    var IsTaxExist = GetAllQuerable()
                    .FirstOrDefault(x => x.TaxName.Trim() == model.TaxName.Trim() && x.IsDelete == false && x.IsActive == true && x.TenantId == claimsProperty.GetTenantId());

                    if(IsTaxExist != null)
                    {
                        responseVm.Message = "TaxName" + AppConstant.AlreadyExists;
                        return responseVm;
                    }

                  
                    var tax = new TblTaxRate
                    {
                        TaxName = model.TaxName.Trim(),
                        Municipality = model.Municipality,
                        State = model.State,
                        City = model.City,
                        IsActive = true,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        TenantId = claimsProperty.GetTenantId()
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

            return responseVm;
        }

        public Task<IResponseVm<string>> DeleteTax(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExist = tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
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
                              where t2.TenantId == claimsProperty.GetTenantId() && t2.IsDelete == false
                              select new GetAllTax
                              {
                                  TaxRateId = t2.TaxRateId,
                                  TaxName = t2.TaxName??"",
                                  Municipality = t2.Municipality ?? null,
                                  State = t2.State ?? null,
                                  City = t2.City ?? null,
                                  TenantId = t2.TenantId.ToString(),
                                  IsActive = t2.IsActive,

                              }).ToList();
                
                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                var FilterRecords = taxlist
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (string.IsNullOrEmpty(filter.TenantId) ? true : (filter.TenantId == x.TenantId.ToString())) &&
                                    (filter.SearchValue != "" ?
                                    (x.TaxRateId.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.TaxName.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.Municipality.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.State.ToString() ?? "" ).ToLower().Contains(filter.SearchValue)||
                                    (x.City.ToString() ?? "" ).ToLower().Contains(filter.SearchValue)
                                    : true)).ToList();



                var TakenRecords =Paginate.Pagination(FilterRecords,filter);



                responseVm.Data = TakenRecords.Select(x => new GetAllTax
                {
                    TaxRateId = x.TaxRateId,
                    TaxName = x.TaxName,
                    Municipality = x.Municipality,
                    State = x.State,
                    City = x.City,
                    IsActive = x.IsActive,
                    TenantId = x.TenantId,
                }).ToList();


                responseVm.IsSuccess = true;
                responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                responseVm.TotalRecords = FilterRecords.Count;


            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateTax(UpdateTax model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExist = tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExist == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var IsTaxExist = GetAllQuerable()
                 .FirstOrDefault(x => x.TaxName.Trim() == model.TaxName.Trim() && x.IsDelete == false && x.IsActive == true && x.TenantId == claimsProperty.GetTenantId()
                 && x.TaxRateId != model.TaxRateId);

                if (IsTaxExist != null)
                {
                    responseVm.Message = "TaxName" + AppConstant.AlreadyExists;
                    return responseVm;
                }

                var TaxExist = GetAllQuerable().Where(x => x.TaxRateId == model.TaxRateId && x.IsDelete == false).FirstOrDefault();
            
                if (TaxExist != null)
                {
                    TaxExist.TaxName = model.TaxName.Trim();
                    TaxExist.Municipality = model.Municipality;
                    TaxExist.State = model.State;
                    TaxExist.City = model.City;
                    TaxExist.UpdatedAt = DateTime.Now;
                    TaxExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = Update(TaxExist);
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
            return responseVm;
        }
    }
}
