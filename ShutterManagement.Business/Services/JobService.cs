using System.Linq;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using Microsoft.EntityFrameworkCore;
using static ShutterManagement.Core.ViewModels.Response.GetAllJobsSubtenant_tenant;
using System.Security.Cryptography.X509Certificates;
using ShutterManagement.GHL.Services;
using ShutterManagment.GHL.ViewModel.Response;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagment.GHL.ViewModel.Request;
using System.Diagnostics.Contracts;
using Org.BouncyCastle.Asn1.X509.Qualified;
using ShutterManagement.Core.SPModel;

namespace ShutterManagement.Business.Services
{
    public class JobService : BaseRepository<ShutterDBContext, TblJob>, IJobService
    {
        private readonly ShutterDBContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly IAddress address;
        private readonly ITenantService tenantService;
        private readonly INotificationService notification;
        private readonly IGHLConfiguration gHLConfiguration;
        //private readonly GhlService ghlService;
        private readonly IContactService contact;
        private readonly GHLService_v2 gHLService_V2;
        private readonly JobStatusAndStageMapping statusAndStageMapping;
        private readonly HttpClient httpClient;
        private readonly IJobStatusService jobStatusService;
        private readonly IGhlCustomFieldsService ghlCustomFieldsService;
        private string BaseUrl = AppConstant.GHL_BaseAddress;

        public JobService(ShutterDBContext context, ClaimsProperty claimsProperty, IAddress address,
            ITenantService tenantService,INotificationService notification, IGHLConfiguration gHLConfiguration,
            IContactService contact,GHLService_v2 gHLService_V2,JobStatusAndStageMapping statusAndStageMapping,
            HttpClient httpClient,IJobStatusService jobStatusService,IGhlCustomFieldsService ghlCustomFieldsService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.address = address;
            this.tenantService = tenantService;
            this.notification = notification;
            this.gHLConfiguration = gHLConfiguration;

            this.contact = contact;
            this.gHLService_V2 = gHLService_V2;
            this.statusAndStageMapping = statusAndStageMapping;
            this.httpClient = httpClient;
            this.jobStatusService = jobStatusService;
            this.ghlCustomFieldsService = ghlCustomFieldsService;
        }

        private static string GetApiKey()
        {
            return AppConstant.GHL_APIKey; // Replace with a secure method to fetch the API key
        }
        public async Task<IResponseVm<string>> ActiveInactive(string JobId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantvalid = await tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == claimsProperty.GetTenantId());
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = GetAllQuerable().Where(x => x.JobId.ToString() == JobId &&
                  x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId()).FirstOrDefault();
                if (IsJobExist != null)
                {
                    IsJobExist.IsActive = !IsJobExist.IsActive;
                    IsJobExist.UpdatedAt = DateTime.Now;
                    IsJobExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsJobExist);
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

        // handle Webhook for opportunity creation or updation

        #region Ghl CRM related methods
        //public async Task<IResponseVm<string>> ProcessGhlOpportunityWebhook(GhlOpportunityWebhookModel webhookData)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();

        //    FileLogger.Log($"Start: Processing GHL Webhook for Opportunity ID: {webhookData?.Opportunity?.Id}");

        //    try
        //    {
        //        if (webhookData?.Opportunity == null)
        //        {
        //            responseVm.IsSuccess = false;
        //            responseVm.Message = "Invalid webhook data.";
        //            FileLogger.Log("Error: Invalid webhook data received.");
        //            return responseVm;
        //        }

        //        var ghlOpportunity = webhookData.Opportunity;

        //        var existingJob = await _context.TblJobs
        //            .FirstOrDefaultAsync(j => j.GhlOpportunityId == ghlOpportunity.Id);

        //        if (existingJob != null)
        //        {
        //            // Update existing job details
        //            existingJob.JobName = ghlOpportunity.Title?.Trim();
        //            existingJob.Notes = ghlOpportunity.Description?.Trim();
        //            existingJob.UpdatedAt = DateTime.UtcNow;

        //            await Update(existingJob);
        //            responseVm.Message = "Job successfully updated with GHL opportunity.";
        //            FileLogger.Log($"Success: Job updated for Opportunity ID: {ghlOpportunity.Id}");
        //        }
        //        else
        //        {
        //            // Extract relevant details from webhook data
        //            var stage =  _context.TblGhlStages
        //                .FirstOrDefault(s => s.GhlStageId == ghlOpportunity.StageId.ToString());



        //            // Create a new job based on webhook data
        //            var createJobModel = new CreateJob
        //            {
        //                JobName = ghlOpportunity.Title?.Trim(),
        //                Notes = ghlOpportunity.Description?.Trim(),
        //                //GHL_LocationId = ghlOpportunity.StageId,
        //                //ContactId = contact.ContactId,
        //                //AddressId = address.AddressId,
        //                //JobType = "Default",
        //                ScheduledDate = DateTime.Now,

        //            };

        //            responseVm = await Create(createJobModel);
        //        }

        //        responseVm.IsSuccess = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.IsSuccess = false;
        //        responseVm.Message = $"Error processing webhook: {ex.Message}";
        //        FileLogger.Log($"Exception: {ex.Message}");
        //    }

        //    return responseVm;
        //}

        private Guid? GetValidTenantId(Guid? tenantId)
        {
            return (tenantId != Guid.Empty && tenantId != null) ? tenantId : claimsProperty.GetTenantId();
        }

        private async Task<bool> IsTenantValid(Guid? tenantId)
        {
            return await tenantService.AnyAsync(x => x.TenantId == tenantId && x.IsActive == true && x.IsDelete == false);
        }

        private async Task<bool> IsJobNameDuplicate(string jobName, Guid contactId, Guid? tenantId)
        {
            return await AnyAsync(x => x.JobName == jobName.Trim() && x.IsDelete == false && x.TenantId == tenantId && x.ContactId == contactId);
        }

        private async Task<bool> IsContactInactiveOrArchived(Guid contactId, Guid? tenantId)
        {
            return await contact.AnyAsync(x => x.ContactId == contactId && x.TenantId == tenantId && (x.IsActive == false || x.IsArchive == true));
        }

        private async Task<bool> ValidateAddress(CreateJob model, Guid? tenantId)
        {
            if (model.GhlOpportunityId != null)
                return true;

            return await address.AnyAsync(x =>
                x.AddressId.ToString().ToLower() == model.AddressId.ToString().ToLower() &&
                x.ContactId.ToString().ToLower() == model.ContactId.ToString().ToLower() &&
                x.TenantId == tenantId && x.IsDelete == false);
        }

        private async Task<Guid> ResolveJobStatus(CreateJob model, Guid? tenantId)
        {
            var jobStatusName = EnumValue.GetEnumValue(JobStatus.New);

            if (model.GHL_StageId != null && model.GHL_StageId != Guid.Empty)
            {
                var statusWithStage = await statusAndStageMapping.GetJobStatusByName(jobStatusName, model.GHL_StageId, tenantId);
                return statusWithStage.IsSuccess ? statusWithStage.Data.JobStatusId ?? Guid.Empty : Guid.Empty;
            }

            var status = await statusAndStageMapping.GetJobStatusByName(jobStatusName, tenantId);
            return status.IsSuccess ? status.Data.JobStatusId ?? Guid.Empty : Guid.Empty;
        }

        private async Task<string> GenerateJobUniqueId(Guid? tenantId)
        {
            int Version = GetAllQuerable().Where(x => x.TenantId == tenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
                      .Select(p => (p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
            Version += 1;

            var companyCode = tenantService.GetAllQuerable()
                .Where(x => x.TenantId == tenantId && x.IsActive == true && x.IsDelete == false)
                .Select(x => x.Code)
                .FirstOrDefault();

            return GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, companyCode, Version);
        }

        public async Task<IResponseVm<string>> Create(CreateJob model, Guid? loggedInTenantId = null)
        {
            var tenantId = GetValidTenantId(loggedInTenantId);

            var validationResult = await ValidateJobRequestAsync(model, tenantId);
            if (!validationResult.IsSuccess)
                return BuildErrorResponse(validationResult.Message);

            var job = await BuildJobAsync(model, tenantId, validationResult.Data.JobStatus, validationResult.Data.StageName);

            var result = await ProcessJobCreationAsync(job, model, tenantId);
            return result;
        }

        private async Task<IResponseVm<(Guid JobStatus, string StageName)>> ValidateJobRequestAsync(CreateJob model, Guid? tenantId)
        {
            var response = new ResponseVm<(Guid, string)>();

            if (!await IsTenantValid(tenantId))
                return BuildErrorResponse<(Guid, string)>(AppConstant.TenantNotFound);

            if (await IsContactInactiveOrArchived((Guid)model.ContactId, tenantId))
                return BuildErrorResponse<(Guid, string)>("Contact is Archived or Inactive, You cannot create Job.");
            if (string.IsNullOrEmpty(model.GhlOpportunityId))
            {
                if (await IsJobNameDuplicate(model.JobName, (Guid)model.ContactId, tenantId))
                    return BuildErrorResponse<(Guid, string)>("JobName " + AppConstant.AlreadyExists);
            }
            else
            {
                if (await IsGHLOpportunityExist((Guid)model.ContactId,tenantId,model.GhlOpportunityId))
                {
                    var job = new UpdateJob
                    {
                        AddressId = model.AddressId ?? null,
                        ContactId = model.ContactId,
                        JobName = model.JobName,
                        AssignedPersonnel = model.AssignedPersonnel ?? null,
                        GhlOpportunityId = model.GhlOpportunityId,
                        GHL_StageId = model.GHL_StageId,
                        GHL_StageName = model.GHL_StageName,
                        GHL_ContactId = model.GHL_ContactId,
                        lastasync = DateTime.Now,
                        GHLUserId = model.GHLUserId ?? null,
                    };
                    await gHLConfiguration.GetGHLConfiguration(tenantId.ToString());
                    var res = await UpdateJob(job, tenantId);
                }
            }

            if (model.GhlOpportunityId == null && !await ValidateAddress(model, tenantId))
                return BuildErrorResponse<(Guid, string)>("Address " + AppConstant.NoDataFound);

            var jobStatus = await ResolveJobStatus(model, tenantId);
            var stageResponse = await statusAndStageMapping.GetGHLStagesMappingsById(jobStatus, tenantId);
            var stage = _context.TblGhlStages.Where(x => x.TenantId == tenantId && x.GhlStageId == model.GHL_StageId.ToString()).FirstOrDefault();
            string stageName = model.GHL_StageName ?? stage?.GhlStageName ?? string.Empty;

            response.IsSuccess = true;
            response.Data = (jobStatus, stageName);
            return response;
        }
        private async Task<bool> IsGHLOpportunityExist(Guid contactId, Guid? tenantId,string? GHLOpportunityId)
        {
            return await AnyAsync(x => x.ContactId == contactId && x.TenantId == tenantId && x.IsActive == true && x.IsDelete == false && x.GhlOpportunityId == GHLOpportunityId);
        }

        private async Task<TblJob> BuildJobAsync(CreateJob model, Guid? tenantId, Guid jobStatus, string stageName)
        {
            string jobUniqueId = await GenerateJobUniqueId(tenantId);
            string arrayString = string.Join(", ", model.AttachmentsAny);

            return new TblJob
            {
                AddressId = model.AddressId != Guid.Empty ? model.AddressId : null,
                ContactId = model.ContactId,
                JobName = model.JobName?.Trim(),
                JobType = model.JobType != Guid.Empty ? model.JobType : null,
                AssignedPersonnel = model.AssignedPersonnel ?? Guid.Empty,
                ScheduledDate = model.ScheduledDate,
                JobStatus = jobStatus != Guid.Empty ? jobStatus : null,
                Notes = model.Notes ?? string.Empty,
                AttachmentAny = arrayString ?? null,
                JobUniqueId = jobUniqueId,
                GhlStageId = model.GHL_StageId != Guid.Empty ? model.GHL_StageId : null,
                GhlStageName = stageName.Trim(),
                GhlContactId = model.GHL_ContactId ?? string.Empty,
                GhlOpportunityId = model.GhlOpportunityId ?? string.Empty,
                TenantId = tenantId,
                IsActive = true,
                IsDelete = false,
                CreatedAt = DateTime.Now,
                CreatedBy = tenantId ?? Guid.Empty,
                GhluserId = model.GHLUserId ?? string.Empty
            };
        }

        private async Task<IResponseVm<string>> ProcessJobCreationAsync(TblJob job, CreateJob model, Guid? tenantId)
        {
            var response = new ResponseVm<string>();
            var insertedJob = new TblJob();
            if (!await IsGHLOpportunityExist((Guid)model.ContactId, tenantId, model.GhlOpportunityId))
            {
                insertedJob = await Add(job);

            }
            response.IsSuccess = true;
            response.Message = AppConstant.InsertSuccessMsg;

            if (model.GhlOpportunityId == null)
            {
                await HandleGhlOpportunityCreationAsync(insertedJob, tenantId);
            }
            else
            {
                insertedJob.GhlOpportunityId = model.GhlOpportunityId;
                insertedJob.ContactId = model.ContactId;
                insertedJob.LastAsync = DateTime.Now;
                await Update(insertedJob);
            } 

            await CreateJobNotificationAsync(insertedJob, tenantId);
            return response;
        }
       
        private IResponseVm<T> BuildErrorResponse<T>(string message)
        {
            return new ResponseVm<T>
            {
                IsSuccess = false,
                Message = message
            };
        }

        private async Task HandleGhlOpportunityCreationAsync(TblJob job, Guid? tenantId)
        {
            var config = await gHLConfiguration.GetGHLConfiguration(tenantId.ToString());
            var isConfigured = await gHLConfiguration.CheckGHLConfiguration(tenantId);

            if (isConfigured.IsSuccess)
            {
                var response = await gHLService_V2.CreateOpportunityInGhl(config?.Data, job);
                FileLogger.Log($"CreateOpportunityInGhl: {response.Message}, Success: {response.IsSuccess}, Data: {response.Data}");
            }
        }

        private async Task CreateJobNotificationAsync(TblJob job, Guid? tenantId)
        {
            var notificationData = new CreateNotification
            {
                UserId = tenantId,
                Subject = nameof(NotificationModule.Job),
                Body = $"{job.JobName} {AppConstant.Create_Notification}",
                CreatedDate = DateTime.Now,
                Type = nameof(NotificationModule.Job),
                TypeId = job.JobId
            };

            await notification.CreateNotification(notificationData);
        }


        private IResponseVm<string> BuildErrorResponse(string message)
        {
            return BuildErrorResponse<string>(message);
        }


        public async Task<IResponseVm<List<Opportunity>>> SyncOpportunitiesFromGhl1()
        {
            var responseVm = new ResponseVm<List<Opportunity>>();

            try
            {
                var configCheck = await gHLConfiguration.CheckGHLConfiguration();
                if (!configCheck.IsSuccess)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "GHL configuration check failed.";
                    return responseVm;
                }
                var GHLConfiguration = await gHLConfiguration.GetGHLConfiguration(claimsProperty.GetTenantId().ToString());

               
                

                FileLogger.Log("Starting SyncOpportunitiesFromGhl process.");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GHLConfiguration?.Data?.Access_Token??"");

                var pipelineResponse = await gHLService_V2.GetAllPipelinesWithStagesAsync(GHLConfiguration.Data);
                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    FileLogger.Log("No pipelines found in GHL.");
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(GHLConfiguration?.Data?.PipelineId ?? "") || p.Name.Contains(GHLConfiguration?.Data?.PipelineId ?? ""));

                if (targetPipeline == null)
                {
                    FileLogger.Log($"Target pipeline '{GHLConfiguration?.Data?.PipelineId ?? ""}' not found.");
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Target pipeline not found.";
                    return responseVm;
                }

                FileLogger.Log($"Fetching opportunities from pipeline ID: {targetPipeline.Id}");

                var opportunitiesUrl = $"{BaseUrl}/pipelines/{targetPipeline.Id}/opportunities/";
                var response = await client.GetAsync(opportunitiesUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"Error fetching opportunities. StatusCode: {response.StatusCode}, Response: {responseBody}");
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Error: {response.StatusCode}, Details: {responseBody}";
                    return responseVm;
                }

                var ghlResponse = JsonConvert.DeserializeObject<GhlOpportunitiesResponse>(responseBody);
                var opportunities = ghlResponse?.Opportunities ?? new List<Opportunity>();

                FileLogger.Log($"Fetched {opportunities.Count} opportunities from GHL.");

                var ghlStages = await GetGHLStages();
                var contacts = await gHLService_V2.GetContactsAsync(GHLConfiguration.Data);

                foreach (var opportunity in opportunities)
                {
                    try
                    {
                        FileLogger.Log($"Processing opportunity ID: {opportunity.OpportunityId}, Name: {opportunity.Name}");

                        var contact = contacts.FirstOrDefault(c => c.id == opportunity.Contact?.Id);
                        if (contact == null)
                        {
                            FileLogger.Log($"GHL Contact not found for Opportunity ID: {opportunity.OpportunityId}");
                            continue;
                        }

                        var dbContact = await _context.TblContacts
                            .FirstOrDefaultAsync(x => x.GhlcontactId == contact.id);
                        if (dbContact == null)
                        {
                            FileLogger.Log($"Local DB Contact not found for GHL Contact ID: {contact.id}");
                            continue;
                        }

                        var stage = ghlStages?.Data?.FirstOrDefault(s => s.GhlStageId == opportunity.PipelineStageId);
                        var jobStatusId = await _context.MstJobStatuses
                            .Where(x => x.JobStatusName == opportunity.Status)
                            .Select(x => x.JobStatusId)
                            .FirstOrDefaultAsync();

                        var existingJob = GetAllQuerable()
                            .FirstOrDefault(x => x.GhlOpportunityId == opportunity.OpportunityId);

                        if (existingJob != null)
                        {
                            FileLogger.Log($"Updating existing job: {existingJob.JobId}");

                            existingJob.JobName = opportunity.Name?.Trim();
                            existingJob.JobStatus = jobStatusId;
                            existingJob.GhlContactId = contact.id;
                            existingJob.ContactId = dbContact.ContactId;
                            existingJob.GhlStageName = stage?.GhlStageName;
                            existingJob.GhlStageId = Guid.Parse(opportunity.PipelineStageId);
                            existingJob.UpdatedAt = DateTime.Now;
                            existingJob.LastAsync = DateTime.Now;

                            await Update(existingJob);
                        }
                        else
                        {
                            FileLogger.Log($"Creating new job for opportunity ID: {opportunity.OpportunityId}");

                            var createJobModel = new CreateJob
                            {
                                ContactId = dbContact.ContactId,
                                GHL_ContactId = contact.id,
                                GhlOpportunityId = opportunity.OpportunityId,
                                AddressId = dbContact.PrimaryAddress,
                                GHL_StageName = stage?.GhlStageName,
                                JobName = opportunity.Name?.Trim(),
                                ScheduledDate = DateTime.Now,
                                lastasync = DateTime.Now,
                                Notes = "Synced from GHL",
                                GHL_StageId = Guid.Parse(opportunity.PipelineStageId)
                            };

                            await Create(createJobModel);
                        }
                    }
                    catch (Exception exInner)
                    {
                        FileLogger.Log($"Error processing opportunity ID {opportunity.OpportunityId}: {exInner.Message}");
                        continue;
                    }
                }

                FileLogger.Log("Synchronization completed successfully.");
                responseVm.Data = opportunities;
                responseVm.IsSuccess = true;
                responseVm.Message = "Opportunity Sync completed successfully.";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception occurred in SyncOpportunitiesFromGhl: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception occurred: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateOpportunityInGHLfromApp(Guid? LoggedInTenantId = null)
        {
          
                IResponseVm<string> responseVm = new ResponseVm<string>();
                try
                {
                var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration(LoggedInTenantId);
                var ghlconfigdetails = await gHLConfiguration.GetGHLConfiguration(LoggedInTenantId.ToString());
                if (Checkghlresponse.IsSuccess)
                {
                    FileLogger.Log("Starting UpdateOpportunityInGHLfromApp process.");

                    var getAppOpportunities = GetAllQuerable()
                        .Where(x => x.TenantId == LoggedInTenantId && x.IsActive ==true && x.IsDelete ==false)
                        .ToList();

                    var ghlOpportunitiesResponse = await gHLService_V2.GetOpportunitiesFromGhl(ghlconfigdetails?.Data);
                    var ghlOpportunities = ghlOpportunitiesResponse?.Data ?? new List<Opportunity>();

                    foreach (var opportunity in getAppOpportunities)
                    {
                        if (string.IsNullOrEmpty(opportunity.GhlOpportunityId))
                        {
                            var job = new TblJob
                            {
                                ContactId = opportunity.ContactId,
                                JobName = opportunity.JobName,
                                JobStatus = opportunity.JobStatus,
                                GhlStageId = opportunity.GhlStageId
                            };

                            var responseghl = await gHLService_V2.CreateOpportunityInGhl(ghlconfigdetails.Data,job);
                            FileLogger.Log($"Created Opportunity in GHL: {responseghl.Message}, Success: {responseghl.IsSuccess}");
                        }
                        else
                        {
                            var existingGHLOpportunity = ghlOpportunities.FirstOrDefault(x => x.OpportunityId == opportunity.GhlOpportunityId);

                            if (existingGHLOpportunity != null)
                            {
                                var job = new TblJob
                                {
                                    ContactId = opportunity.ContactId,
                                    JobName = opportunity.JobName,
                                    JobStatus = opportunity.JobStatus,
                                    GhlStageId = opportunity.GhlStageId
                                };

                                var responseghl = await UpdateOpportunityInGhl(job,LoggedInTenantId);
                                FileLogger.Log($"Updated Opportunity in GHL: {responseghl.Message}, Success: {responseghl.IsSuccess}");
                            }
                            else
                            {
                                FileLogger.Log($"TThis job Deleted on GHL but in The its Already have{existingGHLOpportunity}");
                                //var res = await DeleteJob(opportunity.JobId.ToString());
                                //if (!res.IsSuccess)
                                //{
                                //    FileLogger.Log($"Opportunity {opportunity.GhlOpportunityId} not found in GHL. But it's not deleted in App.");
                                //}
                            }
                        }
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Opportunities synced successfully with GHL.";
                }
                }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in UpdateOpportunityInGHLfromApp: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<Opportunity>>> SyncOpportunitiesFromGhl(GetGHLConfiguration getGHLConfiguration, Guid? LoggedInTenantId = null)
        {
            var responseVm = new ResponseVm<List<Opportunity>>();

            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

                FileLogger.Log($"Exception occurred GHLContact is null.{LoggedInTenantId}");

                //var ghlConfigdetails = await gHLConfiguration.GetGHLConfiguration(LoggedInTenantId.ToString());
                //var configCheck = await gHLConfiguration.CheckGHLConfiguration(LoggedInTenantId);

                //if (!configCheck.IsSuccess)
                //{
                //    responseVm.IsSuccess = false;
                //    responseVm.Message = "GHL configuration check failed.";
                //    return responseVm;
                //}

                FileLogger.Log("Starting SyncOpportunitiesFromGhl process.");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration.Access_Token);

                var pipelineResponse = await gHLService_V2.GetAllPipelinesWithStagesAsync(getGHLConfiguration);
                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    FileLogger.Log("No pipelines found in GHL.");
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(getGHLConfiguration?.PipelineId??"") || p.Name.Contains(getGHLConfiguration?.PipelineId ?? ""));

                if (targetPipeline == null)
                {
                    FileLogger.Log($"Target pipeline '{getGHLConfiguration?.PipelineId ?? ""}' not found.");
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Target pipeline not found.";
                    return responseVm;
                }

                FileLogger.Log($"Fetching opportunities from pipeline ID: {targetPipeline.Id}");

                var opportunities = await gHLService_V2.GetOpportunitiesFromGhl(getGHLConfiguration);
                var stages = await GetGHLStages(LoggedInTenantId);
                var allContacts = await gHLService_V2.GetContactsAsync(getGHLConfiguration);
                FileLogger.Log($"getting contacts from ghl process.{allContacts}");

                foreach (var opportunity in opportunities.Data)
                {
                    try
                    {
                        FileLogger.Log($"Processing opportunity ID: {opportunity.OpportunityId}, Name: {opportunity.Name}");

                        var existingJob = GetAllQuerable()
                            .FirstOrDefault(x => x.GhlOpportunityId.ToLower() == opportunity.OpportunityId.ToLower() && 
                            x.TenantId== LoggedInTenantId && x.IsActive==true && x.IsDelete==false);

                        var stage = stages?.Data.FirstOrDefault(x => x.GhlStageId == opportunity.PipelineStageId);
                        var dbcontact = contact.GetAllQuerable()
                            .FirstOrDefault(x => x.GhlcontactId == opportunity.Contact.Id && x.TenantId == LoggedInTenantId
                            && x.IsDelete == false && x.IsActive == true && x.IsArchive == false);

                        if (dbcontact == null)
                        {
                            FileLogger.Log($"Contact not found for GHL Contact ID: {opportunity.Contact.Id}");
                            continue; // Skip this record instead of exiting the entire sync
                        }

                        var jobstatusId = jobStatusService.GetAllQuerable()
                            .Where(x => x.JobStatusName == EnumValue.GetEnumValue(JobStatus.New))
                            .Select(x => x.JobStatusId)
                            .FirstOrDefault();
                       
                        if (existingJob != null)
                        {
                            FileLogger.Log($"Updating existing job: {existingJob.JobId}");
                            existingJob.AddressId = existingJob.AddressId == null ? (dbcontact.PrimaryAddress == null ? null : dbcontact.PrimaryAddress) : existingJob.AddressId;
                            existingJob.JobName = opportunity.Name??"";
                            existingJob.JobStatus = jobstatusId;
                            existingJob.GhlContactId = opportunity.Contact?.Id;
                            existingJob.ContactId = dbcontact.ContactId;
                            existingJob.GhlStageName = stage?.GhlStageName??"";
                            existingJob.GhlStageId = Guid.Parse(opportunity.PipelineStageId);
                            existingJob.UpdatedAt = DateTime.Now;
                            existingJob.LastAsync = DateTime.Now;

                            await Update(existingJob);
                        }
                        else
                        {
                            FileLogger.Log($"Creating new job for opportunity ID: {opportunity.OpportunityId}");

                            var createJobModel = new CreateJob
                            {
                                ContactId = dbcontact?.ContactId,
                                GHL_ContactId = opportunity.Contact?.Id??"",
                                GhlOpportunityId = opportunity.OpportunityId??"",
                                AddressId = dbcontact?.PrimaryAddress ?? Guid.Empty,
                                GHL_jobstatus = jobstatusId.ToString()??"",
                                GHL_StageName = stage?.GhlStageName??"",
                                JobName = opportunity.Name?.Trim()??"",
                                ScheduledDate = DateTime.Now,
                                lastasync = DateTime.Now,
                                Notes = "Synced from GHL",
                                GHL_StageId = Guid.Parse(opportunity.PipelineStageId)
                            };

                            await Create(createJobModel, LoggedInTenantId);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        FileLogger.Log($"Error processing opportunity ID: {opportunity.OpportunityId}. Exception: {innerEx.Message}");
                    }
                }

                var updateResult = await UpdateOpportunityInGHLfromApp(LoggedInTenantId);
                FileLogger.Log("All Jobs synced completed successfully.");
                FileLogger.Log("Update GHL FromApp completed successfully.");

                responseVm.Data = opportunities.Data;
                responseVm.IsSuccess = true;
                responseVm.Message = "All Jobs synced completed successfully.";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception occurred in SyncOpportunitiesFromGhl: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception occurred: {ex.Message}";
            }

            return responseVm;
        }


        public async Task<IResponseVm<List<PipelineAndStage>>> GetGHLStages(Guid? LoggedInTenantId = null)
        {
            IResponseVm<List<PipelineAndStage>> responseVm = new ResponseVm<List<PipelineAndStage>>();
            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
                var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration(LoggedInTenantId);
                if (Checkghlresponse.IsSuccess)
                {
                    var gHLStages = dbContext.TblGhlStages
                    .Where(x => x.TenantId == LoggedInTenantId).OrderByDescending(x => x.CreatedAt)
                    .ToList();

                    var stageList = new List<PipelineAndStage>();

                    foreach (var stage in gHLStages)
                    {
                        stageList.Add(new PipelineAndStage
                        {
                            GhlPipelineId = stage.GhlPipelineId ?? "",
                            GhlStageId = stage.GhlStageId ?? "",
                            GhlStageName = stage.GhlStageName ?? ""
                        });
                    }

                    responseVm.Data = stageList;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateOpportunityInGhl(TblJob job, Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
                //await gHLConfiguration.GetGHLConfiguration(LoggedInTenantId.ToString());

                if (string.IsNullOrEmpty(job.GhlOpportunityId))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No associated GHL Opportunity ID found.";
                    FileLogger.Log($"Error in UpdateOpportunityInGhl: {job.GhlOpportunityId}");
                    return responseVm;
                }

                var jobStatusName = jobStatusService.GetAllQuerable()
                    .Where(x => x.JobStatusId == job.JobStatus)
                    .Select(x => x.JobStatusName)
                    .FirstOrDefault();

                var contactData = contact.GetAllQuerable()
                    .Where(x => x.GhlcontactId == job.GhlContactId)
                    .FirstOrDefault();

                if (jobStatusName == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No job status name found";
                    FileLogger.Log($"Error in UpdateOpportunityInGhl: No job status name found for {job.JobName}");
                    return responseVm;
                }

                var configdetails = dbContext.TblGhlconfigurations.Where(s => s.TenantId == LoggedInTenantId && s.IsDelete == false && s.IsActive == true).FirstOrDefault();
                if (configdetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "GHL configuration not found.";
                    FileLogger.Log("Error in UpdateOpportunityInGhl: GHL configuration not found.");
                    return responseVm;
                }

                string userProvidedPipelineNameOrId = configdetails?.PipelineId??"";

                var targetPipeline = dbContext.TblGhlStages
                    .FirstOrDefault(p => p.GhlPipelineId.Contains(userProvidedPipelineNameOrId) || p.GhlPipelineName.Contains(userProvidedPipelineNameOrId));

                if (targetPipeline == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Pipeline ID not found";
                    FileLogger.Log($"Error in UpdateOpportunityInGhl: Pipeline ID {userProvidedPipelineNameOrId} not found.");
                    return responseVm;
                }

                var GHUserId = dbContext.TblUsers.Where(x => x.Id == job.AssignedPersonnel && x.IsDelete == false && x.IsActive == true)
                   .Select(y => y.GhlUserId).FirstOrDefault() ?? ""; 

                var ghlOpportunityModel = new
                {
                    name = job.JobName,
                    pipelineStageId = job.GhlStageId,
                    assignedTo = GHUserId,
                };

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configdetails?.AccessToken ?? "");
                httpClient.DefaultRequestHeaders.Add("Version", AppConstant.GHL_Version);

                var url = $"{AppConstant.GHL_BaseAddress}/opportunities/{job.GhlOpportunityId}";
                var content = new StringContent(JsonConvert.SerializeObject(ghlOpportunityModel), Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"GHL Update Failed: {response.StatusCode}, Details: {responseBody}";
                    FileLogger.Log($"Error in UpdateOpportunityInGhl: {response.StatusCode}, Details: {responseBody}");
                    return responseVm;
                }

                responseVm.IsSuccess = true;
                responseVm.Message = "Opportunity successfully updated in GHL.";
                FileLogger.Log($"Opportunity successfully updated in GHL.: {job.GhlOpportunityId}, Details: {responseBody}");
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
                FileLogger.Log($"Error in UpdateOpportunityInGhl: {ex.Message}");
            }

            return responseVm;
        }


        //public async Task<List<GhlContactVM>> GetContactsAsync()
        //{
        //    var requestUrl = $"{BaseUrl}contacts/";
        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
        //    var response = await client.GetAsync(requestUrl);
        //    response.EnsureSuccessStatusCode();

        //    var json = await response.Content.ReadAsStringAsync();
        //    var result = System.Text.Json.JsonSerializer.Deserialize<GhlContactResponse>(json);
        //    //var result1 = GetClientIpAddress();
        //    return result?.Contacts ?? new List<GhlContactVM>();
        //}

        #endregion Ghl CRM related methods


        //#region Contact Sync Related Method 
        //public async Task<IResponseVm<string>> GetAndSaveContactsAsync()
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();

        //    try
        //    {
        //        var ghlContactsResponse = await GetContactsAsync();

        //        if (ghlContactsResponse == null || !ghlContactsResponse.Any())
        //        {
        //            responseVm.IsSuccess = false;
        //            responseVm.Message = "No contacts found.";
        //            return responseVm;
        //        }

        //        // 2. Get existing contacts from the database
        //        var existingContacts = dbContext.TblContacts
        //            .Where(x => x.TenantId == claimsProperty.GetTenantId())
        //            .ToList();

        //        var existingContactsDict = existingContacts.ToDictionary(x => x.GhlcontactId, x => x);

        //        List<TblContact> contactsToUpdate = new();
        //        List<TblContact> contactsToInsert = new();

        //        foreach (var ghlContact in ghlContactsResponse)
        //        {
        //            if (existingContactsDict.TryGetValue(ghlContact.id, out var existingContact))
        //            {
        //                // 3. Update existing contacts
        //                existingContact.FirstName = ghlContact.firstName;
        //                existingContact.Email = ghlContact.email;
        //                existingContact.PhoneNumber = ghlContact.phone;
        //                existingContact.UpdatedAt = DateTime.Now;
        //                existingContact.ModifiedBy = AppConstant.LoggedInUserID;
        //                contactsToUpdate.Add(existingContact);
        //            }
        //            else
        //            {
        //                // 4. Insert new contacts
        //                contactsToInsert.Add(new TblContact
        //                {
        //                    ContactId = Guid.NewGuid(),
        //                    GhlcontactId = ghlContact.id,
        //                    FirstName = ghlContact.firstName,
        //                    Email = ghlContact.email,
        //                    PhoneNumber = ghlContact.phone,
        //                    CreatedAt = DateTime.Now,
        //                    UpdatedAt = DateTime.Now,
        //                    TenantId = claimsProperty.GetTenantId(),
        //                    CreatedBy = AppConstant.LoggedInUserID,
        //                    IsActive = true
        //                });
        //            }
        //        }

        //        // 5. Save changes to database
        //        if (contactsToUpdate.Any())
        //        {
        //            dbContext.TblContacts.UpdateRange(contactsToUpdate);
        //        }

        //        if (contactsToInsert.Any())
        //        {
        //            await dbContext.TblContacts.AddRangeAsync(contactsToInsert);
        //        }

        //        await dbContext.SaveChangesAsync();

        //        // 6. Update Last Sync Configuration
        //        var isUpdated = await ghlService.UpdateLastSyncConfigurationAsync();

        //        if (!isUpdated)
        //        {
        //            responseVm.IsSuccess = false;
        //            responseVm.Message = "Error: Configuration not found or update failed.";
        //            return responseVm;
        //        }

        //        responseVm.IsSuccess = true;
        //        responseVm.Message = "All contacts synced successfully.";
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.IsSuccess = false;
        //        responseVm.Message = $"Error: {ex.Message}";
        //    }

        //    return responseVm;
        //}
        //#endregion Contact Sync Related Method

        //public async Task<IResponseVm<string>> Create(CreateJob model, Guid? LoggedInTenantId = null)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
        //        bool IsAddressExist = false;
        //      //  var TenantId = Guid.Empty;
        //        var IsTenantValid = false;
        //        TblGhlStage stage = new TblGhlStage();
        //        Guid jobStatus = Guid.Empty;
        //        //if (model.locationId != null)
        //        //{
        //        //    //await gHLConfiguration.GetGHLConfiguration(model.locationId);
        //        //    TenantId = (Guid)LoggedInTenantId;

        //        //    if (TenantId != null)
        //        //    {
        //        //        IsTenantValid = true;

        //        //        stage.GhlStageName = model.GHL_StageName;
        //        //        Guid.TryParse(model.GHL_jobstatus, out jobStatus);
        //        //        IsAddressExist = true;
        //        //        //else
        //        //        //{
        //        //        //    responseVm.Message = "Contact is not synced in Application.";
        //        //        //    return responseVm;
        //        //        //}
        //        //    }
        //        //}
        //        //else
        //        //{
        //            IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
        //            IsAddressExist = await address.AnyAsync(x => x.AddressId.ToString().ToLower() == model.AddressId.ToString().ToLower() && x.ContactId.ToString().ToLower() == model.ContactId.ToString().ToLower()
        //            && x.TenantId == LoggedInTenantId && x.IsDelete == false);
        //            //stage = _context.TblGhlStages.Where(x => x.TenantId == claimsProperty.GetTenantId() && x.GhlStageId == model.GHL_StageId.ToString()).FirstOrDefault();
        //            //jobStatus = _context.MstJobStatuses.Where(s => s.JobStatusName.ToLower() == EnumValue.GetEnumValue(JobStatus.New).ToLower() && s.IsActive == true).Select(x=> x.JobStatusId).FirstOrDefault();

        //            if (model.GHL_StageId != null && model.GHL_StageId != Guid.Empty)
        //            {
        //                var JobStatusResponseWithStage = await statusAndStageMapping.GetJobStatusByName(EnumValue.GetEnumValue(JobStatus.New), model.GHL_StageId, LoggedInTenantId);
        //                if (JobStatusResponseWithStage.IsSuccess)
        //                {
        //                    jobStatus = JobStatusResponseWithStage.Data.JobStatusId ?? Guid.Empty;
        //                }
        //            }
        //            else
        //            {
        //                var JobStatusResponse = await statusAndStageMapping.GetJobStatusByName(EnumValue.GetEnumValue(JobStatus.New), LoggedInTenantId);
        //                if (JobStatusResponse.IsSuccess)
        //                {
        //                    jobStatus = JobStatusResponse.Data.JobStatusId ?? Guid.Empty;
        //                }
        //            }


        //        //}
        //        if (!IsTenantValid)
        //        {
        //            responseVm.Message = AppConstant.TenantNotFound;
        //            return responseVm;
        //        }
        //        int Version = GetAllQuerable().Where(x => x.TenantId == LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
        //              .Select(p => (p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
        //        Version += 1;
        //        var Companycode = tenantService.GetAllQuerable().Where(x => x.TenantId == LoggedInTenantId && x.IsActive == true && x.IsDelete == false).Select(x => x.Code).FirstOrDefault();
        //        var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, Companycode, Version);
        //        bool IsjobNameExists = await AnyAsync(x => x.JobName == model.JobName.Trim() && x.IsDelete == false && x.TenantId == LoggedInTenantId && x.ContactId == model.ContactId);
        //        var StageResponse = await statusAndStageMapping.GetGHLStagesMappingsById(jobStatus, LoggedInTenantId);
        //        bool IsContactArchiveorInactive = await contact.AnyAsync(x => x.ContactId == model.ContactId && x.TenantId == LoggedInTenantId && (x.IsActive == false || x.IsArchive == true));
        //        if (IsContactArchiveorInactive)
        //        {
        //            responseVm.Message = "Contact is Archived or Inactive,You can not create Job.";
        //            return responseVm;
        //        }
        //        if (IsjobNameExists)
        //        {
        //            responseVm.Message = "JobName" + AppConstant.AlreadyExists;
        //            return responseVm;
        //        }

        //        if (model.GhlOpportunityId != null)
        //        {
        //            IsAddressExist = true;
        //        }
        //        if (IsAddressExist)
        //        {
        //            string arrayString = string.Join(", ", model.AttachmentsAny);
        //            TblJob job = new TblJob
        //            {
        //                AddressId = (model.AddressId != null && model.AddressId != Guid.Empty) ? model.AddressId : null,
        //                ContactId = model.ContactId,
        //                JobName = model.JobName?.Trim(),
        //                JobType = (model.JobType != null && model.JobType != Guid.Empty) ? model.JobType : null,
        //                AssignedPersonnel = model.AssignedPersonnel ?? Guid.Empty,
        //                ScheduledDate = model.ScheduledDate,
        //                JobStatus = jobStatus != Guid.Empty ? jobStatus : null,
        //                Notes = model.Notes ?? string.Empty,
        //                AttachmentAny = arrayString,
        //                JobUniqueId = generateUnique,
        //                GhlStageId = ((model.GHL_StageId != null && model.GHL_StageId != Guid.Empty) ? model.GHL_StageId : StageResponse?.Data?.StageId) ?? null,
        //                GhlStageName = ((model.GHL_StageName != null && model.GHL_StageName != string.Empty) ? model.GHL_StageName : stage?.GhlStageName?.Trim() ?? string.Empty) ?? string.Empty,
        //                GhlContactId = model.GHL_ContactId ?? string.Empty,
        //                TenantId = LoggedInTenantId,
        //                IsActive = true,
        //                IsDelete = false,
        //                CreatedAt = DateTime.Now,
        //                CreatedBy = LoggedInTenantId ?? Guid.Empty,
        //                GhluserId = model.GHLUserId ?? string.Empty
        //            };

        //            var res = await Add(job);
        //            responseVm.IsSuccess = true;
        //            responseVm.Message = AppConstant.InsertSuccessMsg;
        //            if (responseVm.IsSuccess)
        //            {
        //                if (model.GhlOpportunityId == null)
        //                {
        //                    var ghlconfigdetails =  await gHLConfiguration.GetGHLConfiguration(LoggedInTenantId.ToString());
        //                    var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration(LoggedInTenantId);
        //                    if (Checkghlresponse.IsSuccess)
        //                    {

        //                        var responseghl = await gHLService_V2.CreateOpportunityInGhl(ghlconfigdetails?.Data,job);
        //                        FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.Message}");
        //                        FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.IsSuccess}");
        //                        FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.Data}");
        //                    }
        //                }
        //                else
        //                {
        //                    res.GhlOpportunityId = model.GhlOpportunityId;
        //                    res.ContactId = model.ContactId;
        //                    res.LastAsync = DateTime.Now;
        //                    await Update(res);
        //                }
        //                CreateNotification notification1 = new CreateNotification()
        //                {
        //                    UserId = LoggedInTenantId,
        //                    Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Job),
        //                    Body = $"{job.JobName} {AppConstant.Create_Notification}",
        //                    CreatedDate = DateTime.Now,
        //                    Type = Enum.GetName(typeof(NotificationModule), NotificationModule.Job),
        //                    TypeId = job.JobId,
        //                };
        //                var result = await notification.CreateNotification(notification1);
        //            }
        //        }
        //        else
        //        {
        //            responseVm.Message = "Address " + AppConstant.NoDataFound;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }
        //    return responseVm;
        //}


        public async Task<IResponseVm<string>> DeleteJob(string JobId)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantvalid = await tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == claimsProperty.GetTenantId());
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = GetAllQuerable().Where(x => x.JobId.ToString() == JobId &&
                   x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId()).FirstOrDefault();
                if (IsJobExist != null)
                {
                    IsJobExist.IsActive = false;
                    IsJobExist.IsDelete = true;
                    IsJobExist.UpdatedAt = DateTime.Now;
                    IsJobExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsJobExist);

                    AllModuleDeleted(Guid.Parse(JobId));
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
            return responseVm;
        }

        public async Task<IResponseVm<string>> AllModuleDeleted(Guid JobId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var WindowExists = dbContext.TblWindows.Where(x => x.JobId == JobId && x.IsDelete == false).ToList();
                if (WindowExists != null)
                {
                    foreach (var item in WindowExists)
                    {
                        item.IsDelete = true;
                        item.IsActive = false;
                        item.UpdatedAt = DateTime.Now;
                        item.ModifiedBy = AppConstant.LoggedInUserID;

                        dbContext.TblWindows.Update(item);
                        dbContext.SaveChanges();
                    }
                }
                var EstimateExists = dbContext.TblEstimates.Where(x => x.JobId == JobId && x.IsDelete == false).ToList();
                if (EstimateExists != null)
                {
                    foreach (var item in EstimateExists)
                    {
                        item.IsDelete = true;
                        item.IsActive = false;
                        item.UpdatedAt = DateTime.Now;
                        item.ModifiedBy = AppConstant.LoggedInUserID;

                        dbContext.TblEstimates.Update(item);
                        dbContext.SaveChanges();
                    }
                }
                var OrderExists = dbContext.TblOrders.Where(x => x.JobId == JobId && x.IsDelete == false).ToList();
                if (OrderExists != null)
                {
                    foreach (var item in OrderExists)
                    {
                        item.IsDelete = true;
                        item.IsActive = false;
                        item.UpdatedAt = DateTime.Now;
                        item.ModifiedBy = AppConstant.LoggedInUserID;

                        dbContext.TblOrders.Update(item);
                        dbContext.SaveChanges();
                    }
                }
                var InvoiceExists = dbContext.TblInvoices.Where(x => x.JobId == JobId && x.IsDelete == false).ToList();
                if (InvoiceExists != null)
                {
                    foreach (var item in InvoiceExists)
                    {
                        item.IsDelete = true;
                        item.IsActive = false;
                        item.UpdatedAt = DateTime.Now;
                        item.ModifiedBy = AppConstant.LoggedInUserID;

                        dbContext.TblInvoices.Update(item);
                        dbContext.SaveChanges();
                    }
                }
                foreach (var items in InvoiceExists)
                {
                    var InvoicePaymentExists = dbContext.TblInvoicePayments.Where(x => x.InvoiceId == items.InvoiceId && x.IsDelete == false).ToList();
                    if (InvoicePaymentExists != null)
                    {
                        foreach (var item in InvoicePaymentExists)
                        {
                            item.IsDelete = true;
                            item.IsActive = false;
                            item.UpdatedAt = DateTime.Now;
                            item.ModifiedBy = AppConstant.LoggedInUserID;

                            dbContext.TblInvoicePayments.Update(item);
                            dbContext.SaveChanges();
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
        public async Task<IResponseVm<List<GetAllJobsdetails>>> GetAll(Pagination filter)
        {

            IResponseVm<List<GetAllJobsdetails>> responseVm = new ResponseVm<List<GetAllJobsdetails>>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {
                    var JobList = (from t2 in _context.TblJobs.OrderByDescending(x => x.CreatedAt)
                                   join t1 in _context.TblAddresses on t2.AddressId equals t1.AddressId into addr
                                   from t1 in addr.DefaultIfEmpty()
                                   join t3 in _context.MstJobTypes on t2.JobType equals t3.Id into jobType
                                   from t3 in jobType.DefaultIfEmpty()
                                   join t4 in _context.TblUsers on t2.AssignedPersonnel equals t4.Id into assignedUser
                                   from t4 in assignedUser.DefaultIfEmpty()
                                   join p1 in _context.TblUsers on t2.CreatedBy equals p1.Id into createdUser
                                   from p1 in createdUser.DefaultIfEmpty()
                                   join p2 in _context.TblTenants on t2.TenantId equals p2.TenantId into tenant
                                   from p2 in tenant.DefaultIfEmpty()
                                   join t5 in _context.MstJobStatuses on t2.JobStatus equals t5.JobStatusId into jobStatus
                                   from t5 in jobStatus.DefaultIfEmpty()
                                   join t6 in _context.TblContacts.Where(x => x.IsActive == true && x.IsDelete == false) on
                                   t2.ContactId equals t6.ContactId into contact
                                   from t6 in contact.DefaultIfEmpty()
                                   join t7 in _context.MstPowderCoatColors on t2.PowderCoatColorId equals t7.Id into PowderCoatColor
                                   from t7 in PowderCoatColor.DefaultIfEmpty()
                                   where t2.TenantId == claimsProperty.GetTenantId() && t2.IsDelete == false
                                   select new GetAllJobsdetails
                                   {
                                       IsActive = t2.IsActive,
                                       JobId = t2.JobId,
                                       JobName = t2.JobName ?? "",
                                       ContactId = t2.ContactId,
                                       ContactFirstName = t6.FirstName ?? "",
                                       ContactLasttName = t6.LastName ?? "",
                                       Suffix = t6.Suffix ?? "",
                                       AddressId = (t2.AddressId != null || t2.AddressId != Guid.Empty) ? t2.AddressId : null,
                                       JobType = (t2.JobType != null || t2.JobType != Guid.Empty) ? t2.JobType : Guid.Empty,
                                       JobTypeName = t3.JobTypeName != string.Empty ? t3.JobTypeName : "",
                                       AssignedPersonnel = t2.AssignedPersonnel != Guid.Empty ? t2.AssignedPersonnel : Guid.Empty,
                                       AssignedPersonnelName = t2.AssignedPersonnel != null ? t4.FirstName + " " + t4.LastName : "",
                                       ScheduledDate = t2.ScheduledDate != null ? t2.ScheduledDate : null,
                                       JobStatus = (t2.JobStatus != null || t2.JobStatus != Guid.Empty) ? t2.JobStatus : Guid.Empty,
                                       JobStatusName = (t2.JobStatus != null || t2.JobStatus != Guid.Empty) ? t5.JobStatusName : string.Empty,
                                       Notes = (!string.IsNullOrWhiteSpace(t2.Notes) || t2.Notes != null) ? t2.Notes : string.Empty,
                                       AttachmentsAny = string.IsNullOrWhiteSpace(t2.AttachmentAny) ? Array.Empty<string>(): t2.AttachmentAny.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                                       Label = t1.Label != null ? t1.Label : "",
                                       StreetAddress = t1.StreetAddress != null ? t1.StreetAddress : "",
                                       City = t1.City != null ? t1.City : string.Empty,
                                       PowderCoatColorId = t2.PowderCoatColorId,
                                       //State = dbContext.MstStates.Where(s => s.Id == t1.State).Select(y => y.StateName ?? "").FirstOrDefault(),
                                       ZipCode = t1.ZipCode != null ? t1.ZipCode : string.Empty,
                                       JobUniqueCode = t2.JobUniqueId ?? "",
                                       GHL_StageId = t2.GhlStageId != null ? t2.GhlStageId : Guid.Empty,
                                       GHL_StageName = (!string.IsNullOrWhiteSpace(t2.GhlStageName) || t2.GhlStageName != null) ? t2.GhlStageName : "",
                                   }).ToList();
                                   filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                                   var FilterRecords = JobList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                      (string.IsNullOrEmpty(filter.JobStatus) ? true : (filter.JobStatus == x.JobStatus.ToString())) &&
                                      (string.IsNullOrEmpty(filter.Address) ? true : (filter.Address == x.AddressId.ToString())) &&
                                      (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                        (x.JobId.ToString() == filter.SearchValue) ||
                                        (x.ContactId.ToString() == filter.SearchValue) ||
                                        (x.JobStatus.ToString() == filter.SearchValue) ||
                                        (x.AddressId.ToString() == filter.SearchValue)
                                        || (x.JobName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.ContactFirstName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.GHL_StageName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.ContactLasttName ?? "").ToLower().Contains(filter.SearchValue)
                                        || ((x.ContactFirstName??"" )+ " " + (x.ContactLasttName??"") ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobType.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobTypeName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.AssignedPersonnelName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobStatus.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobStatusName?.ToLower()?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.AssignedPersonnel.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.Notes ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.Label ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.StreetAddress ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.State ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.ZipCode ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.City ?? "").ToLower().Contains(filter.SearchValue)))
                                    .ToList();

                    //var TakenRecords = FilterRecords
                    //    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    //    .Take(filter.RecordPerPage)
                    //    .ToList();

                    var queryable = FilterRecords.AsQueryable();

                    queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                    queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                    var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                    responseVm.Data = TakenRecords;
                    responseVm.TotalRecords = FilterRecords.Count;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
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

        public async Task<IResponseVm<string>> UpdateJob(UpdateJob model, Guid? loggedInTenantId = null)
        {
            var responseVm = new ResponseVm<string>();

            try
            {
                var tenantId = GetValidTenantId(loggedInTenantId);

                if (!await IsTenantValid(tenantId))
                    return Fail(responseVm, AppConstant.TenantNotFound);

                var jobStatus = await ResolveJobStatus(model, tenantId);
                var stage = await GetStage(model.GHL_StageId, tenantId);

                if (model.GhlOpportunityId == null)
                {
                    return await HandleNonOpportunityJobUpdate(model, tenantId, jobStatus, stage);
                }
                else
                {
                    return await HandleOpportunityJobUpdate(model, tenantId, jobStatus, stage);
                }
            }
            catch (Exception ex)
            {
                return Fail(responseVm, AppConstant.ExceptionError + ex.Message);
            }
        }

        private async Task<IResponseVm<string>> HandleNonOpportunityJobUpdate(UpdateJob model, Guid? tenantId, Guid jobStatus, TblGhlStage stage)
        {
            var responseVm = new ResponseVm<string>();

            var existingJob = (await GetAll())
                .FirstOrDefault(x => x.JobId == model.JobId && x.ContactId == model.ContactId &&
                                     x.JobUniqueId == model.JobUniqueCode.Trim() &&
                                     x.TenantId == tenantId && x.IsDelete == false);

            if (await IsJobNameDuplicate(model.JobName, (Guid)model.ContactId, tenantId, (Guid)model.JobId))
                return Fail(responseVm, "Job Name " + AppConstant.AlreadyExists);

            if (existingJob == null)
                return Fail(responseVm, AppConstant.NoDataFound);

            UpdateJobDetails(existingJob, model, jobStatus, stage);

            await Update(existingJob);

            responseVm.IsSuccess = true;
            responseVm.Message = AppConstant.UpdateSuccessMsg;

            if (existingJob.GhlOpportunityId != null && (await gHLConfiguration.CheckGHLConfiguration(tenantId)).IsSuccess)
            {
                var ghlUpdateResponse = await UpdateOpportunityInGhl(existingJob);
                FileLogger.Log($"GHL Sync: Job updated in GHL - {ghlUpdateResponse.Message}");
            }

            return responseVm;
        }

        private async Task<IResponseVm<string>> HandleOpportunityJobUpdate(UpdateJob model, Guid? tenantId, Guid jobStatus, TblGhlStage stage)
        {
            var responseVm = new ResponseVm<string>();

            var existingJob = (await GetAll())
                .FirstOrDefault(x => x.GhlOpportunityId == model.GhlOpportunityId && x.TenantId == tenantId && x.IsDelete == false);

            if (await IsJobNameDuplicateWithOpportunity(model.JobName, model.GHL_ContactId, tenantId, model.GhlOpportunityId))
                return Fail(responseVm, "Job Name " + AppConstant.AlreadyExists);

            if (existingJob != null)
            {
                model.AddressId = (existingJob.AddressId != null && existingJob.AddressId != Guid.Empty) ? existingJob.AddressId : model.AddressId;
                model.JobType = (existingJob.JobType != null && existingJob.JobType != Guid.Empty) ? existingJob.JobType : model.JobType;
                UpdateJobDetails(existingJob, model, jobStatus, stage);
                await Update(existingJob);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
                return responseVm;
            }

            var contactId = contact.GetAllQuerable().Where(x => x.GhlcontactId == model.GHL_ContactId && x.IsDelete == false)
                              .Select(x => x.ContactId).FirstOrDefault();

            var createModel = new CreateJob
            {
                JobName = model.JobName,
                JobType = model.JobType,
                AssignedPersonnel = model.AssignedPersonnel,
                AddressId = model.AddressId,
                GHL_ContactId = model.GHL_ContactId,
                GhlOpportunityId = model.GhlOpportunityId,
                GHL_StageId = model.GHL_StageId,
                GHL_StageName = model.GHL_StageName,
                GHL_jobstatus = jobStatus.ToString(),
                lastasync = DateTime.Now,
                locationId = model.locationId,
                ContactId = contactId,
                GHLUserId = model.GHLUserId
            };

            var createResponse = await Create(createModel, tenantId);
            if (createResponse.IsSuccess)
            {
                responseVm.IsSuccess = true;
                responseVm.Message = "Job Created by GHL opportunity.";
            }

            return responseVm;
        }

        private void UpdateJobDetails(TblJob job, UpdateJob model, Guid jobStatus, TblGhlStage stage)
        {
            string attachments = string.Join(", ", model.AttachmentsAny);

            job.JobName = model.JobName?.Trim();
            job.JobType = model.JobType;
            job.AssignedPersonnel = model.AssignedPersonnel;
            job.ScheduledDate = model.ScheduledDate;
            job.JobStatus = jobStatus;
            job.Notes = model.Notes?.Trim() ?? string.Empty;
            job.GhlStageId = model.GHL_StageId;
            job.GhlStageName = stage?.GhlStageName?.Trim() ?? string.Empty;
            job.AddressId = model.AddressId != Guid.Empty ? model.AddressId : null;
            job.AttachmentAny = attachments;
            job.UpdatedAt = DateTime.Now;
            job.ModifiedBy = AppConstant.LoggedInUserID;
            job.GhluserId = model.GHLUserId ?? string.Empty;
        }

        private async Task<TblGhlStage> GetStage(Guid? stageId, Guid? tenantId)
        {
            if (stageId == null || stageId == Guid.Empty)
                return null;

            return await Task.FromResult(
                _context.TblGhlStages
                    .FirstOrDefault(x => x.TenantId == tenantId && x.GhlStageId == stageId.ToString())
            );
        }

        private async Task<bool> IsJobNameDuplicate(string jobName, Guid contactId, Guid? tenantId, Guid jobId)
        {
            return await AnyAsync(x => x.JobId != jobId &&
                                       x.JobName.Trim() == jobName.Trim() &&
                                       x.IsDelete == false &&
                                       x.TenantId == tenantId &&
                                       x.ContactId == contactId);
        }

        private async Task<bool> IsJobNameDuplicateWithOpportunity(string jobName, string ghlContactId, Guid? tenantId, string ghlOpportunityId)
        {
            return await AnyAsync(x => x.GhlOpportunityId != ghlOpportunityId &&
                                       x.JobName.Trim() == jobName.Trim() &&
                                       x.IsDelete == false &&
                                       x.TenantId == tenantId &&
                                       x.GhlContactId == ghlContactId);
        }

        private async Task<Guid> ResolveJobStatus(UpdateJob model, Guid? tenantId)
        {
            if (model.GHL_StageId == null || model.GHL_StageId == Guid.Empty)
                return (Guid)model.JobStatus;

            var jobStatusName = dbContext.MstJobStatuses
                .Where(x => x.JobStatusId == model.JobStatus && x.IsActive == true && x.IsDelete == false)
                .Select(x => x.JobStatusName)
                .FirstOrDefault();

            var jobStatusResponse = await statusAndStageMapping.GetJobStatusByName(jobStatusName ?? string.Empty, model.GHL_StageId, tenantId);

            return (Guid)(jobStatusResponse.IsSuccess ? jobStatusResponse.Data.JobStatusId ?? Guid.Empty : model.JobStatus);
        }

        private IResponseVm<string> Fail(IResponseVm<string> responseVm, string message)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = message;
            return responseVm;
        }

        public async Task<IResponseVm<List<GetAllJobsSubtenant_tenant>>> GetAllJobsTenant_SubTenant(Pagination filter)
        {
            IResponseVm<List<GetAllJobsSubtenant_tenant>> responseVm = new ResponseVm<List<GetAllJobsSubtenant_tenant>>();
            try
            {
               
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false && x.IsActive == true);

                var subTenantlist = (await tenantService.GetAll())
                  .Where(x => x.ParentTenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false)
                  .Select(pt => pt.TenantId)
                  .ToList(); 

                if (IsTenant)
                {
                    var JobList = (from t2 in _context.TblJobs.OrderByDescending(x => x.CreatedAt)
                                  join t1 in _context.TblAddresses on t2.AddressId equals t1.AddressId into addr
                                  from t1 in addr.DefaultIfEmpty() 
                                  join t3 in _context.MstJobTypes on t2.JobType equals t3.Id into jobType
                                  from t3 in jobType.DefaultIfEmpty()
                                  join t4 in _context.TblUsers on t2.AssignedPersonnel equals t4.Id into assignedUser
                                  from t4 in assignedUser.DefaultIfEmpty()
                                  join p1 in _context.TblUsers on t2.CreatedBy equals p1.Id into createdUser
                                  from p1 in createdUser.DefaultIfEmpty()
                                  join p2 in _context.TblTenants on t2.TenantId equals p2.TenantId into tenant
                                  from p2 in tenant.DefaultIfEmpty()
                                  join p3 in _context.TblTenantBrandingInfos on p2.BrandingInfo equals p3.Id into brandinginfo
                                   from p3 in brandinginfo.DefaultIfEmpty()
                                   join t5 in _context.MstJobStatuses on t2.JobStatus equals t5.JobStatusId into jobStatus
                                  from t5 in jobStatus.DefaultIfEmpty()
                                  join t6 in _context.TblContacts on t2.ContactId equals t6.ContactId into contact
                                  from t6 in contact.DefaultIfEmpty()
                                  join s in  _context.MstStates on t1.State equals s.Id into state
                                  from s in state.DefaultIfEmpty()
                                  where t6.IsDelete == false && t6.IsActive == true && t2.IsActive == true && t2.IsDelete == false &&
                                  (t2.TenantId == claimsProperty.GetTenantId() || subTenantlist.Contains((Guid)t2.TenantId))
                                         select new GetAllJobsSubtenant_tenant
                                         {
                                             IsActive = t2.IsActive,
                                             JobId = t2.JobId,
                                             JobName = t2.JobName??"",
                                             JobCode = t2.JobUniqueId??"",
                                             ContactId = t2.ContactId??Guid.Empty,
                                             ContactFirstName = t6.FirstName??"",
                                             ContactLastName = t6.LastName??"",
                                             Suffix = t6.Suffix??"",
                                             ContactEmail = t6.Email??"",
                                             ContactPhone = t6.PhoneNumber??"",
                                             AddressId = t2.AddressId != null ? t2.AddressId : Guid.Empty,
                                             JobType = (t2.JobType != null || t2.JobType != Guid.Empty) ? t3.Id : Guid.Empty,
                                             JobTypeName = (t2.JobType != null || t2.JobType != Guid.Empty) ? t3.JobTypeName : "",
                                             AssignedPersonnel = (t2.AssignedPersonnel != null || t2.AssignedPersonnel != Guid.Empty) ? t4.Id : Guid.Empty,
                                             AssignedPersonnelName = (t2.AssignedPersonnel != null || t2.AssignedPersonnel != Guid.Empty) ? t4.FirstName + " " + t4.LastName : "",
                                             ScheduledDate = t2.ScheduledDate != null ? t2.ScheduledDate : null,
                                             JobStatus = (t2.JobStatus != null || t2.JobStatus != Guid.Empty) ? t2.JobStatus : Guid.Empty,
                                             JobStatusName = (t2.JobStatus != null || t2.JobStatus != Guid.Empty) ? t5.JobStatusName : string.Empty,
                                             Notes = (!string.IsNullOrWhiteSpace(t2.Notes) || t2.Notes != null) ? t2.Notes : string.Empty,
                                             AttachmentsAny = t2.AttachmentAny != null ? t2.AttachmentAny : "",
                                             Label = t1.Label != null ? t1.Label : "",
                                             StreetAddress = t2.AddressId != null ? t1.StreetAddress :"",
                                             City = t2.AddressId != null ? t1.City : "" ,
                                             State = t2.AddressId != null ? s.StateName : "",
                                             ZipCode = t2.AddressId != null ?  t1.ZipCode :"",
                                             JobUniqueCode = t2.JobUniqueId,
                                             CreatedBy = t2.CreatedBy.ToString(),
                                             CreatedbyUsername = p1.FirstName + " " + p1.LastName,
                                             SubtenantName = p2.FirstName + " " + p2.LastName,
                                             CompanyName = p3.CompanyName ?? "",
                                             TenantId = t2.TenantId,
                                             CreatedAt = t2.CreatedAt,
                                             UpdatedAt = t2.UpdatedAt,
                                         }).ToList();

                   
                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();

                    var FilterRecords = JobList
                        .Where(x =>
                            (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                            (string.IsNullOrEmpty(filter.JobStatus) ? true : (filter.JobStatus == x.JobStatus.ToString())) &&
                            (string.IsNullOrEmpty(filter.Address) ? true : (filter.Address == x.AddressId.ToString())) &&
                            (string.IsNullOrEmpty(filter.CustomerId) ? true : (filter.CustomerId == x.ContactId.ToString())) &&
                            (string.IsNullOrEmpty(filter.TenantId) ? true : (Guid.Parse(filter.TenantId) == x.TenantId)) &&
                            (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                (x.JobId.ToString() == filter.SearchValue) ||
                                (x.ContactId.ToString() == filter.SearchValue) ||
                                (x.JobStatus.ToString() == filter.SearchValue) ||
                                (x.AddressId.ToString() == filter.SearchValue)
                                || (x.JobName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.CompanyName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.ContactFirstName ?? "").ToLower().Contains(filter.SearchValue)
                               || ((x.ContactFirstName ?? "") + " " + (x.ContactLastName ?? "")).ToLower().Contains(filter.SearchValue.ToLower())
                                || (x.ContactLastName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.ContactPhone ?? "").Contains(filter.SearchValue)
                                || (x.ContactEmail ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.JobType.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.JobTypeName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.AssignedPersonnelName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.CreatedbyUsername ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.JobStatus.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.AssignedPersonnel.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.Notes.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.Label ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.StreetAddress ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.State ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.ZipCode ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.City ?? "").ToLower().Contains(filter.SearchValue)))
                        .ToList();

                    var queryable = FilterRecords.AsQueryable();

                    queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                    queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                    var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);


                    //var TakenRecords = Paginate.Pagination(FilterRecords, filter);

                    responseVm.Data = TakenRecords;
                    responseVm.TotalRecords = FilterRecords.Count();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
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

        public async Task<IResponseVm<string>> UpdateJobStatus(UpdateJobStatus model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                bool IsTenantvalid = await tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == claimsProperty.GetTenantId());
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = GetAllQuerable().Where(x => x.JobId == model.JobId &&
                  x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId()).FirstOrDefault();
                if (IsJobExist != null)
                {
                    IsJobExist.JobStatus = model.JobStatus;
                    IsJobExist.UpdatedAt = DateTime.Now;
                    IsJobExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsJobExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                if (responseVm.IsSuccess)
                {
                    if (IsJobExist?.GhlOpportunityId != null)
                    {
                        await gHLConfiguration.GetGHLConfiguration(claimsProperty.GetTenantId().ToString());
                        var ghlUpdateResponse = await UpdateOpportunityInGhl(IsJobExist);
                        FileLogger.Log($"GHL Sync: Job updated in GHL - {ghlUpdateResponse.Message}");
                    }
                    CreateNotification notification1 = new CreateNotification()
                    {
                        UserId = AppConstant.LoggedInUserID,
                        Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Job),
                        Body = $"{IsJobExist.JobName} {AppConstant.UpdateStatus_Notification}",
                        CreatedDate = DateTime.Now,
                        Type = Enum.GetName(typeof(NotificationModule), NotificationModule.Job),
                        TypeId = IsJobExist.JobId,
                    };

                    var result = await notification.CreateNotification(notification1);
                }

                //else
                //{
                //    responseVm.Message = AppConstant.NoDataFound;
                //}

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        //Update opportunity(Job) from GHL webhook trigger
        public async Task<IResponseVm<string>> OpportunityFromGHL(GHLOpportunityTrigger opportunitydata)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            {
                try
                {
                    var contact1 = new TblContact();
                    var AssignedPersonnel = Guid.Empty;

                    var tenantId = string.IsNullOrEmpty(opportunitydata.WebhookTenantId)
             ? claimsProperty.GetTenantId().ToString()
             : opportunitydata.WebhookTenantId;

                    var (contact, ghlUser, stageId) = ResolveOpportunityDependencies(opportunitydata);
                    if(contact == null)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Contact Not found or Deleted in Application.";
                        return responseVm;
                    }
                    if (ghlUser != null)
                    {
                        AssignedPersonnel = dbContext.TblUsers
                            .Where(x => x.GhlUserId == ghlUser.GhlUserId && x.IsDelete == false && x.IsActive == true && x.TenantId.ToString() == tenantId)
                            .Select(x => x.Id)
                            .FirstOrDefault();
                    }

                    var job = new CreateJob
                    {
                        AddressId = contact.PrimaryAddress ?? null,
                        ContactId = contact.ContactId ,
                        JobName = opportunitydata.opportunity_name ??"",
                        AssignedPersonnel = (AssignedPersonnel != null && AssignedPersonnel != Guid.Empty) ? AssignedPersonnel : Guid.Empty,
                        GhlOpportunityId = opportunitydata.id,
                        GHL_StageId = stageId == "" ? null : Guid.Parse(stageId),
                        GHL_StageName = opportunitydata.pipleline_stage,
                        GHL_ContactId = opportunitydata.contact_id,
                        lastasync = DateTime.Now,
                        GHLUserId = (ghlUser != null && ghlUser.GhlUserId != null) ? ghlUser.GhlUserId : string.Empty,
                    };
                    await gHLConfiguration.GetGHLConfiguration(opportunitydata.WebhookTenantId);
                    var res = await Create(job, Guid.Parse(opportunitydata.WebhookTenantId));
                    var AllSubetenants = tenantService.GetAllQuerable().Where(x => x.ParentTenantId.ToString() ==
                   opportunitydata.WebhookTenantId && x.IsActive == true && x.IsDelete == false && x.IsGhlconfigSameAsTenant == true)
                     .ToList();
                    if (AllSubetenants != null && AllSubetenants.Count > 0)
                    {
                        foreach (var partner in AllSubetenants)
                        {
                            if (!string.IsNullOrEmpty(partner.GhluserId) && partner.GhluserId.ToLower() == ghlUser.GhlUserId.ToLower())
                            {
                                await gHLConfiguration.GetGHLConfiguration(opportunitydata.WebhookTenantId);
                                var res1 = await Create(job, Guid.Parse(opportunitydata.WebhookTenantId));
                            }
                        }
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = res.Message;
                }
                catch (Exception ex)
                {
                    FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                    responseVm.Message += ex.ToString();
                }
                return responseVm;
            }
        }

        //Update opportunity(Job) from GHL webhook trigger
        public async Task<IResponseVm<string>> UpdateOpportunityFromGHL(GHLOpportunityTrigger opportunitydata)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            {
                try
                {
                    var addressId = Guid.Empty;
                  //  var contact1 = new TblContact();
                    var assignedPersonnel = Guid.Empty;

                    var tenantId = string.IsNullOrEmpty(opportunitydata.WebhookTenantId)
             ? claimsProperty.GetTenantId().ToString()
             : opportunitydata.WebhookTenantId;
                    var (contact1, ghlUser, stageId) = ResolveOpportunityDependencies(opportunitydata);
                    if (contact1 == null)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Contact Not found or Deleted in Application.";
                        return responseVm;
                    }
                    if (ghlUser != null)
                    {
                        assignedPersonnel = dbContext.TblUsers
                            .Where(x => x.GhlUserId == ghlUser.GhlUserId && x.IsDelete == false && x.IsActive == true && x.TenantId.ToString() == tenantId)
                            .Select(x => x.Id)
                            .FirstOrDefault();
                    }

                    var job = new UpdateJob
                    {
                        AddressId = contact1.PrimaryAddress ?? null,
                        ContactId = contact1.ContactId,
                        JobName = opportunitydata.opportunity_name,
                        AssignedPersonnel = (assignedPersonnel != null && assignedPersonnel != Guid.Empty) ? assignedPersonnel : Guid.Empty,
                        GhlOpportunityId = opportunitydata.id,
                        GHL_StageId = Guid.Parse(stageId),
                        GHL_StageName = opportunitydata.pipleline_stage,
                        GHL_ContactId = opportunitydata.contact_id,
                        lastasync = DateTime.Now,
                        GHLUserId = (ghlUser != null && ghlUser.GhlUserId != null) ? ghlUser.GhlUserId : string.Empty,
                    };
                    await gHLConfiguration.GetGHLConfiguration(opportunitydata.WebhookTenantId);
                    var res = await UpdateJob(job, Guid.Parse(opportunitydata.WebhookTenantId));
                    var AllSubetenants = tenantService.GetAllQuerable().Where(x => x.ParentTenantId.ToString() ==
                    opportunitydata.WebhookTenantId && x.IsActive == true && x.IsDelete == false && x.IsGhlconfigSameAsTenant == true)
                      .ToList();
                    if (AllSubetenants != null && AllSubetenants.Count > 0)
                    {
                        foreach (var partner in AllSubetenants)
                        {
                            if(!string.IsNullOrEmpty(partner.GhluserId) && partner.GhluserId.ToLower() == ghlUser.GhlUserId.ToLower())
                            {
                                await gHLConfiguration.GetGHLConfiguration(opportunitydata.WebhookTenantId);
                                var res1 = await UpdateJob(job, Guid.Parse(opportunitydata.WebhookTenantId));
                            }
                        }
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                catch (Exception ex)
                {
                    FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                    responseVm.Message += ex.ToString();
                }
                return responseVm;
            }
        }

        // Resolve dependencies for opportunity data(contactId,StageId,AssignedPersonnel)
        private (TblContact contact, TblGhlUser ghlUser, string stageId) ResolveOpportunityDependencies(GHLOpportunityTrigger opportunitydata)
        {
            TblContact contacts = null;
            TblGhlUser ghlUser = null;
            Guid assignedPersonnel = Guid.Empty;
            string stageId = string.Empty;

            var tenantId = string.IsNullOrEmpty(opportunitydata.WebhookTenantId)
                ? claimsProperty.GetTenantId().ToString()
                : opportunitydata.WebhookTenantId;

            if (opportunitydata != null)
            {
                contacts = contact.GetAllQuerable()
                    .FirstOrDefault(x => x.GhlcontactId == opportunitydata.contact_id && x.IsDelete == false && x.IsActive == true && x.TenantId.ToString() ==tenantId);
            }

            if (opportunitydata.Users != null)
            {
                ghlUser = dbContext.TblGhlUsers
                    .FirstOrDefault(x =>
                        x.GhlEmail == opportunitydata.Users.email &&
                        x.TenantId.ToString() == tenantId &&
                        x.IsDelete == false &&
                        x.IsActive == true);
            }
            if (opportunitydata.pipleline_stage != null)
            {
                stageId = dbContext.TblGhlStages
                    .Where(x => x.GhlStageName.ToLower() == opportunitydata.pipleline_stage.ToLower())
                    .Select(x => x.GhlStageId ?? "")
                    .FirstOrDefault() ?? string.Empty;
            }

            return (contacts, ghlUser, stageId);
        }


        // update Estimate URLs links incoming
        public async Task<IResponseVm<string>> UpdateEstimateUrlInGhlOnly(TblJob job, string newEstimateUrl,decimal Grandtotal)
         {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var ghlconfigDetails =  await gHLConfiguration.GetGHLConfiguration(claimsProperty.GetTenantId().ToString());

                if (string.IsNullOrEmpty(job.GhlOpportunityId))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No associated GHL Opportunity ID found.";
                    FileLogger.Log("UpdateEstimateUrlInGhlOnly: Missing GHL Opportunity ID.");
                    return responseVm;
                }

                if (new[] { job.EstimateUrl1, job.EstimateUrl2, job.EstimateUrl3, job.EstimateUrl4 }
                    .Any(url => url == newEstimateUrl))
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Estimate URL already exists. No update needed.";
                    FileLogger.Log("UpdateEstimateUrlInGhlOnly: URL already exists.");
                    return responseVm;
                }

                string fieldKey = null;
                string valueFieldKey = null;

                if (string.IsNullOrEmpty(job.EstimateUrl1))
                {
                    job.EstimateUrl1 = newEstimateUrl;
                    fieldKey = "opportunity.estimate_url_1";
                    valueFieldKey = "opportunity.estimate_value_1";
                }
                else if (string.IsNullOrEmpty(job.EstimateUrl2))
                {
                    job.EstimateUrl2 = newEstimateUrl;
                    fieldKey = "opportunity.estimate_url_2";
                    valueFieldKey = "opportunity.estimate_value_2";
                }
                else if (string.IsNullOrEmpty(job.EstimateUrl3))
                {
                    job.EstimateUrl3 = newEstimateUrl;
                    fieldKey = "opportunity.estimate_url_3";
                    valueFieldKey = "opportunity.estimate_value_3";
                }
                else if (string.IsNullOrEmpty(job.EstimateUrl4))
                {
                    job.EstimateUrl4 = newEstimateUrl;
                    fieldKey = "opportunity.estimate_url_4";
                    valueFieldKey = "opportunity.estimate_value_4";
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "All estimate URL slots are already filled.";
                    FileLogger.Log("UpdateEstimateUrlInGhlOnly: All URL slots filled.");
                    return responseVm;
                }

         


                var fieldRecord = ghlCustomFieldsService.GetAllQuerable()
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false)
                    .ToList();

                var urlField = fieldRecord.FirstOrDefault(x => x.FieldKey == fieldKey);
                var valueField = fieldRecord.FirstOrDefault(x => x.FieldKey == valueFieldKey);

                if (fieldRecord == null || string.IsNullOrEmpty(urlField.GhlFieldId))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Custom field '{fieldKey}' not configured.";
                    FileLogger.Log("UpdateEstimateUrlInGhlOnly: Missing custom field ID in DB for " + fieldKey);
                    return responseVm;
                }
                if (valueField == null || string.IsNullOrEmpty(valueField.GhlFieldId))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Custom field '{valueFieldKey}' not configured.";
                    FileLogger.Log("UpdateEstimateUrlInGhlOnly: Missing VALUE field ID for " + valueFieldKey);
                    return responseVm;
                }

                await Update(job);

                var payload = new
                {
                    customFields = new[]
                    {
                new
                {
                    id = urlField.GhlFieldId,
                    key = fieldKey,
                    field_value = newEstimateUrl
                },
                 new
                {
                    id = valueField.GhlFieldId,
                    key = valueFieldKey,
                    field_value = Grandtotal.ToString("0.00")
                }

                    }


                };

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ghlconfigDetails?.Data?.Access_Token);
                httpClient.DefaultRequestHeaders.Remove("Version");
                httpClient.DefaultRequestHeaders.Add("Version", AppConstant.GHL_Version);

                var url = $"{AppConstant.GHL_BaseAddress}/opportunities/{job.GhlOpportunityId}";
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Failed to update GHL: {response.StatusCode}, {responseBody}";
                    FileLogger.Log($"UpdateEstimateUrlInGhlOnly: Failed API call. Status: {response.StatusCode}, Body: {responseBody}");
                    return responseVm;
                }

                responseVm.IsSuccess = true;
                responseVm.Message = $"Estimate URL updated in GHL field '{fieldKey}'.";
                FileLogger.Log($"UpdateEstimateUrlInGhlOnly: Success for field {fieldKey}");
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
                FileLogger.Log($"Error in UpdateEstimateUrlInGhlOnly: {ex}");
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateJobPowderCoatColor(Guid? JobId, Guid? powderCoatColorId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var existingJob = await _context.TblJobs.FindAsync(JobId);
                if (existingJob != null)
                {
                    existingJob.PowderCoatColorId = powderCoatColorId;
                    existingJob.UpdatedAt = DateTime.Now;
                    existingJob.ModifiedBy = claimsProperty.GetUserId();
                    
                    _context.TblJobs.Update(existingJob);
                    await _context.SaveChangesAsync();
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Job powder coat color updated successfully.";
                }
                else
                {
                    responseVm.Message = "Job not found.";
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

    }
}
