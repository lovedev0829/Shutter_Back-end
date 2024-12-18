using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using System.IO;

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
        [HttpGet("ExportContacts")]
        [Authorize(Roles = "ContactView")]
        public async Task<IActionResult> ExportToCsv(string Type)
        {
            try
            {
                switch (Type.ToLower())
                {
                    case "contact":
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Contacts.csv");
                        var result = await importandExportCSV.ExportContactsToCsvAsync(filePath);
                        if (result.IsSuccess)
                        {
                            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                            return File(fileBytes, "text/csv", "Contacts.csv");

                        }
                        break;
                    case "material":
                        var filePath2 = Path.Combine(Directory.GetCurrentDirectory(), "Materials.csv");
                        var result2 = await importandExportCSV.ExportMaterialsToCsvAsync(filePath2);
                        if (result2.IsSuccess)
                        {
                            var fileBytes2 = await System.IO.File.ReadAllBytesAsync(filePath2);
                            return File(fileBytes2, "text/csv", "Materials.csv");
                        }
                        break;
                    case "jobs":
                        var filePath3 = Path.Combine(Directory.GetCurrentDirectory(), "Jobs.csv");
                        var result3 = await importandExportCSV.ExportJobsToCsv(filePath3);
                        var fileBytes3 = await System.IO.File.ReadAllBytesAsync(filePath3);
                        return File(fileBytes3, "text/csv", "Jobs.csv");

                    default:
                        return StatusCode(200, "Please Select valid Type");
                }
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            return Ok();
        }
    }
}
