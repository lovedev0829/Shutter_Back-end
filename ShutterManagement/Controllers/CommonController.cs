using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.Common;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using Azure;
using Microsoft.AspNetCore.Authorization;
using ShutterManagement.Core;
using ShutterManagement.Business;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel;

namespace ShutterManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly IMasterCreate masterCreate;
        private readonly FileUploadHelper fileUpload;
        private readonly ClaimsProperty claimsProperty;
        private readonly IJobTitleService jobTitleService;
        private readonly IMaterial material;
        public Pagination filter = new Pagination();
        private readonly IVendorService vendorService;
        private readonly IComponentService componentService;
        private readonly IMstProductType mstProductType;
        private readonly IAddress address;
        private readonly EmailHelper emailHelper;
        private readonly IOption option;
        private readonly IProductService productService;
        private readonly IUnitofMeasurementService unitofMeasurementService;

        public CommonController(IMasterCreate masterCreate, FileUploadHelper fileUpload, ClaimsProperty claimsProperty,
            IJobTitleService jobTitleService,IMaterial material,IVendorService vendorService 
            ,EmailHelper emailHelper,
            IComponentService componentService,IMstProductType mstProductType,IAddress address,IOption option,IProductService productService,IUnitofMeasurementService unitofMeasurementService)
        {
            this.masterCreate = masterCreate;
            this.fileUpload = fileUpload;
            this.claimsProperty = claimsProperty;
            this.jobTitleService = jobTitleService;
            this.material = material;
            filter.PageNo = 1;
            filter.RecordPerPage = 100;
            filter.SearchValue = "";
            filter.TenantId = "";
            this.vendorService = vendorService;
            this.componentService = componentService;
            this.address= address;
            this.emailHelper = emailHelper;
            this.mstProductType = mstProductType;
            this.option = option;
            this.productService = productService;
            this.unitofMeasurementService = unitofMeasurementService;
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


        [HttpGet]
        [Route("GetAllJobTitle")]
        public async Task<IActionResult> GetAllJobTitle()
        {
            var res = await jobTitleService.GetAll(filter);
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }
        [HttpGet]
        [Route("GetAllMaterial")]
        public async Task<IActionResult> GetAllMeterial()
        {

            var res = await material.GetAll_Material(filter);
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllVendor")]
        public async Task<IActionResult> GetAllVendor()
        {
            var res = await vendorService.GetAll_Vendor(filter);
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }
        [HttpGet]
        [Route("GetAllComponent")]
        public async Task<IActionResult> GetAllComponent()
        {
            var res = await componentService.GetAll_Component(filter);
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }
        [HttpGet]
        [Route("GetAllAddress")]
        public async Task<IActionResult> GetAllAddress()
        {
            var res = await address.GetAll_Address(filter);
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllProductType")]
        public async Task<IActionResult> GetAllProductType()
        {
            var res = await mstProductType.GetAll_ProductType();
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }


        [HttpPost]
        [Route("SendEmail")]
        public void SendMail(SendEmail sendEmail)
        {
            emailHelper.SendMail(sendEmail);
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
        [HttpGet]
        [Route("GetAllOption")]
        public async Task<IActionResult> GetAllOption()
        {
            var res = await option.GetAll_Option(filter);
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAllProduct()
        {
            var res =await productService.GetAll_Product(filter);
            res.Data=res.Data.Where(x=>x.IsActive == true).ToList();    
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllUnitOfMeasurement")]
        public async Task<IActionResult> GetAllUnitOfMeasurement()
        {
            var res = await unitofMeasurementService.GetAll(filter);
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

    }
}
