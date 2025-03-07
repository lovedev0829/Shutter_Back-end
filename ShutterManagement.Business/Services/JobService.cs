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

namespace ShutterManagement.Business.Services
{
    public class JobService : BaseRepository<ShutterDBContext, TblJob>, IJobService
    {
        private readonly ShutterDBContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly IAddress address;
        private readonly ITenantService tenantService;
        private readonly INotificationService notification;
        private readonly GhlService ghlService;
        private const string BaseUrl = "https://rest.gohighlevel.com/v1/";

        public JobService(ShutterDBContext context, ClaimsProperty claimsProperty, IAddress address,
            ITenantService tenantService,INotificationService notification, GhlService ghlService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.address = address;
            this.tenantService = tenantService;
            this.notification = notification;
            this.ghlService = ghlService;
        }

        private static string GetApiKey()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2NhdGlvbl9pZCI6IlBUbklDTU4wbUtyMDNrekpRcVBRIiwidmVyc2lvbiI6MSwiaWF0IjoxNzI0MDcwNDM3MTYzLCJzdWIiOiJkOGhlWkl2dEliZDVPcGhGdXNmYiJ9.yRrmWRgUC5VtrhKMsE4JsLjpwROYvvQnwoYu5JbVQWU"; // Replace with a secure method to fetch the API key
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
        public async Task<IResponseVm<string>> ProcessGhlOpportunityWebhook(GhlOpportunityWebhookModel webhookData)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            FileLogger.Log($"Start: Processing GHL Webhook for Opportunity ID: {webhookData?.Opportunity?.Id}");

            try
            {
                if (webhookData?.Opportunity == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Invalid webhook data.";
                    FileLogger.Log("Error: Invalid webhook data received.");
                    return responseVm;
                }

                var ghlOpportunity = webhookData.Opportunity;

                var existingJob = await _context.TblJobs
                    .FirstOrDefaultAsync(j => j.GhlOpportunityId == ghlOpportunity.Id);

                if (existingJob != null)
                {
                    // Update existing job details
                    existingJob.JobName = ghlOpportunity.Title?.Trim();
                    existingJob.Notes = ghlOpportunity.Description?.Trim();
                    existingJob.UpdatedAt = DateTime.UtcNow;

                    await Update(existingJob);
                    responseVm.Message = "Job successfully updated with GHL opportunity.";
                    FileLogger.Log($"Success: Job updated for Opportunity ID: {ghlOpportunity.Id}");
                }
                else
                {
                    // Extract relevant details from webhook data
                    var stage =  _context.TblGhlStages
                        .FirstOrDefault(s => s.GhlStageId == ghlOpportunity.StageId.ToString());

                    

                    // Create a new job based on webhook data
                    var createJobModel = new CreateJob
                    {
                        JobName = ghlOpportunity.Title?.Trim(),
                        Notes = ghlOpportunity.Description?.Trim(),
                        //GHL_StageId = ghlOpportunity.StageId,
                        //ContactId = contact.ContactId,
                        //AddressId = address.AddressId,
                        //JobType = "Default",
                        ScheduledDate = DateTime.Now,
                       
                    };

                    responseVm = await Create(createJobModel);
                }

                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error processing webhook: {ex.Message}";
                FileLogger.Log($"Exception: {ex.Message}");
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<Opportunity>>> SyncOpportunitiesFromGhl()
        {
            IResponseVm<List<Opportunity>> responseVm = new ResponseVm<List<Opportunity>>();

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());

                var pipelineResponse = await ghlService.GetAllPipelinesWithStagesAsync();

                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                string userProvidedPipelineNameOrId = "TestPipeline"; 

                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(userProvidedPipelineNameOrId) || p.Name.Contains(userProvidedPipelineNameOrId));


                if (targetPipeline == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Target pipeline not found.";
                    return responseVm;
                }

                var url = $"{BaseUrl}pipelines/{targetPipeline.Id}/opportunities/";
                var response = await client.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Error: {response.StatusCode}, Details: {responseBody}";
                    return responseVm;
                }

                var ghlResponse = JsonConvert.DeserializeObject<GhlOpportunitiesResponse>(responseBody);
                var opportunities = ghlResponse?.Opportunities ?? new List<Opportunity>();

                foreach (var opportunity in opportunities)
                {
                    var existingJob =  GetAllQuerable()
                        .Where(x => x.GhlOpportunityId == opportunity.OpportunityId).FirstOrDefault();

                    // Fetch job status dynamically from DB based on GHL stage
                    var res = await GetGHLStages();
                    var stage = res?.Data.FirstOrDefault();

                    var contacts = await GetContactsAsync();
                    var contact = contacts.FirstOrDefault(c => c.Id == opportunity.Contact?.Id);
                    var dbcontact = _context.TblContacts.Where(x => x.GhlcontactId == contact.Id).FirstOrDefault();
                    if(dbcontact==null)
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Contact Not Found related GHl Contact id";
                        return responseVm;
                    }

                    if (existingJob != null)
                    {
                        // Update existing job
                        existingJob.JobName = opportunity.Name;
                        existingJob.GhlStageId = Guid.Parse(opportunity.PipelineStageId);
                        existingJob.UpdatedAt = DateTime.Now;
                        await Update(existingJob);
                    }
                    else
                    {
                        var createJobModel = new CreateJob
                        {
                            ContactId= dbcontact?.ContactId,
                            GHL_ContactId = opportunity.Contact?.Id,  
                            GhlOpportunityId= opportunity.OpportunityId,  
                            JobName = opportunity.Name?.Trim(),
                            ScheduledDate = DateTime.Now, 
                            Notes = "Synced from GHL",
                            GHL_StageId = Guid.Parse(opportunity.PipelineStageId),
                            
                        };
                        await Create(createJobModel);
                    }
                }

                responseVm.Data = opportunities;
                responseVm.IsSuccess = true;
                responseVm.Message = "Synchronization completed successfully.";
            }
            catch (Exception ex)
            {
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
                if (string.IsNullOrEmpty(job.GhlOpportunityId))
                {
                    responseVm.Message = "No associated GHL Opportunity ID found.";
                    FileLogger.Log($"Error in UpdateOpportunityInGhl: {job.GhlOpportunityId}");
                    return responseVm;
                }

                var ghlOpportunityModel = new
                {
                    id = job.GhlOpportunityId,
                    name = job.JobName,
                    stageId = job.GhlStageId,
                    status = "open",
                    notes = job.Notes,
                    
                };

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());

                var content = new StringContent(JsonConvert.SerializeObject(ghlOpportunityModel), Encoding.UTF8, "application/json");
                var url = $"{BaseUrl}opportunities/{job.GhlOpportunityId}";

                var response = await client.PutAsync(url, content);
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
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
                FileLogger.Log($"Error in UpdateOpportunityInGhl: {ex.Message}");
            }

            return responseVm;
        }

        public async Task<List<GhlContactVM>> GetContactsAsync()
        {
            var requestUrl = $"https://rest.gohighlevel.com/v1/contacts/";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
            var response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<GhlContactResponse>(json);
            //var result1 = GetClientIpAddress();
            return result?.Contacts ?? new List<GhlContactVM>();
        }

        #endregion Ghl CRM related methods

        public async Task<IResponseVm<string>> Create(CreateJob model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (!IsTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                int Version = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
                      .Select(p => (p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.JobUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                Version += 1;
                var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, AppConstant.LoggedInCompanyCode, Version);
                bool IsAddressExist = await address.AnyAsync(x => x.AddressId == model.AddressId && x.ContactId == model.ContactId
                && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);

                bool IsjobNameExists = await AnyAsync(x => x.JobName == model.JobName.Trim() && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.ContactId == model.ContactId);
                if (IsjobNameExists)
                {
                    responseVm.Message = "JobName" + AppConstant.AlreadyExists;
                    return responseVm;
                }
                if (IsAddressExist)
                {
                    var stage = _context.TblGhlStages.Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.GhlStageId== model.GHL_StageId.ToString()).FirstOrDefault();
                    var jobStatus = _context.MstJobStatuses.Where(s => s.JobStatusName == "Open" && s.IsActive == true).FirstOrDefault();
                    string arrayString = string.Join(", ", model.AttachmentsAny);
                    TblJob job = new TblJob
                    {
                        AddressId = model.AddressId,
                        ContactId = model.ContactId,
                        JobName = model.JobName?.Trim(),
                        JobType = model.JobType,
                        AssignedPersonnel = model.AssignedPersonnel,
                        ScheduledDate = model.ScheduledDate,
                        JobStatus = jobStatus?.JobStatusId,
                        Notes = model.Notes.Trim(),
                        AttachmentAny = arrayString,
                        JobUniqueId = generateUnique,
                        GhlStageId = model.GHL_StageId,
                        GhlStageName = stage?.GhlStageName?.Trim()??"",
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID
                    };
                    var res = await Add(job);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                    if (responseVm.IsSuccess)
                    {
                        if(model.GhlOpportunityId==null)
                        {
                            var responseghl = await ghlService.CreateOpportunityInGhl(job);
                            FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.Message}");
                            FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.IsSuccess}");
                            FileLogger.Log($"Start: Creating CreateOpportunityInGhl : {responseghl.Data}");
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

        public async Task<IResponseVm<string>> Delete(string JobId)
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
                                   join t1 in _context.TblAddresses on t2.AddressId equals t1.AddressId
                                   join t3 in _context.MstJobTypes on t2.JobType equals t3.Id
                                   join t4 in _context.TblUsers on t2.AssignedPersonnel equals t4.Id
                                   join t5 in _context.MstJobStatuses on t2.JobStatus equals t5.JobStatusId
                                   join t6 in _context.TblContacts on t2.ContactId equals t6.ContactId
                                   where t2.TenantId == AppConstant.LoggedInTenantId && t2.IsDelete == false && t6.IsActive == true && t6.IsDelete == false
                                   select new GetAllJobsdetails
                                   {
                                       IsActive = t2.IsActive,
                                       JobId = t2.JobId,
                                       JobName = t2.JobName,
                                       ContactId = t2.ContactId,
                                       ContactFirstName = t6.FirstName,
                                       ContactLasttName = t6.LastName,
                                       AddressId = t2.AddressId,
                                       JobType = t3.Id,
                                       JobTypeName = t3.JobTypeName,
                                       AssignedPersonnel = t4.Id,
                                       AssignedPersonnelName = t4.FirstName + " " + t4.LastName,
                                       ScheduledDate = t2.ScheduledDate,
                                       JobStatus = t5.JobStatusId,
                                       JobStatusName = t5.JobStatusName,
                                       Notes = t2.Notes,
                                       AttachmentsAny = t2.AttachmentAny.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                                       Label = t1.Label,
                                       StreetAddress = t1.StreetAddress,
                                       City = t1.City,
                                       State = t1.State,
                                       ZipCode = t1.ZipCode,
                                       JobUniqueCode = t2.JobUniqueId,
                                       GHL_StageId = t2.GhlStageId,
                                       GHL_StageName = t2.GhlStageName??"",
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
                bool IsTenantvalid = await tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = (await GetAll()).Where(x => x.JobId == model.JobId && x.AddressId == model.AddressId &&
                x.ContactId == model.ContactId && x.JobUniqueId == model.JobUniqueCode.Trim() 
                && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                bool IsjobNameExists = await AnyAsync(x =>x.JobId !=model.JobId && x.JobName.Trim() == model.JobName.Trim() && 
                x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.ContactId == model.ContactId);
                if (IsjobNameExists)
                {
                    responseVm.Message = "Job Name " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                var stage = _context.TblGhlStages.Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.GhlStageId == model.GHL_StageId.ToString()).FirstOrDefault();

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
                    IsJobExist.GhlStageName = stage?.GhlStageName?.Trim()??"";
                    IsJobExist.AttachmentAny = arrayString;
                    IsJobExist.UpdatedAt = DateTime.Now;
                    IsJobExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsJobExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else if (!string.IsNullOrEmpty(IsJobExist.GhlOpportunityId))
                {
                    var ghlUpdateResponse = await UpdateOpportunityInGhl(IsJobExist);
                    FileLogger.Log($"GHL Sync: Job updated in GHL - {ghlUpdateResponse.Message}");
                }

                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.NoDataFound;
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
                    var JobList =  (from t2 in _context.TblJobs.OrderByDescending(x => x.CreatedAt)
                                         join t1 in _context.TblAddresses on t2.AddressId equals t1.AddressId
                                         join t3 in _context.MstJobTypes on t2.JobType equals t3.Id
                                         join t4 in _context.TblUsers on t2.AssignedPersonnel equals t4.Id
                                         join p1 in _context.TblUsers on t2.CreatedBy equals p1.Id
                                         join p2 in _context.TblTenants on t2.TenantId equals p2.TenantId
                                         join t5 in _context.MstJobStatuses on t2.JobStatus equals t5.JobStatusId
                                         join t6 in _context.TblContacts on t2.ContactId equals t6.ContactId
                                         where t6.IsDelete == false && t6.IsActive == true && t2.IsActive == true && t2.IsDelete == false && t2.TenantId == AppConstant.LoggedInTenantId || subTenantlist.Contains((Guid)t2.TenantId)
                                         select new GetAllJobsSubtenant_tenant
                                         {
                                             IsActive = t2.IsActive,
                                             JobId = t2.JobId,
                                             JobName = t2.JobName,
                                             ContactId = t2.ContactId,
                                             ContactFirstName = t6.FirstName,
                                             ContactLastName = t6.LastName,
                                             ContactEmail = t6.Email,
                                             ContactPhone = t6.PhoneNumber,
                                             AddressId = t2.AddressId,
                                             JobType = t3.Id,
                                             JobTypeName = t3.JobTypeName,
                                             AssignedPersonnel = t4.Id,
                                             AssignedPersonnelName = t4.FirstName + " " + t4.LastName,
                                             ScheduledDate = t2.ScheduledDate,
                                             JobStatus = t5.JobStatusId,
                                             JobStatusName = t5.JobStatusName,
                                             Notes = t2.Notes,
                                             AttachmentsAny = t2.AttachmentAny,
                                             Label = t1.Label,
                                             StreetAddress = t1.StreetAddress,
                                             City = t1.City,
                                             State = t1.State,
                                             ZipCode = t1.ZipCode,
                                             JobUniqueCode = t2.JobUniqueId,
                                             CreatedBy = t2.CreatedBy.ToString(),
                                             CreatedbyUsername = p1.FirstName + " " + p1.LastName,
                                             SubtenantName = p2.FirstName + " " + p2.LastName,
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
                                || (x.ContactFirstName ?? "").ToLower().Contains(filter.SearchValue)
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

                    var TakenRecords = Paginate.Pagination(FilterRecords, filter);

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

       

    }
}
