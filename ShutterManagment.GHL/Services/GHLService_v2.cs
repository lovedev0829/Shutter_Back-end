
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.Crypto;
using ShutterManagement.Business;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagment.GHL.ViewModel.Request;
using ShutterManagment.GHL.ViewModel.Response;
using Stripe;
using System;
using System.Linq;


//using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace ShutterManagement.GHL.Services
{
    public class GHLService_v2
    {
        private readonly HttpClient _httpClient;
        private readonly ShutterDBContext dBContext;
        private readonly ClaimsProperty claimsProperty;
        private readonly IHttpContextAccessor httpContextAccessor;


        public GHLService_v2(HttpClient httpClient, ShutterDBContext dBContext,ClaimsProperty claimsProperty,
            IHttpContextAccessor _httpContextAccessor )
        {
            _httpClient = httpClient;
            this.dBContext = dBContext;
            this.claimsProperty = claimsProperty;
            httpContextAccessor = _httpContextAccessor;


        }

        #region Start pipeLine and Stages Related Methods

        public async Task<PipelineResponse> GetAllPipelinesWithStagesAsync(GetGHLConfiguration ghlconfiguration)
        {
            try
            {
                
                var requestUrl = $"{AppConstant.GHL_BaseAddress}/opportunities/pipelines?locationId={ghlconfiguration.LocationId}";
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ghlconfiguration.Access_Token);
                requestMessage.Headers.Add("Version", AppConstant.GHL_Version);
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






        public async Task<IResponseVm<string>> GetAndSaveDefaultPipelineAndStage(GetGHLConfiguration getGHLConfiguration, Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
                var pipelineResponse = await GetAllPipelinesWithStagesAsync(getGHLConfiguration);

                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No pipelines found.";
                    return responseVm;
                }

                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(getGHLConfiguration?.PipelineId??"") || p.Name.Contains(getGHLConfiguration?.PipelineId ?? ""));

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
                    .Where(x => x.GhlPipelineId == targetPipeline.Id && x.TenantId== LoggedInTenantId)
                    .ToList();

                FileLogger.Log($"Exception occurred GHLContact is null.{existingStages}");

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
                        existingStage.ModifiedBy = LoggedInTenantId;
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
                            TenantId = LoggedInTenantId,
                            CreatedBy = LoggedInTenantId,
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

                var isUpdated = await UpdateLastSyncConfigurationAsync(LoggedInTenantId);

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

        public async Task<bool> UpdateLastSyncConfigurationAsync(Guid? LoggedInTenantId = null)
        {
            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

                var config = await dBContext.TblGhlconfigurations
                                             .FirstOrDefaultAsync(c => c.TenantId == LoggedInTenantId);

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
                    .Where(x => x.TenantId == claimsProperty.GetTenantId()).OrderBy(x => x.StageOrder)
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
        //public async Task<List<GhlContactVM>> GetContactsAsync()
        //{
        //    try
        //    {
        //        var requestUrl = $"{AppConstant.GHL_BaseAddress}/contacts/?locationId={AppConstant.GHL_LocationId}";
        //        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        //        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
        //        requestMessage.Headers.Add("Version", AppConstant.GHL_Version);
        //        HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
        //        response.EnsureSuccessStatusCode();

        //        var json = await response.Content.ReadAsStringAsync();
        //        var result = System.Text.Json.JsonSerializer.Deserialize<GhlContactResponse>(json, new JsonSerializerOptions
        //        {
        //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //        });

        //        return result?.Contacts ?? new List<GhlContactVM>();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error fetching contacts: {ex.Message}");
        //        return new List<GhlContactVM>();
        //    }
        //}

        public async Task<List<GhlContactVM>> GetContactsAsync(GetGHLConfiguration getGHLConfiguration)
        {
            var allContacts = new List<GhlContactVM>();

            try
            {
                var baseUrl = $"{AppConstant.GHL_BaseAddress}/contacts/search";
                var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {getGHLConfiguration.Access_Token}" },
            { "Version", AppConstant.GHL_Version }
        };

                var initialPayload = new
                {
                    locationId = getGHLConfiguration.LocationId,
                    pageLimit = 1,
                    page = 1
                };

                var initialResponse = await SendGhlPostRequest<GhlContactResponse>(baseUrl, initialPayload, headers);

                var total = initialResponse?.Total ?? 0;

                if (total == 0)
                    return allContacts;

                const int maxLimit = 100;
                int totalPages = (int)Math.Ceiling(total / (double)maxLimit);

                for (int page = 1; page <= totalPages; page++)
                {
                    var pagedPayload = new
                    {
                        locationId = getGHLConfiguration.LocationId,
                        pageLimit = maxLimit,
                        page = page
                    };

                    var pageResponse = await SendGhlPostRequest<GhlContactResponse>(baseUrl, pagedPayload,headers);

                    if (pageResponse?.Contacts != null)
                        allContacts.AddRange(pageResponse.Contacts);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching contacts: {ex.Message}");
            }

            return allContacts;
        }





        private async Task<T?> SendGhlPostRequest<T>(string url, object payload, Dictionary<string, string> headers)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(payload),
                        Encoding.UTF8,
                        "application/json")
                };

                foreach (var header in headers)
                {
                    if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.Authorization = AuthenticationHeaderValue.Parse(header.Value);
                    }
                    else
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                return System.Text.Json.JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                LogException(nameof(SendGhlPostRequest), ex);
                return default;
            }
        }


        private void LogException(string context, Exception ex)
        {
            FileLogger.Log($"[ERROR] Context    : {context}");
            FileLogger.Log($"Message   : {ex.Message}");
            FileLogger.Log($"StackTrace: {ex.StackTrace}");
        }


       public async Task<IResponseVm<GhlContactSingleResponse>> CreateContactAsync(GetGHLConfiguration getGHLConfiguration, GhlContactAdd contact)
        {
            IResponseVm<GhlContactSingleResponse> responseVm = new ResponseVm<GhlContactSingleResponse>();
            try
            {
                if (contact == null)
                {
                    FileLogger.Log("CreateContactAsync: Contact object is null.");
                    responseVm.Message = "Contact object is null";
                    return responseVm;
                }

                if (string.IsNullOrEmpty(getGHLConfiguration.Access_Token))
                {
                    FileLogger.Log("CreateContactAsync: OAuth token is missing or invalid.");
                    responseVm.Message = "OAuth token is missing or invalid.";
                    return responseVm;
                }

                var contactData = new Dictionary<string, object>
                {
                    ["firstName"] = contact.FirstName,
                    ["lastName"] = contact.LastName,
                    ["locationId"] = getGHLConfiguration.LocationId,
                    ["address1"] = contact.StreetAddress ?? "",
                    ["city"] = contact.City ?? "",
                    ["state"] = contact.State ?? "",
                    ["postalCode"] = contact.ZipCode ?? ""
                };

                // Only include email if valid
                if (!string.IsNullOrWhiteSpace(contact.Email))
                    contactData["email"] = contact.Email;

                // Only include phone if valid
                if (!string.IsNullOrWhiteSpace(contact.Phone))
                    contactData["phone"] = contact.Phone;

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/contacts";
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration.Access_Token);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Version", AppConstant.GHL_Version);

                HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {

                    string errorMessage = $"Failed to get access token. StatusCode: {response.StatusCode}";

                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<ErrorResponse>(jsonResponse);
                        if (errorObj != null)
                        {
                            responseVm.Message = $", Error: {errorObj.Error} - {errorObj.ErrorDescription}";
                            FileLogger.Log($", Error: {errorObj.Error} - {errorObj.ErrorDescription}");
                            errorMessage += $", Error: {errorObj.Error} - {errorObj.ErrorDescription}";
                        }
                        else
                        {
                            errorMessage += $", Raw Response: {jsonResponse}";
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        errorMessage += $", Error parsing error response: {jsonEx.Message}";
                        FileLogger.Log($"Error deserializing error response: {jsonEx.Message}");
                    }

                    responseVm.IsSuccess = false;
                    responseVm.Message = errorMessage;
                    FileLogger.Log(errorMessage);
                    return responseVm;

                }
                responseVm.IsSuccess = true;
                responseVm.Data = JsonConvert.DeserializeObject<GhlContactSingleResponse>(jsonResponse);
                responseVm.Message = AppConstant.InsertSuccessMsg;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in CreateContactAsync: {ex.Message}");
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<GhlContactSingleResponse?>> UpdateContactAsync(GetGHLConfiguration getGHLConfiguration, string contactId, GhlContactAdd contact)
        {
            IResponseVm<GhlContactSingleResponse> responseVm = new ResponseVm<GhlContactSingleResponse>();
            try
            {
                if (string.IsNullOrEmpty(contactId) || contact == null)
                {
                    FileLogger.Log("UpdateContactAsync: Invalid input. ContactId or Contact is null.");
                    responseVm.Message = "Contact object is null";
                    return responseVm;
                }

                if (string.IsNullOrEmpty(getGHLConfiguration.Access_Token))
                {
                    FileLogger.Log("UpdateContactAsync: OAuth token is missing or invalid.");
                    responseVm.Message = "OAuth token is missing or invalid.";
                    return responseVm;
                }

                var contactData = new Dictionary<string, object>
                {
                    ["firstName"] = contact.FirstName,
                    ["lastName"] = contact.LastName,
                    ["address1"] = contact.StreetAddress ?? "",
                    ["city"] = contact.City ?? "",
                    ["state"] = contact.State ?? "",
                    ["postalCode"] = contact.ZipCode ?? ""
                };

                // Only include email if valid
                if (!string.IsNullOrWhiteSpace(contact.Email))
                    contactData["email"] = contact.Email;

                // Only include phone if valid
                if (!string.IsNullOrWhiteSpace(contact.Phone))
                    contactData["phone"] = contact.Phone;

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/contacts/{contactId}";
                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration.Access_Token);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Version", AppConstant.GHL_Version);

                HttpResponseMessage response = await httpClient.PutAsync(requestUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
                responseVm.Data = JsonConvert.DeserializeObject<GhlContactSingleResponse>(jsonResponse);
                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"UpdateContactAsync Error: {response.StatusCode} - {jsonResponse}");
                    responseVm.Message = "Contact object is null";
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in UpdateContactAsync: {ex.Message}");
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public string GetClientIpAddress()
        {
            var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return ipAddress;
        }

        #endregion End Contact Related Methods

        #region Start Opportunity Related Methods

        public async Task<IResponseVm<string>> CreateOpportunityInGhl(GetGHLConfiguration getGHLConfiguration, TblJob job)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {  
                 var stageResponse = await GetGHLStagesMappingsById(job.JobStatus);
                if(stageResponse?.Data== null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No Stage Found On the Basis JobStatus";
                    return responseVm;
                }

                var res = await GetGHLStages();
                var stage = res?.Data?.FirstOrDefault();
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
                var GHUserId = dBContext.TblUsers.Where(x => x.Id == job.AssignedPersonnel && x.IsDelete == false && x.IsActive == true)
                    .Select(y => y.GhlUserId).FirstOrDefault() ?? "";

                var opportunityData = new
                {
                    pipelineId = stage.GhlPipelineId,
                    locationId = getGHLConfiguration.LocationId,
                    name = job.JobName ?? "Unnamed Job",
                    pipelineStageId = stageResponse?.Data?.StageId?.ToString()??"",
                    status = "open",
                    contactId = contact.GhlcontactId,
                    assignedTo = GHUserId ?? "",
                };

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/opportunities/";
                var opportunityJson = JsonConvert.SerializeObject(opportunityData);
                var content = new StringContent(opportunityJson, Encoding.UTF8, "application/json");
                FileLogger.Log($"[CreateOpportunityInGhl] Request URL: {requestUrl}");
                FileLogger.Log($"[CreateOpportunityInGhl] Request Payload: {opportunityJson}");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration.Access_Token);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Version", AppConstant.GHL_Version);

                HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                FileLogger.Log($"[CreateOpportunityInGhl] Response Status Code: {response.StatusCode}");
                FileLogger.Log($"[CreateOpportunityInGhl] Response Content: {jsonResponse}");

                if (!response.IsSuccessStatusCode)
                {

                    string errorMessage = $"Failed to get access token. StatusCode: {response.StatusCode}";

                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<ErrorResponse>(jsonResponse);
                        if (errorObj != null)
                        {
                            responseVm.Message = $", Error: {errorObj.Error} - {errorObj.ErrorDescription}";
                            FileLogger.Log($", Error: {errorObj.Error} - {errorObj.ErrorDescription}");
                            errorMessage += $", Error: {errorObj.Error} - {errorObj.ErrorDescription}";
                        }
                        else
                        {
                            errorMessage += $", Raw Response: {jsonResponse}";
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        errorMessage += $", Error parsing error response: {jsonEx.Message}";
                        FileLogger.Log($"Error deserializing error response: {jsonEx.Message}");
                    }

                    responseVm.IsSuccess = false;
                    responseVm.Message = errorMessage;
                    FileLogger.Log(errorMessage);
                    return responseVm;
                    

                }

                var result = JsonConvert.DeserializeObject<OpportunityResponsees>(jsonResponse);
                if (result == null )
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Failed to retrieve opportunity details from response.";
                    return responseVm;
                }

                var existingJob = await dBContext.TblJobs.Where(j => j.JobId == job.JobId).FirstOrDefaultAsync();
                if (existingJob != null)
                {
                    existingJob.GhlOpportunityId = result.Opportunity.OpportunityId;
                    existingJob.GhlContactId = result.Opportunity.Contact?.ContactId;
                    dBContext.TblJobs.Update(existingJob);
                    await dBContext.SaveChangesAsync();
                }

                responseVm.IsSuccess = true;
                responseVm.Message = "Opportunity added successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception occurred: {ex.Message}";
                FileLogger.Log($"[CreateOpportunityInGhl] Exception: {ex}");
            }

            return responseVm;
        }




        //public async Task<IResponseVm<List<Opportunity>>> GetOpportunitiesFromGhl()
        //{
        //    IResponseVm<List<Opportunity>> responseVm = new ResponseVm<List<Opportunity>>();

        //    try
        //    {



        //        var pipelineResponse = await GetAllPipelinesWithStagesAsync();

        //        if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
        //        {
        //            responseVm.IsSuccess = false;
        //            responseVm.Message = "No pipelines found.";
        //            return responseVm;
        //        }

        //        string userProvidedPipelineNameOrId = AppConstant.GHL_PipelineId;

        //        var targetPipeline = pipelineResponse.Pipelines
        //            .FirstOrDefault(p => p.Id.Contains(userProvidedPipelineNameOrId) || p.Name.Contains(userProvidedPipelineNameOrId));
        //        string url = $"{AppConstant.GHL_BaseAddress}/opportunities/search?location_id={AppConstant.GHL_LocationId}&"+$"pipeline_id={AppConstant.GHL_PipelineId}";
        //        //string url = $"{AppConstant.GHL_BaseAddress}/opportunities/search?location_id={AppConstant.GHL_LocationId}";
        //        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
        //        _httpClient.DefaultRequestHeaders.Add("Version", AppConstant.GHL_Version);

        //        HttpResponseMessage response = await _httpClient.GetAsync(url);
        //        string responseBody = await response.Content.ReadAsStringAsync();

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            responseVm.IsSuccess = false;
        //            responseVm.Message = $"Error: {response.StatusCode}, Details: {responseBody}";
        //            return responseVm;
        //        }

        //        var ghlResponse = JsonConvert.DeserializeObject<GhlOpportunitiesResponse>(responseBody);
        //        responseVm.Data = ghlResponse?.Opportunities ?? new List<Opportunity>();
        //        responseVm.IsSuccess = true;
        //        responseVm.Message = AppConstant.FetchSuccessMsg;
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.IsSuccess = false;
        //        responseVm.Message = $"Exception occurred: {ex.Message}";
        //    }

        //    return responseVm;
        //}


        public async Task<IResponseVm<List<Opportunity>>> GetOpportunitiesFromGhl(GetGHLConfiguration getGHLConfiguration)
        {
            IResponseVm<List<Opportunity>> responseVm = new ResponseVm<List<Opportunity>>();

            try
            {
                var pipelineResponse = await GetAllPipelinesWithStagesAsync(getGHLConfiguration);

                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
                {
                    return new ResponseVm<List<Opportunity>>
                    {
                        IsSuccess = false,
                        Message = "No pipelines found.",
                        Data = new List<Opportunity>()
                    };
                }


                var targetPipeline = pipelineResponse.Pipelines
                    .FirstOrDefault(p => p.Id.Contains(getGHLConfiguration?.PipelineId ?? "") ||
                                         p.Name.Contains(getGHLConfiguration?.PipelineId ?? ""));

                if (targetPipeline == null)
                {
                    return new ResponseVm<List<Opportunity>>
                    {
                        IsSuccess = false,
                        Message = "Target pipeline not found.",
                        Data = new List<Opportunity>()
                    };
                }

                string url = $"{AppConstant.GHL_BaseAddress}/opportunities/search" +
                             $"?location_id={getGHLConfiguration?.LocationId ?? ""}" +
                             $"&pipeline_id={targetPipeline.Id}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration?.Access_Token ?? "");
                request.Headers.Add("Version", AppConstant.GHL_Version);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseVm<List<Opportunity>>
                    {
                        IsSuccess = false,
                        Message = $"Error: {response.StatusCode}, Details: {responseBody}",
                        Data = new List<Opportunity>()
                    };
                }

                var ghlResponse = JsonConvert.DeserializeObject<GhlOpportunitiesResponse>(responseBody);
                return new ResponseVm<List<Opportunity>>
                {
                    IsSuccess = true,
                    Data = ghlResponse?.Opportunities ?? new List<Opportunity>(),
                    Message = AppConstant.FetchSuccessMsg
                };
            }
            catch (Exception ex)
            {
                return new ResponseVm<List<Opportunity>>
                {
                    IsSuccess = false,
                    Message = $"Exception occurred: {ex.Message}",
                    Data = new List<Opportunity>()
                };
            }
        }




        #endregion End Opportunity Related Methods

        public async Task<IResponseVm<GHLAppointment>> CreateAppointmentAsync(GetGHLConfiguration getGHLConfiguration, GHLAppointmentAdd appointment)
        {
            IResponseVm<GHLAppointment> responseVm = new ResponseVm<GHLAppointment>();
            try
            {
                var CalendarId = GetCalendarAsync(getGHLConfiguration).Result.Where(x => x.name.ToLower() == getGHLConfiguration.CalendarName.ToLower() || x.id.ToLower() == getGHLConfiguration.CalendarName.ToLower()).Select(x=> x.id).FirstOrDefault();
                FileLogger.Log($"Base Address from Create Appointment: {AppConstant.GHL_BaseAddress}");
                if (CalendarId == null)
                {
                    responseVm.Message = "Calendar does not exist in GHL.";
                    FileLogger.Log("Calendar does not exist in GHL.");
                    return responseVm;
                }

                var contactData = new
                {
                    title= appointment.Title??"",
                    calendarId = CalendarId,
                    contactId = appointment.contactId??"",
                    //contactId = "TYWPKcoMp715KTOPeCHb",
                    startTime = appointment.startTime??"",
                    locationId = appointment.locationId??"",
                    endTime = appointment.endtime,
                    ignoreDateRange = true,
                    meetingLocationType = "default",
                    ignoreFreeSlotValidation = true,
                    assignedUserId= appointment.assignedUserId??"",
                };

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/calendars/events/appointments";

                // Create the HTTP client

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
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration.Access_Token);
                requestMessage.Headers.Add("Version", AppConstant.GHL_Version);

                var response = await _httpClient.SendAsync(requestMessage);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {

                    string errorMessage = $"Failed to get access token. StatusCode: {response.StatusCode}";

                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<ErrorResponse>(jsonResponse);
                        if (errorObj != null)
                        {
                            responseVm.Message = $", Error: {errorObj.Error} - {errorObj.ErrorDescription}";
                            FileLogger.Log($", Error: {errorObj.Error} - {errorObj.ErrorDescription}");
                            errorMessage += $", Error: {errorObj.Error} - {errorObj.ErrorDescription}";
                            return responseVm;
                        }
                        else
                        {
                            errorMessage += $", Raw Response: {jsonResponse}";
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        errorMessage += $", Error parsing error response: {jsonEx.Message}";
                        FileLogger.Log($"Error deserializing error response: {jsonEx.Message}");
                    }

                    responseVm.IsSuccess = false;
                    responseVm.Message = errorMessage;
                    FileLogger.Log(errorMessage);
                    return responseVm;

                }

               
                var appointmentResponse = JsonConvert.DeserializeObject<GHLAppointment>(jsonResponse);
                responseVm.IsSuccess = true;
                responseVm.Message = response.ToString();
                responseVm.Data = appointmentResponse;

            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in CreateAppointmentAsync: {ex.Message}");
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<GHLAppointment>> UpdateAppointmentAsync(GetGHLConfiguration getGHLConfiguration, string eventId, GHLAppointmentAdd appointment)
        {
            IResponseVm<GHLAppointment> responseVm = new ResponseVm<GHLAppointment>();
            try
            {
                var calendarId = (await GetCalendarAsync(getGHLConfiguration))
                                     .Where(x => x.name.ToLower() == getGHLConfiguration?.CalendarName?.ToLower() || x.id.ToLower() == getGHLConfiguration?.CalendarName?.ToLower())
                                     .Select(x => x.id)
                                     .FirstOrDefault();

                if(calendarId == null)
                {
                    responseVm.Message = "Calendar does not exist in GHL.";
                    FileLogger.Log("Calendar does not exist in GHL.");
                    return responseVm;
                }

                FileLogger.Log($"Base Address from Update Appointment: {AppConstant.GHL_BaseAddress}");

                var contactData = new
                {
                    title= appointment.Title ?? "",
                    calendarId = calendarId??"",
                    startTime = appointment.startTime??"",
                    endTime = appointment.endtime??"",
                    ignoreDateRange = true,
                    ignoreFreeSlotValidation = true,
                    assignedUserId = appointment.assignedUserId??"",
                };

                var requestUrl = $"{AppConstant.GHL_BaseAddress}/calendars/events/appointments/{eventId}";

                using var httpClient = new HttpClient();

                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration?.Access_Token);
                httpClient.DefaultRequestHeaders.Add("Version", "2021-04-15");
                var response = await httpClient.PutAsync(requestUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    FileLogger.Log($"Error: {response.StatusCode} - {errorResponse}");
                    responseVm.Message = errorResponse;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var appointmentResponse = JsonConvert.DeserializeObject<GHLAppointment>(jsonResponse);

                responseVm.IsSuccess = true;
                responseVm.Message = "Appointment updated at GoHighLevel";
                responseVm.Data = appointmentResponse;
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the process
                FileLogger.Log($"Exception in UpdateAppointmentAsync: {ex.Message}");
                responseVm.Message = ex.Message;
            }
            return responseVm;

        }

        public async Task<List<Calendar>> GetCalendarAsync( GetGHLConfiguration getGHLConfiguration)
        {
            try
            {
                FileLogger.Log("Fetching Calendar:-");
                // Construct the URL for the API request
                var requestUrl = $"{AppConstant.GHL_BaseAddress}/calendars/?locationId={getGHLConfiguration.LocationId}";

                // Create a new HttpRequestMessage with GET method
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Set the necessary headers (Authorization and Version)
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration.Access_Token);
                requestMessage.Headers.Add("Version", AppConstant.GHL_Version);

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
                FileLogger.Log($"Unexpected error: {ex.Message}");
                return new List<Calendar>();
            }
        }
        //public async Task<List<User>> GetUsersAsync()
        //{
        //    var users = new List<User>();
        //    try
        //    {
        //        var requestUrl = $"{AppConstant.GHL_BaseAddress}/users/?locationId={AppConstant.GHL_LocationId}";
        //        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl)
        //        {
        //            Headers =
        //        {
        //        Accept = { new MediaTypeWithQualityHeaderValue("application/json") },
        //        Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken),
        //        }
        //        };
        //        requestMessage.Headers.Add("Version", AppConstant.GHL_Version);
        //        HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
        //        response.EnsureSuccessStatusCode();
        //        var json = await response.Content.ReadAsStringAsync();

        //        var result = System.Text.Json.JsonSerializer.Deserialize<GetUserVM>(json, new JsonSerializerOptions
        //        {
        //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //        });

        //        if (result?.users != null)
        //        {
        //            users = result.users
        //                .Where(user =>
        //                    user.Calendars != null &&
        //                    user.Calendars.Any(c =>
        //                        string.Equals(c.id.ToLower(), AppConstant.GHL_CalendarName.ToLower(), StringComparison.OrdinalIgnoreCase) ||
        //                        string.Equals(c.name, AppConstant.GHL_CalendarName, StringComparison.OrdinalIgnoreCase)))
        //                .ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FileLogger.Log($"Error fetching users: {ex.Message}");
        //        return new List<User>(); 
        //    }
        //    return users;
        //}

        public async Task<IResponseVm<List<User>>> GetUsersByCalendarIdAsync(GetGHLConfiguration getGHLConfiguration, Guid? LoggedInTenantId = null)
        {
            var responseVm = new ResponseVm<List<User>>
            {
                Data = new List<User>()
            };

            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

                var calendarRequestUrl = $"{AppConstant.GHL_BaseAddress}/calendars/?locationId={getGHLConfiguration?.LocationId}";
                using var calendarRequest = new HttpRequestMessage(HttpMethod.Get, calendarRequestUrl);
                calendarRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                calendarRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration?.Access_Token);
                calendarRequest.Headers.Add("Version", AppConstant.GHL_Version);

                var calendarResponse = await _httpClient.SendAsync(calendarRequest);
                calendarResponse.EnsureSuccessStatusCode();

                var calendarJson = await calendarResponse.Content.ReadAsStringAsync();
                var calendarResult = System.Text.Json.JsonSerializer.Deserialize<GetGHLAppointment>(calendarJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var calendar = calendarResult?.calendars?.FirstOrDefault(c => c.name == getGHLConfiguration?.CalendarName || c.id == getGHLConfiguration?.CalendarName);
                if (calendar == null || calendar.teamMembers == null || !calendar.teamMembers.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No matching calendar or team members found.";
                    return responseVm;
                }

                var userIds = calendar.teamMembers.Select(tm => tm.userId).ToHashSet();

                var userRequestUrl = $"{AppConstant.GHL_BaseAddress}/users/?locationId={getGHLConfiguration?.LocationId}";
                using var userRequest = new HttpRequestMessage(HttpMethod.Get, userRequestUrl);
                userRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration?.Access_Token);
                userRequest.Headers.Add("Version", AppConstant.GHL_Version);

                var userResponse = await _httpClient.SendAsync(userRequest);
                userResponse.EnsureSuccessStatusCode();

                var userJson = await userResponse.Content.ReadAsStringAsync();
                var userResult = System.Text.Json.JsonSerializer.Deserialize<GetUserVM>(userJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (userResult?.users != null)
                {
                    responseVm.Data = userResult.users
                        .Where(u => userIds.Contains(u.id))
                        .Select(u =>
                        {
                            u.CalendarId = calendar.id;
                            u.CalendarName = calendar.name;
                            return u;
                        })
                        .ToList();

                    responseVm.IsSuccess = true;
                    responseVm.Message = "Users fetched successfully.";

                    if (responseVm.IsSuccess)
                    {
                        var usersyncResponse=await SaveOrUpdateGHLUsersAsync(responseVm.Data, LoggedInTenantId);
                        if(usersyncResponse.IsSuccess)
                        {

                           responseVm.Message = "GHL Users synced successfully.";
                        }
                        else
                        {
                            responseVm.IsSuccess = false;
                            responseVm.Message = usersyncResponse.Message;
                        }
                    }
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No users found in response.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                FileLogger.Log($"HTTP error in GetUsersByCalendarIdAsync: {httpEx.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"HTTP error: {httpEx.Message}";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Unexpected error in GetUsersByCalendarIdAsync: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"Unexpected error: {ex.Message}";
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> SaveOrUpdateGHLUsersAsync(List<User> users ,Guid? LoggedInTenantId=null)
        {
            var responseVm = new ResponseVm<string>();
            var now = DateTime.Now;

            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());
                // 1. Get existing users for the tenant
                var existingUsers = await dBContext.TblGhlUsers
                    .Where(u => u.TenantId == LoggedInTenantId && u.IsActive==true && u.IsDelete==false)
                    .ToListAsync();

                var incomingEmails = users
                    .Where(u => !string.IsNullOrEmpty(u.email))
                    .Select(u => u.email.ToLower())
                    .ToList();

                // 2. Remove users not in the incoming list
                var usersToRemove = existingUsers
                    .Where(u => !incomingEmails.Contains(u.GhlEmail.ToLower()))
                    .ToList();

                if (usersToRemove.Any())
                {
                    dBContext.TblGhlUsers.RemoveRange(usersToRemove);
                }

                // 3. Add/update incoming users
                foreach (var user in users)
                {
                    var existingUser = existingUsers
                        .FirstOrDefault(u => u.GhlEmail.Equals(user.email, StringComparison.OrdinalIgnoreCase));

                    if (existingUser != null)
                    {
                        // Update user
                        existingUser.GhlUserName = user.name;
                        existingUser.GhlFirstName = user.firstName;
                        existingUser.GhlLastName = user.lastName;
                        existingUser.GhlEmail = user.email;
                        existingUser.GhlCalendarId = user.CalendarId;
                        existingUser.GhlCalendarName = user.CalendarName;
                        existingUser.GhlLastSync = now;
                        existingUser.UpdatedAt = now;
                        existingUser.ModifiedBy = LoggedInTenantId;
                    }
                    else
                    {
                        // Add new user
                        var newUser = new TblGhlUser
                        {
                            Id = Guid.NewGuid(),
                            GhlUserId = user.id,
                            GhlUserName = user.name,
                            GhlFirstName = user.firstName,
                            GhlLastName = user.lastName,
                            GhlEmail = user.email,
                            GhlCalendarId = user.CalendarId,
                            GhlCalendarName = user.CalendarName,
                            GhlLastSync = now,
                            TenantId = LoggedInTenantId,
                            CreatedBy = AppConstant.LoggedInUserID,
                            UpdatedAt = now,
                            ModifiedBy = AppConstant.LoggedInUserID,
                            IsActive = true,
                            IsDelete = false
                        };

                        await dBContext.TblGhlUsers.AddAsync(newUser);
                    }

                    // 4. Update system user if exists
                    if (!string.IsNullOrEmpty(user.email))
                    {
                        var systemUser = await dBContext.TblUsers
                            .FirstOrDefaultAsync(u =>
                                u.Email == user.email &&
                                u.TenantId == LoggedInTenantId &&
                                u.IsActive==true && u.IsDelete==false);

                        if (systemUser != null)
                        {
                            systemUser.GhlUserId = user.id;
                            systemUser.ModifiedAt = now;
                            systemUser.ModifiedBy = AppConstant.LoggedInUserID;
                        }
                    }
                }

                await dBContext.SaveChangesAsync();

                responseVm.IsSuccess = true;
                responseVm.Data = "GHL users saved or updated successfully.";
                responseVm.Message = "GHL Users successfully synced.";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error in SaveOrUpdateGHLUsersAsync: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = "An error occurred while saving GHL users.";
            }

            return responseVm;
        }


        public async Task<IResponseVm<List<TblGhlUser>>> GetGHLUsersByCalendarId(GetGHLConfiguration getGHLConfiguration, Guid? LoggedInTenantId=null)
        {
            IResponseVm<List<TblGhlUser>> responseVm = new ResponseVm<List<TblGhlUser>>();

            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : claimsProperty.GetTenantId());

                var users = await dBContext.TblGhlUsers
                    .Where(x => x.TenantId == LoggedInTenantId && x.IsDelete==false && (x.GhlCalendarId == getGHLConfiguration.CalendarName ||  x.GhlCalendarName == getGHLConfiguration.CalendarName))
                    .ToListAsync();

                if (users != null && users.Count > 0)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Users fetched successfully.";
                    responseVm.Data = users;
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No users found.";
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
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.StatusId == JobstatusId)
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

        public class OpportunityResponsees
        {
            [JsonProperty("opportunity")]
            public OpportunityDetails Opportunity { get; set; }
        }

        public class OpportunityDetails
        {
            [JsonProperty("id")]
            public string OpportunityId { get; set; }

            [JsonProperty("contactId")]
            public string ContactId { get; set; }

            [JsonProperty("contact")]
            public ContactDetails Contact { get; set; }
        }

        public class ContactDetails
        {
            [JsonProperty("id")]
            public string ContactId { get; set; }
        }

        public async Task<IResponseVm<GHLJobAssignedtoPartner>> GetOpportunityforPartner(GetGHLConfiguration getGHLConfiguration, string? GHLUserId,string? ContactId = null)
        {
            var responseVm = new ResponseVm<GHLJobAssignedtoPartner>();

            try
            {
                FileLogger.Log("Fetching Opportunities...");
                var requestUrl = "";
                var PipelineId = dBContext.TblGhlStages.Where(x => x.GhlPipelineName.ToLower() == getGHLConfiguration.PipelineId.ToLower() && x.IsActive == true).
                    Select(x => x.GhlPipelineId).FirstOrDefault();
                if (!string.IsNullOrEmpty(ContactId))
                {
                    requestUrl = $"{AppConstant.GHL_BaseAddress}/opportunities/search?location_id={getGHLConfiguration.LocationId}&pipeline_id={PipelineId}&contact_id={ContactId}&status=open";
                }
                if (!string.IsNullOrEmpty(GHLUserId))
                {
                    requestUrl = $"{AppConstant.GHL_BaseAddress}/opportunities/search?location_id={getGHLConfiguration.LocationId}&pipeline_id={PipelineId}&assigned_to={GHLUserId}";
                }

                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Set the necessary headers (Authorization and Version)
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration.Access_Token);
                requestMessage.Headers.Add("Version", AppConstant.GHL_Version);

                // Send the request using the _httpClient
                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

                // Ensure the status code indicates success (200-299 range)
                response.EnsureSuccessStatusCode();

                // Read and parse the response content as a string
                var json = await response.Content.ReadAsStringAsync();

                // Deserialize the response to the GetGHLAppointment object
                var result = System.Text.Json.JsonSerializer.Deserialize<GHLJobAssignedtoPartner>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
               
                responseVm.IsSuccess = true;
                responseVm.Data = result;
            }
            catch (HttpRequestException httpEx)
            {
                FileLogger.Log($"HTTP Request error: {httpEx.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"HTTP error: {httpEx.Message}";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Unexpected error: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"Unexpected error: {ex.Message}";
            }

            return responseVm;
        }



        //public async Task<IResponseVm<string>> SyncGhlCustomFields(Guid? LoggedInTenantId = null)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();

        //    try
        //    {
        //        LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : AppConstant.LoggedInTenantId);

        //        string url = $"{AppConstant.GHL_BaseAddress}/locations/{AppConstant.GHL_LocationId}/customFields?model=all";

        //        var request = new HttpRequestMessage(HttpMethod.Get, url);
        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppConstant.GHL_AccessToken);
        //        request.Headers.Add("Version", "2021-07-28");
        //        request.Headers.Add("Accept", "application/json");

        //        var response = await _httpClient.SendAsync(request);
        //        var jsonString = await response.Content.ReadAsStringAsync();

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            responseVm.Message = $"Failed to fetch custom fields: {response.StatusCode}, Body: {jsonString}";
        //            return responseVm;
        //        }

        //        var fieldResponse = JsonConvert.DeserializeObject<GhlCustomFieldResponse>(jsonString);

        //        if (fieldResponse?.customFields == null || !fieldResponse.customFields.Any())
        //        {
        //            responseVm.Message = "No custom fields found.";
        //            return responseVm;
        //        }

        //        foreach (var field in fieldResponse.customFields)
        //        {
        //            var existingField = await dBContext.TblGhlCustomFields
        //                .FirstOrDefaultAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.FieldName == field.name);

        //            if (existingField != null)
        //            {
        //                existingField.GhlFieldId = field.id;
        //                existingField.Model = field.model;
        //                existingField.FieldKey = field.fieldKey;
        //                existingField.Placeholder = field.placeholder;
        //                existingField.DataType = field.dataType;
        //                existingField.Position = field.position;
        //                existingField.DocumentType = field.documentType;
        //                existingField.ParentId = field.parentId;
        //                existingField.LocationId = field.locationId;
        //                existingField.DateAdded = field.dateAdded;
        //                existingField.IsStandard = field.standard;
        //                existingField.ModifiedAt = DateTime.Now;
        //                existingField.IsDelete = false;
        //                existingField.IsActive = true;
        //            }
        //            else
        //            {
        //                var newField = new TblGhlCustomField
        //                {
        //                    Id = Guid.NewGuid(),
        //                    GhlFieldId = field.id,
        //                    FieldName = field.name,
        //                    Model = field.model,
        //                    FieldKey = field.fieldKey,
        //                    Placeholder = field.placeholder,
        //                    DataType = field.dataType,
        //                    Position = field.position,
        //                    DocumentType = field.documentType,
        //                    ParentId = field.parentId,
        //                    LocationId = field.locationId,
        //                    DateAdded = field.dateAdded,
        //                    IsStandard = field.standard,
        //                    TenantId = AppConstant.LoggedInTenantId,
        //                    CreatedAt = DateTime.Now,
        //                    IsActive = true,
        //                    IsDelete = false
        //                };

        //                dBContext.TblGhlCustomFields.Add(newField);
        //            }
        //        }

        //        await dBContext.SaveChangesAsync();

        //        responseVm.IsSuccess = true;
        //        responseVm.Message = "Custom fields synced and saved successfully.";
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = $"Exception: {ex.Message}";
        //    }

        //    return responseVm;
        //}


        public async Task<IResponseVm<List<GhlCustomField>>> GetGhlCustomFieldsAsync(GetGHLConfiguration getGHLConfiguration)
        {
            IResponseVm<List<GhlCustomField>> responseVm = new ResponseVm<List<GhlCustomField>>();

            try
            {
                FileLogger.Log("Fetching GHL Custom Fields...");
                string url = $"{AppConstant.GHL_BaseAddress}/locations/{getGHLConfiguration?.LocationId}/customFields?model=all";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getGHLConfiguration?.Access_Token);
                request.Headers.Add("Version", "2021-07-28");
                request.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(request);
                var jsonString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Failed to fetch custom fields: {response.StatusCode}, Body: {jsonString}";
                    return responseVm;
                }

                var fieldResponse = JsonConvert.DeserializeObject<GhlCustomFieldResponse>(jsonString);
                responseVm.IsSuccess = true;
                responseVm.Data = fieldResponse?.customFields ?? new List<GhlCustomField>();
                responseVm.Message = "Custom fields fetched successfully.";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception while fetching custom fields: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception: {ex.Message}";
            }

            return responseVm;
        }










    }
}






