using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration configuration;

        public GHLConfigurationService(ClaimsProperty claimsProperty, GoHighLevelOAuthService goHighLevelOAuthService, IConfiguration configuration)
        {
            this.claimsProperty = claimsProperty;
            this.goHighLevelOAuthService = goHighLevelOAuthService;
            this.configuration = configuration;
        }

        //This method to set the GHLConfiguration in AppConstant...
        public async Task GetGHLConfiguration(string Id)
        {
            FileLogger.Log("details GHl ID: ", Id);
            var Checkghlresponse = await CheckGHLConfiguration();
            var details = GetAllQuerable()
                   .Where(x => x.LocationId == Id || x.TenantId.ToString().ToLower() == Id.ToLower())
                   .FirstOrDefault();
            FileLogger.Log("details GHl: ", details);
            if (details != null)
            {
                AppConstant.GHL_BaseAddress = configuration["GoHighLevelOAuthSettings:BaseUrl"] ?? "";
                AppConstant.GHL_APIKey = details.Apikey??"";
                AppConstant.GHL_PipelineId = details.PipelineId ?? "";
                AppConstant.GHL_Locationd = details.LocationId??"";
                AppConstant.LoggedInTenantId = details.TenantId;
                AppConstant.GHL_AccessToken = details.AccessToken ?? "";
                AppConstant.GHL_RefreshToken = details.RefreshToken ??"";
                AppConstant.GHL_CalendarName = details.CalendarName ?? "";
                // Fetch ClientId and ClientSecret from appsettings.json
                AppConstant.GHL_ClientId = AppConstant.GHL_Marketplace_ClientId ?? "";
                AppConstant.GHL_ClientSecret = AppConstant.GHL_Marketplace_ClientSecret ?? "";
                AppConstant.GHL_RedirectUri = AppConstant.GHL_Marketplace_RedirectUri ?? "";
            }
            
        }

        public async Task<IResponseVm<string>> GHLConfiguration(GHLConfiguration gHLConfiguration)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsGHLConfigurationTenantExist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                if (IsGHLConfigurationTenantExist != null)
                {
                    IsGHLConfigurationTenantExist.Apikey = gHLConfiguration.APIkey??"";
                    IsGHLConfigurationTenantExist.LocationId = gHLConfiguration.LocationId ?? "";
                    IsGHLConfigurationTenantExist.PipelineId = gHLConfiguration.PipelineId??"";
                    IsGHLConfigurationTenantExist.BaseAddress = gHLConfiguration.BaseAddress ?? "";
                    IsGHLConfigurationTenantExist.GeneratedToken = gHLConfiguration.GeneratedToken??"";
                    IsGHLConfigurationTenantExist.CalendarName = gHLConfiguration.CalendarName ?? "";
                    IsGHLConfigurationTenantExist.TenantId = AppConstant.LoggedInTenantId;
                    AppConstant.GHL_ClientId = configuration["GoHighLevelOAuthSettings:ClientId"] ?? "";
                    AppConstant.GHL_ClientSecret =configuration["GoHighLevelOAuthSettings:ClientSecret"] ?? "";
                    AppConstant.GHL_RedirectUri = configuration["GHLSettings:RedirectUri"] ?? "";

                    var res = await Update(IsGHLConfigurationTenantExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    TblGhlconfiguration tblGhlconfiguration = new TblGhlconfiguration()
                    {
                        Id=Guid.NewGuid(),
                        PipelineId = gHLConfiguration.PipelineId ?? "",
                        BaseAddress = gHLConfiguration.BaseAddress??"",
                        LocationId = gHLConfiguration.LocationId ?? "",
                        Apikey = gHLConfiguration.APIkey??"",
                        TenantId = AppConstant.LoggedInTenantId,
                        LastSync = DateTime.Now,
                        IsDelete = false,
                        IsActive = true,
                        GeneratedToken = gHLConfiguration.GeneratedToken ??"",
                        CalendarName = gHLConfiguration.CalendarName ?? "",
                        ClientId = configuration["GoHighLevelOAuthSettings:ClientId"] ?? "",
                        ClientSecret = configuration["GoHighLevelOAuthSettings:ClientSecret"] ?? "",
                        RedirectUri = configuration["GHLSettings:RedirectUri"] ?? "",
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
                var gHLConfiguration = GetAllQuerable().Where(x=>x.TenantId==AppConstant.LoggedInTenantId && x.IsDelete==false).FirstOrDefault();

                if (gHLConfiguration == null)
                {
                    responseVm.Message = "Configuration not found for the current tenant.";
                    return responseVm; 
                }

                if (code != null)
                {
                    try
                    {
                        var res = await goHighLevelOAuthService.GetRefreshToken(code);
                        FileLogger.Log($"This is the Refresh Token Rsponse{res}");
                        if (res != null)
                        {
                            // Update the gHLConfiguration with new tokens and expiration times
                            gHLConfiguration.RefreshToken = res.Data?.RefreshToken ?? "";
                            gHLConfiguration.AccessToken = res.Data?.AccessToken ?? "";
                            gHLConfiguration.AccessExpirationtime = DateTime.Now.AddDays(1);
                            gHLConfiguration.RefreshExpirationtime = DateTime.Now.AddDays(365);
                            gHLConfiguration.TenantId= AppConstant.LoggedInTenantId;
                            gHLConfiguration.LastSync = DateTime.Now;

                            // Save the updated configuration
                            await Update(gHLConfiguration);
                            FileLogger.Log($"Configuration Updated Successfully  in link to ghl");
                        }
                        else
                        {
                            responseVm.Message = "Failed to retrieve refresh token.";
                            FileLogger.Log($"Failed to retrieve refresh token in link to ghl{responseVm}");
                            return responseVm;
                        }
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Log($"Failed to retrieve refresh token in link to ghl {ex.Message}");
                        responseVm.Message = $"An error occurred while retrieving the refresh token: {ex.Message}";
                        return responseVm;
                    }
                }
                responseVm.Data = new GetGHLConfiguration
                {
                    Id= gHLConfiguration.Id,
                    APIkey = gHLConfiguration?.Apikey ?? "",
                    BaseAddress = gHLConfiguration?.BaseAddress ?? "",
                    LocationId = gHLConfiguration?.LocationId ?? "",
                    PipelineId = gHLConfiguration?.PipelineId ?? "",
                    LastSync = gHLConfiguration?.LastSync ?? null,
                    CalendarName = gHLConfiguration?.CalendarName ?? "",
                    GeneratedToken = gHLConfiguration?.GeneratedToken ?? "",
                    ClientId = gHLConfiguration?.ClientId ?? "",
                    ClientSecret = gHLConfiguration?.ClientSecret ?? "",
                    RedirectURI = gHLConfiguration?.RedirectUri ?? "",
                };
                FileLogger.Log($"Failed to retrieve refresh token in link to ghl{responseVm?.Data}");

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An unexpected error occurred: {ex.Message}";
                // Optionally log the error for further investigation
                // logger.LogError(ex, "Error in GetGHLConfigurations API");
                FileLogger.Log($"Failed to retrieve refresh token in link to ghl {ex.Message}");
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> GetOauthGHL()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                await GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
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
                var isExist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete==false).FirstOrDefault();
                if (isExist != null)
                {
                    if (isExist.AccessExpirationtime > DateTime.Now && isExist.RefreshExpirationtime > DateTime.Now)
                    {
                        response.IsSuccess = true;
                        response.Message = "AccessToken and RefreshToken is valid.";
                    }
                    else if(isExist.RefreshExpirationtime > DateTime.Now)
                        {
                            //if(isExist.AccessExpirationtime > DateTime.Now) {
                            //    response.IsSuccess = true;
                            //    response.Message = "AccessToken and RefreshToken is valid.";
                            //}

                            var res1 = goHighLevelOAuthService.GetAccessToken(isExist.RefreshToken);
                            if (res1 != null)
                            {
                                isExist.AccessExpirationtime = DateTime.Now.AddDays(1);
                                isExist.RefreshExpirationtime = DateTime.Now.AddDays(365);
                                isExist.RefreshToken = res1?.Result?.Data?.RefreshToken??"";
                                isExist.AccessToken = res1?.Result?.Data?.AccessToken ?? "";
                                isExist.LastSync= DateTime.Now;
                                await Update(isExist);
                            }
                            response.IsSuccess = true;
                            response.Message = "AccessToken Generated.";
                        }
                        else
                        {
                            response.Message = "User need to verify GoHighLevelConfiguration.";
                        }
                }
                else
                {
                    response.Message = "GoHighLevel Configuration. Not Found.";
                }
                
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<IResponseVm<string>> MapGHLStagesStages(StageMappingRequest request)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                foreach (var stagemapping in request.Mappings)
                {
                    var existingMapping = dbContext.TblGhlStagesmappings
                        .FirstOrDefault(x =>
                            x.TenantId == AppConstant.LoggedInTenantId &&
                            x.StatusId == stagemapping.StatusId);

                    if (existingMapping != null)
                    {
                        dbContext.TblGhlStagesmappings.Remove(existingMapping);
                    }

                    TblGhlStagesmapping newMapping = new TblGhlStagesmapping()
                    {
                        Id = Guid.NewGuid(),
                        StageId = stagemapping.StageId ?? Guid.Empty,
                        StatusId = stagemapping.StatusId ?? Guid.Empty,
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId
                    };

                    await dbContext.TblGhlStagesmappings.AddAsync(newMapping);
                }

                await dbContext.SaveChangesAsync();

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message += ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<StageMapping>>> GetGHLStagesMappings()
        {
            IResponseVm<List<StageMapping>> responseVm = new ResponseVm<List<StageMapping>>();

            try
            {
                var mappings =  dbContext.TblGhlStagesmappings
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId)
                    .Select(x => new StageMapping
                    {
                        StageId = x.StageId??Guid.Empty,
                        StatusId = x.StatusId ?? Guid.Empty,
                        CreatedAt = x.CreatedAt
                    }).OrderBy(x => x.CreatedAt)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Message = "Mappings fetched successfully.";
                responseVm.Data = mappings;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<StageMapping>> GetGHLStagesMappingsById(Guid statusId)
        {
            IResponseVm<StageMapping> responseVm = new ResponseVm<StageMapping>();

            try
            {
                var mapping = dbContext.TblGhlStagesmappings
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.StatusId == statusId)
                    .Select(x => new StageMapping
                    {
                        StageId = x.StageId ?? Guid.Empty,
                        StatusId = x.StatusId ?? Guid.Empty,
                        CreatedAt = x.CreatedAt
                    }).FirstOrDefault();

                if (mapping != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Mapping fetched successfully.";
                    responseVm.Data = mapping;
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Mapping not found.";
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteGHLConfiguration(Guid? configurationid)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                var IsStripeConfiExists = GetAllQuerable().Where(x => x.IsDelete == false && x.Id == configurationid &&
                x.TenantId == AppConstant.LoggedInTenantId
                ).FirstOrDefault();
                if (IsStripeConfiExists != null)
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
