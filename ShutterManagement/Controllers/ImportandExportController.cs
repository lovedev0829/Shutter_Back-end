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

        //[Authorize]
        //[HttpGet("export/{model}")]
        //public async Task<IActionResult> ExportCsv(string model)
        //{
        //    try
        //    {
        //        switch (model.ToLower())
        //        {
        //            case "rawmaterial":
        //                var rawMaterials = await importandExportCSV.GetRawMaterialsForExport();  
        //                var rawMaterialCsv = await importandExportCSV.ExportToCsv(rawMaterials);  
        //                return File(rawMaterialCsv, "text/csv", "rawmaterials.csv");

        //            case "contact":
        //                var contacts = await importandExportCSV.GetContactsForExport(); 
        //                var contactCsv = await importandExportCSV.ExportToCsv(contacts);  
        //                return File(contactCsv, "text/csv", "contacts.csv");

        //            default:
        //                return BadRequest("Invalid model name.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error exporting data: {ex.Message}");
        //    }
        //}


    }
}
