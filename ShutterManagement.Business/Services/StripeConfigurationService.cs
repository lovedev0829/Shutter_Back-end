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



        public async Task<IResponseVm<string>> Create_OrUpdate_StripeConfiguration(CreateStripeConfiguration StripeConfiguration)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsStripeConfigurationTenantExist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (IsStripeConfigurationTenantExist != null)
                {
                    IsStripeConfigurationTenantExist.SecretKey = StripeConfiguration.SecretKey;
                    IsStripeConfigurationTenantExist.PublishableKey = StripeConfiguration.PublishableKey;
                    IsStripeConfigurationTenantExist.SuccessUrl = StripeConfiguration.successUrl;
                    IsStripeConfigurationTenantExist.CancelUrl = StripeConfiguration.cancelUrl;
                    IsStripeConfigurationTenantExist.WebhookSecret = StripeConfiguration.WebhookSecret;
                    IsStripeConfigurationTenantExist.ModifiedBy = AppConstant.LoggedInUserID;


                    var res = await Update(IsStripeConfigurationTenantExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    TblStripeConfiguration tblstripeconfiguration = new TblStripeConfiguration()
                    {
                        SecretKey = StripeConfiguration.SecretKey,
                        PublishableKey = StripeConfiguration.PublishableKey,
                        SuccessUrl = StripeConfiguration.successUrl,
                        CancelUrl = StripeConfiguration.cancelUrl,
                        WebhookSecret=StripeConfiguration.WebhookSecret,
                        TenantId = AppConstant.LoggedInTenantId,
                        CreatedAt = DateTime.Now,
                    };
                    var res1 = await Add(tblstripeconfiguration);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message += ex.Message;
            }
            return responseVm;
        }


        public async Task<IResponseVm<StripeConfigurationVm>> GetAll_StripeConfiguration()
        {
            IResponseVm<StripeConfigurationVm> responseVm = new ResponseVm<StripeConfigurationVm>();
            try
            {
                var StripeConfigurationVm = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                responseVm.Data = new StripeConfigurationVm
                {
                    //Id = StripeConfigurationVm.Id,
                    SecretKey = StripeConfigurationVm?.SecretKey??"",
                    PublishableKey = StripeConfigurationVm?.PublishableKey??"",
                    successUrl = StripeConfigurationVm?.SuccessUrl ?? "",
                    cancelUrl = StripeConfigurationVm?.CancelUrl??"",
                    WebhookSecret = StripeConfigurationVm?.WebhookSecret??""
                };
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message += ex.Message;
            }
            return responseVm;

        }


        public async Task GetStripeConfiguration(string tenantId)
        {
            FileLogger.Log("Fetching Stripe configuration for Tenant ID:", tenantId);

            var details = GetAllQuerable()
                   .Where(x => x.TenantId.ToString().ToLower() == tenantId.ToLower())
                   .FirstOrDefault();

            if (details != null)
            {
                AppConstant.Stripe_SecretKey = details.SecretKey ?? "";
                AppConstant.Stripe_PublishableKey = details.PublishableKey ?? "";
                AppConstant.Stripe_WebhookSecret = details.WebhookSecret ?? "";
                StripeConfiguration.ApiKey = AppConstant.Stripe_SecretKey;

                FileLogger.Log("Stripe configuration updated successfully.", tenantId);
            }
            else
            {
                FileLogger.Log("No Stripe configuration found for Tenant ID:", tenantId);
            }
        }
    }
    }


