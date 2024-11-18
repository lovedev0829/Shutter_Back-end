using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.Common;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using Azure;
using Microsoft.AspNetCore.Authorization;
using ShutterManagement.Core;

namespace ShutterManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly IMasterCreate masterCreate;
        private readonly FileUploadHelper fileUpload;

        public CommonController(IMasterCreate masterCreate, FileUploadHelper fileUpload)
        {
            this.masterCreate = masterCreate;
            this.fileUpload = fileUpload;
        }


        [HttpPost]
        [Route("FileUpload")]
        public async Task<IActionResult> Upload(List<IFormFile> files, FileType fileType) 
        {
            List<IResponseVm<string>> result = new List<IResponseVm<string>>();
            foreach (var file in files)
            {
                var res = await fileUpload.UploadFile(file, fileType, AppConstant.LoggedInTenantId.ToString() ?? "");
                result.Add(res);
            }
            return Ok(result);
        }

        //[HttpPost]
        //[Route("MasterCreate")]
        //public IActionResult MasterCreate(CreatePayloadVm model)
        //{

        //    CreateServiceVm vm = new CreateServiceVm
        //    {
        //        Data = model.Data,
        //        Type = (MasterItemEnum)model.Type
        //    };
        //    switch ((MasterItemEnum)model.Type)
        //    {
        //        case MasterItemEnum.Contact:
        //            masterCreate.Create(vm);
        //            break;
        //        case MasterItemEnum.Phone:
        //            break;
        //        case MasterItemEnum.Assembly:
        //            break;
        //        case MasterItemEnum.Address:
        //            break;
        //        default:
        //            break;

        //    }
        //    return Ok();

        //}

        //[HttpPost]
        //[Route("MasterUpdate")]
        //public IActionResult MasterUpdate(CreatePayloadVm model)
        //{

        //    CreateServiceVm vm = new CreateServiceVm
        //    {
        //        Data = model.Data,
        //        Type = (MasterItemEnum)model.Type
        //    };
        //    switch ((MasterItemEnum)model.Type)
        //    {
        //        case MasterItemEnum.Contact:
        //            masterCreate.Update(vm);
        //            break;
        //        case MasterItemEnum.Phone:
        //            break;
        //        case MasterItemEnum.Assembly:
        //            break;
        //        case MasterItemEnum.Address:
        //            break;
        //        default:
        //            break;

        //    }
        //    return Ok();

        //}

        //[HttpGet]
        //[Route("MasterGetAll")]
        //public async Task<IActionResult> MasterGetAll([FromQuery] MasterItemEnum type)
        //{

        //    List<Sp_Contact> contacts = null;

        //    switch (type)
        //    {
        //        case MasterItemEnum.Contact:
        //            try
        //            {
        //                var response = await masterCreate.GetAll();
        //                contacts = response.Contacts;
        //            }
        //            catch (Exception ex)
        //            {
        //                return StatusCode(500, $"Internal server error: {ex.Message}");
        //            }
        //            break;

        //        case MasterItemEnum.Phone:
        //            break;

        //        case MasterItemEnum.Assembly:
        //            break;

        //        case MasterItemEnum.Address:
        //            break;

        //        default:
        //            return BadRequest("Invalid type specified.");
        //    }

        //    return Ok(contacts);
        //}

        //[HttpGet]
        //[Route("MasterGetById")]
        //public async Task<IActionResult> MasterGetById([FromQuery] Guid id, [FromQuery] MasterItemEnum type)
        //{
        //    if (id == Guid.Empty)
        //    {
        //        return BadRequest("Invalid ID specified.");
        //    }

        //    Sp_Contact contact = null;

        //    try
        //    {
        //        switch (type)
        //        {
        //            case MasterItemEnum.Contact:
        //                contact = await masterCreate.GetById(id);

        //                if (contact == null)
        //                {
        //                    return NotFound($"Contact with ID {id} not found.");
        //                }
        //                break;

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }

        //    return Ok(contact);
        //}


        //[HttpGet]
        //[Route("MasterDeleteById")]
        //public async Task<IActionResult> MasterDeleteById([FromQuery] Guid id, [FromQuery] MasterItemEnum type)
        //{
        //    if (id == Guid.Empty)
        //    {
        //        return BadRequest("Invalid ID specified.");
        //    }
        //    switch (type)
        //    {
        //        case MasterItemEnum.Contact:
        //            await masterCreate.DeleteContact(id);
        //            break;

        //        default:
        //            return BadRequest("Invalid item type specified.");
        //    }

        //    return Ok($"Contact with ID {id} successfully deleted.");

        //}


    }
}
