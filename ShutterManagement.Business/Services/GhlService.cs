//using ShutterManagement.Core.ViewModels;
//using ShutterManagement.Core;
//using ShutterManagement.DAL.Context;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Threading.Tasks;
//using ShutterManagement.Core.ViewModels.Response;

//namespace GHLConfiguration_Demo
//{
//    public class GhlService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly ShutterDBContext dBContext;
//        private const string BaseUrl = "https://rest.gohighlevel.com/v1/";

//        public GhlService(HttpClient httpClient,ShutterDBContext dBContext)
//        {
//            _httpClient = httpClient;
//            this.dBContext = dBContext;
           
//        }

//        public async Task<IResponseVm<string>> GetAndSaveDefaultPipelineAndStage()
//        {
//            IResponseVm<string> responseVm = new ResponseVm<string>();
//            try
//            {
//                var requestUrl = $"{BaseUrl}pipelines/";
//                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey());
//                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
//                response.EnsureSuccessStatusCode();

//                string jsonResponse = await response.Content.ReadAsStringAsync();
//                var pipelineResponse = JsonSerializer.Deserialize<PipelineResponse>(jsonResponse);

//                if (pipelineResponse?.Pipelines == null || !pipelineResponse.Pipelines.Any())
//                {
//                    responseVm.IsSuccess = false;
//                    responseVm.Message = "No pipelines found.";
//                    return responseVm;
//                }

//                var targetPipeline = pipelineResponse.Pipelines.FirstOrDefault(p => p.Id == "CFoi90n88zrDhMFP8R2I");

//                if (targetPipeline?.Stages == null || !targetPipeline.Stages.Any())
//                {
//                    responseVm.IsSuccess = false;
//                    responseVm.Message = "Target pipeline not found or has no stages.";
//                    return responseVm;
//                }

//                var existingStages = dBContext.TblGhlStages
//                    .Where(x => x.GhlPipelineId == targetPipeline.Id)
//                    .ToList();
//                var existingStagesDict = existingStages.ToDictionary(x => x.GhlStageId, x => x);

//                List<TblGhlStage> stagesToUpdate = new();
//                List<TblGhlStage> stagesToInsert = new();

//                foreach (var stage in targetPipeline.Stages)
//                {
//                    if (existingStagesDict.TryGetValue(stage.Id, out var existingStage))
//                    {
//                        existingStage.GhlStageName = stage.Name;
//                        existingStage.GhlLastSync = DateTime.Now;
//                        existingStage.UpdatedAt = DateTime.Now;
//                        existingStage.ModifiedBy = AppConstant.LoggedInTenantId;

//                        stagesToUpdate.Add(existingStage);
//                    }
//                    else
//                    {
//                        stagesToInsert.Add(new TblGhlStage
//                        {
//                            Id = Guid.NewGuid(),
//                            GhlPipelineId = targetPipeline.Id,
//                            GhlStageId = stage.Id,
//                            GhlStageName = stage.Name,
//                            GhlLastSync = DateTime.Now,
//                            TenantId = AppConstant.LoggedInTenantId,
//                            CreatedBy = AppConstant.LoggedInUserID,
//                            IsActive = true,
//                            ModifiedBy = AppConstant.LoggedInUserID
//                        });
//                    }
//                }
//                if (stagesToUpdate.Any())
//                {
//                    dBContext.TblGhlStages.UpdateRange(stagesToUpdate);
//                }

//                if (stagesToInsert.Any())
//                {
//                    await dBContext.TblGhlStages.AddRangeAsync(stagesToInsert);
//                }
//                await dBContext.SaveChangesAsync();

//                responseVm.IsSuccess = true;
//                responseVm.Message = "All pipeline stages updated or inserted successfully.";
//            }
//            catch (Exception ex)
//            {
//                responseVm.IsSuccess = false;
//                responseVm.Message = $"Error: {ex.Message}";
//            }

//            return responseVm;
//        }


//        public async Task<IResponseVm<List<PipelineAndStage>>> GetGHLStages()
//        {
//            IResponseVm<List<PipelineAndStage>> responseVm = new ResponseVm<List<PipelineAndStage>>();
//            try
//            {
//                var gHLStages = dBContext.TblGhlStages
//                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId)
//                    .ToList();

//                var stageList = new List<PipelineAndStage>();

//                foreach (var stage in gHLStages)
//                {
//                    stageList.Add(new PipelineAndStage
//                    {
//                        GhlPipelineId = stage.GhlPipelineId ?? "",
//                        GhlStageId = stage.GhlStageId ?? "",
//                        GhlStageName = stage.GhlStageName ?? ""
//                    });
//                }

//                responseVm.Data = stageList;
//                responseVm.IsSuccess = true;
//                responseVm.Message = AppConstant.FetchSuccessMsg;
//            }
//            catch (Exception ex)
//            {
//                responseVm.IsSuccess = false;
//                responseVm.Message = $"Error: {ex.Message}";
//            }

//            return responseVm;
//        }


//        private static string GetApiKey()
//        {
//            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2NhdGlvbl9pZCI6IlBUbklDTU4wbUtyMDNrekpRcVBRIiwidmVyc2lvbiI6MSwiaWF0IjoxNzI0MDcwNDM3MTYzLCJzdWIiOiJkOGhlWkl2dEliZDVPcGhGdXNmYiJ9.yRrmWRgUC5VtrhKMsE4JsLjpwROYvvQnwoYu5JbVQWU"; // Replace with a secure method to fetch the API key
//        }
//    }

    

  
//}
