using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadDocumentController : ControllerBase
    {
        private readonly IUploadBlobDocument uploadBlobDocument;

        public UploadDocumentController(IUploadBlobDocument uploadBlobDocument)
        {
            this.uploadBlobDocument = uploadBlobDocument;
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAllDocuments(Pagination filter)
        {
            var res = await uploadBlobDocument.GetAllDocument(filter);
            return Ok(res);

        }

        [HttpPost("Create")]
        public async Task<IActionResult> UploadDocument([FromBody] CreateBlobDocument model)
        {

            var res = await uploadBlobDocument.UploadDocument(model);
            return Ok(res);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateDocument([FromBody] UpdateBlobDocument model)
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


    }
}
