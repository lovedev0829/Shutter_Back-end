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

namespace ShutterManagement.Business.Services
{
    public class JobStatusAndStageMapping
    {
        private readonly ShutterDBContext dBContext;
        private readonly IGHLConfiguration gHLConfiguration;

        public JobStatusAndStageMapping(ShutterDBContext dBContext,IGHLConfiguration gHLConfiguration)
        {
            this.dBContext = dBContext;
            this.gHLConfiguration = gHLConfiguration;
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

        public async Task<IResponseVm<JobStatusList>> GetJobStatusByName(string? JobstatusName)
        {
            IResponseVm<JobStatusList> responseVm = new ResponseVm<JobStatusList>();

            try
            {
                var mapping = dBContext.MstJobStatuses
                    .Where(x => x.JobStatusName == JobstatusName && x.IsActive == true)
                    .Select(x => new JobStatusList
                    {
                        JobStatusId = x.JobStatusId,
                        JobStatusName = x.JobStatusName
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

        public async Task<IResponseVm<StageMapping>> GetGHLStagesMappingsById(Guid? JobstatusId)
        {
            IResponseVm<StageMapping> responseVm = new ResponseVm<StageMapping>();

            try
            {
                var mapping = dBContext.TblGhlStagesmappings
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.StatusId == JobstatusId)
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
                await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                
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
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Version", "2021-07-28");

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


        public async Task<IResponseVm<string>> UpdateOpportunityAndJobStatus(Guid? jobId, string? jobStatusName)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var opportunityResponse = await GetGHLOpportunityIdByJobId(jobId);
                if (!opportunityResponse.IsSuccess || string.IsNullOrEmpty(opportunityResponse.Data))
                    return new ResponseVm<string> { IsSuccess = false, Message = "Opportunity ID not found for this Job." };

                string opportunityId = opportunityResponse.Data;

                var statusResponse = await GetJobStatusByName(jobStatusName);
                if (!statusResponse.IsSuccess)
                    return new ResponseVm<string> { IsSuccess = false, Message = "Job status not found." };

                var jobStatus = statusResponse.Data;

                var stageResponse = await GetGHLStagesMappingsById(jobStatus.JobStatusId);
                if (!stageResponse.IsSuccess)
                    return new ResponseVm<string> { IsSuccess = false, Message = "Stage mapping not found for the provided status." };

                var stageMapping = stageResponse?.Data;
                if (stageMapping?.StageId != null && stageMapping?.StageId != Guid.Empty)
                {
                    var updateGhlResponse = await UpdateOpportunityStage(opportunityId, stageMapping?.StageId);
                    if (!updateGhlResponse.IsSuccess)
                        return new ResponseVm<string> { IsSuccess = false, Message = updateGhlResponse.Message };
                }
                
                var updateJobResponse = await UpdateJobStatus(jobId, jobStatus.JobStatusId, stageMapping?.StageId);
                if (!updateJobResponse.IsSuccess)
                    return new ResponseVm<string> { IsSuccess = false, Message = updateJobResponse.Message };

                responseVm.IsSuccess = true;
                responseVm.Message = "Opportunity and Job status updated successfully.";
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
