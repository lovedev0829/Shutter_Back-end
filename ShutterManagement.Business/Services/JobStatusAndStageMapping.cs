using Newtonsoft.Json;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using Microsoft.EntityFrameworkCore;
using Azure;
using ShutterManagement.Core.Common;

namespace ShutterManagement.Business.Services
{
    public class JobStatusAndStageMapping
    {
        private readonly ShutterDBContext dBContext;
        private readonly ClaimsProperty claimsProperty;
        private readonly IGHLConfiguration gHLConfiguration;
        private readonly IjobStageStatusAutomAtionHistory automAtionHistory;

        public JobStatusAndStageMapping(ShutterDBContext dBContext, ClaimsProperty claimsProperty, IGHLConfiguration gHLConfiguration,IjobStageStatusAutomAtionHistory automAtionHistory)
        {
            this.dBContext = dBContext;
            this.claimsProperty = claimsProperty;
            this.gHLConfiguration = gHLConfiguration;
            this.automAtionHistory = automAtionHistory;
        }

        public async Task<IResponseVm<string>> GetGHLOpportunityIdByJobId(Guid? jobId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var mapping = dBContext.TblJobs
                    .Where(x => x.JobId == jobId && x.IsActive == true)
                    .Select(x => x.GhlOpportunityId).FirstOrDefault();

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

        public async Task<IResponseVm<JobStatusList>> GetJobStatusByName(string? JobstatusName ,Guid? Stageid =null, Guid? LoggedInTenantId = null)
        {
            IResponseVm<JobStatusList> responseVm = new ResponseVm<JobStatusList>();

            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
                JobStatusList? mapping = null;

                if (!string.IsNullOrEmpty(JobstatusName))
                {
                    mapping = dBContext.MstJobStatuses
                        .Where(x => x.JobStatusName == JobstatusName && x.IsActive==true)
                        .Select(x => new JobStatusList
                        {
                            JobStatusId = x.JobStatusId,
                            JobStatusName = x.JobStatusName
                        }).FirstOrDefault();
                }

                // Priority 2: Use stageId to get statusId if jobStatusName was not found or not provided
                if (mapping == null && Stageid != null && Stageid != Guid.Empty)
                {
                    var stageMapping = dBContext.TblGhlStagesmappings
                        .Where(x => x.StageId == Stageid && x.TenantId == LoggedInTenantId)
                        .FirstOrDefault();

                    if (stageMapping?.StatusId != null)
                    {
                        mapping = dBContext.MstJobStatuses
                            .Where(x => x.JobStatusId == stageMapping.StatusId && x.IsActive==true)
                            .Select(x => new JobStatusList
                            {
                                JobStatusId = x.JobStatusId,
                                JobStatusName = x.JobStatusName
                            }).FirstOrDefault();
                    }
                }


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

        public async Task<IResponseVm<StageMapping>> GetGHLStagesMappingsById(Guid? JobstatusId, Guid? LoggedInTenantId = null)
        {
            IResponseVm<StageMapping> responseVm = new ResponseVm<StageMapping>();

            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
                var mapping = dBContext.TblGhlStagesmappings
                    .Where(x => x.TenantId == LoggedInTenantId && x.StatusId == JobstatusId)
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

        #region Update Opportunity Status
        public async Task<IResponseVm<string>> UpdateOpportunityStage(string opportunityId, Guid? stageId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var ghlconfigdetails = await gHLConfiguration.GetGHLConfiguration(claimsProperty.GetTenantId().ToString());
                
                if (stageId == Guid.Empty || stageId == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Stage ID is not valid.";
                    return responseVm;
                }
                var opportunityData = new
                {
                    pipelineStageId = stageId,
                };

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/opportunities/{opportunityId}";
                var opportunityJson = JsonConvert.SerializeObject(opportunityData);
                var content = new StringContent(opportunityJson, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ghlconfigdetails?.Data?.Access_Token??"");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Version", AppConstant.GHL_Version);

                HttpResponseMessage response = await httpClient.PutAsync(requestUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"API request failed: {response.StatusCode} - {jsonResponse}";
                    return responseVm;
                }

                responseVm.IsSuccess = true;
                responseVm.Message = "Opportunity status updated successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception occurred: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateJobStatus(Guid? jobId, Guid? statusId,Guid? Stageid)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var job = dBContext.TblJobs.FirstOrDefault(x => x.JobId == jobId && x.IsActive==true);
                if (job != null)
                {
                    job.JobStatus = statusId;
                    if(Stageid != Guid.Empty && Stageid != null)
                    {
                        job.GhlStageId = Stageid;
                    }
                    dBContext.SaveChanges();
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Job status updated successfully.";
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Job not found.";
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception occurred: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateOpportunityAndJobStatus(Guid? jobId, string? jobStatusName,Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {

                var statusResponse = await GetJobStatusByName(jobStatusName,null,LoggedInTenantId);
                if (!statusResponse.IsSuccess)
                    return new ResponseVm<string> { IsSuccess = false, Message = "Job status not found." };

                var jobStatus = statusResponse.Data;

                // if ghlstageid is not null then fetch id using getstagemapping id and then continue the process 

                var stageResponse = await GetGHLStagesMappingsById(jobStatus.JobStatusId);
                var stageMapping = stageResponse?.Data;
                Guid? stageId = stageMapping?.StageId;

                var opportunityResponse = await GetGHLOpportunityIdByJobId(jobId);
                bool isOpportunityAvailable = opportunityResponse.IsSuccess && !string.IsNullOrEmpty(opportunityResponse.Data);
                string opportunityId = opportunityResponse.Data;

                if (isOpportunityAvailable && stageId != null && stageId != Guid.Empty)
                {
                    var updateGhlResponse = await UpdateOpportunityStage(opportunityId, stageId);
                    if (!updateGhlResponse.IsSuccess)
                    {

                        FileLogger.Log($"GHL Update Failed: {updateGhlResponse.Message}");
                    }
                }

                var updateJobResponse = await UpdateJobStatus(jobId, jobStatus.JobStatusId, stageId);
                if (!updateJobResponse.IsSuccess)
                    return new ResponseVm<string> { IsSuccess = false, Message = updateJobResponse.Message };

                var job = dBContext.TblJobs.FirstOrDefault(x => x.JobId == jobId && x.IsActive == true);
                if (job != null)
                {
                    var res = await automAtionHistory.SaveHistory(job);
                }

                responseVm.IsSuccess = true;
                responseVm.Message = "Job status updated successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception occurred: {ex.Message}";
            }

            return responseVm;
        }

        #endregion Update Opportunity Status
    }
}
