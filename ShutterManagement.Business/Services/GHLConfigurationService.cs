using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;


namespace ShutterManagement.Business.Services
{
    public class GHLConfigurationService : BaseRepository<ShutterDBContext, TblGhlconfiguration>, IGHLConfiguration
    {
        private readonly ClaimsProperty claimsProperty;
        private readonly GoHighLevelOAuthService goHighLevelOAuthService;

        public GHLConfigurationService(ClaimsProperty claimsProperty, GoHighLevelOAuthService goHighLevelOAuthService)
        {
            this.claimsProperty = claimsProperty;
            this.goHighLevelOAuthService = goHighLevelOAuthService;
        }

        //This method to set the GHLConfiguration in AppConstant...
        public async Task GetGHLConfiguration(string Id)
        {
            FileLogger.Log("details GHl ID: ", Id);

            var details = GetAllQuerable()
                   .Where(x => x.LocationId == Id || x.TenantId.ToString().ToLower() == Id.ToLower())
                   .FirstOrDefault();
            FileLogger.Log("details GHl: ", details);
            if (details != null)
            {
                AppConstant.GHL_BaseAddress = details.BaseAddress;
                AppConstant.GHL_APIKey = details.Apikey;
                AppConstant.GHL_PipelineId = details.PipelineId;
                AppConstant.GHL_Locationd = details.LocationId;
                AppConstant.LoggedInTenantId = details.TenantId;
                AppConstant.GHL_AccessToken = details.AccessToken;
                AppConstant.GHL_RefreshToken = details.RefreshToken;
            }
        }

        public async Task<IResponseVm<string>> GHLConfiguration(GHLConfiguration gHLConfiguration)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsGHLConfigurationTenantExist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (IsGHLConfigurationTenantExist != null)
                {
                    IsGHLConfigurationTenantExist.Apikey = gHLConfiguration.APIkey;
                    IsGHLConfigurationTenantExist.LocationId = gHLConfiguration.LocationId;
                    IsGHLConfigurationTenantExist.PipelineId = gHLConfiguration.PipelineId;
                    IsGHLConfigurationTenantExist.BaseAddress = gHLConfiguration.BaseAddress;
                    IsGHLConfigurationTenantExist.GeneratedToken = gHLConfiguration.GeneratedToken;

                    var res = await Update(IsGHLConfigurationTenantExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    TblGhlconfiguration tblGhlconfiguration = new TblGhlconfiguration()
                    {
                        PipelineId = gHLConfiguration.PipelineId,
                        BaseAddress = gHLConfiguration.BaseAddress,
                        LocationId = gHLConfiguration.LocationId,
                        Apikey = gHLConfiguration.APIkey,
                        TenantId = AppConstant.LoggedInTenantId,
                        LastSync = DateTime.Now,
                        GeneratedToken = gHLConfiguration.GeneratedToken,
                    };
                    var res1 = await Add(tblGhlconfiguration);
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


        //This method is to get the GHL configuration...
        public async Task<IResponseVm<GetGHLConfiguration>> GetGHLConfigurations(string? code)
        {
            IResponseVm<GetGHLConfiguration> responseVm = new ResponseVm<GetGHLConfiguration>();
            try
            {
                var gHLConfiguration = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (code != null)
                {
                    var res = goHighLevelOAuthService.GetRefreshToken(code);
                    if (res != null)
                    {
                        gHLConfiguration.RefreshToken = res.Result.RefreshToken;
                        gHLConfiguration.AccessToken = res.Result.AccessToken;
                        gHLConfiguration.AccessExpirationtime = DateTime.Now.AddDays(1);
                        gHLConfiguration.RefreshExpirationtime = DateTime.Now.AddDays(365);
                        await Update(gHLConfiguration);
                    }
                }
                responseVm.Data = new GetGHLConfiguration
                {
                    APIkey = gHLConfiguration?.Apikey ?? "",
                    BaseAddress = gHLConfiguration?.BaseAddress ?? "",
                    LocationId = gHLConfiguration?.LocationId ?? "",
                    PipelineId = gHLConfiguration?.PipelineId ?? "",
                    LastSync = gHLConfiguration?.LastSync ?? DateTime.Now,
                    GeneratedToken = gHLConfiguration?.GeneratedToken ?? "",
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

        public async Task<IResponseVm<string>> GetOauthGHL()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var res = goHighLevelOAuthService.GetAuthorizationUrl();
                if (res != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Data = res;
                    responseVm.Message = "Authorization URL for Oauth2.0";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message += ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> CheckGHLConfiguration()
        {
            IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                bool res = false;
                var isExist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (isExist != null)
                {
                    if (isExist.AccessExpirationtime > DateTime.Now && isExist.RefreshExpirationtime > DateTime.Now)
                    {
                        response.IsSuccess = true;
                        response.Message = "AccessToken and RefreshToken is valid.";
                    }
                    else
                    {
                        if(isExist.AccessExpirationtime > DateTime.Now)
                        {
                            var res1 = goHighLevelOAuthService.GetAccessToken(isExist.RefreshToken);
                            if (res != null)
                            {
                                isExist.AccessExpirationtime = DateTime.Now.AddDays(1);
                                isExist.RefreshExpirationtime = DateTime.Now.AddDays(365);
                                isExist.RefreshToken = res1.Result.RefreshToken;
                                isExist.AccessToken = res1.Result.AccessToken;
                                await Update(isExist);
                            }
                            response.IsSuccess = true;
                            response.Message = "AccessToken Generated.";
                        }
                        else if(isExist.RefreshExpirationtime > DateTime.Now)
                        {
                            response.IsSuccess = false;
                            response.Message = "User need to verify GoHighLevelConfiguration.";
                        }
                    }
                }
                else
                {
                    response.IsSuccess = false;
                }
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
