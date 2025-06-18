
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
        private string BaseUrl = string.Empty;
        private string accesstoken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoQ2xhc3MiOiJMb2NhdGlvbiIsImF1dGhDbGFzc0lkIjoiUFRuSUNNTjBtS3IwM2t6SlFxUFEiLCJzb3VyY2UiOiJJTlRFR1JBVElPTiIsInNvdXJjZUlkIjoiNjdlMTFjNzJjOTBhMmY4MTIwMGNjMTJhLW04bXRzcXJuIiwiY2hhbm5lbCI6Ik9BVVRIIiwicHJpbWFyeUF1dGhDbGFzc0lkIjoiUFRuSUNNTjBtS3IwM2t6SlFxUFEiLCJvYXV0aE1ldGEiOnsic2NvcGVzIjpbImNvbnZlcnNhdGlvbnMvbWVzc2FnZS5yZWFkb25seSIsImNvbnZlcnNhdGlvbnMvbWVzc2FnZS53cml0ZSJdLCJjbGllbnQiOiI2N2UxMWM3MmM5MGEyZjgxMjAwY2MxMmEiLCJjbGllbnRLZXkiOiI2N2UxMWM3MmM5MGEyZjgxMjAwY2MxMmEtbThtdHNxcm4ifSwiaWF0IjoxNzQyOTg3NTQyLjIwMywiZXhwIjoxNzQzMDczOTQyLjIwM30.fMjXamfhw4jgfLo2tzDKJS1kA_S89MX02jPxTCheOv4UwJrIjd1DJD3RtW8FIpmOhEvrCaZGw5MKVzqczzzDrhGx3Q_dfJ3ZdMKzH_q8D4pgYKo0V7TlKXH6wp2mu63__iV7ZfWO2XxVFBS9zyg5O2fDYhx209owntBKEYK5DjDfnyxeuryzGBDTaAP7fCor_BFmlcgtpmHpKsy3XNV-faFY-Ha3-XTGN4VQwFv2JAkEKNshJmnBbuF2UdoKcWjCs2C4O62eicNGhj0KC-JelG81aTQr1rqHNC3jdEGT7FPSajkdpJuqzAfrWBitm4FE0QGVe7QVLonfVTzX_om0B8h3QsTtrHKWGyZXaddrtsV-1ssjeE9ojMM2qiyGnDE0VFTWcDicbySyB24bWK5x5VO1f79XpkSKZHgBYeU9vKtYaZcRVE3gcnKNKYT9wl82ARYVKmzHSYv5IEJfdq4epmL9TmuERuowW31aznFFgzuluFqED2dockpRUrDqZjtAklT-66RGex3Xfiu6SW3ImsKscZm5gmjUtAEEY0whoP4FIJf4VWFuId8DEmJhBKRSp2gJDqhro5yuNv_OxLdwOJK07N2AjhjW6TP7L856NJzJBUUR-KonLSuJ9C1tU0C8Y2BDioDEn-P0Qonp38aoCzSnmHQiaYnpv0iNnWoa6Ws";

        public GhlService(HttpClient httpClient, ShutterDBContext dBContext, IHttpContextAccessor _httpContextAccessor)
        {
            _httpClient = httpClient;
            this.dBContext = dBContext;
            httpContextAccessor = _httpContextAccessor;
            BaseUrl = AppConstant.GHL_BaseAddress;
            FileLogger.Log($"Base Address from Constr: {AppConstant.GHL_BaseAddress}");
            FileLogger.Log($"Base Address from Constr Base: {BaseUrl}");

        }

        #region Start pipeLine and Stages Related Methods

        public async Task<PipelineResponse> GetAllPipelinesWithStagesAsync()
        {
            try
            {
                var requestUrl = $"{AppConstant.GHL_BaseAddress}pipelines/";
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
                var orderedStages = targetPipeline.Stages;
                //var orderedStages = targetPipeline.Stages.OrderBy(s => s.Name).ToList();

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
                        existingStage.ModifiedBy = claimsProperty.GetTenantId();
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
                            TenantId = claimsProperty.GetTenantId(),
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
                                             .FirstOrDefaultAsync(c => c.TenantId == claimsProperty.GetTenantId());

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
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && (x.GhlPipelineName.ToLower() == AppConstant.GHL_PipelineId.ToLower() || x.GhlPipelineId.ToLower()
                    == AppConstant.GHL_PipelineId.ToLower())).OrderBy(x => x.StageOrder)
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
            try
            {
                
                var requestUrl = $"https://services.leadconnectorhq.com/v2/contacts";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<GhlContactResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return result?.Contacts ?? new List<GhlContactVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching contacts: {ex.Message}");
                return new List<GhlContactVM>();
            }
        }


        public async Task<GhlContactSingleResponse> CreateContactAsync(GhlContactAdd contact)
        {
            try
            {
                FileLogger.Log($"Base Address from CRete contact: {AppConstant.GHL_BaseAddress}");

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

                var url = $"{AppConstant.GHL_BaseAddress}contacts/";

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

                var url = $"{AppConstant.GHL_BaseAddress}contacts/{contactId}";

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
                email = IsContactExists?.Email,
                phone = IsContactExists?.PhoneNumber,
                monetaryValue = 122.22,
                source = "public hh",
                name = IsContactExists?.FirstName + " " + IsContactExists?.LastName,
                companyName = "Brain Technosys PVT LTd",
            };

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                var endpoint = $"{AppConstant.GHL_BaseAddress}/pipelines/{stage.GhlPipelineId}/opportunities/";
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

                var url = $"{AppConstant.GHL_BaseAddress}pipelines/{targetPipeline?.Id}/opportunities/";
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






