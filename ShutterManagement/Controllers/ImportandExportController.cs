using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;

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
        public async Task<IActionResult> ExportContactsToCsvAsync()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Contacts.csv");
                var result = await importandExportCSV.ExportContactsToCsvAsync(filePath);
                if (result.IsSuccess)
                {
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    return File(fileBytes, "text/csv", "Contacts.csv");

                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [Authorize]
        [HttpGet("ExportMaterials")]
        [Authorize(Roles = "MaterialView")]
        public async Task<IActionResult> ExportmaterialsToCsvAsync()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Materials.csv");
                var result = await importandExportCSV.ExportMaterialsToCsvAsync(filePath);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                responseVm.IsSuccess = true;
                return File(fileBytes, "text/csv", "Materials.csv");


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [Authorize]
        [HttpGet("ExportJobs")]
        public async Task<IActionResult> ExportJobsToCsvc()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Jobs.csv");
                var result = await importandExportCSV.ExportJobsToCsv(filePath);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                responseVm.IsSuccess = true;
                return File(fileBytes, "text/csv", "Jobs.csv");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
