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
        private string BaseUrl = AppConstant.GHL_BaseAddress;

        public JobService(ShutterDBContext context, ClaimsProperty claimsProperty, IAddress address,
            ITenantService tenantService,INotificationService notification, IGHLConfiguration gHLConfiguration,
            IContactService contact,GHLService_v2 gHLService_V2,JobStatusAndStageMapping statusAndStageMapping, HttpClient httpClient,IJobStatusService jobStatusService)
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
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = GetAllQuerable().Where(x => x.JobId.ToString() == JobId &&
                  x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
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
        //                //GHL_Locationd = ghlOpportunity.StageId,
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

                FileLogger.Log("Starting SyncOpportunitiesFromGhl process.");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);

                var pipelineResponse = await gHLService_V2.GetAllPipelinesWithStagesAsync();
                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    FileLogger.Log("No pipelines found in GHL.");
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(AppConstant.GHL_PipelineId) || p.Name.Contains(AppConstant.GHL_PipelineId));

                if (targetPipeline == null)
                {
                    FileLogger.Log($"Target pipeline '{AppConstant.GHL_PipelineId}' not found.");
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
                var contacts = await gHLService_V2.GetContactsAsync();

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

        public async Task<IResponseVm<string>> UpdateOpportunityInGHLfromApp()
        {
          
                IResponseVm<string> responseVm = new ResponseVm<string>();
                try
                {
                var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration();
                if (Checkghlresponse.IsSuccess)
                {
                    FileLogger.Log("Starting UpdateOpportunityInGHLfromApp process.");

                    var getAppOpportunities = GetAllQuerable()
                        .Where(x => x.TenantId == AppConstant.LoggedInTenantId)
                        .ToList();

                    var ghlOpportunitiesResponse = await gHLService_V2.GetOpportunitiesFromGhl();
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

                            var responseghl = await gHLService_V2.CreateOpportunityInGhl(job);
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

                                var responseghl = await UpdateOpportunityInGhl(job);
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

        public async Task<IResponseVm<List<Opportunity>>> SyncOpportunitiesFromGhl()
        {
            var responseVm = new ResponseVm<List<Opportunity>>();

            try
            {
                await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                var configCheck = await gHLConfiguration.CheckGHLConfiguration();

                if (!configCheck.IsSuccess)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "GHL configuration check failed.";
                    return responseVm;
                }

                FileLogger.Log("Starting SyncOpportunitiesFromGhl process.");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);

                var pipelineResponse = await gHLService_V2.GetAllPipelinesWithStagesAsync();
                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    FileLogger.Log("No pipelines found in GHL.");
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                var userProvidedPipelineNameOrId = AppConstant.GHL_PipelineId;
                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(userProvidedPipelineNameOrId) || p.Name.Contains(userProvidedPipelineNameOrId));

                if (targetPipeline == null)
                {
                    FileLogger.Log($"Target pipeline '{userProvidedPipelineNameOrId}' not found.");
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Target pipeline not found.";
                    return responseVm;
                }

                FileLogger.Log($"Fetching opportunities from pipeline ID: {targetPipeline.Id}");

                var opportunities = await gHLService_V2.GetOpportunitiesFromGhl();
                var stages = await GetGHLStages();
                var allContacts = await gHLService_V2.GetContactsAsync();
                FileLogger.Log($"getting contacts from ghl process.{allContacts}");

                foreach (var opportunity in opportunities.Data)
                {
                    try
                    {
                        FileLogger.Log($"Processing opportunity ID: {opportunity.OpportunityId}, Name: {opportunity.Name}");

                        var existingJob = GetAllQuerable()
                            .FirstOrDefault(x => x.GhlOpportunityId.ToLower() == opportunity.OpportunityId.ToLower() && 
                            x.TenantId==AppConstant.LoggedInTenantId && x.IsActive==true && x.IsDelete==false);

                        var stage = stages?.Data.FirstOrDefault(x => x.GhlStageId == opportunity.PipelineStageId);
                        var dbcontact = contact.GetAllQuerable()
                            .FirstOrDefault(x => x.GhlcontactId == opportunity.Contact.Id && x.TenantId == AppConstant.LoggedInTenantId
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
                                AddressId = dbcontact?.PrimaryAddress ??Guid.Empty,
                                GHL_jobstatus = jobstatusId.ToString()??"",
                                GHL_StageName = stage?.GhlStageName??"",
                                JobName = opportunity.Name?.Trim()??"",
                                ScheduledDate = DateTime.Now,
                                lastasync = DateTime.Now,
                                Notes = "Synced from GHL",
                                GHL_StageId = Guid.Parse(opportunity.PipelineStageId)
                            };

                            await Create(createJobModel);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        FileLogger.Log($"Error processing opportunity ID: {opportunity.OpportunityId}. Exception: {innerEx.Message}");
                    }
                }

                var updateResult = await UpdateOpportunityInGHLfromApp();
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


        public async Task<IResponseVm<List<PipelineAndStage>>> GetGHLStages()
        {
            IResponseVm<List<PipelineAndStage>> responseVm = new ResponseVm<List<PipelineAndStage>>();
            try
            {
                var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration();
                if (Checkghlresponse.IsSuccess)
                {
                    var gHLStages = dbContext.TblGhlStages
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt)
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

        public async Task<IResponseVm<string>> UpdateOpportunityInGhl(TblJob job)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());

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

                string userProvidedPipelineNameOrId = AppConstant.GHL_PipelineId;

                var targetPipeline = dbContext.TblGhlStages
                    .FirstOrDefault(p => p.GhlPipelineId.Contains(userProvidedPipelineNameOrId) || p.GhlPipelineName.Contains(userProvidedPipelineNameOrId));

                if (targetPipeline == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Pipeline ID not found";
                    FileLogger.Log($"Error in UpdateOpportunityInGhl: Pipeline ID {userProvidedPipelineNameOrId} not found.");
                    return responseVm;
                }

                var ghlOpportunityModel = new
                {
                    name = job.JobName,
                    pipelineStageId = job.GhlStageId,
                  
                };

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
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
        //            .Where(x => x.TenantId == AppConstant.LoggedInTenantId)
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
        //                    TenantId = AppConstant.LoggedInTenantId,
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

        public async Task<IResponseVm<string>> Create(CreateJob model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsAddressExist = false;
                var TenantId = Guid.Empty;
                var IsTenantValid = false;
                TblGhlStage stage = new TblGhlStage();
                Guid jobStatus = Guid.Empty;
                if (model.locationId != null)
                {
                    //await gHLConfiguration.GetGHLConfiguration(model.locationId);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;

                    if (TenantId != null)
                    {
                        IsTenantValid = true;
                        
                        stage.GhlStageName = model.GHL_StageName;
                        Guid.TryParse(model.GHL_jobstatus,out jobStatus);
                        IsAddressExist = true;
                        //else
                        //{
                        //    responseVm.Message = "Contact is not synced in Application.";
                        //    return responseVm;
                        //}
                    }
                }
                else
                {
                     IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;
                    IsAddressExist = await address.AnyAsync(x => x.AddressId.ToString().ToLower() == model.AddressId.ToString().ToLower() && x.ContactId.ToString().ToLower() == model.ContactId.ToString().ToLower()
                    && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                    //stage = _context.TblGhlStages.Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.GhlStageId == model.GHL_StageId.ToString()).FirstOrDefault();
                    //jobStatus = _context.MstJobStatuses.Where(s => s.JobStatusName.ToLower() == EnumValue.GetEnumValue(JobStatus.New).ToLower() && s.IsActive == true).Select(x=> x.JobStatusId).FirstOrDefault();

                    var JobStatusResponse = await statusAndStageMapping.GetJobStatusByName(EnumValue.GetEnumValue(JobStatus.New));
                    if(JobStatusResponse.IsSuccess)
                    {
                        jobStatus = JobStatusResponse.Data.JobStatusId ?? Guid.Empty;
                    }
                }
                    if (!IsTenantValid)
                    {
                        responseVm.Message = AppConstant.TenantNotFound;
                        return responseVm;
                    }
                    int Version = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
                          .Select(p => (p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                    Version += 1;
                    var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, AppConstant.LoggedInCompanyCode, Version);
                    bool IsjobNameExists = await AnyAsync(x => x.JobName == model.JobName.Trim() && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.ContactId == model.ContactId);
                    var StageResponse = await statusAndStageMapping.GetGHLStagesMappingsById(jobStatus);
                    bool IsContactArchiveorInactive = await contact.AnyAsync(x => x.ContactId == model.ContactId && x.TenantId==AppConstant.LoggedInTenantId &&  (x.IsActive == false || x.IsArchive == true) );
                if(IsContactArchiveorInactive)
                {
                    responseVm.Message = "Contact is Archived or Inactive,You can not create Job.";
                    return responseVm;
                }
                if (IsjobNameExists)
                    {
                        responseVm.Message = "JobName" + AppConstant.AlreadyExists;
                        return responseVm;
                    }

                    if (model.GhlOpportunityId != null)
                    {
                        IsAddressExist = true;
                    }
                    if (IsAddressExist)
                    {
                        string arrayString = string.Join(", ", model.AttachmentsAny);
                    TblJob job = new TblJob
                    {
                        AddressId = (model.AddressId!=null && model.AddressId!=Guid.Empty)? model.AddressId:  null,
                        ContactId = model.ContactId,
                        JobName = model.JobName?.Trim(),
                        JobType = (model.JobType != null && model.JobType != Guid.Empty) ? model.JobType : null,
                        AssignedPersonnel = model.AssignedPersonnel ?? Guid.Empty, 
                        ScheduledDate = model.ScheduledDate,
                        JobStatus = jobStatus != Guid.Empty ? jobStatus : null,
                        Notes = model.Notes ?? string.Empty,
                        AttachmentAny = arrayString,
                        JobUniqueId = generateUnique,
                        GhlStageId = StageResponse?.Data?.StageId, 
                        GhlStageName = stage?.GhlStageName?.Trim() ?? string.Empty,
                        GhlContactId = model.GHL_ContactId ?? string.Empty,
                        TenantId = TenantId,
                        IsActive = true,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID ?? Guid.Empty,
                        GhluserId = model.GHLUserId ?? string.Empty
                    };

                       var res = await Add(job);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InsertSuccessMsg;
                        if (responseVm.IsSuccess)
                        {
                        if (model.GhlOpportunityId == null)
                        {
                            await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                            var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration();
                            if (Checkghlresponse.IsSuccess)
                            {
                                
                                var responseghl = await gHLService_V2.CreateOpportunityInGhl(job);
                                FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.Message}");
                                FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.IsSuccess}");
                                FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.Data}");
                            }
                        }
                        else
                        {
                            res.GhlOpportunityId = model.GhlOpportunityId;
                            res.ContactId = model.ContactId;
                            res.LastAsync = DateTime.Now;
                            await Update(res);
                        }
                            CreateNotification notification1 = new CreateNotification()
                            {
                                UserId = AppConstant.LoggedInUserID,
                                Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Job),
                                Body = $"{job.JobName} {AppConstant.Create_Notification}",
                                CreatedDate = DateTime.Now,
                                Type = Enum.GetName(typeof(NotificationModule), NotificationModule.Job),
                                TypeId = job.JobId,
                            };
                            var result = await notification.CreateNotification(notification1);
                        }
                    }
                    else
                    {
                        responseVm.Message = "Address " + AppConstant.NoDataFound;
                    }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteJob(string JobId)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantvalid = await tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = GetAllQuerable().Where(x => x.JobId.ToString() == JobId &&
                   x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

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
                                   where t2.TenantId == AppConstant.LoggedInTenantId && t2.IsDelete == false
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

                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

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

        public async Task<IResponseVm<string>> UpdateJob(UpdateJob model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsAddressExist = false;
                var TenantId = Guid.Empty;
                var IsTenantValid = false;
                TblGhlStage stage = new TblGhlStage();
                var jobStatus = Guid.Empty;
                if (model.locationId != null)
                {
                    //await gHLConfiguration.GetGHLConfiguration(model.locationId);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;

                    if (TenantId != null)
                    {
                        IsTenantValid = true;

                        stage.GhlStageName = model.GHL_StageName;
                        jobStatus = Guid.Parse(model.GHL_jobstatus);
                        //if (model.AddressId != null)
                        //{
                            IsAddressExist = true;
                        //}
                        //else
                        //{
                        //    responseVm.Message = "Contact is not synced in Application.";
                        //    return responseVm;
                        //}
                    }
                }
                else
                {
                    IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                    TenantId = (Guid)AppConstant.LoggedInTenantId;
                    IsAddressExist = await address.AnyAsync(x => x.AddressId == model.AddressId && x.ContactId == model.ContactId
                    && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                    stage = _context.TblGhlStages.Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.GhlStageId == model.GHL_StageId.ToString()).FirstOrDefault();
                    jobStatus = _context.MstJobStatuses.Where(s => s.JobStatusName == "open" && s.IsActive == true).Select(x => x.JobStatusId).FirstOrDefault();
                }
                if (!IsTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                if (model.GhlOpportunityId == null)
                {
                    var IsJobExist = (await GetAll()).Where(x => x.JobId == model.JobId &&
                    x.ContactId == model.ContactId && x.JobUniqueId == model.JobUniqueCode.Trim()
                    && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                    bool IsjobNameExists = await AnyAsync(x => x.JobId != model.JobId && x.JobName.Trim() == model.JobName.Trim() &&
                    x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.ContactId == model.ContactId);
                    if (IsjobNameExists)
                    {
                        responseVm.Message = "Job Name " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (IsJobExist != null)
                    {
                        string arrayString = string.Join(", ", model.AttachmentsAny);
                        IsJobExist.JobName = model.JobName?.Trim();
                        IsJobExist.JobType = model.JobType;
                        IsJobExist.AssignedPersonnel = model.AssignedPersonnel;
                        IsJobExist.ScheduledDate = model.ScheduledDate;
                        IsJobExist.JobStatus = model.JobStatus;
                        IsJobExist.Notes = model.Notes.Trim();
                        IsJobExist.GhlStageId = model.GHL_StageId;
                        IsJobExist.GhlStageName = stage?.GhlStageName?.Trim() ?? "";
                        IsJobExist.AddressId = model?.AddressId;
                        IsJobExist.AttachmentAny = arrayString;
                        IsJobExist.UpdatedAt = DateTime.Now;
                        IsJobExist.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = await Update(IsJobExist);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;

                        if (responseVm.IsSuccess && IsJobExist?.GhlOpportunityId != null)
                        {
                            var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration();
                            if (Checkghlresponse.IsSuccess)
                            {  //authService.GetGHLConfiguration(model.locationId);
                                var ghlUpdateResponse = await UpdateOpportunityInGhl(IsJobExist);
                                FileLogger.Log($"GHL Sync: Job updated in GHL - {ghlUpdateResponse.Message}");
                            }
                            else
                            {
                                responseVm.Message = Checkghlresponse.Message;
                            }
                        }
                    }
                    else
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = AppConstant.NoDataFound;
                    }

                }
                else
                {
                    var IsJobExist = (await GetAll()).Where(x => x.GhlOpportunityId == model.GhlOpportunityId
                    && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                    bool IsjobNameExists = await AnyAsync(x => x.GhlOpportunityId != model.GhlOpportunityId && x.JobName.Trim() == model.JobName.Trim() &&
                    x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.GhlContactId == model.GHL_ContactId);
                    if (IsjobNameExists)
                    {
                        responseVm.Message = "Job Name " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (IsJobExist != null)
                    {
                        string arrayString = string.Join(", ", model.AttachmentsAny);
                        IsJobExist.JobName = model.JobName?.Trim();
                        IsJobExist.JobType = model.JobType;
                        IsJobExist.AssignedPersonnel = model.AssignedPersonnel;
                        IsJobExist.ScheduledDate = model.ScheduledDate;
                        IsJobExist.JobStatus = jobStatus;
                        IsJobExist.Notes = model.Notes ?? "";
                        IsJobExist.GhlStageId = model.GHL_StageId;
                        IsJobExist.GhlStageName = stage?.GhlStageName?.Trim() ?? "";
                        IsJobExist.AttachmentAny = arrayString;
                        IsJobExist.AddressId = model.AddressId;
                        IsJobExist.UpdatedAt = DateTime.Now;
                        IsJobExist.ModifiedBy = AppConstant.LoggedInUserID ?? Guid.Empty;
                        IsJobExist.GhluserId = model.GHLUserId ?? string.Empty;
                        var res = await Update(IsJobExist);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                           var contact1 = contact.GetAllQuerable().Where(x => x.GhlcontactId == model.GHL_ContactId && x.IsDelete == false)
                                 .Select(x => x.ContactId).FirstOrDefault();
                           var addressId = address.GetAllQuerable().Where(x => x.ContactId == contact1).Select(x => x.AddressId).FirstOrDefault();
                        
                        var Jobcreated = new CreateJob
                        {
                            JobName = model.JobName,
                            JobType = model.JobType,
                            AssignedPersonnel = model.AssignedPersonnel,
                            AddressId = addressId,
                            GHL_ContactId = model.GHL_ContactId,
                            GhlOpportunityId = model.GhlOpportunityId,
                            GHL_StageId = model.GHL_StageId,
                            GHL_StageName = model.GHL_StageName,
                            GHL_jobstatus = jobStatus.ToString(),
                            lastasync = DateTime.Now,
                            locationId = model.locationId,
                            ContactId = contact1,
                            GHLUserId = model.GHLUserId,

                        };
                        var CreateJob = await Create(Jobcreated);
                        if(CreateJob != null)
                        {
                            responseVm.IsSuccess = true;
                            responseVm.Message = "Job Created by GHL opportunity."; 
                        }
                    }
                
                    
                    //else
                    //{
                    //    responseVm.IsSuccess = false;
                    //    responseVm.Message = AppConstant.NoDataFound;
                    //}

                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<GetAllJobsSubtenant_tenant>>> GetAllJobsTenant_SubTenant(Pagination filter)
        {
            IResponseVm<List<GetAllJobsSubtenant_tenant>> responseVm = new ResponseVm<List<GetAllJobsSubtenant_tenant>>();
            try
            {
               
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

                var subTenantlist = (await tenantService.GetAll())
                  .Where(x => x.ParentTenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false)
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
                                  where t6.IsDelete == false && t6.IsActive == true && t2.IsActive == true && t2.IsDelete == false && t2.TenantId == AppConstant.LoggedInTenantId || subTenantlist.Contains((Guid)t2.TenantId)
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
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = GetAllQuerable().Where(x => x.JobId == model.JobId &&
                  x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
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
                        await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
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

        public async Task<IResponseVm<string>> OpportunityFromGHL(GHLOpportunityTrigger opportunitydata)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            {
                try 
                { 
                var addressId = Guid.Empty;
                    var GHLUserId = "";
                var contact1 = new TblContact();
                if (opportunitydata != null)
                {
                    contact1 = contact.GetAllQuerable().Where(x => x.GhlcontactId == opportunitydata.contact_id && x.IsDelete == false && x.IsActive == true)
                         .FirstOrDefault();
                   //     addressId = (Guid)contact1.PrimaryAddress;
                }
                if(opportunitydata.Users != null)
                {
                 GHLUserId = (dbContext.TblGhlUsers.Where(x => x.GhlEmail == opportunitydata.Users.email && x.TenantId
                     == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true).Select(x=> x.GhlUserId).FirstOrDefault()) ?? string.Empty;
                }
                Guid jobtype = Guid.Parse("34623376-9CE3-4C9B-A538-2D3129B1A6B2");
                    // Get Stage ID
                    var stageId = (dbContext.TblGhlStages
                        .Where(x => x.GhlStageName.ToLower() == opportunitydata.pipleline_stage.ToLower())
                        .Select(x => x.GhlStageId ?? "")
                        .FirstOrDefault()) ?? string.Empty;

                    // Get Job Status ID
                    var jobstatus = (dbContext.MstJobStatuses
                        .Where(x => x.JobStatusName.ToLower() == opportunitydata.status.ToLower())
                        .Select(x => (Guid)x.JobStatusId)
                        .FirstOrDefault());

                    var jobstatuss = (jobstatus != null && jobstatus != Guid.Empty) ? jobstatus.ToString() : "";
                var job = new CreateJob
                {
                    ContactId = contact1.ContactId,
                    JobName = opportunitydata.opportunity_name,
                    JobType = jobtype,
                    AssignedPersonnel = Guid.Empty,
                    GhlOpportunityId = opportunitydata.id,
                    GHL_StageId = stageId == "" ? null : Guid.Parse(stageId) ,
                    GHL_StageName = opportunitydata.pipleline_stage,
                    locationId = opportunitydata.location.id,
                    GHL_ContactId = opportunitydata.contact_id,
                    lastasync = DateTime.Now,
                    GHL_jobstatus = jobstatuss,
                    GHLUserId = GHLUserId,
                };
                    await gHLConfiguration.GetGHLConfiguration(opportunitydata.WebhookTenantId);
                    var res = await Create(job);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
                catch (Exception ex)
                {
                    FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                    responseVm.Message += ex.ToString();
                }
                return responseVm;
            }
        }

        public async Task<IResponseVm<string>> UpdateOpportunityFromGHL(GHLOpportunityTrigger opportunitydata)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            {
                try
                {
                    var addressId = Guid.Empty;
                    var contact1 = new TblContact();
                    var GHLUserId = "";
                    if (opportunitydata != null)
                    {
                        contact1 = contact.GetAllQuerable().Where(x => x.GhlcontactId == opportunitydata.contact_id && x.IsDelete == false).
                             FirstOrDefault();
                        //addressId = (Guid)contact1.PrimaryAddress;
                    }
                    if (opportunitydata.Users != null)
                    {
                        GHLUserId = dbContext.TblGhlUsers.Where(x => x.GhlEmail == opportunitydata.Users.email && x.TenantId
                            == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true).Select(x => x.GhlUserId).FirstOrDefault();
                    }
                    Guid jobtype = Guid.Parse("34623376-9CE3-4C9B-A538-2D3129B1A6B2");
                    // Get Stage ID
                    var stageId = (dbContext.TblGhlStages
                        .Where(x => x.GhlStageName.ToLower() == opportunitydata.pipleline_stage.ToLower())
                        .Select(x => x.GhlStageId ?? "")
                        .FirstOrDefault()) ?? string.Empty;

                    // Get Job Status ID
                    var jobstatus = (dbContext.MstJobStatuses
                        .Where(x => x.JobStatusName.ToLower() == opportunitydata.status.ToLower())
                        .Select(x => (Guid)x.JobStatusId)
                        .FirstOrDefault());

                    var jobstatuss = (jobstatus != null && jobstatus != Guid.Empty) ? jobstatus.ToString() : "";
                 
                    var job = new UpdateJob
                    {
                        ContactId = contact1.ContactId,
                        //AddressId = addressId,
                        JobName = opportunitydata.opportunity_name,
                        JobType = jobtype,
                        AssignedPersonnel = Guid.Empty,
                        GhlOpportunityId = opportunitydata.id,
                        GHL_StageId = Guid.Parse(stageId),
                        GHL_StageName = opportunitydata.pipleline_stage,
                        locationId = opportunitydata.location.id,
                        GHL_ContactId = opportunitydata.contact_id,
                        lastasync = DateTime.Now,
                        GHL_jobstatus = jobstatuss,
                        GHLUserId = GHLUserId,
                    };
                    await gHLConfiguration.GetGHLConfiguration(opportunitydata.WebhookTenantId);
                    var res = await UpdateJob(job);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
                catch (Exception ex)
                {
                    FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                    responseVm.Message += ex.ToString();
                }
                return responseVm;
            }
        }


    }
}
