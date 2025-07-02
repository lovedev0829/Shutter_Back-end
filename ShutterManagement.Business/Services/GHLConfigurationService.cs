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
            var Checkghlresponse = await CheckGHLConfiguration(Guid.Parse(Id));
            var details = GetAllQuerable()
                   .Where(x => x.LocationId == Id || x.TenantId.ToString().ToLower() == Id.ToLower() && x.IsDelete == false)
                   .FirstOrDefault();
            FileLogger.Log("details GHl: ", details);
            if (details != null)
            {
                AppConstant.GHL_BaseAddress = configuration["GoHighLevelOAuthSettings:BaseUrl"] ?? "";
                AppConstant.GHL_PipelineId = details.PipelineId ?? "";
                AppConstant.GHL_LocationId = details.LocationId??"";
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
                var IsGHLConfigurationTenantExist = GetAllQuerable().Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false).FirstOrDefault();
                if (IsGHLConfigurationTenantExist != null)
                {
                   // IsGHLConfigurationTenantExist.Apikey = gHLConfiguration.APIkey??"";
                    IsGHLConfigurationTenantExist.LocationId = gHLConfiguration.LocationId ?? "";
                    IsGHLConfigurationTenantExist.PipelineId = gHLConfiguration.PipelineId??"";
                    //IsGHLConfigurationTenantExist.BaseAddress = gHLConfiguration.BaseAddress ?? "";
                    IsGHLConfigurationTenantExist.GeneratedToken = gHLConfiguration.GeneratedToken??"";
                    IsGHLConfigurationTenantExist.CalendarName = gHLConfiguration.CalendarName ?? "";
                    IsGHLConfigurationTenantExist.TenantId = claimsProperty.GetTenantId();

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
                        TenantId = claimsProperty.GetTenantId(),
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
                var gHLConfiguration = GetAllQuerable().Where(x=>x.TenantId==claimsProperty.GetTenantId() && x.IsDelete==false).FirstOrDefault();

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
                            gHLConfiguration.TenantId= claimsProperty.GetTenantId();
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
                bool? isConnected = false;
                if (gHLConfiguration.RefreshExpirationtime.HasValue)
                {
                    isConnected = gHLConfiguration.RefreshExpirationtime.Value > DateTime.Now;
                }
                responseVm.Data = new GetGHLConfiguration
                {
                    Id = gHLConfiguration.Id,
                    LocationId = gHLConfiguration?.LocationId ?? "",
                    PipelineId = gHLConfiguration?.PipelineId ?? "",
                    LastSync = gHLConfiguration?.LastSync ?? null,
                    CalendarName = gHLConfiguration?.CalendarName ?? "",
                    GeneratedToken = gHLConfiguration?.GeneratedToken ?? "",
                    Refresh_Token = gHLConfiguration?.RefreshToken ?? "",
                    Access_Token = gHLConfiguration?.AccessToken ?? "",
                    RefreshExpirationtime = gHLConfiguration?.RefreshExpirationtime ?? null,
                    AccessExpirationtime = gHLConfiguration?.AccessExpirationtime ?? null,
                    IsConnected = isConnected ?? false,
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
                await GetGHLConfiguration(claimsProperty.GetTenantId().ToString());
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

        public async Task<IResponseVm<string>> CheckGHLConfiguration(Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
                var isExist = GetAllQuerable().Where(x => x.TenantId == LoggedInTenantId && x.IsDelete==false).FirstOrDefault();
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
                            x.TenantId == claimsProperty.GetTenantId() &&
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
                        TenantId = claimsProperty.GetTenantId()
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

        public async Task<IResponseVm<List<StageMapping>>> GetGHLStagesMappings(Guid? LoggedInTenantId = null)
        {
            IResponseVm<List<StageMapping>> responseVm = new ResponseVm<List<StageMapping>>();

            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

                var mappings =  dbContext.TblGhlStagesmappings
                    .Where(x => x.TenantId == LoggedInTenantId)
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
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.StatusId == statusId)
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
                x.TenantId == claimsProperty.GetTenantId()
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

        public Task<IResponseVm<string>> CheckStageMapping(Guid? LoggedInTenantId = null)
        {
           // get record check tblghlstagemapping not null
           IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

                var isExist = dbContext.TblGhlStagesmappings.Where(x => x.TenantId == LoggedInTenantId).FirstOrDefault();
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
                && x.ParentTenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false).FirstOrDefault();
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
                else
                {
                    response.Message = AppConstant.NoDataFound;
                }
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;  
            }
            return response;
        }




        //public async Task<IResponseVm<string>> SyncContactandJobforPartner(string TenantId)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        var subtenantdetails = dbContext.TblTenants.Where(x => x.TenantId.ToString() == TenantId
        //        && x.ParentTenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false).FirstOrDefault();

        //        if ((bool)subtenantdetails.IsGhlconfigSameAsTenant)
        //        {
        //            await GetGHLConfiguration(claimsProperty.GetTenantId().ToString());
        //            var ghlContact = await gHLService_V2.GetOpportunityforPartner(subtenantdetails.GhluserId);
        //            foreach (var item in ghlContact.Data.opportunities)
        //            {
        //                var IsContactExist = dbContext.TblContacts.Where(x => x.Email == item.contact.Email && x.TenantId.ToString() == TenantId && x.IsArchive == false && x.IsActive == true && x.IsDelete
        //                == false).FirstOrDefault();
        //                string fullName = item.contact.Name?.Trim() ?? "";
        //                string[] nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //                string firstName = nameParts.Length > 0 ? nameParts[0] : "";
        //                string lastName = nameParts.Length > 1
        //                    ? string.Join(' ', nameParts.Skip(1))
        //                    : "";

        //                if (IsContactExist != null)
        //                {
        //                    IsContactExist.FirstName = firstName;
        //                    IsContactExist.LastName = lastName;
        //                    IsContactExist.PhoneNumber = item.contact.Phone;
        //                    IsContactExist.Email = item.contact.Email;
        //                    IsContactExist.UpdatedAt = DateTime.Now;
        //                    IsContactExist.ModifiedBy = AppConstant.LoggedInUserID;
        //                    dbContext.TblContacts.Update(IsContactExist);
        //                    await dbContext.SaveChangesAsync();
        //                    var jobExist = dbContext.TblJobs.Where(x => x.JobName == item.name && x.ContactId.ToString() == item.contactId.ToString()
        //                   && x.IsDelete == false && x.IsActive == true && x.TenantId.ToString() == TenantId).FirstOrDefault();
        //                    if (jobExist != null)
        //                    {
        //                        var stageId1 = dbContext.TblGhlStages.Where(x => x.GhlStageId.ToLower() == item.pipelineStageId.ToLower())
        //                            .FirstOrDefault();
        //                        jobExist.JobName = item.name?.Trim();
        //                        jobExist.GhlStageId = Guid.Parse(item.pipelineStageId);
        //                        jobExist.GhlStageName = stageId1?.GhlStageName?.Trim() ?? string.Empty;
        //                        jobExist.GhluserId = subtenantdetails.GhluserId ?? string.Empty;
        //                        jobExist.GhlOpportunityId = item.id ?? string.Empty;
        //                        dbContext.TblJobs.Update(jobExist);
        //                        await dbContext.SaveChangesAsync();
        //                    }
        //                    else
        //                    {
        //                        int Version1 = dbContext.TblJobs.Where(x => x.TenantId == claimsProperty.GetTenantId() && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
        //                                 .Select(p => (p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
        //                        Version1 += 1;
        //                        var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, claimsProperty.GetCompanyCode(), Version1);
        //                        Guid jobtype = Guid.Parse("34623376-9CE3-4C9B-A538-2D3129B1A6B2");
        //                        var stageId = dbContext.TblGhlStages.Where(x => x.GhlStageId.ToLower() == item.pipelineStageId.ToLower())
        //                            .FirstOrDefault();
        //                        var jobstatus = dbContext.MstJobStatuses.Where(x => x.JobStatusName.ToLower() == stageId.GhlStageName.ToLower()).Select(x => x.JobStatusId).FirstOrDefault();
        //                        TblJob job = new TblJob
        //                        {
        //                            AddressId = null,
        //                            ContactId = IsContactExist.ContactId,
        //                            JobName = item.name?.Trim(),
        //                            JobType = jobtype,
        //                            AssignedPersonnel = null,
        //                            ScheduledDate = null,
        //                            JobStatus = jobstatus,
        //                            Notes = null,
        //                            JobUniqueId = generateUnique,
        //                            GhlStageId = Guid.Parse(item.pipelineStageId),
        //                            GhlStageName = stageId?.GhlStageName?.Trim() ?? string.Empty,
        //                            GhlContactId = item.contactId ?? string.Empty,
        //                            TenantId = Guid.Parse(TenantId),
        //                            IsActive = true,
        //                            IsDelete = false,
        //                            CreatedAt = DateTime.Now,
        //                            CreatedBy = AppConstant.LoggedInUserID ?? Guid.Empty,
        //                            GhluserId = subtenantdetails.GhluserId ?? string.Empty,
        //                            GhlOpportunityId = item.id ?? string.Empty,
        //                            AttachmentAny = null
        //                        };
        //                        dbContext.TblJobs.Add(job);
        //                        await dbContext.SaveChangesAsync();
        //                        //responseVm.IsSuccess = true;
        //                        //responseVm.Message = AppConstant.InsertSuccessMsg;
        //                    }
        //                }

        //                else
        //                {
        //                    //generate unique version
        //                    int Version = dbContext.TblContacts.Where(x => x.TenantId == claimsProperty.GetTenantId() && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
        //                        .Select(p => (p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.ContactUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
        //                    Version += 1;
        //                    var contactUniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Contact, claimsProperty.GetCompanyCode(), Version);
        //                    string fullName1 = item.contact.Name?.Trim() ?? "";
        //                    string[] nameParts1 = fullName1.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //                    string firstName1 = nameParts1.Length > 0 ? nameParts[0] : "";
        //                    string lastName1 = nameParts1.Length > 1
        //                        ? string.Join(' ', nameParts1.Skip(1))
        //                        : "";

        //                    TblContact CreateContact = new TblContact
        //                    {
        //                        ContactId = Guid.NewGuid(),
        //                        // ContactType = item.contact.,
        //                        FirstName = firstName1.Trim(),
        //                        LastName = lastName1.Trim(),
        //                        PhoneNumber = item.contact.Phone,
        //                        Email = item.contact.Email,
        //                        ContactUniqueId = contactUniqueId,
        //                        IsTaxExemption = false,
        //                        IsBillingAddressSameAsAddress = false,
        //                        CreatedBy = AppConstant.LoggedInUserID,
        //                        CreatedAt = DateTime.Now,
        //                        TenantId = Guid.Parse(TenantId),
        //                        IsActive = true,
        //                        IsArchive = false,
        //                        IsDelete = false
        //                    };
        //                    dbContext.TblContacts.Add(CreateContact);
        //                    var res = await dbContext.SaveChangesAsync();
        //                    if (CreateContact.LastSync == null && CreateContact.GhlcontactId == null)
        //                    {
        //                        CreateContact.LastSync = DateTime.Now;
        //                        CreateContact.GhlcontactId = item.contact.Id;
        //                        dbContext.TblContacts.Update(CreateContact);
        //                        await dbContext.SaveChangesAsync();
        //                    }
        //                    var jobExist = dbContext.TblJobs.Where(x => x.JobName == item.name && x.ContactId.ToString() == item.contactId.ToString()
        //                    && x.IsDelete == false && x.IsActive == true && x.TenantId.ToString() == TenantId).FirstOrDefault();
        //                    if (jobExist != null)
        //                    {
        //                        var stageId1 = dbContext.TblGhlStages.Where(x => x.GhlStageId.ToLower() == item.pipelineStageId.ToLower())
        //                            .FirstOrDefault();
        //                        jobExist.JobName = item.name?.Trim();
        //                        jobExist.GhlStageId = Guid.Parse(item.pipelineStageId);
        //                        jobExist.GhlStageName = stageId1?.GhlStageName?.Trim() ?? string.Empty;
        //                        jobExist.GhluserId = subtenantdetails.GhluserId ?? string.Empty;
        //                        jobExist.GhlOpportunityId = item.id ?? string.Empty;
        //                        dbContext.TblJobs.Update(jobExist);
        //                        await dbContext.SaveChangesAsync();
        //                    }
        //                    else
        //                    {
        //                        int Version1 = dbContext.TblJobs.Where(x => x.TenantId == claimsProperty.GetTenantId() && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
        //                                 .Select(p => (p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
        //                        Version1 += 1;
        //                        var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, claimsProperty.GetCompanyCode(), Version);
        //                        Guid jobtype = Guid.Parse("34623376-9CE3-4C9B-A538-2D3129B1A6B2");
        //                        var stageId = dbContext.TblGhlStages.Where(x => x.GhlStageId.ToLower() == item.pipelineStageId.ToLower())
        //                            .FirstOrDefault();
        //                       var jobstatus = dbContext.MstJobStatuses.Where(x => x.JobStatusName.ToLower() == stageId.GhlStageName.ToLower()).Select(x => x.JobStatusId).FirstOrDefault();
        //                        TblJob job = new TblJob
        //                        {
        //                            AddressId = null,
        //                            ContactId = CreateContact.ContactId,
        //                            JobName = item.name?.Trim(),
        //                            JobType = jobtype,
        //                            AssignedPersonnel = null,
        //                            ScheduledDate = null,
        //                            JobStatus = jobstatus,
        //                            Notes = null,
        //                            JobUniqueId = generateUnique,
        //                            GhlStageId = Guid.Parse(item.pipelineStageId),
        //                            GhlStageName = stageId?.GhlStageName?.Trim() ?? string.Empty,
        //                            GhlContactId = item.contactId ?? string.Empty,
        //                            TenantId = Guid.Parse(TenantId),
        //                            IsActive = true,
        //                            IsDelete = false,
        //                            CreatedAt = DateTime.Now,
        //                            CreatedBy = AppConstant.LoggedInUserID ?? Guid.Empty,
        //                            GhluserId = subtenantdetails.GhluserId ?? string.Empty,
        //                            GhlOpportunityId = item.id ?? string.Empty,
        //                            AttachmentAny = null
        //                        };
        //                        dbContext.TblJobs.Add(job);
        //                        await dbContext.SaveChangesAsync();
        //                        //responseVm.IsSuccess = true;
        //                        //responseVm.Message = AppConstant.InsertSuccessMsg;
        //                    }
        //                }
        //            }
        //            responseVm.IsSuccess = true;
        //            responseVm.Message = "Contact and Opportunity synced successfully for Partner.";
        //        }

        //        else
        //        {
        //            responseVm.IsSuccess = false;
        //            responseVm.Message = "Please , Select GHL User.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = ex.Message;
        //    }
        //    return responseVm;

        //}

        public async Task<IResponseVm<GetGhlConfigforPartner>> GetGHlConfigforPartner(string TenantId)
        {
            IResponseVm<GetGhlConfigforPartner> response = new ResponseVm<GetGhlConfigforPartner>();
            try
            {
                var IsSubtenant = dbContext.TblTenants.Where(x => x.TenantId.ToString() == TenantId
                && x.ParentTenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false)
                    .Select(x => new GetGhlConfigforPartner
                    {
                        IsGHLConfigSameasTenant = x.IsGhlconfigSameAsTenant,
                        GHLUserId = x.GhluserId,
                    }).FirstOrDefault();
                response.IsSuccess = true;
                response.Data = IsSubtenant;
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
           // GHLJobAssignedtoPartner ghlContact = new GHLJobAssignedtoPartner();
            try
            {
                var subtenant = dbContext.TblTenants.FirstOrDefault(x =>
                    x.TenantId.ToString() == TenantId &&
                    x.ParentTenantId == claimsProperty.GetTenantId() &&
                    x.IsActive == true && x.IsDelete == false);

                if (subtenant?.IsGhlconfigSameAsTenant == true)
                {
                    await GetGHLConfiguration(claimsProperty.GetTenantId().ToString());

                    var ghlContact = await gHLService_V2.GetOpportunityforPartner(subtenant.GhluserId);

                    foreach (var item in ghlContact.Data.opportunities)
                    {
                        var contact = await AddOrUpdateContact(item, TenantId);
                        await AddOrUpdateJob(item, contact.ContactId, subtenant, TenantId);
                    }

                    responseVm.IsSuccess = true;
                    responseVm.Message = "Contact and Opportunity synced successfully for Partner.";
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Please, select GHL User.";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }

            return responseVm;
        }
        private async Task<TblContact> AddOrUpdateContact(dynamic item, string tenantId)
        {
            string email = item.contact.Email;
            var contact = dbContext.TblContacts.Where(x =>
                x.Email == email &&
                x.TenantId.ToString() == tenantId &&
                x.IsArchive == false && x.IsActive == true && x.IsDelete == false).FirstOrDefault();

            string[] nameParts = (item.contact.Name ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string firstName = nameParts.Length > 0 ? nameParts[0] : "";
            string lastName = nameParts.Length > 1 ? string.Join(' ', nameParts.Skip(1)) : "";

            if (contact != null)
            {
                contact.FirstName = firstName;
                contact.LastName = lastName;
                contact.PhoneNumber = item.contact.Phone;
                contact.Email = item.contact.Email;
                contact.UpdatedAt = DateTime.Now;
                contact.ModifiedBy = AppConstant.LoggedInUserID;

                dbContext.TblContacts.Update(contact);
            }
            else
            {
                int version = dbContext.TblContacts
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.CreatedAt.Value.Date == DateTime.Today)
                    .AsEnumerable()
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => (p.ContactUniqueId.Split("-").Length == 3 ? Convert.ToInt32(p.ContactUniqueId.Split("-")[2]) : 0))
                    .FirstOrDefault() + 1;

                string contactUniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Contact, claimsProperty.GetCompanyCode(), version);

                contact = new TblContact
                {
                    ContactId = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = item.contact.Phone,
                    Email = item.contact.Email,
                    ContactUniqueId = contactUniqueId,
                    IsTaxExemption = false,
                    IsBillingAddressSameAsAddress = false,
                    CreatedBy = AppConstant.LoggedInUserID,
                    CreatedAt = DateTime.Now,
                    TenantId = Guid.Parse(tenantId),
                    IsActive = true,
                    IsArchive = false,
                    IsDelete = false,
                    LastSync = DateTime.Now,
                    GhlcontactId = item.contact.Id
                };

                dbContext.TblContacts.Add(contact);
            }

            await dbContext.SaveChangesAsync();
            return contact;
        }
        private async Task AddOrUpdateJob(dynamic item, Guid contactId, TblTenant subtenant, string tenantId)
        {
            string jobname = item.name;
            string contactId1 = item.contactId;

            var existingJob = dbContext.TblJobs.FirstOrDefault(x =>
                x.JobName == jobname &&
                x.GhlContactId == contactId1 &&
                x.TenantId.ToString() == tenantId &&
                x.IsDelete == false && x.IsActive == true);

            string pipelineStageId = item.pipelineStageId.ToLower();
            var stage = dbContext.TblGhlStages.Where(x =>
                x.GhlStageId.ToLower() == pipelineStageId).FirstOrDefault();

            string StageId1 = item.pipelineStageId;
            var jobStatusId = dbContext.TblGhlStagesmappings
           .Where(x => x.StageId.ToString() == StageId1.ToLower())
           .Select(x => x.StatusId)
           .FirstOrDefault();

            if (existingJob != null)
            {
                existingJob.JobName = item.name?.Trim();
                existingJob.GhlStageId = Guid.Parse(item.pipelineStageId);
                existingJob.GhlStageName = stage?.GhlStageName?.Trim() ?? string.Empty;
                existingJob.GhluserId = subtenant.GhluserId ?? string.Empty;
                existingJob.GhlOpportunityId = item.id ?? string.Empty;

                dbContext.TblJobs.Update(existingJob);
            }
            else
            {
                int version = dbContext.TblJobs
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.CreatedAt.Value.Date == DateTime.Today)
                    .AsEnumerable()
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => (p.JobUniqueId.Split("-").Length == 3 ? Convert.ToInt32(p.JobUniqueId.Split("-")[2]) : 0))
                    .FirstOrDefault() + 1;

                string StageId = item.pipelineStageId;
                string jobUniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, claimsProperty.GetCompanyCode(), version);
                Guid jobType = Guid.Parse("34623376-9CE3-4C9B-A538-2D3129B1A6B2");
                var jobStatusId1 = dbContext.TblGhlStagesmappings
               .Where(x => x.StageId.ToString() == StageId.ToLower())
               .Select(x => x.StatusId)
               .FirstOrDefault();
                var newJob = new TblJob
                {
                    AddressId = null,
                    ContactId = contactId,
                    JobName = item.name?.Trim(),
                    JobType = jobType,
                    AssignedPersonnel = null,
                    ScheduledDate = null,
                    JobStatus = jobStatusId1,
                    Notes = null,
                    JobUniqueId = jobUniqueId,
                    GhlStageId = Guid.Parse(item.pipelineStageId),
                    GhlStageName = stage?.GhlStageName ?? string.Empty,
                    GhlContactId = item.contactId ?? string.Empty,
                    TenantId = Guid.Parse(tenantId),
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID ?? Guid.Empty,
                    GhluserId = subtenant.GhluserId ?? string.Empty,
                    GhlOpportunityId = item.id ?? string.Empty,
                    AttachmentAny = null
                };

                dbContext.TblJobs.Add(newJob);
            }

            await dbContext.SaveChangesAsync();
        }


    }
}
