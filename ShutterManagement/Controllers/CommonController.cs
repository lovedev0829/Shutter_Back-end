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
using ShutterManagement.DAL.Context;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.ViewModels.Response;
using System.Text.Json.Serialization;
using System.Text.Json;
using ShutterManagement.GHL.Services;
using ShutterManagment.GHL.ViewModel.Response;

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
        private readonly IUserService userService;
        private readonly IJobStatusService jobStatusService;
        private readonly IJobTypeService jobTypeService;
        private readonly IDiscountService discountService;
        private readonly IComponentPriceService componentPriceService;
        private readonly IOption option;
        private readonly IProductService productService;
        private readonly IUnitofMeasurementService unitofMeasurementService;
        private readonly IRoleService roleService;
        private readonly IRegion region;
        private readonly IContactService contactService;
        private readonly IMstContactType contactType;
        private readonly IJobService jobService;
        private readonly IExpressionService expression;
        private readonly IInvoiceService invoiceService;
        private readonly ITaxRate taxService;
        private readonly IEstimateService estimateService;
        private readonly IEstimationBreakdownService estimationBreakdownService;
        private readonly ILocationService locationService;
        private readonly IDashboard dashboard;
        private readonly INotificationService notification;
        private readonly IGHLConfiguration gHLConfiguration;
        private readonly IStripeConfiguration stripeConfiguration;
        private readonly GhlService ghlService;

        public CommonController(IMasterCreate masterCreate, 
            FileUploadHelper fileUpload, ClaimsProperty claimsProperty,
            IJobTitleService jobTitleService, IMaterial material, IVendorService vendorService
            , IUserService userService, IJobStatusService jobStatusService,
            IJobTypeService jobTypeService, IDiscountService discountService,
            IComponentPriceService componentPriceService,
            IComponentService componentService, IMstProductType mstProductType,
            IAddress address, IOption option, IProductService productService,
            IUnitofMeasurementService unitofMeasurementService, IRoleService roleService, IRegion region, IContactService contactService,
            IMstContactType contactType, IJobService jobService,IExpressionService expression ,IInvoiceService invoiceService,
            ITaxRate taxService,IEstimateService estimateService,
            IEstimationBreakdownService estimationBreakdownService, ILocationService locationService,
           IDashboard dashboard, INotificationService notification, IGHLConfiguration gHLConfiguration,IStripeConfiguration stripeConfiguration, GhlService ghlService)
        {
            this.masterCreate = masterCreate;
            this.fileUpload = fileUpload;
            this.claimsProperty = claimsProperty;
            this.jobTitleService = jobTitleService;
            this.material = material;
            filter.PageNo = 1;
            filter.RecordPerPage = 1000;
            filter.SearchValue = "";
            filter.TenantId = "";
            filter.Status = 1;
            filter.Vendor = "";
            filter.JobStatus = "";
            filter.ProductType = "";
            filter.Region = "";
            filter.Address = "";
            this.vendorService = vendorService;
            this.componentService = componentService;
            this.address = address;
            this.userService = userService;
            this.jobStatusService = jobStatusService;
            this.jobTypeService = jobTypeService;
            this.discountService = discountService;
            this.componentPriceService = componentPriceService;
            this.mstProductType = mstProductType;
            this.option = option;
            this.productService = productService;
            this.unitofMeasurementService = unitofMeasurementService;
            this.roleService = roleService;
            this.region = region;
            this.contactService = contactService;
            this.contactType = contactType;
            this.jobService = jobService;
            this.expression = expression;
            this.invoiceService = invoiceService;
            this.taxService = taxService;
            this.estimateService = estimateService;
            this.estimationBreakdownService = estimationBreakdownService;
            this.locationService = locationService;
            this.dashboard = dashboard;
            this.notification = notification;
            this.gHLConfiguration = gHLConfiguration;
            this.stripeConfiguration = stripeConfiguration;
            this.ghlService = ghlService;
        }


        [HttpPost]
        [Route("FileUpload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            IResponseVm<List<string>> result = new ResponseVm<List<string>>();
            List<string> uploads = new List<string>();
            foreach (var file in files)
            {
                var res = await fileUpload.UploadFile(file, AppConstant.LoggedInTenantId.ToString() ?? "");
                if (res.IsSuccess)
                {
                    result.IsSuccess = res.IsSuccess;
                    result.Message = res.Message;
                    uploads.Add(res.Data);
                }
            }
            result.Data = uploads;
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllJobTitle")]
        public async Task<IActionResult> GetAllJobTitle()
        {
            var res = await jobTitleService.GetAll(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllDiscountCoupon")]
        public async Task<IActionResult> GetAllDiscountCoupon(Guid ContactId)
        {
            filter.SearchValue = ContactId.ToString();
            var res = await discountService.GetAll_Coupons(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllAssignedPerson")]
        public async Task<IActionResult> GetAllAssignedPerson()
        {
            var res = await userService.GetAll(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }
        [HttpGet]
        [Route("GetAllJobStatus")]
        public async Task<IActionResult> GetAllJobStatus()
        {
            var res = await jobStatusService.GetAll(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllJobType")]
        public async Task<IActionResult> GetAllJobType()
        {
            var res = await jobTypeService.GetAllJobType(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }
        [HttpGet]
        [Route("GetAllMaterial")]
        public async Task<IActionResult> GetAllMeterial(string ProductType)
        {
            filter.SearchValue = ProductType;
            var res = await material.GetAll_Material(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllVendor")]
        public async Task<IActionResult> GetAllVendor()
        {
            var res = await vendorService.GetAll_Vendor(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllComponent")]
        public async Task<IActionResult> GetAllComponent(string ProductType)
        {
            filter.SearchValue = ProductType;
            var res = await componentService.GetAll_Component(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }
        [HttpGet]
        [Route("GetAllAddress")]
        public async Task<IActionResult> GetAllAddress(string ContactId)
        {
            filter.SearchValue = ContactId;
            var res = await address.GetAll_Address(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllProductType")]
        public async Task<IActionResult> GetAllProductType()
        {
            var res = await mstProductType.GetAll_ProductType(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllRegion")]
        public async Task<IActionResult> GetAllRegion()
        {
            var res = await region.GetAll(filter);
            res.Data = res.Data.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }


        
        //public IActionResult Dashboard()
        //{
        //    var res = new ResponseVm<string>();
        //    res.IsSuccess = true;
        //    return Ok(res);
        //}
        [HttpGet]
        [Route("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var result = await dashboard.GetallDashBoardData();

            return Ok(result);
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[Route("SendEmail")]
        //public void SendMail(SendEmail sendEmail)
        //{
        //    emailHelper.SendMail(sendEmail);
        //}

        [HttpPost]
        [Route("GetAllJobs")]
        public async Task<IActionResult> GetAll(Pagination filter1)
        {
            var result = await jobService.GetAllJobsTenant_SubTenant(filter1);
            return Ok(result);
        }


        [HttpGet]
        [Route("GetAllContacts")]
        public async Task<IActionResult> GetAllContact()
        {
            var res = await contactService.GetAll_Contact(filter,false);
            res.Data = res.Data?.Where(x => x.IsActive == true && x.IsArchive==false).ToList();
            return Ok(res);
        }


        [HttpGet]
        [Route("GetAllExpressionChip")]
        public async Task<IActionResult> GetAllExpressionChip(string? Product, string? ProductType, string Option = "")
        {
            var res = await componentPriceService.GetAllComponentsChip(Product,ProductType, Option);
            return Ok(res);
        }


        [HttpGet]
        [Route("ContactBreakdown")]
        public async Task<IActionResult> ContactBreakdown()
        {
            var res = await estimationBreakdownService.GetDistinctActiveContacts(filter);
            return Ok(res);
            
        }
        [HttpGet]
        [Route("ContactUniqueIdBreakdown")]
        public async Task<IActionResult> ContactUniqueIdBreakdown(string ContactName)
        {
            filter.SearchValue = ContactName.ToString().ToLower();
            var res = await estimationBreakdownService.GetContactUniqueId(filter);
            return Ok(res);
        }

        [HttpGet]
        [Route("ContactJobBreakdown")]
        public async Task<IActionResult> ContactJobBreakdown(Guid ContactId)
        {
            filter.SearchValue = ContactId.ToString().ToLower();
            var res = await estimationBreakdownService.GetContactJobsBreakdown(filter);
            return Ok(res);
        }

        [HttpGet]
        [Route("ContactEstimatioBreakdown")]
        public async Task<IActionResult> ContactEstimatioBreakdown(Guid JobId)
        {
            filter.SearchValue = JobId.ToString().ToLower();
            var res = await estimationBreakdownService.GetContactEstimationsBreakdown(filter);
            return Ok(res);
        }

        [HttpGet]
        [Route("ContactEstimationProductBreakdown")]
        public async Task<IActionResult> ContactEstimationProductBreakdown(Guid EstimationId, string WindowProdcutId = "")
        {
            var res = await estimationBreakdownService.GetProductBreakdown(EstimationId, WindowProdcutId);
            return Ok(res);
          
        }

        [HttpGet]
        [Route("GetProductBreakdown")]
        public async Task<IActionResult> GetProductBreakdown(Guid EstimationId)
        {
            var productList = await estimationBreakdownService.GetProductListForEstimation(EstimationId);
            return Ok(productList);

        }

        [HttpGet]
        [Route("EstimationUniquename")]
        public async Task<IActionResult> EstimationCode()
        {
            var EstimationCode = await estimateService.EstimationUniqueName();
            return Ok(EstimationCode);

        }
    

        //[HttpGet]
        //[Route("ContactEstimationProductCompoentBreakdown")]
        //public async Task<IActionResult> ContactEstimationProductCompoentBreakdown(string EstimationId, Guid ProductId)
        //{
        //    filter.SearchValue = EstimationId.ToLower();
        //    var res = await estimateService.GetAll_Estimate(filter);
        //    var windowProductIds = res.Data.FirstOrDefault()?.WindowProductId;

        //    var estimateResponse = await estimateService.BuildEstimate(windowProductIds, true);
        //    var filteredData = estimateResponse.Data.Where(item => item.ProductId == ProductId).ToList();
        //        return Ok(filteredData);
        //}









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
        public async Task<IActionResult> GetAllOption(string ProductTypeId)
        {
            var res = await option.GetAll_Option(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true && x.ProductTypeId == Guid.Parse(ProductTypeId)).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllProduct")]
        public async Task<IActionResult> GetAllProduct(string? ProductTypeId)
        {
           var res = await productService.GetAll_Product(filter);
           res.Data = res.Data?.Where(x => x.IsActive == true && x.ProductType == ProductTypeId).ToList();
           return Ok(res);
        }

        [HttpGet]
        [Route("GetAllUnitOfMeasurement")]
        public async Task<IActionResult> GetAllUnitOfMeasurement()
        {
            var res = await unitofMeasurementService.GetAll(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var res = await roleService.GetAll(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllExpression")]
        public async Task<IActionResult> GetAllExpression(Guid ProductId)
        {

            filter.SearchValue = ProductId.ToString();
            var res = await expression.Get_AllExpression(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true).ToList();
            return Ok(res);
        }

        [HttpPost]
        [Route("SettingById")]
        [Authorize]
        public async Task<IActionResult> SettingById()
        {
            var settingGetById = await masterCreate.SettingGetById();
            return Ok(settingGetById);
        }
        [HttpPost]
        [Authorize]
        [Route("UpdateSettingById")]
        public async Task<IActionResult> UpdateSettingById(SettingUpdate model)
        {
            var settingGetById = await masterCreate.UpdateSetting(model);
            return Ok(settingGetById);
        }
        [HttpPost]
        [Authorize]
        [Route("GetAllMstContactType")]
        public async Task<IActionResult> GetAllMstContactType()
        {
            var mstcontactType = await contactType.GetAll_MstContactType(filter);
            return Ok(mstcontactType);
        }


        [HttpGet]
        [Route("GetAllDiscount")]
        public async Task<IActionResult> GetAllDiscount(Guid? CustomerId)
        {
            filter.CustomerId = CustomerId.ToString();
            var res = await discountService.GetAll_Coupons(filter);
            DateTime current = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var d = res.Data
        .Where(x => x.IsActive == true && x.StartDate <= current && x.ExpiryDate >= current)
        .ToList();
            res.Data = d;
            return Ok(res);
        }


        [HttpGet]
        [Authorize]
        [Route("GetAllInvoice")]
        public async Task<IActionResult> GetAllInvoice(Guid? InvoiceId)
        {
            var res = await invoiceService.GetAllInvoice(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true && x.InvoiceId == InvoiceId).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllJobs")]
        public async Task<IActionResult> GetAllJobs()
        {
            var res = await jobService.GetAllJobsTenant_SubTenant(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true && x.TenantId==AppConstant.LoggedInTenantId).ToList();
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllmaterialList")]
        public async Task<IActionResult> GetMaterial(Guid? VendorId)
        {
            var res = await material.GetAll_Material(filter);
            res.Data = res.Data?.Where(x => x.IsActive == true && x.VendorId==VendorId).ToList();
            return Ok(res);
        }


        [HttpGet]
        [Route("GetAllTax")]
        public async Task<IActionResult> GetAllTax()
        {
            var res = await taxService.GetAll_Tax(filter);
            DateTime current = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var d = res.Data
        .Where(x => x.IsActive == true && x.EffectiveDate <= current && x.ExpiryDate >= current)
        .ToList();
            res.Data = d;
            return Ok(res);
        }

        [HttpGet]
        [Route("GetAllEstimate")]
        public async Task<IActionResult> GetAllEstimate(string? JobId )
        {
            filter.Status = 1;
            filter.JobId=JobId?.ToLower();
            var res = await estimateService.GetAll_Estimate(filter);
            return Ok(res);
        }

        [HttpPost]
        [Route("CreateLocation")]
        public async Task<IActionResult> CreateLocation(CreateLocation mode)
        {
            var res = await locationService.Create_Location(mode);
            return Ok(res);

        }

        [HttpGet]
        [Route("GeltallLocation")]
        public async Task<IActionResult> GetallLocation()
        {
            var res = await locationService.GetAll_Location();
            return Ok(res);

        }


        [HttpGet]
        [Route("GetAllEstimateContact")]
        public async Task<IActionResult> GetAllEstimateContact(string? ContactId)
        {
            filter.Status = 1;
            filter.CustomerId = ContactId?.ToLower();
            var res = await estimateService.GetAll_Estimate(filter);
            res.Data = res.Data?.Where(x => x.jobDetails?.ContactId?.ToString().ToLower() == ContactId.ToLower()).ToList();
            return Ok(res);
        }
        [HttpGet]
        [Route("GetAllJobDetails")]
        public async Task<IActionResult> GetAllJobDetails()
        {
            filter.Status = 1;
            filter.TenantId = AppConstant.LoggedInTenantId.ToString();
            var res = await jobService.GetAllJobsTenant_SubTenant(filter);
            return Ok(res);
        }

        [HttpPost]
        [Route("UpdateNotification")]
        public async Task<IActionResult> UpdateNotification(Guid Id)
        {
            var result = await notification.UpdateNotification(Id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllNotification")]
        public async Task<IActionResult> GetAllNotication()
        {
            var result = await notification.GetAllNotifications();
            return Ok(result);
        }


        [HttpGet]
        [Route("GetAllGHLContact")]
        public async Task<IActionResult> GetAllGHLContact()
        {
            var result = await ghlService.GetContactsAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("SaveStageGHLToDB")]
        public async Task<IActionResult> GetPipeline()
        {
            var pipelineData = await ghlService.GetAndSaveDefaultPipelineAndStage();

            if (pipelineData == null)
            {
                return NotFound("Pipeline not found");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(pipelineData, options);
            return Ok(json);
        }

        [HttpGet]
        [Route("GetAllStages")]
        public async Task<IActionResult> GetStages()
        {
            var result = await ghlService.GetGHLStages();
            return Ok(result);
        }


        [HttpGet]
        [Route("GetOpportunity")]
        public async Task<IActionResult> GetOpportunity()
        {
            var result = await ghlService.GetOpportunitiesFromGhl();
            return Ok(result);
        }

        [HttpPost("HandleOpportunityWebhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleOpportunityWebhook([FromBody] GhlOpportunityWebhookModel webhookData)
        {
            var response = await ghlService.ProcessGhlOpportunityWebhook(webhookData);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }



    }
}
