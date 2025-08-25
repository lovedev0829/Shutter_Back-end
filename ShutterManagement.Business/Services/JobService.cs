using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509.Qualified;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.GHL.Services;
using ShutterManagement.Services;
using ShutterManagment.GHL.ViewModel.Request;
using ShutterManagment.GHL.ViewModel.Response;
using Stripe;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using static QRCoder.PayloadGenerator.SwissQrCode;
using static ShutterManagement.Core.ViewModels.Response.GetAllJobsSubtenant_tenant;

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
            return await AnyAsync(x => x.JobName == jobName.Trim() && x.IsDelete == false && x.IsActive == true && x.TenantId == tenantId && x.ContactId == contactId);
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
            var jobStatusName = string.Empty;
            if (model.GHL_StageId == null && string.IsNullOrEmpty(model.GHL_StageId.ToString()))
            { jobStatusName = EnumValue.GetEnumValue(JobStatus.New); }

          
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
            int Version = GetAllQuerable().Where(x => x.TenantId == tenantId).OrderByDescending(p => p.CreatedAt)
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
            IResponseVm<string> res = new ResponseVm<string>();
            try
            {
                var tenantId = GetValidTenantId(loggedInTenantId);
                var validationResult = await ValidateJobRequestAsync(model, tenantId);
                if (string.IsNullOrEmpty(model.GHL_StageId.ToString()))
                {
                    model.GHL_StageId = Guid.Parse(validationResult.Data.StageId);
                }
                if (string.IsNullOrEmpty(model.IsJobCreate.ToString()) && string.IsNullOrEmpty(model.ExistingJobId) && string.IsNullOrEmpty(model.ExistingOpportunityId))
                {
                    var IsMultipleOpportunityExist = await GetGHLOpportunity(model.ContactId, loggedInTenantId);
                    res.IsSuccess = IsMultipleOpportunityExist.IsSuccess;
                    res.Message = IsMultipleOpportunityExist.Message;

                    if (IsMultipleOpportunityExist.IsSuccess && IsMultipleOpportunityExist.Data?.opportunities?.Count > 0)
                    {
                        var isSubtenant = tenantService.GetAllQuerable()
                            .Where(x => x.TenantId == tenantId
                                && !string.IsNullOrEmpty(x.ParentTenantId.ToString())
                                && x.IsActive == true
                                && x.IsDelete == false
                                && x.IsGhlconfigSameAsTenant == true)
                            .Select(x => x.GhluserId ?? string.Empty)
                            .FirstOrDefault();

                        var wrapper = new GHLJobAssignedtoPartner();

                        // If subtenant exists, filter opportunities by assigned user
                        if (!string.IsNullOrEmpty(isSubtenant))
                        {
                            wrapper.opportunities = IsMultipleOpportunityExist.Data.opportunities
                                .Where(o => o.assignedTo == isSubtenant)
                                .ToList();
                        }
                        else
                        {
                            wrapper.opportunities = IsMultipleOpportunityExist.Data.opportunities;
                        }

                        res.Data = System.Text.Json.JsonSerializer.Serialize(wrapper);
                        return res;
                    }
                    else
                    {
                        model.IsJobCreate = true;
                    }
                }
                if(model.IsJobCreate ?? false && string.IsNullOrEmpty(model.ExistingJobId) && string.IsNullOrEmpty(model.ExistingOpportunityId))
                {
                    res = await JobOpportunityCreate(model,loggedInTenantId);
                    if(res.IsSuccess)
                    {
                        var GHLUserId = GetGHLUserId(model.AssignedPersonnel);
                        var insertedJob = new TblJob
                        {
                            JobName = model.JobName,
                            JobStatus = validationResult.Data.JobStatus,
                            AssignedPersonnel = model.AssignedPersonnel,
                            ContactId = model.ContactId,
                            JobId = Guid.Parse(res.Data),
                            GhlStageId = model.GHL_StageId ?? Guid.Empty
                        };
                       await HandleGhlOpportunityCreationAsync(insertedJob, tenantId);
                    }
                    return res;
                }
                else if(!(model.IsJobCreate ?? false) && !string.IsNullOrEmpty(model.ExistingJobId) && !string.IsNullOrEmpty(model.ExistingOpportunityId))
                {
                   res = await JobOpportunityUpdate(model, loggedInTenantId);
                    if (res.IsSuccess)
                    {
                        var GHLUserId = GetGHLUserId(model.AssignedPersonnel);
                        var insertedJob = new TblJob
                        {
                            JobName = model.JobName,
                            JobStatus = validationResult.Data.JobStatus,
                            AssignedPersonnel = model.AssignedPersonnel,
                            ContactId = model.ContactId,
                            GhlOpportunityId = model.ExistingOpportunityId,
                            GhlStageId = model.GHL_StageId ?? Guid.Empty
                        };
                        await UpdateOpportunityInGhl(insertedJob, tenantId);
                    }
                    return res;
                }
                else if (!(model.IsJobCreate ?? false)  && string.IsNullOrEmpty(model.ExistingJobId) && !string.IsNullOrEmpty(model.ExistingOpportunityId))
                {
                    res = await JobOpportunityCreate(model, loggedInTenantId);
                    if (res.IsSuccess)
                    {
                        var GHLUserId = GetGHLUserId(model.AssignedPersonnel);
                        var insertedJob = new TblJob
                        {
                            JobName = model.JobName,
                            JobStatus = validationResult.Data.JobStatus,
                            AssignedPersonnel = model.AssignedPersonnel,
                            ContactId = model.ContactId,
                            GhlOpportunityId = model.ExistingOpportunityId,
                        };
                        await UpdateOpportunityInGhl(insertedJob, tenantId);
                    }
                    return res;
                }
                //    if (!validationResult.IsSuccess)
                //    return BuildErrorResponse(validationResult.Message);

                //var job = await BuildJobAsync(model, tenantId, validationResult.Data.JobStatus, validationResult.Data.StageName, validationResult.Data.StageId);

                //var result = await ProcessJobCreationAsync(job, model, tenantId);
                //if(result.IsSuccess)
                //{
                //    result.Message = result.Message;
                //}
            }
            catch(Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        private async Task<IResponseVm<(Guid JobStatus, string StageName,string StageId)>> ValidateJobRequestAsync(CreateJob model, Guid? tenantId)
        {
            var response = new ResponseVm<(Guid, string,string)>();

            if (!await IsTenantValid(tenantId))
                return BuildErrorResponse<(Guid, string,string)>(AppConstant.TenantNotFound);

            if (await IsContactInactiveOrArchived((Guid)model.ContactId, tenantId))
                return BuildErrorResponse<(Guid, string,string)>("Contact is Archived or Inactive, You cannot create Job.");
            //if (string.IsNullOrEmpty(model.GhlOpportunityId))
            //{
            //    if (await IsJobNameDuplicate(model.JobName, (Guid)model.ContactId, tenantId))
            //        return BuildErrorResponse<(Guid, string,string)>("JobName " + AppConstant.AlreadyExists);
            //}
            //else
            //{
            //    if (await IsGHLOpportunityExist((Guid)model.ContactId,tenantId,model.GhlOpportunityId))
            //    {
            //        var job = new UpdateJob
            //        {
            //            AddressId = model.AddressId ?? null,
            //            ContactId = model.ContactId,
            //            JobName = model.JobName,
            //            AssignedPersonnel = model.AssignedPersonnel ?? null,
            //            GhlOpportunityId = model.GhlOpportunityId,
            //            GHL_StageId = model.GHL_StageId,
            //            GHL_StageName = model.GHL_StageName,
            //            GHL_ContactId = model.GHL_ContactId,
            //            lastasync = DateTime.Now,
            //            GHLUserId = model.GHLUserId ?? null,
            //        };
            //        await gHLConfiguration.GetGHLConfiguration(tenantId.ToString());
            //        var res = await UpdateJob(job, tenantId);
            //        response.IsSuccess = res.IsSuccess;
            //        response.Message = res.Message;
            //        return response;
            //    }
            //}

            if (model.GhlOpportunityId == null && !await ValidateAddress(model, tenantId))
                return BuildErrorResponse<(Guid, string,string)>("Address " + AppConstant.NoDataFound);
           
            var jobStatus = await ResolveJobStatus(model, tenantId);
            var stageResponse = await statusAndStageMapping.GetGHLStagesMappingsById(jobStatus, tenantId);
            if (!stageResponse.IsSuccess)
            {
                var res = await gHLConfiguration.GetGHLConfiguration(tenantId.ToString());
                if(res.IsSuccess)
                {
                    response.IsSuccess = stageResponse.IsSuccess;
                    response.Message = stageResponse.Message;
                    return response;
                }
            }
            var globalStageId = string.IsNullOrEmpty(model.GHL_StageId.ToString()) ? stageResponse?.Data?.StageId ?? Guid.Empty : model?.GHL_StageId ?? Guid.Empty;
            var stage = _context.TblGhlStages.Where(x => x.TenantId == tenantId && x.GhlStageId == globalStageId.ToString()).FirstOrDefault();
            string stageName = model.GHL_StageName ?? stage?.GhlStageName ?? string.Empty;

            response.IsSuccess = true;
            response.Data = (jobStatus, stageName, globalStageId.ToString());
            return response;
        }
        private async Task<bool> IsGHLOpportunityExist(Guid contactId, Guid? tenantId,string? GHLOpportunityId)
        {
            return await AnyAsync(x => x.ContactId == contactId && x.TenantId == tenantId && x.IsActive == true && x.IsDelete == false && 
            ((string.IsNullOrEmpty(GHLOpportunityId) ? false : x.GhlOpportunityId == GHLOpportunityId)));
        }

        private async Task<TblJob> BuildJobAsync(CreateJob model, Guid? tenantId, Guid jobStatus, string stageName,string StageId)
        {
            string jobUniqueId = await GenerateJobUniqueId(tenantId);
            string arrayString = string.Join(", ", model.AttachmentsAny);
            var GHLUserId = string.Empty;
            if (!string.IsNullOrEmpty(model.AssignedPersonnel.ToString()))
            {
                GHLUserId = dbContext.TblUsers.Where(x => x.Id == model.AssignedPersonnel && x.IsDelete == false && x.IsActive == true).Select(u => u.GhlUserId ?? string.Empty).FirstOrDefault();
            }
            return new TblJob
            {
                AddressId = model.AddressId != Guid.Empty ? model.AddressId : null,
                ContactId = model.ContactId,
                JobName = model.JobName?.Trim(),
                JobType = model.JobType != Guid.Empty ? model.JobType : null,
                AssignedPersonnel = model.AssignedPersonnel ?? Guid.Empty,
                ScheduledDate = model.ScheduledDate,
                JobStatus = (!string.IsNullOrEmpty(jobStatus.ToString()) && jobStatus != Guid.Empty) ? jobStatus : Guid.Empty,
                Notes = model.Notes ?? string.Empty,
                AttachmentAny = arrayString ?? null,
                JobUniqueId = jobUniqueId,
                GhlStageId = (!string.IsNullOrEmpty(model.GHL_StageId.ToString()) && model.GHL_StageId != Guid.Empty) ? model.GHL_StageId : Guid.Parse(StageId),
                GhlStageName = (!string.IsNullOrEmpty(stageName)) ? stageName.Trim() : string.Empty,
                GhlContactId = model.GHL_ContactId ?? string.Empty,
                GhlOpportunityId = model.GhlOpportunityId ?? string.Empty,
                GhluserId = (string.IsNullOrEmpty(model.GHLUserId)) ? GHLUserId : model.GHLUserId,
                TenantId = tenantId,
                IsActive = true,
                IsDelete = false,
                CreatedAt = DateTime.Now,
                CreatedBy = claimsProperty.GetUserId() ?? Guid.Empty,
            };
        }

        private async Task<IResponseVm<string>> ProcessJobCreationAsync(TblJob job, CreateJob model, Guid? tenantId)
        {
            var response = new ResponseVm<string>();
            var insertedJob = new TblJob();
            if (string.IsNullOrEmpty(model.GhlOpportunityId) && !await IsGHLOpportunityExist((Guid)model.ContactId, tenantId, model.GhlOpportunityId))
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
                insertedJob.GhlContactId = model.GHL_ContactId;
                insertedJob.LastAsync = DateTime.Now;
                await Update(insertedJob);
            }

            var customerDetails = dbContext.TblContacts.Where(x => x.ContactId == job.ContactId).FirstOrDefault();

            var jobvm = BuildJobVm(job, customerDetails, tenantId);
            var appmsg = notification.SendInAppNotification(jobvm, EnumValue.GetEnumValue(Estimation.JobCreate).ToString(),jobvm.JobId,NotificationModule.Job);


            return response;
        }

        private EstimateOrOrderBaseVm BuildJobVm(TblJob jobdetails , TblContact tblContact, Guid? tenantId)
        {
            return new EstimateOrOrderBaseVm
            {
                JobId = jobdetails?.JobId,
                JobCode = jobdetails?.JobUniqueId,
                CustomerName = $"{tblContact?.FirstName ?? ""} {tblContact?.LastName ?? ""}".Trim(),
                TenantId = tenantId,
            };
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

        //private async Task CreateJobNotificationAsync(TblJob job, Guid? tenantId)
        //{
        //    var notificationData = new CreateNotification
        //    {
        //        UserId = tenantId,
        //        Subject = nameof(NotificationModule.Job),
        //        Body = $"{job.JobName} {AppConstant.Create_Notification}",
        //        CreatedDate = DateTime.Now,
        //        Type = nameof(NotificationModule.Job),
        //        TypeId = job.JobId
        //    };

        //    await notification.CreateNotification(notificationData);
        //}


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

                    var ghlOpportunitiesResponse = await gHLService_V2.GetOpportunitiesFromGhl(ghlconfigdetails.Data);
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

                            var responseghl = await gHLService_V2.CreateOpportunityInGhl(ghlconfigdetails.Data, job);
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


                bool IsGhlconfigSameAsTenant = false;
                var IsGHLConfigurationTenantExist = dbContext.TblTenants.Where(x => x.TenantId == LoggedInTenantId && !string.IsNullOrEmpty(x.ParentTenantId.ToString()) && x.IsDelete == false && x.IsActive == true).FirstOrDefault();
                if (IsGHLConfigurationTenantExist?.IsGhlconfigSameAsTenant ?? false)
                {
                    IsGhlconfigSameAsTenant = true;
                }
                LoggedInTenantId = IsGhlconfigSameAsTenant ? IsGHLConfigurationTenantExist.ParentTenantId : LoggedInTenantId;
                var ghlConfigDetails = await gHLConfiguration.GetGHLConfiguration(LoggedInTenantId.ToString());
               
                var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration(LoggedInTenantId);
                if (Checkghlresponse.IsSuccess)
                {
                    var gHLStages = dbContext.TblGhlStages
                   .Where(x => x.TenantId == LoggedInTenantId && (x.GhlPipelineName.ToLower() == ghlConfigDetails.Data.PipelineId.ToLower() || x.GhlPipelineId.ToLower()
                   == ghlConfigDetails.Data.PipelineId.ToLower())).OrderBy(x => x.StageOrder)
                   .ToList();
                    //var gHLStages = dbContext.TblGhlStages
                    //.Where(x => x.TenantId == LoggedInTenantId).OrderByDescending(x => x.CreatedAt)
                    //.ToList();

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
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
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

                bool IsGhlconfigSameAsTenant = false;
                var IsGHLConfigurationTenantExist = dbContext.TblTenants.Where(x => x.TenantId == LoggedInTenantId && !string.IsNullOrEmpty(x.ParentTenantId.ToString()) && x.IsDelete == false && x.IsActive == true).FirstOrDefault();
                if (IsGHLConfigurationTenantExist?.IsGhlconfigSameAsTenant ?? false)
                {
                    IsGhlconfigSameAsTenant = true;
                }

                LoggedInTenantId = IsGhlconfigSameAsTenant ? IsGHLConfigurationTenantExist.ParentTenantId : LoggedInTenantId;
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



        #endregion Ghl CRM related methods




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

                var logdinTenantId = claimsProperty.GetTenantId();

                var subTenantIds = new List<Guid>();
                if (string.IsNullOrEmpty(claimsProperty.GetParentTenantId()))
                {
                    subTenantIds = tenantService.GetAccessibleTenantIds(logdinTenantId);
                }

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
                                   where (t2.TenantId == logdinTenantId || subTenantIds.Contains(t2.TenantId ?? Guid.Empty) || p2.ParentTenantId == logdinTenantId) && t2.IsDelete == false
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
                                       TenantId = t2.TenantId,
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
                                     x.TenantId == tenantId && x.IsDelete == false);

            if (await IsJobNameDuplicate(model.JobName, (Guid)model.ContactId, tenantId, (Guid)model.JobId))
                return Fail(responseVm, "Job Name " + AppConstant.AlreadyExists);

            if (existingJob == null)
                return Fail(responseVm, AppConstant.NoDataFound);

            UpdateJobDetails(existingJob, model, jobStatus, stage);

            await Update(existingJob);

            responseVm.IsSuccess = true;
            responseVm.Message = AppConstant.UpdateSuccessMsg;

            if (!string.IsNullOrEmpty(existingJob.GhlOpportunityId) && (await gHLConfiguration.CheckGHLConfiguration(tenantId)).IsSuccess)
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
            job.ModifiedBy = claimsProperty.GetUserId() ?? Guid.Empty;
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
                                       x.GhlContactId != ghlContactId);
        }

        private async Task<Guid> ResolveJobStatus(UpdateJob model, Guid? tenantId)
        {
            if (model.GHL_StageId == null || model.GHL_StageId == Guid.Empty)
                return (Guid)model.JobStatus;

            var statusId = dbContext.TblGhlStagesmappings.Where(x => x.StageId == model.GHL_StageId && x.TenantId == tenantId).Select(x => x.StatusId).FirstOrDefault();

            var jobStatusName = dbContext.MstJobStatuses
                .Where(x => x.JobStatusId == (model.JobStatus != null && model.JobStatus != Guid.Empty ? model.JobStatus : statusId) && x.IsActive == true && x.IsDelete == false)
                .Select(x => x.JobStatusName)
                .FirstOrDefault();
            if(jobStatusName == null)
            {
                jobStatusName = EnumValue.GetEnumValue(JobStatus.New);
            }
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
                    responseVm.TotalRecords = TakenRecords.Count();
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

                    var customerDetails = dbContext.TblContacts.Where(x => x.ContactId == IsJobExist.ContactId).FirstOrDefault();

                    var jobvm = BuildJobVm(IsJobExist, customerDetails, claimsProperty.GetTenantId());
                    var appmsg = notification.SendInAppNotification(jobvm, EnumValue.GetEnumValue(Estimation.JobUpdate).ToString(), jobvm.JobId, NotificationModule.Job);
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
            var responseVm = new ResponseVm<string>();

            try
            {
                var tenantId = string.IsNullOrEmpty(opportunitydata.WebhookTenantId)
                    ? claimsProperty.GetTenantId().ToString()
                    : opportunitydata.WebhookTenantId;

                var (contact, ghlUser, stageId) = ResolveOpportunityDependencies(opportunitydata);
                if (contact == null)
                    return BuildErrorResponse("Contact not found or deleted in application.");

                var assignedPersonnel = GetAssignedPersonnel(ghlUser, tenantId);

                var isOpportunityExist = GetOpportunity(opportunitydata.contact_id,opportunitydata.id,tenantId);

                if (isOpportunityExist == null)
                {
                    // Create
                    var createJob = BuildCreateJob(contact, opportunitydata, ghlUser, assignedPersonnel, stageId);
                    var res = await JobOpportunityCreate(createJob, Guid.Parse(tenantId));

                    await ProcessSubTenants(opportunitydata, ghlUser, contact, assignedPersonnel, stageId, false);

                    responseVm.IsSuccess = res.IsSuccess;
                    responseVm.Message = res.Message;
                }
                else
                {
                    // Update

                    var updateJob = BuildUpdateJob(isOpportunityExist.JobId, contact, opportunitydata, ghlUser, assignedPersonnel, stageId);
                    var res = await JobOpportunityUpdate(updateJob, Guid.Parse(tenantId));

                    await ProcessSubTenants(opportunitydata, ghlUser, contact, assignedPersonnel, stageId, true);

                    responseVm.IsSuccess = res.IsSuccess;
                    responseVm.Message = res.Message;
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log("Error while creating/updating opportunity: {ExceptionMessage}", ex.Message);
                responseVm.IsSuccess = false;
                responseVm.Message = ex.ToString();
            }

            return responseVm;
        }



        //Update opportunity(Job) from GHL webhook trigger

        public async Task<IResponseVm<string>> UpdateOpportunityFromGHL(GHLOpportunityTrigger opportunityData)
        {
            var responseVm = new ResponseVm<string>();

            try
            {
                // Get tenant ID (webhook or current)
                var tenantId = string.IsNullOrEmpty(opportunityData.WebhookTenantId)
                    ? claimsProperty.GetTenantId().ToString()
                    : opportunityData.WebhookTenantId;

                // Check if opportunity already exists for tenant
                var existingOpportunity = GetOpportunity(opportunityData.contact_id, opportunityData.id, tenantId);

                // Resolve contact, user, and stage ID
                var (contact, ghlUser, stageId) = ResolveOpportunityDependencies(opportunityData);
                if (contact == null)
                    return BuildErrorResponse("Contact not found or deleted in application.");

                // Get assigned personnel
                var assignedPersonnel = GetAssignedPersonnel(ghlUser, tenantId);

                await gHLConfiguration.GetGHLConfiguration(tenantId);

                if (existingOpportunity != null)
                {
                    var updateJob = BuildUpdateJob(existingOpportunity.JobId, contact, opportunityData, ghlUser, assignedPersonnel, stageId);
                    var res = await JobOpportunityUpdate(updateJob, Guid.Parse(tenantId));

                    await ProcessSubTenants(opportunityData, ghlUser, contact, assignedPersonnel, stageId, true);
                    responseVm.IsSuccess = res.IsSuccess;
                    responseVm.Message = res.Message;
                }
                else
                {
                    var createJob = BuildCreateJob(contact, opportunityData, ghlUser, assignedPersonnel, stageId);
                    var res = await JobOpportunityCreate(createJob, Guid.Parse(tenantId));

                    await ProcessSubTenants(opportunityData, ghlUser, contact, assignedPersonnel, stageId, false);
                    responseVm.IsSuccess = res.IsSuccess;
                    responseVm.Message = res.Message;
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log("Error while creating/updating opportunity: {ExceptionMessage}", ex.Message);
                return BuildErrorResponse(ex.ToString());
            }

            return responseVm;
        }

        #region Helper Methods

        private TblJob GetOpportunity(string contactId, string opportunityId, string tenantId) =>
            GetAllQuerable().FirstOrDefault(x =>
                x.GhlContactId == contactId &&
                x.GhlOpportunityId == opportunityId &&
                x.TenantId.ToString() == tenantId &&
                x.IsActive == true &&
                x.IsDelete == false);

        private Guid GetAssignedPersonnel(TblGhlUser ghlUser, string tenantId) =>
            ghlUser != null
                ? dbContext.TblUsers
                    .Where(x => x.GhlUserId == ghlUser.GhlUserId &&
                                x.IsDelete == false &&
                                x.IsActive  == true &&
                                x.TenantId.ToString() == tenantId)
                    .Select(x => x.Id)
                    .FirstOrDefault()
                : Guid.Empty;

        private UpdateJob BuildUpdateJob(Guid jobId, TblContact contact, GHLOpportunityTrigger opportunityData, TblGhlUser ghlUser, Guid assignedPersonnel, string stageId) =>
            new UpdateJob
            {
                ExistingJobId = jobId.ToString(),
                AddressId = contact.PrimaryAddress,
                ContactId = contact.ContactId,
                JobName = opportunityData.opportunity_name,
                AssignedPersonnel = assignedPersonnel != Guid.Empty ? assignedPersonnel : Guid.Empty,
                ExistingOpportunityId = opportunityData.id,
                GHL_StageId = Guid.Parse(stageId),
                GHL_StageName = opportunityData.pipleline_stage,
                GHL_ContactId = opportunityData.contact_id,
                lastasync = DateTime.Now,
                GHLUserId = ghlUser?.GhlUserId ?? string.Empty,
                JobId = jobId
            };

        private CreateJob BuildCreateJob(TblContact contact, GHLOpportunityTrigger opportunityData, TblGhlUser ghlUser, Guid assignedPersonnel, string stageId) =>
            new CreateJob
            {
                AddressId = contact.PrimaryAddress,
                ContactId = contact.ContactId,
                JobName = opportunityData.opportunity_name ?? "",
                AssignedPersonnel = assignedPersonnel != Guid.Empty ? assignedPersonnel : Guid.Empty,
                GhlOpportunityId = opportunityData.id,
                GHL_StageId = string.IsNullOrEmpty(stageId) ? null : Guid.Parse(stageId),
                GHL_StageName = opportunityData.pipleline_stage,
                GHL_ContactId = opportunityData.contact_id,
                lastasync = DateTime.Now,
                GHLUserId = ghlUser?.GhlUserId ?? string.Empty,
                ExistingOpportunityId = opportunityData.id
            };

        private async Task ProcessSubTenants(GHLOpportunityTrigger opportunityData, TblGhlUser ghlUser, TblContact contact, Guid assignedPersonnel, string stageId, bool isUpdate)
        {
            var subTenants = tenantService.GetAllQuerable()
                .Where(x => x.ParentTenantId.ToString() == opportunityData.WebhookTenantId &&
                            x.IsActive == true &&
                            x.IsDelete == false &&
                            x.IsGhlconfigSameAsTenant == true)
                .ToList();
          

            foreach (var partner in subTenants)
            {
                if (!string.IsNullOrEmpty(partner.GhluserId) &&
                    ghlUser != null &&
                    partner.GhluserId.Equals(partner.GhluserId, StringComparison.OrdinalIgnoreCase))
                {
                    opportunityData.WebhookTenantId = partner.TenantId.ToString();
                    var (contact1, ghlUser1, stageId1) = ResolveOpportunityDependencies(opportunityData);
                    await gHLConfiguration.GetGHLConfiguration(opportunityData.WebhookTenantId);

                    var existingOpp = GetOpportunity(opportunityData.contact_id, opportunityData.id, partner.TenantId.ToString());

                    if (isUpdate && existingOpp != null)
                    {
                        var updateJob = BuildUpdateJob(existingOpp.JobId, contact1, opportunityData, ghlUser, assignedPersonnel, stageId);
                        await JobOpportunityUpdate(updateJob, partner.TenantId);
                    }
                    else if (!isUpdate)
                    {
                        var createJob = BuildCreateJob(contact1, opportunityData, ghlUser1, assignedPersonnel, stageId1);
                        await JobOpportunityCreate(createJob, partner.TenantId);
                    }
                }
            }
        }

        private IResponseVm<string> BuildResponse(IResponseVm<string> res) =>
            new ResponseVm<string> { IsSuccess = res.IsSuccess, Message = res.Message };

        #endregion



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


        public async Task<IResponseVm<GHLJobAssignedtoPartner>> GetGHLOpportunity(Guid? ContactId,Guid? LoggedInTenantId = null)
        {
            IResponseVm<GHLJobAssignedtoPartner> response = new ResponseVm<GHLJobAssignedtoPartner>();
            try
            {
                LoggedInTenantId = (LoggedInTenantId != null && LoggedInTenantId != Guid.Empty)
                    ? LoggedInTenantId.Value
                    : claimsProperty.GetTenantId();
                var res = new GHLJobAssignedtoPartner();
                var IsGHLContact = contact.GetAllQuerable().Where(x => x.ContactId == ContactId && x.IsActive == true && x.TenantId == LoggedInTenantId
                && x.IsArchive == false && x.IsDelete == false).FirstOrDefault();
                if (!string.IsNullOrEmpty(IsGHLContact.GhlcontactId))
                {
                    var GetGHLCongig = await gHLConfiguration.GetGHLConfiguration(LoggedInTenantId.ToString());
                    var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration(LoggedInTenantId);
                    if (Checkghlresponse.IsSuccess)
                    {

                        var GetOpportunity = await gHLService_V2.GetOpportunityforPartner(GetGHLCongig.Data, null, IsGHLContact.GhlcontactId);
                        if (GetOpportunity.IsSuccess && GetOpportunity.Data != null && GetOpportunity.Data.opportunities.Count >= 1)
                        {
                            foreach (var item in GetOpportunity.Data.opportunities)
                            {
                                var piplineName = dbContext.TblGhlStages.Where(x => x.GhlPipelineName.ToLower() == GetGHLCongig.Data.PipelineId.ToLower() && x.IsActive == true).
                                  Select(x => x.GhlPipelineName).FirstOrDefault();
                                item.pipelineName = piplineName;
                                var Job = GetAllQuerable().Where(x => x.ContactId == ContactId && x.GhlOpportunityId == item.id && x.TenantId == LoggedInTenantId &&
                                x.IsActive == true && x.IsDelete == false).FirstOrDefault();
                                item.JobId = Job != null ? Job.JobId.ToString() : string.Empty ;
                                item.JobName = Job != null ? Job.JobName : string.Empty;
                                var res1 = await  GetGHLStages(LoggedInTenantId);
                                var stage = res1?.Data?.Where(x => x.GhlStageId.ToString().ToLower() == item.pipelineStageId.ToLower()).Select(x => x.GhlStageName ?? string.Empty).FirstOrDefault() ?? string.Empty;
                                item.pipelineStageName = stage;
                            }

                            res = GetOpportunity.Data;

                            response.IsSuccess = true;
                            response.Data = res;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<IResponseVm<string>> JobOpportunityCreate(CreateJob model, Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                LoggedInTenantId = (LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId: claimsProperty.GetTenantId();
                if (!await IsTenantValid(LoggedInTenantId))
                    return BuildErrorResponse<string>(AppConstant.TenantNotFound);

                if (await IsContactInactiveOrArchived((Guid)model.ContactId, LoggedInTenantId))
                    return BuildErrorResponse<string>("Contact is Archived or Inactive, You cannot create Job.");

                if (await IsJobNameDuplicate(model.JobName, (Guid)model.ContactId, LoggedInTenantId))
                    return BuildErrorResponse<string>("JobName " + AppConstant.AlreadyExists);

                var jobStatus = await ResolveJobStatus(model, LoggedInTenantId);
                var stageResponse = await statusAndStageMapping.GetGHLStagesMappingsById(jobStatus, LoggedInTenantId);
                if (!stageResponse.IsSuccess)
                {
                    var res = await gHLConfiguration.GetGHLConfiguration(LoggedInTenantId.ToString());
                    if (res.IsSuccess)
                    {
                        response.IsSuccess = stageResponse.IsSuccess;
                        response.Message = stageResponse.Message;
                        return response;
                    }
                }
                var globalStageId = string.IsNullOrEmpty(model.GHL_StageId.ToString()) ? stageResponse?.Data?.StageId ?? Guid.Empty : model?.GHL_StageId ?? Guid.Empty;
                var stage = _context.TblGhlStages.Where(x => x.TenantId == LoggedInTenantId && x.GhlStageId == globalStageId.ToString()).FirstOrDefault();
                string stageName = model.GHL_StageName ?? stage?.GhlStageName ?? string.Empty;
                model.GhlOpportunityId = model.ExistingOpportunityId ?? string.Empty;
                var job = await BuildJobAsync(model, LoggedInTenantId, jobStatus, stageName, globalStageId.ToString());
                var res1 = await Add(job);
                response.IsSuccess = true;
                response.Data = res1.JobId.ToString();
                response.Message = AppConstant.InsertSuccessMsg;

            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<IResponseVm<string>> JobOpportunityUpdate(CreateJob model, Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                LoggedInTenantId = (LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId();
                if (!await IsTenantValid(LoggedInTenantId))
                    return BuildErrorResponse<string>(AppConstant.TenantNotFound);

                if (await IsContactInactiveOrArchived((Guid)model.ContactId, LoggedInTenantId))
                    return BuildErrorResponse<string>("Contact is Archived or Inactive, You cannot create Job.");

                if (await IsJobNameDuplicate(model.JobName, (Guid)model.ContactId, LoggedInTenantId,Guid.Parse(model.ExistingJobId)))
                    return BuildErrorResponse<string>("JobName " + AppConstant.AlreadyExists);

                var GhlContactId = contact.GetAllQuerable().Where(x => x.ContactId == model.ContactId && x.IsActive == true && x.IsDelete == false && x.TenantId
                == LoggedInTenantId).Select(x => x.GhlcontactId ?? string.Empty).FirstOrDefault();
                var jobStatus = await ResolveJobStatus(model, LoggedInTenantId);
                var stage = await GetStage(model.GHL_StageId, LoggedInTenantId);
                var job = new UpdateJob
                {
                    JobId = Guid.Parse(model.ExistingJobId),
                    JobName = model.JobName?.Trim(),
                    JobType = model.JobType,
                    AssignedPersonnel = model.AssignedPersonnel,
                    ScheduledDate = model.ScheduledDate,
                    JobStatus = jobStatus,
                    Notes = model.Notes?.Trim() ?? string.Empty,
                    AddressId = model.AddressId != Guid.Empty ? model.AddressId : null,
                    AttachmentsAny = model.AttachmentsAny ?? null,
                    GhlOpportunityId = model.ExistingOpportunityId,
                    GHL_StageId = model.GHL_StageId,
                    GHL_ContactId = GhlContactId,
                    lastasync = DateTime.Now
                };

                var existingJob = (await GetAll())
                .FirstOrDefault(x => x.JobId == job.JobId && x.ContactId == model.ContactId &&
                                     x.TenantId == LoggedInTenantId && x.IsDelete == false);


                if (existingJob == null)
                    return Fail(response, AppConstant.NoDataFound);

                UpdateJobDetails(existingJob, job, jobStatus, stage);

                await Update(existingJob);

                response.IsSuccess = true;
                response.Message = AppConstant.UpdateSuccessMsg;

            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        private string GetGHLUserId(Guid? AssignedPersonnel)
        {
            var GHUserId = dbContext.TblUsers.Where(x => x.Id == AssignedPersonnel && x.IsDelete == false && x.IsActive == true)
                  .Select(y => y.GhlUserId).FirstOrDefault() ?? "";
            return GHUserId;
        }
    }
}
