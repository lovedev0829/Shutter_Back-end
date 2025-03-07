
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509.Qualified;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagment.GHL.ViewModel.Request;
using ShutterManagment.GHL.ViewModel.Response;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace ShutterManagement.GHL.Services
{
    public class GhlService
    {
        private readonly HttpClient _httpClient;
        private readonly ShutterDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private const string BaseUrl = "https://rest.gohighlevel.com/v1/";

        public GhlService(HttpClient httpClient, ShutterDBContext dBContext, IHttpContextAccessor _httpContextAccessor)
        {
            _httpClient = httpClient;
            this.dBContext = dBContext;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            httpContextAccessor = _httpContextAccessor;
        }

        #region Start pipeLine and Stages Related Methods 
        public async Task<IResponseVm<string>> GetAndSaveDefaultPipelineAndStage()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var requestUrl = $"{BaseUrl}pipelines/";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var pipelineResponse = System.Text.Json.JsonSerializer.Deserialize<PipelineResponse>(jsonResponse);

                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                var targetPipeline = pipelineResponse.Pipelines.FirstOrDefault(p => p.Id == "CFoi90n88zrDhMFP8R2I");

                if (targetPipeline?.Stages == null || !targetPipeline.Stages.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Target pipeline not found or has no stages.";
                    return responseVm;
                }

                var existingStages = dBContext.TblGhlStages
                    .Where(x => x.GhlPipelineId == targetPipeline.Id)
                    .ToList();
                var existingStagesDict = existingStages.ToDictionary(x => x.GhlStageId, x => x);

                List<TblGhlStage> stagesToUpdate = new();
                List<TblGhlStage> stagesToInsert = new();

                foreach (var stage in targetPipeline.Stages)
                {
                    if (existingStagesDict.TryGetValue(stage.Id, out var existingStage))
                    {
                        existingStage.GhlStageName = stage.Name;
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
                            GhlLastSync = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            TenantId = AppConstant.LoggedInTenantId,
                            CreatedBy = AppConstant.LoggedInUserID,
                            IsActive = true,
                            ModifiedBy = AppConstant.LoggedInUserID
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

                responseVm.IsSuccess = true;
                responseVm.Message = "All pipeline stages updated or inserted successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<PipelineAndStage>>> GetGHLStages()
        {
            IResponseVm<List<PipelineAndStage>> responseVm = new ResponseVm<List<PipelineAndStage>>();
            try
            {
                var gHLStages = dBContext.TblGhlStages
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x=>x.CreatedAt)
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
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2NhdGlvbl9pZCI6IlBUbklDTU4wbUtyMDNrekpRcVBRIiwidmVyc2lvbiI6MSwiaWF0IjoxNzI0MDcwNDM3MTYzLCJzdWIiOiJkOGhlWkl2dEliZDVPcGhGdXNmYiJ9.yRrmWRgUC5VtrhKMsE4JsLjpwROYvvQnwoYu5JbVQWU"; // Replace with a secure method to fetch the API key
        }

        #endregion Start pipeLine and Stages Related Methods 

        #region Start Contact Related Methods
        public async Task<List<GhlContactVM>> GetContactsAsync()
        {
            var requestUrl = $"{BaseUrl}contacts/";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<GhlContactResponse>(json);
            var result1 = GetClientIpAddress();
            return result?.Contacts ?? new List<GhlContactVM>();
        }

        public async Task<bool> CreateContactAsync(GhlContactAdd contact)
        {
            try
            {
                var contactData = new
                {
                    firstName = contact.FirstName,
                    lastName = contact.LastName,
                    email = contact.Email,
                    phone = contact.Phone,
                };
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("contacts", content);
                var body = response.Content;
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
                response.EnsureSuccessStatusCode();
                var result1 = GetClientIpAddress();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var contactResponse = JsonConvert.DeserializeObject<GhlContactVM>(jsonResponse);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateContactAsync(string contactId, GhlContactAdd contact)
        {
            try
            {
                if (string.IsNullOrEmpty(contactId) || contact == null)
                {
                    return false;
                }

                var contactData = new
                {
                    firstName = contact.FirstName,
                    lastName = contact.LastName,
                    email = contact.Email,
                    phone = contact.Phone
                };
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"contacts/{contactId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to update contact. Status code: {response.StatusCode}");
                    return false;
                }

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var contactResponse = JsonConvert.DeserializeObject<GhlContactVM>(jsonResponse);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating contact: {ex.Message}");
                return false;
            }
        }


        public string GetClientIpAddress()
        {
            var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return ipAddress;
        }



        #endregion End Contact Related Methods

        #region Start Opportunity Related Methods

        public async Task<IResponseVm<string>> CreateOpportunityInGhl(Guid? contactId, string jobName, string notes)
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
            var opportunityData = new
            {
                title = jobName,
                status = "open",
                stageId = stage.GhlStageId,
                email="am@gmail.com",
                phone= "+1202-555-0196",
                monetaryValue= 122.22,
                source= "public hh",
                name="Amit Kumar",
                companyName="Brain",
               
            };

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
                var endpoint = $"{BaseUrl}pipelines/{stage.GhlPipelineId}/opportunities/";
                var opportunityJson = JsonConvert.SerializeObject(opportunityData);
                var content = new StringContent(opportunityJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Error creating opportunity: {response.StatusCode}";
                    return responseVm;
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

        // handle Webhook for opportunity creation or updation
        public async Task<IResponseVm<string>> ProcessGhlOpportunityWebhook(GhlOpportunityWebhookModel webhookData)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            if (webhookData?.Opportunity == null)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = "Invalid webhook data.";
                return responseVm;
            }

            var ghlOpportunity = webhookData.Opportunity;
            var existingJob = dBContext.TblJobs.FirstOrDefault(j => j.GhlOpportunityId == ghlOpportunity.Id);

            if (existingJob != null)
            {
                
                existingJob.JobName = ghlOpportunity.Title;
                existingJob.Notes = ghlOpportunity.Description;
                //existingJob.status = ghlOpportunity.Status;
                existingJob.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                
                var newJob = new TblJob
                {
                    JobId = Guid.NewGuid(),
                    JobName = ghlOpportunity.Title,
                    Notes = ghlOpportunity.Description,
                    //JobStatus = ghlOpportunity.Status,
                    GhlOpportunityId = ghlOpportunity.Id,
                    CreatedAt = DateTime.UtcNow
                };

                 dBContext.TblJobs.Add(newJob);
            }

            responseVm.IsSuccess = true;
            responseVm.Message = "Job successfully synced with GHL opportunity.";
            return responseVm;
        }


        public async Task<IResponseVm<List<Opportunity>>> GetOpportunitiesFromGhl()
        {
            IResponseVm<List<Opportunity>> responseVm = new ResponseVm<List<Opportunity>>();

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());

                var url = "https://rest.gohighlevel.com/v1/pipelines/CFoi90n88zrDhMFP8R2I/opportunities/";
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



        #endregion End Opportunity Related Methods
    }
}






