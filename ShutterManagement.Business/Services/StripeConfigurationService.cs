using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.GHL.Services;
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

        public StripeConfigurationService(ITenantService tenantService,ClaimsProperty claimsProperty
            ){
                this.tenantService = tenantService;
                this.claimsProperty = claimsProperty;
        }



        public async Task<IResponseVm<string>> Create_OrUpdate_StripeConfiguration(CreateStripeConfiguration StripeConfiguration)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsStripeConfigurationTenantExist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete ==false).FirstOrDefault();
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
                        Id= Guid.NewGuid(),
                        SecretKey = StripeConfiguration.SecretKey,
                        PublishableKey = StripeConfiguration.PublishableKey,
                        SuccessUrl = StripeConfiguration.successUrl,
                        CancelUrl = StripeConfiguration.cancelUrl,
                        WebhookSecret=StripeConfiguration.WebhookSecret,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsDelete= false,
                        IsActive=true,
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
                var StripeConfigurationVm = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                responseVm.Data = new StripeConfigurationVm
                {
                    Id = StripeConfigurationVm?.Id,
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
                FileLogger.Log("Stripe configuration updated successfully AppConstant.Stripe_SecretKey .", AppConstant.Stripe_SecretKey);
                FileLogger.Log("Stripe configuration updated successfully AppConstant.Stripe_PublishableKey.", AppConstant.Stripe_PublishableKey);
                FileLogger.Log("Stripe configuration updated successfully AppConstant.Stripe_WebhookSecret .", AppConstant.Stripe_WebhookSecret);
            }
            else
            {
                FileLogger.Log("No Stripe configuration found for Tenant ID:", tenantId);
            }
        }


        public async Task<IResponseVm<string>> DeleteStripeConfiguration(Guid? configurationid)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                var IsStripeConfiExists = GetAllQuerable().Where(x => x.IsDelete == false && x.Id == configurationid &&
                x.TenantId == AppConstant.LoggedInTenantId
                ).FirstOrDefault();
                if(IsStripeConfiExists!=null)
                {
                    IsStripeConfiExists.IsActive = false;
                    IsStripeConfiExists.IsDelete = true;
                    IsStripeConfiExists.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsStripeConfiExists);
                    result.IsSuccess = true;
                    result.Message = AppConstant.DeleteSuccessMsg;
                }
                else
                {
                    result.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex)
            {
                result.Message = AppConstant.ExceptionError + ex.Message;
            }
            return result;
        }


       

    }
}


