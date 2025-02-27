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
        public class StripeConfigurationService : BaseRepository<ShutterDBContext, TblStripeConfiguration>, IStripeConfiguration
        {
            private readonly ITenantService tenantService;
            private readonly ClaimsProperty claimsProperty;

        public StripeConfigurationService(ITenantService tenantService,ClaimsProperty claimsProperty){
                this.tenantService = tenantService;
                this.claimsProperty = claimsProperty;
        }

            public async Task<IResponseVm<string>> Create_StripeConfiguration(CreateStripeConfiguration model)
            {
                IResponseVm<string> responseVm = new ResponseVm<string>();

                try
                {
                    var isTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);
                    if (!isTenant)
                    {
                        responseVm.Message = "Tenant not found";
                        responseVm.IsSuccess = false;
                        return responseVm;
                    }

                    var stripeData = new TblStripeConfiguration
                    {
                        Id = Guid.NewGuid(),
                        SecretKey = model.SecretKey,
                        PublishableKey = model.PublishableKey,
                        CancelUrl = model.cancelUrl,
                        SuccessUrl = model.successUrl,
                        IsDelete = false,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId,
                        CreatedBy = AppConstant.LoggedInUserID,
                    };

                    var result = Add(stripeData);

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

            public async Task<IResponseVm<string>> DeleteStripeConfiguration(Guid? id)
            {
                IResponseVm<string> responseVm = new ResponseVm<string>();

                try
                {
                    if (id == null)
                    {
                        responseVm.Message = "Invalid Stripe Configuration ID";
                        responseVm.IsSuccess = false;
                        return responseVm;
                    }

                    var stripeConfig = GetAllQuerable().Where(x => x.Id == id && x.IsActive==true && x.IsDelete==false && x.TenantId==AppConstant.LoggedInTenantId).FirstOrDefault() ;
                    if (stripeConfig == null)
                    {
                        responseVm.Message = "Stripe Configuration not found";
                        responseVm.IsSuccess = false;
                        return responseVm;
                    }

                    stripeConfig.IsDelete = true;
                    stripeConfig.IsActive = false;
                    stripeConfig.UpdatedAt = DateTime.Now;
                    stripeConfig.ModifiedBy = AppConstant.LoggedInUserID;

                    await Update(stripeConfig);

                    responseVm.Message = AppConstant.DeleteSuccessMsg;
                    responseVm.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                    responseVm.IsSuccess = false;
                }

                return responseVm;
            }

            public async Task<IResponseVm<List<StripeConfigurationVm>>> GetAll_StripeConfiguration(Pagination filter)
            {
                IResponseVm<List<StripeConfigurationVm>> responseVm = new ResponseVm<List<StripeConfigurationVm>>();

                try
                {
                    var isTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

                    if (isTenant)
                    {
                        var stripeList = (from stripe in dbContext.TblStripeConfigurations.OrderByDescending(x => x.CreatedAt)
                                          where stripe.TenantId == AppConstant.LoggedInTenantId && stripe.IsDelete==false
                                          select new StripeConfigurationVm
                                          {
                                              Id = stripe.Id,
                                              SecretKey = stripe.SecretKey,
                                              PublishableKey = stripe.PublishableKey,
                                              cancelUrl = stripe.CancelUrl,
                                              successUrl = stripe.SuccessUrl,
                                              IsActive = stripe.IsActive,
                                          }).ToList();

                        filter.SearchValue = filter.SearchValue?.Trim().ToLower();

                        var filteredRecords = stripeList
                            .Where(x =>
                                (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                    x.Id.ToString().Contains(filter.SearchValue) ||
                                    (x.SecretKey ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.PublishableKey ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.cancelUrl ?? "").ToLower().Contains(filter.SearchValue) ||
                                    (x.successUrl ?? "").ToLower().Contains(filter.SearchValue))
                            ).ToList();

                        var takenRecords = filteredRecords
                            .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                            .Take(filter.RecordPerPage)
                            .ToList();

                        responseVm.Data = takenRecords;
                        responseVm.TotalRecords = filteredRecords.Count;
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.FetchSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.AccessDenied;
                        responseVm.IsSuccess = false;
                    }
                }
                catch (Exception ex)
                {
                    responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                    responseVm.IsSuccess = false;
                }

                return responseVm;
            }


            public async Task<IResponseVm<string>> Update_StripeConfiguration(UpdateStripeConfiguration model)
            {
                IResponseVm<string> responseVm = new ResponseVm<string>();

                try
                {
                var stripeConfig = GetAllQuerable().Where(x => x.Id == model.Id && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (stripeConfig == null)
                {
                    responseVm.Message = "Stripe Configuration not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                    stripeConfig.SecretKey = model.SecretKey;
                    stripeConfig.PublishableKey = model.PublishableKey;
                    stripeConfig.CancelUrl = model.cancelUrl;
                    stripeConfig.SuccessUrl = model.successUrl;
                    stripeConfig.UpdatedAt = DateTime.Now;
                    stripeConfig.ModifiedBy = AppConstant.LoggedInUserID;

                    await  Update(stripeConfig);

                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                    responseVm.IsSuccess = false;
                }

                return responseVm;
            }
        }
    }


