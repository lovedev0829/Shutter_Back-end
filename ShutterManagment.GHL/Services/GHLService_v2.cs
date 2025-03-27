
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShutterManagement.GHL.Services
{
    public class GHLService_v2
    {
        private readonly HttpClient _httpClient;
        private readonly ShutterDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private string BaseUrl = string.Empty;
      //  private string accesstoken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoQ2xhc3MiOiJMb2NhdGlvbiIsImF1dGhDbGFzc0lkIjoiUFRuSUNNTjBtS3IwM2t6SlFxUFEiLCJzb3VyY2UiOiJJTlRFR1JBVElPTiIsInNvdXJjZUlkIjoiNjdlMTFjNzJjOTBhMmY4MTIwMGNjMTJhLW04bXRzcXJuIiwiY2hhbm5lbCI6Ik9BVVRIIiwicHJpbWFyeUF1dGhDbGFzc0lkIjoiUFRuSUNNTjBtS3IwM2t6SlFxUFEiLCJvYXV0aE1ldGEiOnsic2NvcGVzIjpbImNvbnZlcnNhdGlvbnMvbWVzc2FnZS5yZWFkb25seSIsImNvbnZlcnNhdGlvbnMvbWVzc2FnZS53cml0ZSJdLCJjbGllbnQiOiI2N2UxMWM3MmM5MGEyZjgxMjAwY2MxMmEiLCJjbGllbnRLZXkiOiI2N2UxMWM3MmM5MGEyZjgxMjAwY2MxMmEtbThtdHNxcm4ifSwiaWF0IjoxNzQyOTg3NTQyLjIwMywiZXhwIjoxNzQzMDczOTQyLjIwM30.fMjXamfhw4jgfLo2tzDKJS1kA_S89MX02jPxTCheOv4UwJrIjd1DJD3RtW8FIpmOhEvrCaZGw5MKVzqczzzDrhGx3Q_dfJ3ZdMKzH_q8D4pgYKo0V7TlKXH6wp2mu63__iV7ZfWO2XxVFBS9zyg5O2fDYhx209owntBKEYK5DjDfnyxeuryzGBDTaAP7fCor_BFmlcgtpmHpKsy3XNV-faFY-Ha3-XTGN4VQwFv2JAkEKNshJmnBbuF2UdoKcWjCs2C4O62eicNGhj0KC-JelG81aTQr1rqHNC3jdEGT7FPSajkdpJuqzAfrWBitm4FE0QGVe7QVLonfVTzX_om0B8h3QsTtrHKWGyZXaddrtsV-1ssjeE9ojMM2qiyGnDE0VFTWcDicbySyB24bWK5x5VO1f79XpkSKZHgBYeU9vKtYaZcRVE3gcnKNKYT9wl82ARYVKmzHSYv5IEJfdq4epmL9TmuERuowW31aznFFgzuluFqED2dockpRUrDqZjtAklT-66RGex3Xfiu6SW3ImsKscZm5gmjUtAEEY0whoP4FIJf4VWFuId8DEmJhBKRSp2gJDqhro5yuNv_OxLdwOJK07N2AjhjW6TP7L856NJzJBUUR-KonLSuJ9C1tU0C8Y2BDioDEn-P0Qonp38aoCzSnmHQiaYnpv0iNnWoa6Ws";

        public GHLService_v2(HttpClient httpClient, ShutterDBContext dBContext, IHttpContextAccessor _httpContextAccessor)
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
                var requestUrl = $"{AppConstant.GHL_BaseAddress}/opportunities/pipelines?locationId={AppConstant.GHL_Locationd}";
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                requestMessage.Headers.Add("Version", "2021-07-28");
                //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
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
                                             .FirstOrDefaultAsync(c => c.TenantId == AppConstant.LoggedInTenantId);

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
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId).OrderBy(x => x.StageOrder)
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
                var requestUrl = $"{AppConstant.GHL_BaseAddress}/contacts/?locationId={AppConstant.GHL_Locationd}";
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                requestMessage.Headers.Add("Version", "2021-07-28");
                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
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


        public async Task<GhlContactSingleResponse?> CreateContactAsync(GhlContactAdd contact)
        {
            try
            {
                if (contact == null)
                {
                    FileLogger.Log("CreateContactAsync: Contact object is null.");
                    return null;
                }

                if (string.IsNullOrEmpty(AppConstant.GHL_AccessToken))
                {
                    FileLogger.Log("CreateContactAsync: OAuth token is missing or invalid.");
                    return null;
                }

                var contactData = new
                {
                    firstName = contact.FirstName,
                    lastName = contact.LastName,
                    email = contact.Email,
                    phone = contact.Phone,
                    locationId = AppConstant.GHL_Locationd,
                    source = "public api",
                    companyName = "Brain Technosys",
                    assignedTo = "d8heZIvtIbd5OphFusfb",
                    tags = new[] { "tag1", "tag2" },
                    customFields = new[]
                    {
                new { id = "6dvNaf7VhkQ9snc5vnjJ", key = "my_custom_field", field_value = "9039160788" }
            }
                };

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/contacts";
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Version", "2021-07-28");

                HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"CreateContactAsync Error: {response.StatusCode} - {jsonResponse}");
                    return null;
                }

                return JsonConvert.DeserializeObject<GhlContactSingleResponse>(jsonResponse);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in CreateContactAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<GhlContactSingleResponse?> UpdateContactAsync(string contactId, GhlContactAdd contact)
        {
            try
            {
                if (string.IsNullOrEmpty(contactId) || contact == null)
                {
                    FileLogger.Log("UpdateContactAsync: Invalid input. ContactId or Contact is null.");
                    return null;
                }

                if (string.IsNullOrEmpty(AppConstant.GHL_AccessToken))
                {
                    FileLogger.Log("UpdateContactAsync: OAuth token is missing or invalid.");
                    return null;
                }

                var contactData = new
                {
                    firstName = contact.FirstName,
                    lastName = contact.LastName,
                    email = contact.Email,
                    phone = contact.Phone,
                    locationId = AppConstant.GHL_Locationd, 
                    source = "public api",
                    companyName = "Brain Technosys",
                    assignedTo = "d8heZIvtIbd5OphFusfb",
                    tags = new[] { "tag1", "tag2" },
                    customFields = new[]
                    {
                new { id = "6dvNaf7VhkQ9snc5vnjJ", key = "my_custom_field", field_value = "9039160788" }
            }
                };

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/contacts/{contactId}";
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Version", "2021-07-28");

                HttpResponseMessage response = await httpClient.PutAsync(requestUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"UpdateContactAsync Error: {response.StatusCode} - {jsonResponse}");
                    return null;
                }

                return JsonConvert.DeserializeObject<GhlContactSingleResponse>(jsonResponse);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in UpdateContactAsync: {ex.Message}");
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

            try
            {
                    
                var res = await GetGHLStages();
                var stage = res?.Data.FirstOrDefault();
                if (stage == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No available pipeline stage found.";
                    return responseVm;
                }

                var contact = dBContext.TblContacts.FirstOrDefault(c => c.ContactId == job.ContactId && c.IsDelete==false);
                if (contact == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No associated contact found for the job.";
                    return responseVm;
                }

                var opportunityData = new
                {
                    pipelineId = stage.GhlPipelineId,
                    locationId = AppConstant.GHL_Locationd,
                    name = job.JobName,
                    pipelineStageId = job.GhlStageId,
                    status = "open",
                    contactId = contact.ContactId,
                    monetaryValue = 220,
                    assignedTo = "082goXVW3lIExEQPOnd3",
                };

                var endpoint = $"{AppConstant.GHL_BaseAddress}/opportunities";
                var opportunityJson = JsonConvert.SerializeObject(opportunityData);
                var content = new StringContent(opportunityJson, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Version", "2021-07-28");

                var response = await httpClient.PostAsync(endpoint, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

               
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    FileLogger.Log($"Error: {response.StatusCode} - {errorResponse}");
                    return null;
                }

                var result = JsonConvert.DeserializeObject<OpportunityResponse>(jsonResponse);
                if (result == null || string.IsNullOrEmpty(result.OpportunityId))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Failed to retrieve opportunity details from response.";
                    return responseVm;
                }
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

        #endregion End Opportunity Related Methods


        public async Task<GHLAppointmentResponse> CreateAppointmentAsync(GHLAppointmentAdd appointment)
        {
            try
            {

                var CalendarId = GetAppointmentsAsync().Result.Where(x => x.name.ToLower() == AppConstant.GHL_CalendarName.ToLower()).Select(x=> x.id).FirstOrDefault();
                FileLogger.Log($"Base Address from Create Appointment: {AppConstant.GHL_BaseAddress}");

                var contactData = new
                {
                    calendarId = CalendarId,
                    contactId = appointment.contactId,
                    startTime = appointment.startTime,
                    locationId = appointment.locationId,
                    endTime = appointment.endtime,
                };

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/calendars/events/appointments";

                // Create the HTTP client
                using var httpClient = new HttpClient();

                // Prepare the request content (serialized JSON)
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                // Create the request message with necessary headers and set method to POST
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = content
                };

                // Set headers (authorization and versioning)
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                requestMessage.Headers.Add("Version", "2021-07-28");

                var response = await httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    FileLogger.Log($"Error: {response.StatusCode} - {errorResponse}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var appointmentResponse = JsonConvert.DeserializeObject<GHLAppointmentResponse>(jsonResponse);

                return appointmentResponse;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in CreateAppointmentAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<GHLAppointmentResponse> UpdateAppointmentAsync(string eventId ,GHLAppointmentAdd appointment)
        {
            try
            {

                var CalendarId = GetAppointmentsAsync().Result.Where(x => x.name.ToLower() == AppConstant.GHL_CalendarName.ToLower()).Select(x => x.id).FirstOrDefault();
                FileLogger.Log($"Base Address from Create Appointment: {AppConstant.GHL_BaseAddress}");

                var contactData = new
                {
                    calendarId = CalendarId,
                    contactId = appointment.contactId,
                    startTime = appointment.startTime,
                    locationId = appointment.locationId,
                    endTime = appointment.endtime,
                };

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/calendars/events/appointments/{eventId}";

                // Create the HTTP client
                using var httpClient = new HttpClient();

                // Prepare the request content (serialized JSON)
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                // Create the request message with necessary headers and set method to POST
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = content
                };

                // Set headers (authorization and versioning)
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                requestMessage.Headers.Add("Version", "2021-07-28");

                var response = await httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    FileLogger.Log($"Error: {response.StatusCode} - {errorResponse}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var appointmentResponse = JsonConvert.DeserializeObject<GHLAppointmentResponse>(jsonResponse);

                return appointmentResponse;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in CreateAppointmentAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Calendar>> GetAppointmentsAsync()
        {
            try
            {
                FileLogger.Log("Fetching Calendar:-");
                // Construct the URL for the API request
                var requestUrl = $"{AppConstant.GHL_BaseAddress}/calendars/?locationId={AppConstant.GHL_Locationd}";

                // Create a new HttpRequestMessage with GET method
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Set the necessary headers (Authorization and Version)
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
                requestMessage.Headers.Add("Version", "2021-07-28");

                // Send the request using the _httpClient
                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

                // Ensure the status code indicates success (200-299 range)
                response.EnsureSuccessStatusCode();

                // Read and parse the response content as a string
                var json = await response.Content.ReadAsStringAsync();

                // Deserialize the response to the GetGHLAppointment object
                var result = System.Text.Json.JsonSerializer.Deserialize<GetGHLAppointment>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return result.calendars ?? new List<Calendar>();
            }
            catch (HttpRequestException httpEx)
            {
                // Log HTTP-related exceptions (e.g., network errors, 4xx/5xx status codes)
                FileLogger.Log($"HTTP Request error: {httpEx.Message}");
                return new List<Calendar>();
            }
            catch (Exception ex)
            {
                // Catch any other exceptions that might occur
                FileLogger.Log($"Unexpected error: {ex.Message}");
                return new List<Calendar>();
            }
        }


    }
}






