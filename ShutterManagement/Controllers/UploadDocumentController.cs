using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadDocumentController : ControllerBase
    {
        private readonly IUploadBlobDocument uploadBlobDocument;
        private readonly BlobStorageService blobStorageService;

        public UploadDocumentController(IUploadBlobDocument uploadBlobDocument ,BlobStorageService blobStorageService)
        {
            this.uploadBlobDocument = uploadBlobDocument;
            this.blobStorageService = blobStorageService;
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAllDocuments(Pagination filter)
        {
            var res = await uploadBlobDocument.GetAllDocument(filter);
            return Ok(res);

        }

        [HttpPost("Create")]
        public async Task<IActionResult> UploadDocument(CreateBlobDocument model)
        {
            var res = await uploadBlobDocument.UploadDocument(model);
            return Ok(res);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateDocument(UpdateBlobDocument model)
        {

            var res = await uploadBlobDocument.UpdateDocument(model);
            return Ok(res);
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteDocument(Guid? DocumentId)
        {
            var res = await uploadBlobDocument.DeleteDocument(DocumentId);
            return Ok(res);
        }


        [HttpPost("DownloadBlobAsBase64")]
        public async Task<IActionResult> DownloadBlobAsBase64(string blobUrl)
        {
            var result = await blobStorageService.DownloadBlobAsBase64AndSaveAsync(blobUrl);
            return Ok(result);
        }



    }
}
