using System;
using System.Collections.Generic;
using System.IO.Compression;
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
using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.GHL.Services;
using ShutterManagement.Services;
using ShutterManagment.GHL.ViewModel.Request;
using ShutterManagment.GHL.ViewModel.Response;
using Stripe;


namespace ShutterManagement.Business.Services
{
    public class GHLConfigurationService : BaseRepository<ShutterDBContext, TblGhlconfiguration>, IGHLConfiguration
    {
        private readonly ClaimsProperty claimsProperty;
        private readonly GoHighLevelOAuthService goHighLevelOAuthService;
        private readonly IConfiguration configuration;
        private readonly GHLService_v2 gHLService_V2;

        public GHLConfigurationService(ClaimsProperty claimsProperty, 
            GoHighLevelOAuthService goHighLevelOAuthService, IConfiguration configuration,
            GHLService_v2 gHLService_V2)
        {
            this.claimsProperty = claimsProperty;
            this.goHighLevelOAuthService = goHighLevelOAuthService;
            this.configuration = configuration;
            this.gHLService_V2 = gHLService_V2;
        }

        //This method to set the GHLConfiguration in AppConstant...
        public async Task GetGHLConfiguration(string Id)
        {
            FileLogger.Log("details GHl ID: ", Id);
            var Checkghlresponse = await CheckGHLConfiguration();
            var details = GetAllQuerable()
                   .Where(x => x.LocationId == Id || x.TenantId.ToString().ToLower() == Id.ToLower() && x.IsDelete == false)
                   .FirstOrDefault();
            FileLogger.Log("details GHl: ", details);
            if (details != null)
            {
                AppConstant.GHL_BaseAddress = configuration["GoHighLevelOAuthSettings:BaseUrl"] ?? "";
                AppConstant.GHL_PipelineId = details.PipelineId ?? "";
                AppConstant.GHL_Locationd = details.LocationId??"";
                AppConstant.LoggedInTenantId = details.TenantId;
                AppConstant.GHL_AccessToken = details.AccessToken ?? "";
                AppConstant.GHL_RefreshToken = details.RefreshToken ??"";
                AppConstant.GHL_CalendarName = details.CalendarName ?? "";
                // Fetch ClientId and ClientSecret from appsettings.json
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
                   // IsGHLConfigurationTenantExist.Apikey = gHLConfiguration.APIkey??"";
                    IsGHLConfigurationTenantExist.LocationId = gHLConfiguration.LocationId ?? "";
                    IsGHLConfigurationTenantExist.PipelineId = gHLConfiguration.PipelineId??"";
                    //IsGHLConfigurationTenantExist.BaseAddress = gHLConfiguration.BaseAddress ?? "";
                    IsGHLConfigurationTenantExist.GeneratedToken = gHLConfiguration.GeneratedToken??"";
                    IsGHLConfigurationTenantExist.CalendarName = gHLConfiguration.CalendarName ?? "";
                    IsGHLConfigurationTenantExist.TenantId = AppConstant.LoggedInTenantId;

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
                       // BaseAddress = gHLConfiguration.BaseAddress??"",
                        LocationId = gHLConfiguration.LocationId ?? "",
                       // Apikey = gHLConfiguration.APIkey??"",
                        TenantId = AppConstant.LoggedInTenantId,
                        LastSync = DateTime.Now,
                        IsDelete = false,
                        IsActive = true,
                        GeneratedToken = gHLConfiguration.GeneratedToken ??"",
                        CalendarName = gHLConfiguration.CalendarName ?? "",
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
                            var resUpdate = await Update(gHLConfiguration);
                            responseVm.Message = "Ghl linked Successfully..";
                            FileLogger.Log($"Configuration Updated Successfully  in link to ghl {resUpdate}");
                        }
                        else
                        {
                            responseVm.Message = "Failed to retrieve refresh token.";
                            FileLogger.Log($"Failed to retrieve refresh token in link to ghl {responseVm}");
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
                    LocationId = gHLConfiguration?.LocationId ?? "",
                    PipelineId = gHLConfiguration?.PipelineId ?? "",
                    LastSync = gHLConfiguration?.LastSync ?? null,
                    CalendarName = gHLConfiguration?.CalendarName ?? "",
                    GeneratedToken = gHLConfiguration?.GeneratedToken ?? "",
                    Refresh_Token = gHLConfiguration?.RefreshToken ?? "",
                    Access_Token = gHLConfiguration?.AccessToken ?? "",
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
                var isExist = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete==false).FirstOrDefault();
                if (isExist != null)
                {
                    if(isExist.AccessToken !=null && isExist.RefreshToken!=null)
                    {
                        if (isExist.AccessExpirationtime > DateTime.Now && isExist.RefreshExpirationtime > DateTime.Now)
                        {
                            response.IsSuccess = true;
                            response.Message = "AccessToken and RefreshToken is valid.";
                        }
                        else if (isExist.RefreshExpirationtime > DateTime.Now)
                        {

                            var res1 = await goHighLevelOAuthService.GetAccessToken(isExist.RefreshToken);
                            if (res1 != null && res1?.Data?.AccessToken != null && res1?.Data?.RefreshToken != null)
                            {
                                isExist.AccessExpirationtime = DateTime.Now.AddDays(1);
                                isExist.RefreshExpirationtime = DateTime.Now.AddDays(365);
                                isExist.RefreshToken = res1?.Data?.RefreshToken ?? "";
                                isExist.AccessToken = res1?.Data.AccessToken ?? "";
                                isExist.LastSync = DateTime.Now;
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

        public Task<IResponseVm<string>> CheckStageMapping()
        {
           // get record check tblghlstagemapping not null
           IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                var isExist = dbContext.TblGhlStagesmappings.Where(x => x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (isExist != null)
                {
                    response.IsSuccess = true;
                    response.Message = "Stage Mapping Exist.";
                }
                else
                {
                    response.Message = "Stage Mapping Not Exist. Please Mapp Stages And Job Status";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Task.FromResult(response);
        }


        public async Task<IResponseVm<string>> UpdateGHlConfigforPartner(CreateGHLConfigforPartner model)
        {
            IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                var IsSubtenant = dbContext.TblTenants.Where(x => x.TenantId.ToString() == model.PartnerId
                && x.ParentTenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false).FirstOrDefault();
                if (IsSubtenant != null)
                {
                    IsSubtenant.IsGhlconfigSameAsTenant = model.IsGHLConfigSameasTenant;
                    IsSubtenant.GhluserId = model.GHLUserId;
                    IsSubtenant.ModifiedBy = AppConstant.LoggedInUserID;
                    IsSubtenant.UpdatedAt = DateTime.Now;

                    dbContext.TblTenants.Update(IsSubtenant);
                    dbContext.SaveChanges();

                    response.IsSuccess = true;
                    response.Message = "GHL User added successfully.";
                }
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;  
            }
            return response;
        }

        public async Task<IResponseVm<string>> SyncContactandJobforPartner(string TenantId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var subtenantdetails = dbContext.TblTenants.Where(x => x.TenantId.ToString() == TenantId
                && x.ParentTenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false).FirstOrDefault();

                if ((bool)subtenantdetails.IsGhlconfigSameAsTenant)
                {
                    await GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                    var ghlContact = await gHLService_V2.GetOpportunityforPartner(subtenantdetails.GhluserId);
                    foreach (var item in ghlContact.Data.opportunities)
                    {
                        var IsContactExist = dbContext.TblContacts.Where(x => x.Email == item.contact.Email && x.TenantId.ToString() == TenantId && x.IsArchive == false && x.IsActive == true && x.IsDelete
                        == false).FirstOrDefault();
                        if (IsContactExist != null)
                        {
                            IsContactExist.FirstName = item.contact.Name;
                            IsContactExist.LastName = item.contact.Name;
                            IsContactExist.PhoneNumber = item.contact.Phone;
                            IsContactExist.Email = item.contact.Email;
                            IsContactExist.UpdatedAt = DateTime.Now;
                            IsContactExist.ModifiedBy = AppConstant.LoggedInUserID;
                            dbContext.TblContacts.Update(IsContactExist);
                            await dbContext.SaveChangesAsync();
                        }
                        else
                        {
                            //generate unique version
                            int Version = dbContext.TblContacts.Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
                                .Select(p => (p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                            Version += 1;
                            var contactUniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Contact, AppConstant.LoggedInCompanyCode, Version);
                            TblContact CreateContact = new TblContact
                            {
                                ContactId = Guid.NewGuid(),
                                // ContactType = item.contact.,
                                FirstName = item.contact.Name,
                                LastName = item.contact.Name,
                                PhoneNumber = item.contact.Phone,
                                Email = item.contact.Email,
                                ContactUniqueId = contactUniqueId,
                                IsTaxExemption = false,
                                IsBillingAddressSameAsAddress = false,
                                CreatedBy = AppConstant.LoggedInUserID,
                                CreatedAt = DateTime.Now,
                                TenantId = Guid.Parse(TenantId),
                                IsActive = true,
                                IsArchive = false,
                                IsDelete = false
                            };
                            dbContext.TblContacts.Add(CreateContact);
                            var res = await dbContext.SaveChangesAsync();
                            if (CreateContact.LastSync == null && CreateContact.GhlcontactId == null)
                            {
                                CreateContact.LastSync = DateTime.Now;
                                CreateContact.GhlcontactId = item.contact.Id;
                                dbContext.TblContacts.Update(CreateContact);
                                await dbContext.SaveChangesAsync();
                            }
                            var jobExist = dbContext.TblJobs.Where(x => x.JobName == item.name && x.ContactId.ToString() == item.contactId.ToString()
                            && x.IsDelete == false && x.IsActive == true && x.TenantId.ToString() == TenantId).FirstOrDefault();
                            if (jobExist != null)
                            {
                                var stageId1 = dbContext.TblGhlStages.Where(x => x.GhlStageId.ToLower() == item.pipelineStageId.ToLower())
                                    .FirstOrDefault();
                                jobExist.JobName = item.name?.Trim();
                                jobExist.GhlStageId = Guid.Parse(item.pipelineStageId);
                                jobExist.GhlStageName = stageId1?.GhlStageName?.Trim() ?? string.Empty;
                                jobExist.GhluserId = subtenantdetails.GhluserId ?? string.Empty;
                                jobExist.GhlOpportunityId = item.id ?? string.Empty;
                                dbContext.TblJobs.Update(jobExist);
                                await dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                int Version1 = dbContext.TblJobs.Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
                                         .Select(p => (p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                                Version1 += 1;
                                var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, AppConstant.LoggedInCompanyCode, Version);
                                Guid jobtype = Guid.Parse("34623376-9CE3-4C9B-A538-2D3129B1A6B2");
                                var stageId = dbContext.TblGhlStages.Where(x => x.GhlStageId.ToLower() == item.pipelineStageId.ToLower())
                                    .FirstOrDefault();
                               var jobstatus = dbContext.MstJobStatuses.Where(x => x.JobStatusName.ToLower() == stageId.GhlStageName.ToLower()).Select(x => x.JobStatusId).FirstOrDefault();
                                TblJob job = new TblJob
                                {
                                    AddressId = null,
                                    ContactId = CreateContact.ContactId,
                                    JobName = item.name?.Trim(),
                                    JobType = jobtype,
                                    AssignedPersonnel = Guid.Empty,
                                    // ScheduledDate = model.ScheduledDate,
                                    JobStatus = jobstatus,
                                    Notes = string.Empty,
                                    JobUniqueId = generateUnique,
                                    GhlStageId = Guid.Parse(item.pipelineStageId),
                                    GhlStageName = stageId?.GhlStageName?.Trim() ?? string.Empty,
                                    GhlContactId = item.contactId ?? string.Empty,
                                    TenantId = Guid.Parse(TenantId),
                                    IsActive = true,
                                    IsDelete = false,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID ?? Guid.Empty,
                                    GhluserId = subtenantdetails.GhluserId ?? string.Empty,
                                    GhlOpportunityId = item.id ?? string.Empty
                                };
                                dbContext.TblJobs.Add(job);
                                await dbContext.SaveChangesAsync();
                                //responseVm.IsSuccess = true;
                                //responseVm.Message = AppConstant.InsertSuccessMsg;
                            }
                        }
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Contact and Opportunity synced successfully for Partner.";
                }

                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Please , Select GHL User.";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;

        }
    }
}
