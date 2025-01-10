using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using System.IO;
using System.Reflection;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportandExportController : ControllerBase
    {
      
        private readonly ImportandExportCSV importandExportCSV;

        public ImportandExportController(BtprojecShuttermanagementDevContext context,ImportandExportCSV importandExportCSV)
        {
           
            this.importandExportCSV = importandExportCSV;
        }

        [Authorize]
        [HttpPost("import/{model}")]
        [Authorize(Roles = "ContactAdd,MaterialAdd")]
        public async Task<IResponseVm<List<string>>> ImportCsv(string model, IFormFile file)
        {
            IResponseVm<List<string>> responseVm = new ResponseVm<List<string>>();
            
            if (file == null || file.Length == 0)
            {
                responseVm.Message = "No file uploaded.";
                return responseVm;
            }
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".csv" && fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                responseVm.Message = AppConstant.InvalidExcelFile;
                return responseVm;
            }
            try
            {
                using (var stream = file.OpenReadStream())
                {
                  
                    switch (model.ToLower())
                    {
                        case "material":
                            var materialresponse = await importandExportCSV.ImportCsvToDatabase(stream);
                            responseVm = materialresponse;
                            break;        

                        case "contact":
                            var contactresponse = await importandExportCSV.ImportCsvForContact(stream);
                            responseVm = contactresponse;
                            break;

                        default:
                            responseVm.Message = "Please Select Material and Contact";
                            return responseVm;
                    }
                  
                }
              
               
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        [Authorize]
        [HttpPost("ExportAll")]
        [Authorize(Roles = "ContactView")]

        public async Task<IResponseVm<string>> ExportToCsv([FromBody] Pagination filter, string Type,string EstimationId, string WindowProductId = null)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                switch (Type.ToLower())
                {
                    case "contact":
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Contacts.csv");
                        var result = await importandExportCSV.ExportContactsToCsvAsync(filter, filePath);
                        if(result != null)
                        {
                            return result;
                        }
                        break;
                    case "material":
                        var filePath2 = Path.Combine(Directory.GetCurrentDirectory(), "Materials.csv");
                        var result2 = await importandExportCSV.ExportMaterialsToCsvAsync(filter,filePath2);
                        if(result2 != null)
                        {
                            return result2;
                        }
                        break;
                    case "jobs":
                        var filePath3 = Path.Combine(Directory.GetCurrentDirectory(), "Jobs.csv");
                        var result3 = await importandExportCSV.ExportJobsToCsv(filter,filePath3);
                        if (result3 != null)
                        {
                            return result3;
                        }
                        break;
                    case "breakdown":
                        var filePath4 = Path.Combine(Directory.GetCurrentDirectory(), "Breakdown.csv");
                        var result4 = await importandExportCSV.ExportDynamicBreakdown(filter,EstimationId, WindowProductId, filePath4);
                        if (result4 != null)
                        {
                            return result4;
                        }
                        break;
                    default:
                        responseVm.Message = "Please input valid Type";
                        return responseVm;
                }

            }
            catch (Exception ex)
            {
                return responseVm;
            }
            return responseVm;
        }

    }
}
