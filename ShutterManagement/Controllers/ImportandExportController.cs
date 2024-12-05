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

        public ImportandExportController(ShutterDbContext context,ImportandExportCSV importandExportCSV)
        {
           
            this.importandExportCSV = importandExportCSV;
        }

        [Authorize]
        [HttpPost("import/{model}")]
        public async Task<IResponseVm<string>> ImportCsv(string model, IFormFile file)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            
            if (file == null || file.Length == 0)
            {
                responseVm.Message = "No file uploaded.";
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
                            responseVm.IsSuccess = materialresponse.IsSuccess;
                            responseVm.Message = materialresponse.Message;
                            break;

                        case "contact":
                            var contactresponse = await importandExportCSV.ImportCsvForContact(stream);
                            responseVm.IsSuccess = contactresponse.IsSuccess;
                            responseVm.Message = contactresponse.Message;
                            break;

                        default:
                            responseVm.Message = "Not Imported";
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
        [HttpGet("export")]
        public async Task<IActionResult> ExportVendorsToCsvAsync()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "vendors.csv");
                await importandExportCSV.ExportContactsToCsvAsync(filePath);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                responseVm.IsSuccess=true;
                return File(fileBytes, "text/csv", "vendors.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
