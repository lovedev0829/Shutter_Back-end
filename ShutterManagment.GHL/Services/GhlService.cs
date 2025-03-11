
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509.Qualified;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagment.GHL.ViewModel.Request;
using ShutterManagment.GHL.ViewModel.Response;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShutterManagement.GHL.Services
{
    public class GhlService
    {
        private readonly HttpClient _httpClient;
        private readonly ShutterDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private string BaseUrl = AppConstant.GHL_BaseAddress;

        public GhlService(HttpClient httpClient, ShutterDBContext dBContext, IHttpContextAccessor _httpContextAccessor)
        {
            _httpClient = httpClient;
            this.dBContext = dBContext;
            httpContextAccessor = _httpContextAccessor;
        }

        #region Start pipeLine and Stages Related Methods

        public async Task<PipelineResponse> GetAllPipelinesWithStagesAsync()
        {
            try
            {
                var requestUrl = $"{BaseUrl}pipelines/";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_APIKey);
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var pipelineResponse = System.Text.Json.JsonSerializer.Deserialize<PipelineResponse>(jsonResponse);

                return pipelineResponse ?? new PipelineResponse { Pipelines = new List<Pipeline>() };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching pipelines: {ex.Message}");
                return new PipelineResponse { Pipelines = new List<Pipeline>() };
            }
        }

        public async Task<IResponseVm<string>> GetAndSaveDefaultPipelineAndStage()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var pipelineResponse = await GetAllPipelinesWithStagesAsync();

                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                string userProvidedPipelineNameOrId = AppConstant.GHL_PipelineId;

                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(userProvidedPipelineNameOrId) || p.Name.Contains(userProvidedPipelineNameOrId));

                if (targetPipeline == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Target pipeline not found.";
                    return responseVm;
                }

                if (targetPipeline?.Stages == null || !targetPipeline.Stages.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Target pipeline has no stages.";
                    return responseVm;
                }

                var existingStages = dBContext.TblGhlStages
                    .Where(x => x.GhlPipelineId == targetPipeline.Id)
                    .ToList();

                var existingStagesDict = existingStages.ToDictionary(x => x.GhlStageId, x => x);

                List<TblGhlStage> stagesToUpdate = new();
                List<TblGhlStage> stagesToInsert = new();

                // Sort the pipeline stages by name (or another field if required)
                var orderedStages = targetPipeline.Stages.OrderBy(s => s.Name).ToList();

                for (int i = 0; i < orderedStages.Count; i++)
                {
                    var stage = orderedStages[i];

                    if (existingStagesDict.TryGetValue(stage.Id, out var existingStage))
                    {
                        existingStage.GhlStageName = stage.Name;
                        existingStage.GhlPipelineName = targetPipeline.Name;
                        existingStage.StageOrder = i + 1; // Assigning order for sequential storage
                        existingStage.GhlLastSync = DateTime.Now;
                        existingStage.UpdatedAt = DateTime.Now;
                        existingStage.ModifiedBy = AppConstant.LoggedInTenantId;
                        stagesToUpdate.Add(existingStage);
                    }
                    else
                    {
                        stagesToInsert.Add(new TblGhlStage
                        {
                            Id = Guid.NewGuid(),
                            GhlPipelineId = targetPipeline.Id,
                            GhlStageId = stage.Id,
                            GhlStageName = stage.Name,
                            GhlPipelineName = targetPipeline.Name,
                            StageOrder = i + 1, // Assigning order for new records
                            GhlLastSync = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            TenantId = AppConstant.LoggedInTenantId,
                            CreatedBy = AppConstant.LoggedInUserID,
                            IsActive = true,
                        });
                    }
                }

                if (stagesToUpdate.Any())
                {
                    dBContext.TblGhlStages.UpdateRange(stagesToUpdate);
                }

                if (stagesToInsert.Any())
                {
                    await dBContext.TblGhlStages.AddRangeAsync(stagesToInsert);
                }

                await dBContext.SaveChangesAsync();

                var isUpdated = await UpdateLastSyncConfigurationAsync();

                if (!isUpdated)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Error: Configuration not found or update failed.";
                    return responseVm;
                }

                responseVm.IsSuccess = true;
                responseVm.Message = "All stages synced sequentially.";

            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<bool> UpdateLastSyncConfigurationAsync()
        {
            try
            {
                var config = await dBContext.TblGhlconfigurations
                                             .FirstOrDefaultAsync(c => c.Id == AppConstant.LoggedInTenantId);

                if (config == null)
                {
                    return false; 
                }

                config.LastSync = DateTime.Now;  
                await dBContext.SaveChangesAsync(); 

                return true; 
            }
            catch (Exception ex)
            {
               
                FileLogger.Log($"Error updating LastSync: {ex.Message}");
                return false; 
            }
        }



        public async Task<IResponseVm<List<PipelineAndStage>>> GetGHLStages()
        {
            IResponseVm<List<PipelineAndStage>> responseVm = new ResponseVm<List<PipelineAndStage>>();
            try
            {
                var gHLStages = dBContext.TblGhlStages
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId).OrderBy(x=>x.StageOrder)
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

        private static string GetApiKey()
        {
            return AppConstant.GHL_APIKey; 
        }

        #endregion Start pipeLine and Stages Related Methods 

        #region Start Contact Related Methods
        public async Task<List<GhlContactVM>> GetContactsAsync()
        {
            
            using var httpClient = new HttpClient();
            var requestUrl = $"{BaseUrl}contacts/";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
            var response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<GhlContactResponse>(json);
            var result1 = GetClientIpAddress();
            return result?.Contacts ?? new List<GhlContactVM>();
        }

        public async Task<GhlContactSingleResponse> CreateContactAsync(GhlContactAdd contact)
        {
            try
            {
                using var httpClient = new HttpClient();
                var contactData = new
                {
                    firstName = contact.FirstName,
                    lastName = contact.LastName,
                    email = contact.Email,
                    phone = contact.Phone,
                };

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                var url = $"{BaseUrl}contacts/"; 

                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var contactResponse = JsonConvert.DeserializeObject<GhlContactSingleResponse>(jsonResponse);

                return contactResponse;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in CreateContactAsync: {ex.Message}");
                return null;
            }
        }


        public async Task<GhlContactSingleResponse> UpdateContactAsync(string contactId, GhlContactAdd contact)
        {
            try
            {
                if (string.IsNullOrEmpty(contactId) || contact == null)
                {
                    return null;
                }
                using var httpClient = new HttpClient();
                var contactData = new
                {
                    firstName = contact.FirstName,
                    lastName = contact.LastName,
                    email = contact.Email,
                    phone = contact.Phone
                };

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                var url = $"{BaseUrl}contacts/{contactId}"; 

                var response = await httpClient.PutAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                   FileLogger.Log($"Failed to update contact. Status code: {response.StatusCode}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var contactResponse = JsonConvert.DeserializeObject<GhlContactSingleResponse>(jsonResponse);

                return contactResponse;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error updating contact: {ex.Message}");
                return null;
            }
        }



        public string GetClientIpAddress()
        {
            var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return ipAddress;
        }



        #endregion End Contact Related Methods

        #region Start Opportunity Related Methods

        public async Task<IResponseVm<string>> CreateOpportunityInGhl(TblJob job)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            
            var res = await GetGHLStages();
            var stage = res?.Data.FirstOrDefault();
            if (stage == null)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = "No available pipeline stage found.";
                return responseVm;
            }
            var IsContactExists = dBContext.TblContacts.Where(c => c.ContactId == job.ContactId && c.IsDelete == false).FirstOrDefault();

            var opportunityData = new
            {
                title = job.JobName,
                status = "open",
                stageId = job.GhlStageId,
                email= IsContactExists?.Email,
                phone= IsContactExists?.PhoneNumber,
                monetaryValue= 122.22,
                source= "public hh",
                name= IsContactExists?.FirstName +" "+IsContactExists?.LastName,
                companyName="Brain Technosys PVT LTd",
            };

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
                var endpoint = $"{BaseUrl}pipelines/{stage.GhlPipelineId}/opportunities/";
                var opportunityJson = JsonConvert.SerializeObject(opportunityData);
                var content = new StringContent(opportunityJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Error creating opportunity: {response.StatusCode}";
                    return responseVm;
                }

                // Deserialize response to fetch created opportunityId and contactId
                var result = JsonConvert.DeserializeObject<OpportunityResponse>(json);
                if (result == null || string.IsNullOrEmpty(result.OpportunityId))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Failed to retrieve opportunity details from response.";
                    return responseVm;
                }

                // Fetch the existing job and update GhlOpportunityId and GhlContactId
                var existingJob = await dBContext.TblJobs.FindAsync(job.JobId);
                if (existingJob != null)
                {
                    existingJob.GhlOpportunityId = result.OpportunityId;
                    existingJob.GhlContactId = result.Contact.ContactId; 
                    await dBContext.SaveChangesAsync();
                }

                responseVm.IsSuccess = true;
                responseVm.Message = "Opportunity added successfully.";
                
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception occurred: {ex.Message}";
            }

            return responseVm;
        }

      
        public async Task<IResponseVm<List<Opportunity>>> GetOpportunitiesFromGhl()
        {
            IResponseVm<List<Opportunity>> responseVm = new ResponseVm<List<Opportunity>>();

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());

                var pipelineResponse = await GetAllPipelinesWithStagesAsync();

                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                string userProvidedPipelineNameOrId = AppConstant.GHL_PipelineId;

                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(userProvidedPipelineNameOrId) || p.Name.Contains(userProvidedPipelineNameOrId));

                var url = $"{BaseUrl}pipelines/{targetPipeline?.Id}/opportunities/";
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await client.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Error: {response.StatusCode}, Details: {responseBody}";
                    return responseVm;
                }

                var ghlResponse = JsonConvert.DeserializeObject<GhlOpportunitiesResponse>(responseBody);
                responseVm.Data = ghlResponse?.Opportunities ?? new List<Opportunity>();
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception occurred: {ex.Message}";
            }

            return responseVm;
        }

        public class OpportunityResponse
        {
            [JsonProperty("id")]
            public string OpportunityId { get; set; }

            [JsonProperty("contact")]
            public ContactInfo Contact { get; set; }
        }

        public class ContactInfo
        {
            [JsonProperty("id")]
            public string ContactId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("companyName")]
            public string CompanyName { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("phone")]
            public string Phone { get; set; }

            [JsonProperty("tags")]
            public List<string> Tags { get; set; }
        }




        #endregion End Opportunity Related Methods
    }
}






