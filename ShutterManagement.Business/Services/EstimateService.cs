using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.Common;
using static Azure.Core.HttpHeader;
using Azure;
using System.Data.Entity;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using NCalc;
using Antlr.Runtime;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Stripe;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using System.ComponentModel;

namespace ShutterManagement.Business.Services
{
    public class EstimateService : BaseRepository<ShutterDBContext, TblEstimate>, IEstimateService
    {
        private readonly ShutterDBContext _context;
        private readonly IComponentPriceService componentPriceService;
        private readonly IExpressionService expressionService;
        private readonly IProductComponentService productComponentService;
        private readonly IComponentService componentService;
        private readonly IJobService jobService;
        private readonly IContactService contactService;
        private readonly IComponentMaterial componentMaterialService;
        private readonly IMaterial materialService;
        private readonly IWindowService windowService;
        private readonly IWindowProductService windowProductService;
        private readonly IWindowOptionService windowOptionService;
        private readonly IOptionsTypeService optionsTypeService;
        private readonly IEstimateWindowService estimateWindowService;
        private readonly IEstimateWindowProductService estimateWindowProductService;
        private readonly IEstimateWindowProductComponentService estimateWindowProductComponentService;
        private readonly IEstimateWindowProductComponentMaterialService estimateWindowProductComponentMaterialService;
        private readonly IProductService productService;
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IMstProductType productType;
        private string Message = "";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOption optionService;
        private readonly ITaxRate taxRate;
        private readonly IRegion region;
        private readonly ITaxExemption taxExemption;
        private readonly IDiscountService discountservice;
        private readonly ISubtenantProductMarkup subtenantProductMarkup;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly INotificationService notification;
        private readonly IMstProductType mstProductType;
        private readonly JobStatusAndStageMapping statusAndStageMapping;
        private readonly IGHLConfiguration gHLConfiguration;
        //private readonly IManagePartnersProducts managePartnersProducts;
        List<Guid> expressionList = new List<Guid>();

        public EstimateService(ShutterDBContext context,
            IComponentPriceService componentPriceService,
            IExpressionService expressionService,
            IProductComponentService productComponentService,
            IComponentService componentService,
            IJobService jobService,
            IContactService contactService,
            IComponentMaterial componentMaterialService,
            IMaterial materialService,
            IWindowService windowService,
            IWindowProductService windowProductService,
            IWindowOptionService windowOptionService,
            IOptionsTypeService optionsTypeService,
            IEstimateWindowService estimateWindowService,
            IEstimateWindowProductService estimateWindowProductService,
            IEstimateWindowProductComponentService estimateWindowProductComponentService,
            IEstimateWindowProductComponentMaterialService estimateWindowProductComponentMaterialService,
            IProductService productService,
            ITenantService tenantService, ClaimsProperty claimsProperty,
            IMstProductType productType,
            IHttpContextAccessor _httpContextAccessor,
            IOption optionService,
            ITaxRate taxRate,
            IRegion region, ITaxExemption taxExemption, IDiscountService discountservice,
            ISubtenantProductMarkup subtenantProductMarkup, ITenantBrandingService tenantBrandingService,
            INotificationService notification
            , IMstProductType mstProductType,
            JobStatusAndStageMapping statusAndStageMapping, IGHLConfiguration gHLConfiguration
            //IManagePartnersProducts managePartnersProducts
            )
        {
            _context = context;
            this.componentPriceService = componentPriceService;
            this.expressionService = expressionService;
            this.productComponentService = productComponentService;
            this.componentService = componentService;
            this.jobService = jobService;
            this.contactService = contactService;
            this.componentMaterialService = componentMaterialService;
            this.materialService = materialService;
            this.windowService = windowService;
            this.windowProductService = windowProductService;
            this.windowOptionService = windowOptionService;
            this.optionsTypeService = optionsTypeService;
            this.estimateWindowService = estimateWindowService;
            this.estimateWindowProductService = estimateWindowProductService;
            this.estimateWindowProductComponentService = estimateWindowProductComponentService;
            this.estimateWindowProductComponentMaterialService = estimateWindowProductComponentMaterialService;
            this.productService = productService;
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
            this.productType = productType;
            this._httpContextAccessor = _httpContextAccessor;
            this.optionService = optionService;
            this.taxRate = taxRate;
            this.region = region;
            this.taxExemption = taxExemption;
            this.discountservice = discountservice;
            this.subtenantProductMarkup = subtenantProductMarkup;
            this.tenantBrandingService = tenantBrandingService;
            this.notification = notification;
            this.mstProductType = mstProductType;
            this.statusAndStageMapping = statusAndStageMapping;
            this.gHLConfiguration = gHLConfiguration;
            // this.managePartnersProducts = managePartnersProducts;
        }

        public async Task<IResponseVm<string>> ActiveInActiveEstimate(Guid EstimateId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                // Log the start of the operation
                FileLogger.Log($"Start: Toggling Active/Inactive status for Estimate with ID: {EstimateId}");

                var estimate = GetAllQuerable()
                    .Where(c => c.EstimateId == EstimateId
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false)
                    .FirstOrDefault();

                if (estimate == null)
                {
                    // Log failure when estimate is not found
                    FileLogger.Log($"Estimate with ID {EstimateId} not found.");
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }

                // Toggle the active status of the estimate
                estimate.IsActive = !estimate.IsActive;
                estimate.UpdatedAt = DateTime.Now;
                estimate.ModifiedBy = AppConstant.LoggedInUserID;

                // Log the status change
                FileLogger.Log($"Estimate with ID {EstimateId} status changed to: {(estimate.IsActive ?? false ? "Active" : "Inactive")}.");

                await Update(estimate);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

                // Log the successful completion
                FileLogger.Log($"Estimate with ID {EstimateId} status update completed successfully.");
            }
            catch (Exception ex)
            {
                // Log exception if an error occurs
                FileLogger.Log($"Error occurred while updating Estimate with ID {EstimateId}: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteEstimate(Guid EstimateId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                // Log the start of the operation
                FileLogger.Log($"Start: Deleting Estimate with ID: {EstimateId}");

                var estimate = GetAllQuerable()
                    .Where(c => c.EstimateId == EstimateId
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false)
                    .FirstOrDefault();

                if (estimate == null)
                {
                    // Log failure when estimate is not found
                    FileLogger.Log($"Estimate with ID {EstimateId} not found.");
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }

                // Mark the estimate as deleted
                estimate.IsActive = false;
                estimate.IsDelete = true;
                estimate.UpdatedAt = DateTime.Now;
                estimate.ModifiedBy = AppConstant.LoggedInUserID;

                // Log the deletion action
                FileLogger.Log($"Estimate with ID {EstimateId} marked as deleted.");

                await Update(estimate);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

                // Log the successful completion
                FileLogger.Log($"Estimate with ID {EstimateId} deletion completed successfully.");
            }
            catch (Exception ex)
            {
                // Log exception if an error occurs
                FileLogger.Log($"Error occurred while deleting Estimate with ID {EstimateId}: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<EstimateList>>> GetAll_Estimate(Pagination filter)
        {
            IResponseVm<List<EstimateList>> responseVm = new ResponseVm<List<EstimateList>>();
            try
            {
                FileLogger.Log($"Start: Fetching estimates with filter parameters: TenantId = {filter.TenantId}, PageNo = {filter.PageNo}, RecordsPerPage = {filter.RecordPerPage}");

                // Validate Tenant if provided, else use the logged-in tenant
                var estimates = await FetchEstimates(filter);
                if (estimates == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                // Log the number of estimates fetched
                FileLogger.Log($"Fetched {estimates.Count} estimates.");

                // Get product types and filter records based on search and status
                var filteredEstimates = await ApplyFilters(estimates, filter);

                // Paginate the filtered results
                var paginatedEstimates = Paginate.Pagination(filteredEstimates, filter);

                // Prepare final response
                responseVm.IsSuccess = true;
                if (paginatedEstimates.Any())
                {
                    responseVm.Data = paginatedEstimates;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = filteredEstimates.Count;
                    FileLogger.Log($"Successfully fetched {paginatedEstimates.Count} estimates.");
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    FileLogger.Log("No data found after applying filters.");
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred while fetching estimates: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> SendForApproval(Guid EstimateId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsEstimateExists = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.EstimateId == EstimateId).
                    OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                if (IsEstimateExists == null)
                {
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }
                IsEstimateExists.Status = EnumValue.GetEnumValue(Estimation.PendingForApproval);
                IsEstimateExists.UpdatedAt = DateTime.Now;
                IsEstimateExists.ModifiedBy = AppConstant.LoggedInUserID;
                await Update(IsEstimateExists);

                var jobData = jobService.GetAllQuerable().Where(x => x.JobId == IsEstimateExists.JobId && x.IsDelete == false).FirstOrDefault();
                if (jobData != null)
                {
                    var contactData = contactService.GetAllQuerable().Where(x => x.ContactId == jobData.ContactId && x.IsDelete == false).FirstOrDefault();
                    if (contactData != null)
                    {
                        var contactEmail = contactData.Email;
                        SendEmail sendEmail = new SendEmail
                        {
                            ToEmail = contactEmail,
                        };
                        EmailHelper.SendMail(sendEmail);

                    }
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Estimate(CreateEstimate model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log("Start: Create Estimate...");

            try
            {
                // Validate tenant existence
                var isTenantExists = await ValidateTenantExists();
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm; // Return early if tenant is not found
                }

                // Validate if the estimate already exists
                var isEstimateExists = await ValidateEstimateExists(model.EstimateName.Trim(), (Guid)model.JobId, Guid.Empty);
                if (isEstimateExists)
                {
                    responseVm.Message = "Estimates Name " + AppConstant.AlreadyExists;
                    return responseVm; // Return early if estimate exists
                }

                if (model.ProductsList == null || model.ProductsList.Count == 0)
                {
                    responseVm.Message = "Atleast one window is required.";
                    return responseVm; // Return early if window not exists
                }

                // Create and add the estimate
                EstimateComponent createEstimate = await CreateEstimateRecord(model);
                if (createEstimate.EstimateId != Guid.Empty)
                {
                    // Process window products and components
                    await ProcessWindowProducts(model.ProductsList.ToList(), createEstimate.WindowProdcutDetailsForEstimate, (Guid)createEstimate.EstimateId);
                    if (!string.IsNullOrEmpty(Message))
                    {
                        responseVm.Message = Message;
                        return responseVm;
                    }
                    //var TotalPrice = await CalculateEstimate(createEstimate.EstimateId);
                    //createEstimate.TotalPrice = TotalPrice;
                    //await Update(createEstimate);

                    CreateNotification notification1 = new CreateNotification()
                    {
                        UserId = AppConstant.LoggedInUserID,
                        Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Estimate),
                        Body = "Estimate " + AppConstant.Create_Notification,
                        CreatedDate = DateTime.Now,
                        Type = NotificationModule.Estimate.ToString(),
                        TypeId = createEstimate.EstimateId

                    };

                    var result = await notification.CreateNotification(notification1);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                    FileLogger.Log("Estimate created and all related data added successfully.");
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            // Always return the response after completing the operation
            return responseVm;
        }
        public async Task<IResponseVm<string>> ValidateExpression(ValidateExpression model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                string formula = model.Expression ?? "";
                var validate = CalculateExpression(formula, model);
                if (validate.HasErrors())
                {
                    responseVm.Message = "There is some issue in Expression.";
                    return responseVm;
                }
                var res = validate.Evaluate();
                responseVm.IsSuccess = true;
                responseVm.Message = "Expression is correct. You can add this expression now.";
                responseVm.Data = Convert.ToDecimal(res).ToString("0.000") ?? "";

            }
            catch (EvaluationException ex)
            {
                responseVm.Message = "Expression error: " + ex.Message;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> BuildEstimate(Guid[] windowProductIds, bool IsBreakdown, string EstimateId = "", bool IsView = false)
        {
            IResponseVm<List<WindowProdcutDetailsForEstimate>> responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();
            FileLogger.Log("Start: Calculate Estimate price...");

            try
            {
                var WindowProductData = new List<WindowProdcutDetailsForEstimate>();

                bool isContactIsTaxDocument = false;
                // Validate tenant existence
                if (IsView)
                {
                    var data1 = new List<WindowProdcutDetailsForEstimate>();
                    foreach (var windowProductId in windowProductIds)
                    {
                        var productData = await componentPriceService.GetProductDataForView(windowProductId, EstimateId);
                        data1.AddRange(productData.Data.OrderBy(x=>x.ComponentName));
                    }

                    // Set tax for each item in data1
                    //foreach (var item in data1)
                    //{
                    //    item.IsTaxApplied = await IsTaxApplied((Guid)item.JobId);
                    //}
                    responseVm.IsSuccess = true;
                    responseVm.Data = data1;
                    responseVm.TotalRecords = data1.Count;
                    return responseVm;
                }

                else
                {
                    var isTenantExists = await ValidateTenantExists();
                    if (!isTenantExists)
                    {
                        responseVm.Message = AppConstant.TenantNotFound;
                        return responseVm;
                    }
                    if (windowProductIds == null || windowProductIds.Length == 0)
                    {
                        responseVm.Message = "Incorrect data.";
                        return responseVm;
                    }
                    foreach (var windowProductId in windowProductIds)
                    {
                        

                        var data = await componentPriceService.GetProductData(windowProductId);
                      
                        if (data.IsSuccess)
                        {
                            if (data.IsSuccess && data.Data != null)
                            {
                                
                                var itemsWithoutPricingExpression = data.Data
                                    .Where(x => x.ComponentId == null)
                                    .ToList();

                                if (itemsWithoutPricingExpression.Any())
                                {
                                    //var additionalCost = itemsWithoutPricingExpression
                                    //    .SelectMany(x => x.OptionsPrice ?? Array.Empty<decimal>())
                                    //    .Sum();

                                    //itemsWithoutPricingExpression.ForEach(x => x.AdditionalCost = additionalCost);
                                    itemsWithoutPricingExpression.ForEach(x => x.IsComponentAvailable = false);
                                    WindowProductData.AddRange(itemsWithoutPricingExpression);
                                    responseVm.IsSuccess = true;
                                }

                                //var itemsWithPricingExpression = data.Data
                                //    .Where(x => x.ComponentId != null)
                                //    .ToList();

                              
                                    // data.Data.Select(x => x.BuildHeight).First();

                                    var wholesale = await CalculateEstimate(data.Data, true);
                                    data.Data.ForEach(x =>
                                    {
                                        x.WholesalePrice = Convert.ToDecimal(wholesale.ToString("0.00"));
                                    });
                                    var componentPrice = await CalculateEstimate(data.Data, false);
                                    data.Data.ForEach(x =>
                                    {
                                        x.TotalPrice = componentPrice != null && componentPrice > 0.0m ? Convert.ToDecimal(componentPrice.ToString("0.00")) : null;
                                    });

                                    WindowProductData.AddRange(data.Data);
                                    responseVm.IsSuccess = true;
                                
                            }
                        }
                    }
                    foreach (var item in WindowProductData)
                    {
                        isContactIsTaxDocument = GetIsTaxExemptByWindowId(item.WindowId);
                        if (item.ComponentId == null)
                        {
                            var optionMSRPDict = await CalculateOptionPrice(
                                item.OptionsTypeIds,
                                (decimal)item.Quantity,
                                (decimal)item.Height,
                                (decimal)item.Width,
                                  false
                                   );
                            item.OptionTaxDetails.ForEach(x =>
                            {
                                x.OptionPrice = optionMSRPDict.OptionMSRP.Where(y => y.Key == x.OptionTypeId.ToString()).FirstOrDefault().Value;

                            });
                            item.AdditionalCost = optionMSRPDict.TotalCost;
                            item.IsTaxApplied = await IsTaxApplied((Guid)item.JobId);
                            //item.TotalTaxAmount = item.IsTaxAvailable == true
                            //? CalculateTotalTaxAmount(item.TotalPrice, item.MunicipalityTax, item.CityTax, item.StateTax)
                            //: 0.0m;
                        }


                        item.TotalTaxAmount = item.IsTaxAvailable == true && isContactIsTaxDocument==true
                            ? CalculateTotalTaxAmount(item.TotalPrice, item.MunicipalityTax, item.CityTax, item.StateTax)
                            : 0.0m;
                    }
                    
                    if (!IsBreakdown)
                    {
                        responseVm.Data = WindowProductData.DistinctBy(x => x.WindowProdcutId).OrderBy(x => x.ComponentName).ToList();
                        responseVm.TotalRecords = WindowProductData.DistinctBy(x => x.WindowProdcutId).Count();

                    }
                    else
                    {

                        responseVm.Data = WindowProductData.DistinctBy(x => x.ComponentId).OrderBy(x => x.ComponentName).ToList();
                        responseVm.TotalRecords = WindowProductData.DistinctBy(x => x.ComponentId).Count();
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }


            return responseVm;
        }

        private decimal CalculateTotalTaxAmount(decimal? totalPrice, decimal? municipality, decimal? city, decimal? state)
        {
            decimal basePrice = totalPrice ?? 0.0m;
            decimal totalTaxRate = (municipality ?? 0.0m) + (city ?? 0.0m) + (state ?? 0.0m);
            return (basePrice * totalTaxRate) / 100;
        }



        public async Task<bool> IsTaxApplied(Guid JobId)
        {
            try
            {
                var Jobdetail = jobService.GetAllQuerable()
                    .Where(x => x.JobId == JobId && x.IsDelete == false)
                    .Select(pt => pt.ContactId)
                    .FirstOrDefault();
                if (Jobdetail == Guid.Empty)
                {
                    return true;
                }
                bool IsTaxApplied = await taxExemption.AnyAsync(x => x.ContactId.ToString().ToLower() == Jobdetail.ToString().ToLower() && x.IsDelete == false &&
                x.EffectiveDate <= DateTime.Now && x.ExpiryDate >= DateTime.Now);
                return IsTaxApplied;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred tax Applied: {ex.Message}");
                return true;
            }
        }

        private async Task<decimal> CalculateEstimate(List<WindowProdcutDetailsForEstimate> data, bool? iswholesale = false)
        {
            decimal ComponentSubTotal = 0.00M;
            decimal Wholesale = 0.00M;
            bool isContactIsTaxDocument = false;
            var IsSubtenants = (await tenantService.GetAll()).Where
                    (t => t.TenantId == AppConstant.LoggedInTenantId && (t.ParentTenantId != null || !string.IsNullOrEmpty(t.ParentTenantId.ToString()))
                    && t.IsActive == true && t.IsDelete == false)
                  .FirstOrDefault();
            foreach (var item in data.DistinctBy(x => x.ComponentId))
            {
                var formula = item.ExpressionFormulae ?? "";

                //if((bool)iswholesale)
                //{
                //    item.BuildHeight = 1;
                //    item.BuildWidth = 1;
                //    item.Quantity = 1;

                //}    
                if (!string.IsNullOrEmpty(formula)) { 
                // Define an expression with a nudget and a custom function
                var expression = CalculateExpression(formula, item);
                var res = expression.Evaluate();
                var ComponentPrice = Convert.ToDecimal(res);

                //item.AdditionalCost = Convert.ToDecimal(item.OptionsPrice.Sum());
                //finalSubTotal = (decimal)item.AdditionalCost + finalSubTotal;
                var ComponentPriceWithoutMarkup = ComponentPrice;


                    if ((bool)iswholesale)
                    {
                        if (IsSubtenants != null)
                        {
                            var productMarkup = dbContext.TblManagePartnersProducts.Where(x => x.ProductId == item.ProductId && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                            if (productMarkup.MarkupType == "Percentage")
                            {
                                ComponentPriceWithoutMarkup = (decimal)(ComponentPriceWithoutMarkup * (1 + (productMarkup.WholeSalePrice ?? 0) / 100));
                            }
                            else if (productMarkup.MarkupType == "Fixed")
                            {
                                ComponentPriceWithoutMarkup = (decimal)(ComponentPriceWithoutMarkup + (productMarkup.WholeSalePrice ?? 0));
                            }
                        }
                        Wholesale += Convert.ToDecimal(Convert.ToDecimal(ComponentPriceWithoutMarkup).ToString("0.00"));
                        return Wholesale;
                    }
                    if (!(bool)iswholesale)
                    {
                        var ComponentPriceWithMarkup = (ComponentPriceWithoutMarkup * (100 + item.MarkupPercentage)) / 100;
                        var ComponentPriceWithProductMarkup = (ComponentPriceWithMarkup * (100 + item.Markup)) / 100;

                        if (IsSubtenants != null)
                        {
                            var productMarkup = dbContext.TblManagePartnersProducts.Where(x => x.ProductId == item.ProductId && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                            if (productMarkup.MarkupType == "Percentage")
                            {
                                ComponentPriceWithProductMarkup = (ComponentPriceWithProductMarkup * (1 + (productMarkup.PartnerSellingPrice ?? 0) / 100))??0.0m;
                            }
                            if (productMarkup.MarkupType == "Fixed")
                            {
                                ComponentPriceWithProductMarkup = (ComponentPriceWithProductMarkup + (productMarkup.PartnerSellingPrice ?? 0))??0.0m;
                            }
                            //else
                            //{
                            //    var globalmarkup = tenantBrandingService.GetAllQuerable().Where(x => x.Id == IsSubtenants.BrandingInfo).Select(x => x.Markup).FirstOrDefault();
                            //    ComponentPriceWithProductMarkup = (decimal)(ComponentPriceWithProductMarkup * (100 + (globalmarkup ?? 0.00m))) / 100;
                            //}

                            // subtenant their own Tax

                            //var taxAmount = await ApplySubtenantTax(item, productMarkup.TaxId, (decimal)ComponentPriceWithProductMarkup);

                            // ComponentPriceWithProductMarkup = ComponentPriceWithProductMarkup + taxAmount;

                        }



                        //if (item.IsTaxAvailable == true)
                        //{
                        //    var taxRate = (item.CityTax ?? 0) + (item.StateTax ?? 0) + (item.MunicipalityTax ?? 0);
                        //    var taxAmount = (ComponentPriceWithProductMarkup * taxRate) / 100;

                        //}

                        //var optionMSRPDict = await CalculateOptionPrice(
                        // item.OptionsTypeIds,
                        // (decimal)item.Quantity,
                        // (decimal)item.Height,
                        // (decimal)item.Width,
                        //  false
                        // );

                        //foreach (var x in item.OptionTaxDetails)
                        //{
                        //    optionMSRPDict.OptionMSRP.TryGetValue(x.OptionTypeId.ToString(), out var price);
                        //    x.OptionPrice = price;
                        //}

                        //item.AdditionalCost = optionMSRPDict.TotalCost;
                         isContactIsTaxDocument = GetIsTaxExemptByWindowId(item.WindowId);
                        

                        item.Price = Convert.ToDecimal(Convert.ToDecimal(ComponentPriceWithProductMarkup).ToString("0.00"));
                        if (isContactIsTaxDocument == false)
                        {
                            item.MunicipalityTaxAmount = 0.0m;
                            item.StateTaxAmount = 0.0m;
                            item.CityTaxAmount = 0.0m;
                            item.TotalTaxAmount = 0.0m;
                        }
                        else
                        {
                            item.MunicipalityTaxAmount = (item.Price * item.MunicipalityTax / 100);
                            item.StateTaxAmount = (item.Price * item.StateTax / 100);
                            item.CityTaxAmount = (item.Price * item.CityTax / 100);
                            item.TotalTaxAmount = (item.MunicipalityTaxAmount + item.StateTaxAmount + item.CityTaxAmount)??0.0m;
                        }
                        ComponentSubTotal += Convert.ToDecimal(Convert.ToDecimal(ComponentPriceWithProductMarkup).ToString("0.00"));
                    }
                    }
                    var optionMSRPDict = await CalculateOptionPrice(
                             item.OptionsTypeIds,
                             (decimal)item.Quantity,
                             (decimal)item.Height,
                             (decimal)item.Width,
                              false
                              );
                    item.AdditionalCost = optionMSRPDict.TotalCost;
                    foreach (var x in item.OptionTaxDetails)
                    {
                        optionMSRPDict.OptionMSRP.TryGetValue(x.OptionTypeId.ToString(), out var price);
                        x.OptionPrice = price;
                    }
                    if (item.OptionTaxDetails != null && item.OptionTaxDetails.Count > 0 && isContactIsTaxDocument==true)
                    {
                        item.OptionTaxDetails.ForEach(x =>
                        {
                            x.MunicipalityTaxAmount = (x.OptionPrice * x.MunicipalityTax / 100)??0.0m;
                            x.StateTaxAmount = (x.OptionPrice * x.StateTax / 100) ?? 0.0m;
                            x.CityTaxAmount = (x.OptionPrice * x.CityTax / 100 )?? 0.0m;
                            x.TotalTaxAmount = (x.MunicipalityTaxAmount + x.StateTaxAmount + x.CityTaxAmount) ?? 0.0m;
                        });
                    }else
                    {
                    item.OptionTaxDetails.ForEach(x =>
                    {
                        x.MunicipalityTaxAmount =  0.0m;
                        x.StateTaxAmount = 0.0m;
                        x.CityTaxAmount =  0.0m;
                        x.TotalTaxAmount =  0.0m;
                    });

                }
                }
                //if ((bool)iswholesale)
                //{

                //}
                return ComponentSubTotal;
        }

        public NCalc.Expression CalculateExpression(string formula, dynamic values)
        {

            // Find all placeholders in the formula
            const string pattern = @"{{.*?}}";
            var matches = Regex.Matches(formula ?? "", pattern);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var property = match.Value.Replace("{{", "").Replace("}}", "");
                    object val = GetProperty.GetColumnValue(values, property);

                    // Handle null values and special property cases
                    if (val == null)
                    {
                        if (values.IsView)
                        {
                            val = values.UnitCost;
                            var wasteFactor = GetProperty.GetColumnValue(values, "WasteFactor");
                            if (wasteFactor != null)
                                val = (val * (100 + (values.WasteFactor ?? 0)) / 100);
                        }
                        else
                        {
                            var info = property.Split("__");
                            if (info.Length == 3 && info[0] == "Material")
                            {
                                var query = materialService.GetAllQuerable()
                                    .Where(x => x.ProductType.ToString() == info[1] && x.MaterialName == info[2] && x.IsActive == true &&
                                                (x.TenantId == AppConstant.LoggedInTenantId || x.TenantId.ToString() == AppConstant.LoggedInParentTenantId)).FirstOrDefault();
                                val = query.UnitCost;
                                Guid ComponentId = values.ComponentId ?? Guid.Empty;
                                var wasteFactor = componentMaterialService.GetAllQuerable()
                     .Where(x => x.RawMaterialId == query.RawMaterialId && x.ComponentId == ComponentId && x.IsDelete == false
                       && x.TenantId == AppConstant.LoggedInTenantId || x.TenantId.ToString() == AppConstant.LoggedInParentTenantId)
                                 .Select(x => x.WasteFactor)
                                .FirstOrDefault();
                                if (wasteFactor != null)
                                    val = (Convert.ToDecimal(val) * (100 + (wasteFactor ?? 0)) / 100);
                            }
                            else if (info.Length == 4 && info[0] == "Material" && info[3] == "Unit")
                            {
                                var materialUnit = materialService.GetAllQuerable()
                                    .Where(x => x.ProductType.ToString() == info[1] && x.MaterialName == info[2] && x.IsActive == true &&
                                                (x.TenantId == AppConstant.LoggedInTenantId || x.TenantId.ToString() == AppConstant.LoggedInParentTenantId))
                                    .Select(x => x.UnitOfMeasurement).FirstOrDefault();
                                formula = formula?.Replace(match.Value, $"'{materialUnit}'");
                                continue;
                            }
                        }
                    }

                    // Replace the placeholder in the formula with the value
                    if (property == "Unit")
                    {
                        formula = formula?.Replace(match.Value, $"'{val}'");
                    }
                    else if (property == "OptionPrice")
                    {
                        val = values.Optionprice;
                        formula = formula?.Replace(match.Value, Convert.ToDecimal(val).ToString("N2"));
                    }
                    else
                    {
                        formula = formula?.Replace(match.Value, Convert.ToDecimal(val).ToString("N2"));
                    }
                }
            }

            return new NCalc.Expression(formula);
        }

        public async Task<IResponseVm<string>> Update_Estimate(UpdatetEstimate model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log("Start: Update Estimate...");

            try
            {
                // Validate tenant existence
                var isTenantExists = await ValidateTenantExists();
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                // Fetch the estimate
                var estimate = GetAllQuerable()
                    .Where(c => c.EstimateId == model.EstimateId && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false)
                    .FirstOrDefault();

                if (estimate == null)
                {
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }

                if (model.ProductsList == null || model.ProductsList.Count == 0)
                {
                    responseVm.Message = "Atleast one window is required.";
                    return responseVm; // Return early if window not exists
                }
                // Check if the estimate name already exists
                var isEstimateExists = await ValidateEstimateExists(model.EstimateName, (Guid)model.JobId, model.EstimateId);
                if (isEstimateExists)
                {
                    responseVm.Message = "Estimate Name " + AppConstant.AlreadyExists;
                    return responseVm;
                }

                // Check if the estimate is pending approval
                if (estimate.Status == EnumValue.GetEnumValue(Estimation.PendingForApproval))
                {
                    responseVm.Message = "Waiting for approval, can't edit this estimate";
                    return responseVm;
                }
                decimal additionalCost = 0.0M;
                decimal SumofSubtotalandAdditionalCost = 0.0M;
                decimal subtotal = 0.0M;
                decimal grandtotal = 0.0M;
                decimal totalTaxAmount = 0.0m;
                Guid? taxRateId = null;
                Guid? discountId = null;
                var PricedProductsList = model.ProductsList.Where(x => x.ProductPrice != null && x.ProductPrice > 0).ToList();
                var windowproductdetails = await BuildEstimate(model.ProductsList.Select(x => x.WindowProductId).ToArray(), false);
                if (windowproductdetails.Data != null && windowproductdetails.Data.Any())
                {
                    if (windowproductdetails.Data.Any(x => x.TotalPrice == null))
                    {
                        var Ids = windowproductdetails.Data.Where(x => x.TotalPrice == null).Select(x => x.WindowProdcutId).Distinct().ToList();
                        subtotal = PricedProductsList
                            .Where(p => Ids.Contains(p.WindowProductId))
                            .Select(x => x.ProductPrice).Sum();
                    }

                }
                subtotal += (decimal)windowproductdetails.Data.Where(x => x.TotalPrice != null).DistinctBy(x => x.WindowProdcutId).Sum(x => x.TotalPrice);
                additionalCost += (decimal)windowproductdetails.Data.Sum(x => x.AdditionalCost);
                SumofSubtotalandAdditionalCost = subtotal + additionalCost;
                //if (string.IsNullOrEmpty((model.taxRateId).ToString()) && string.IsNullOrEmpty(model.DiscountCoupon))
                //{
                //    grandtotal = SumofSubtotalandAdditionalCost;
                //}
                //else
                //{
                //    if (string.IsNullOrEmpty((model.taxRateId).ToString()))
                //    {
                //        taxRateId = Guid.Empty;
                //    }
                //    else
                //    {
                //        taxRateId = model.taxRateId;
                //    }
                //    if (string.IsNullOrEmpty(model.DiscountId.ToString()))
                //    {
                //        discountId = Guid.Empty;
                //    }
                //    else
                //    {
                //        discountId = model.DiscountId;
                //    }
                //    grandtotal = await CalculateGrandTotal(discountId, SumofSubtotalandAdditionalCost, taxRateId) ?? 0m;
                //}

                if (string.IsNullOrEmpty((model.taxRateId).ToString()))
                {
                    taxRateId = Guid.Empty;
                }
                else
                {
                    taxRateId = model.taxRateId;
                }
                if (string.IsNullOrEmpty(model.DiscountId.ToString()) && string.IsNullOrEmpty(model.DiscountCoupon))
                {
                    discountId = Guid.Empty;
                }
                else
                {
                    discountId = model.DiscountId;
                }
                grandtotal = await CalculateGrandTotal(discountId, SumofSubtotalandAdditionalCost, taxRateId) ?? 0m;
                bool isTaxExemptionInContact = GetIsTaxExemptByWindowId(windowproductdetails.Data.Select(s => s.WindowId).FirstOrDefault());
                if (model.IsTaxApplicable == true && isTaxExemptionInContact)
                {
                    var productTaxAmount = windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).Sum(x => x.TotalTaxAmount) ?? 0.0m;
                    var optionTaxAmount = windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).SelectMany(x => x.OptionTaxDetails).Sum(x => x.TotalTaxAmount) ?? 0.0m;
                    totalTaxAmount = optionTaxAmount + productTaxAmount;
                    grandtotal = grandtotal + totalTaxAmount;
                }


                // Update the estimate details
                estimate.EstimateName = model.EstimateName;
                estimate.Notes = model.Notes;
                estimate.DiscountAmount = model.Discount;
                estimate.DiscountCoupon = model.DiscountCoupon;
                estimate.GrandTotal = grandtotal;
                estimate.JobId = model.JobId;
                estimate.TaxId = model.taxRateId;
                estimate.TaxAmount = model.Tax;
                estimate.TotalPrice = SumofSubtotalandAdditionalCost;
                estimate.AdditionalCost = additionalCost;
                estimate.TermsCondition = model.TermsCondition;
                estimate.ModifiedBy = AppConstant.LoggedInUserID;
                estimate.IsTaxApplied = model.IsTaxApplicable;
                estimate.TotaltaxAmount = totalTaxAmount;
                estimate.UpdatedAt = DateTime.Now;

                await Update(estimate);
                FileLogger.Log($"Estimate {estimate.EstimateName} updated successfully.");

                // Deactivate old related records
                await DeactivateOldEstimateRecords(estimate.EstimateId);
                FileLogger.Log("Old related records deactivated successfully.");

                // Process new window products and components
                await ProcessWindowProducts(model.ProductsList, windowproductdetails.Data, estimate.EstimateId);
                if (!string.IsNullOrEmpty(Message))
                {
                    responseVm.Message = Message;
                    return responseVm;
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
                FileLogger.Log("Estimate update process completed successfully.");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<EstimateList>>> ViewEachEstimation(Guid? EstimateId)
        {
            IResponseVm<List<EstimateList>> responseVm = new ResponseVm<List<EstimateList>>();
            try
            {
                if (EstimateId == Guid.Empty)
                {
                    responseVm.Message = "Invalid Estimate ID.";
                    return responseVm;
                }

                var estimate = GetAllQuerable()
                                       .Where(e => e.EstimateId == EstimateId && e.IsDelete == false)
                                       .FirstOrDefault();

                if (estimate == null)
                {
                    responseVm.Message = "These is an issue.Please contact administration.";
                    return responseVm;
                }

                AppConstant.LoggedInTenantId = estimate.TenantId;

                Pagination filterValue = new Pagination
                {
                    EstimationId = EstimateId.ToString()
                };
                var estimateListFiltered = await GetAll_Estimate(filterValue);

                if (estimateListFiltered != null && estimateListFiltered.TotalRecords > 0 && estimateListFiltered.Data.Any())
                {
                    responseVm.Data = estimateListFiltered.Data;
                    responseVm.IsSuccess = true;
                    responseVm.TotalRecords = responseVm.TotalRecords;
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No estimates found after filtering.";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> ChangeStatus(Guid EstimationId, Estimation NewEstimationStatus)
        {
            // Initial log indicating that the method is processing the request
            FileLogger.Log($"Processing status change for EstimationId: {EstimationId}...");

            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                // Retrieve the EstimationStatus
                var EstimationStatus = GetEstimationStatusById(EstimationId);

                if (EstimationStatus == null)
                {
                    responseVm.Message = "Estimation not found.";
                    FileLogger.Log($"Estimation with ID {EstimationId} not found.");
                    return responseVm;
                }

                // Safely parse the current status to Estimation enum
                if (!Enum.TryParse(EstimationStatus.Status, out Estimation currentStatus))
                {
                    responseVm.Message = "Invalid status found in the system.";
                    FileLogger.Log($"Estimation with ID {EstimationId} has an invalid status.");
                    return responseVm;
                }

                // Check the current status of the Estimation and handle accordingly
                if (CanChangeStatus(currentStatus, NewEstimationStatus))
                {
                    EstimationStatus.Status = EnumValue.GetEnumValue(NewEstimationStatus);
                    var res = await Update(EstimationStatus);


                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.Data = EnumValue.GetEnumValue(NewEstimationStatus);
                    if (responseVm.IsSuccess)
                    {
                        var EstimateName = GetAllQuerable().Where(x => x.EstimateId == EstimationId && x.IsDelete == false).Select(x => x.EstimateName).FirstOrDefault();
                        var JobId = GetAllQuerable().Where(x => x.EstimateId == EstimationId && x.IsDelete == false).Select(x => x.JobId).FirstOrDefault();

                        // update job status and stage 

                        if (responseVm.IsSuccess)
                        {
                            if (NewEstimationStatus == Estimation.Approved)
                            {
                                var jobStatusName = EnumValue.GetEnumValue(JobStatus.Won);
                                var updateResponse = await statusAndStageMapping.UpdateOpportunityAndJobStatus(EstimationStatus?.JobId ?? Guid.Empty, jobStatusName);

                                if (!updateResponse.IsSuccess)
                                {
                                    FileLogger.Log(updateResponse.Message);
                                    // responseVm.IsSuccess = false;
                                    //responseVm.Message = updateResponse.Message;
                                    //return responseVm;
                                }
                            }
                        }

                        CreateNotification notification1 = new CreateNotification()
                        {
                            UserId = AppConstant.LoggedInUserID,
                            Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Estimate),
                            Body = $"{EstimateName} {AppConstant.UpdateStatus_Notification}",
                            CreatedDate = DateTime.Now,
                            Type = Enum.GetName(typeof(NotificationModule), NotificationModule.Estimate),
                            TypeId = EstimationId
                        };

                        var result = await notification.CreateNotification(notification1);
                    }
                    // Log and record the status change in history
                    await EstimationChangeHistory(EstimationId, NewEstimationStatus);
                    FileLogger.Log($"Estimation status changed successfully to {NewEstimationStatus} for EstimationId: {EstimationId}");
                }
                else
                {
                    responseVm.Message = $"You can't change status from {currentStatus} to {NewEstimationStatus}.";
                    responseVm.Data = currentStatus.ToString();
                    FileLogger.Log($"Status change from {currentStatus} to {NewEstimationStatus} is not allowed.");
                }
            }
            catch (Exception ex)
            {
                // Catch any errors and log them
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
                FileLogger.Log($"Error processing status change for EstimationId: {EstimationId}. Error: {ex.Message}");
            }

            return responseVm;
        }


        private async Task<List<TblEstimate>> FetchEstimates(Pagination filter)
        {
            if (string.IsNullOrEmpty(filter.TenantId))
            {
                // Log and fetch estimates for the logged-in tenant
                FileLogger.Log("No TenantId provided, fetching for the logged-in tenant.");
                return (await GetAll()).Where(x => x.IsDelete == false && (!string.IsNullOrEmpty(filter.EstimationId) ? x.EstimateId.ToString() == filter.EstimationId : true) && x.TenantId == AppConstant.LoggedInTenantId)
                                       .OrderByDescending(x => x.CreatedAt)
                                       .ToList();
            }

            // Validate TenantId if provided
            var isRelatedTenant = tenantService.GetAllQuerable()
                .Where(x => x.TenantId.ToString() == filter.TenantId && x.ParentTenantId == AppConstant.LoggedInTenantId && x.IsDelete == false)
                .FirstOrDefault();

            if (isRelatedTenant == null)
            {
                FileLogger.Log($"Tenant with ID {filter.TenantId} is not related or not found.");
                return null; // Return null to indicate tenant validation failure
            }

            // Fetch estimates for the provided tenant
            FileLogger.Log($"Tenant with ID {filter.TenantId} validated successfully.");
            return (await GetAll()).Where(x => x.IsDelete == false && (!string.IsNullOrEmpty(filter.EstimationId) ? x.EstimateId.ToString() == filter.EstimationId : true) && x.TenantId.ToString() == filter.TenantId)
                                   .OrderByDescending(x => x.CreatedAt)
                                   .ToList();
        }

        private async Task<List<EstimateList>> ApplyFilters(List<TblEstimate> estimates, Pagination filter)
        {
            var estimateList = new List<EstimateList>();

            foreach (var estimate in estimates)
            {
                var estimateDetails = new EstimateList
                {
                    EstimateId = estimate.EstimateId,
                    CreatedDate = estimate.CreatedAt,
                    JobId = estimate.JobId,
                    Status = estimate.Status,
                    IsActive = estimate.IsActive,
                    Discount = estimate.DiscountAmount,
                    DiscountCoupon = estimate.DiscountCoupon,
                    taxRateId = estimate.TaxId,
                    EstimateName = estimate.EstimateName,
                    EstimateCode = estimate.EstimateCode,
                    SubTotal = estimate.TotalPrice,
                    //additionalCostAsOptionPrice = estimate.AdditionalCost,
                    GrandTotal = estimate.GrandTotal,
                    Notes = estimate.Notes,
                    Tax = estimate.TotaltaxAmount,
                    TermsCondition = estimate.TermsCondition,
                    IsTaxApplicable = estimate.IsTaxApplied,
                    jobDetails = new JobDetails() // Ensure jobDetails is not null
                };

                // Fetch user details
                var user = dbContext.TblUsers
                    .FirstOrDefault(x => x.Id == estimate.CreatedBy && !(bool)x.IsDelete);

                if (user != null)
                {
                    estimateDetails.CreatedBy = $"{user.FirstName} {user.LastName}";
                    estimateDetails.jobDetails.FirstName = user.FirstName;
                    estimateDetails.jobDetails.LastName = user.LastName;
                }

                // Fetch tenant and branding info
                var tenant = dbContext.TblTenants
                    .FirstOrDefault(t => t.TenantId == estimate.TenantId && !(bool)t.IsDelete);

                if (tenant != null)
                {
                    var brandingInfo = dbContext.TblTenantBrandingInfos
                        .FirstOrDefault(tb => tb.Id == tenant.BrandingInfo);

                    if (brandingInfo != null)
                    {
                        estimateDetails.primarycolor = brandingInfo.PrimaryColor;
                        estimateDetails.CompanyName = brandingInfo.CompanyName;
                    }
                }

                //// Tax and region details
                //var taxDetails = taxRate.GetAllQuerable()
                //    .FirstOrDefault(t => t.TaxRateId == estimate.TaxId && !(bool)t.IsDelete);

                //if (taxDetails != null)
                //{
                //    estimateDetails.TaxRate = taxDetails.Rate;
                //    estimateDetails.Region = region.GetAllQuerable()
                //        .Where(r => r.Id == taxDetails.Region && !(bool)r.IsDelete)
                //        .Select(r => r.RegionName)
                //        .FirstOrDefault() ?? "";
                //}
                //else
                //{
                //    estimateDetails.TaxRate = 0;
                //    estimateDetails.Region = "";
                //}

                // Fetch and add related products and options
                await AddEstimateProducts(estimate, estimateDetails);
                estimateList.Add(estimateDetails);
            }

            var filteredRecords = estimateList
                .Where(x =>
                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                    (string.IsNullOrWhiteSpace(filter.JobStatus) ||
                        string.Equals(x.jobDetails?.JobStatus, filter.JobStatus, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(filter.JobId) ||
                        string.Equals(x.JobId?.ToString(), filter.JobId, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(filter.EstimationId) ||
                        string.Equals(x.EstimateId.ToString(), filter.EstimationId, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(filter.SearchValue) ||
                        (x.Notes ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.JobId?.ToString() ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.jobDetails?.ContactId?.ToString() ?? "").Contains(filter.CustomerId ?? "", StringComparison.OrdinalIgnoreCase) ||
                        (x.Status ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.EstimateId.ToString() ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.EstimateCode ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.EstimateName ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.TermsCondition ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.jobDetails?.JobStatus ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.DiscountCoupon ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.jobDetails?.JobName ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase) ||
                        (x.GrandTotal.ToString() ?? "").Contains(filter.SearchValue, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return filteredRecords;
        }

        private async Task AddJobDetails(EstimateList e)
        {

            var jobdetails1 = (from j1 in dbContext.TblJobs
                               join j2 in dbContext.TblEstimates on j1.JobId equals j2.JobId
                               join con in dbContext.TblContacts on j1.ContactId equals con.ContactId
                               join j3 in dbContext.TblAddresses on j1.AddressId equals j3.AddressId
                               join j4 in dbContext.MstJobTypes on j1.JobType equals j4.Id into JobType
                               from j4 in JobType.DefaultIfEmpty()
                               join j5 in dbContext.TblUsers on j1.AssignedPersonnel equals j5.Id into Users
                               from j5 in Users.DefaultIfEmpty()
                               join j6 in dbContext.TblContacts on j1.ContactId equals j6.ContactId
                               where j1.TenantId == AppConstant.LoggedInTenantId && j1.IsDelete == false && j1.JobId == e.JobId
                               select new JobDetails
                               {
                                   JobName = j1.JobName,
                                   JobUniqueId = j1.JobUniqueId,
                                   JobNotes = j1.Notes,
                                   JobType = j4.JobTypeName,
                                   FirstName = con.FirstName,
                                   LastName = con.LastName,
                                   Suffix = con.Suffix ?? "",
                                   JobStatus = j1.JobStatus.ToString().ToLower(),
                                   AssignedPersonnel = j5.FirstName + " " + j5.LastName,
                                   Address = j3.StreetAddress + " " + j3.City + " " + j3.State + " " + j3.ZipCode,
                                   AddressLabel = j3.Label,
                                   ScheduleTime = j1.ScheduledDate,
                                   ContactId = j6.ContactId,
                                   JobIsActive = j1.IsActive
                               }).FirstOrDefault();

            e.jobDetails = jobdetails1;

        }

        private async Task AddEstimateProducts(TblEstimate estimate, EstimateList e)
        {
            try
            {
                var productList = estimateWindowProductService.GetAllQuerable()
                    .Where(x => x.EstimateId == estimate.EstimateId && x.IsDelete == false)
                    .Select(x => new
                    {
                        x.EstimateWindowProductId,
                        x.ProductId,
                        x.ProductName,
                        x.ProductType,
                        x.Width,
                        x.Height,
                        x.ArchHeight,
                        x.Color,
                        x.Notes,
                        x.WindowId,
                        x.WindowProductId,
                        x.Price,
                        x.ProductTypeName,
                        x.ProductPrice,
                        x.WholeSalePrice,
                    }).ToList();

                e.WindowProductId = productList.Select(p => (Guid)p.WindowProductId).Distinct().ToArray();
                foreach (var p in productList)
                {
                    var Options = (from estimateOption in dbContext.TblEstimateWindowProductOptions
                                   join optionType in dbContext.TblOptionsTypes on estimateOption.OptionTypeid equals optionType.OptionTypeId
                                   join option in dbContext.TblOptions on optionType.OptionId equals option.OptionId
                                   where estimateOption.EstimateWindowProductId == p.EstimateWindowProductId
                                   select new OptionTaxVM
                                   {
                                       TaxId = estimateOption.TaxId,
                                       TaxName = estimateOption.TaxName ?? "",
                                       MunicipalityTax = estimateOption.MunicipilityTax,
                                       StateTax = estimateOption.StateTax,
                                       CityTax = estimateOption.CityTax,
                                       OptionPrice = estimateOption.OptionPrice,
                                       OptionName = estimateOption.OptionName ?? "",
                                       OptionId = estimateOption.Optionid.ToString(),
                                       OptionTypeId = estimateOption.OptionTypeid.ToString(),
                                       OptionTypeName = Convert.ToString(estimateOption.OptionName + " - " + estimateOption.OptionTypeName),
                                   }).ToList();


                    var WindowDetails = estimateWindowService.GetAllQuerable()
                        .Where(x => x.EstimateId == estimate.EstimateId && x.IsDelete == false)
                        .ToList();
                    var WindowQty_Label = windowService.GetAllQuerable()
                            .Where(w => w.WindowId == p.WindowId && w.IsDelete == false)
                            .FirstOrDefault();
                    var IsComponenetAvailable = await estimateWindowProductComponentService.AnyAsync(
                        x => x.EstimateId == estimate.EstimateId && x.WindowId == p.WindowId && x.ProductId == p.ProductId && x.IsDelete == false);
                    EstimateProductList estimateProduct = new EstimateProductList
                    {
                        EstimateWindowProductId = p.EstimateWindowProductId,
                        Label = WindowQty_Label.Label,
                        WindowQty = WindowQty_Label.Quantity,
                        ProductType = p.ProductType,
                        Width = p.Width,
                        Height = p.Height,
                        ArchHeight = p.ArchHeight,
                        ProductName = p.ProductName,
                        ProductTypeName = p.ProductTypeName,
                        WindowProductId = p.WindowProductId,
                        TotalPrice = p.Price,
                        additionalCost = Options.Select(x => x.OptionPrice).Sum(),
                        ProductPrice = p.ProductPrice,
                        Options = Options.Select(x => x.OptionTypeName).ToArray(),
                        IsComponentAvailable = IsComponenetAvailable,
                        WholesalePrice = p.WholeSalePrice,
                        EstimateWindowDetails = WindowDetails.Where(x => x.WindowId == p.WindowId).Select(x => new EstimateWindowList
                        {
                            ArchHeight = x.ArchHeight,
                            Height = x.Height,
                            Width = x.Width
                        }).FirstOrDefault()

                    };
                    e.EstimateProductList.Add(estimateProduct);
                }
                await AddJobDetails(e);
                e.IsTaxApplied = await IsTaxApplied((Guid)e.JobId);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred Add Estimate Product: {ex.Message}");
            }
        }

        private async Task<bool> ValidateTenantExists()
        {
            FileLogger.Log($"Checking if tenant with ID {AppConstant.LoggedInTenantId} exists...");
            var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
            if (!isTenantExists)
            {
                FileLogger.Log("Tenant not found.");
            }
            return isTenantExists;
        }

        private async Task DeactivateOldEstimateRecords(Guid estimateId)
        {
            FileLogger.Log($"Deactivating old records for Estimate ID: {estimateId}");

            var oldWinEstList = (await estimateWindowService.GetAll()).Where(x => x.EstimateId == estimateId && x.IsDelete == false).ToList();
            foreach (var item in oldWinEstList)
            {
                item.IsActive = false;
                item.IsDelete = true;
                item.UpdatedAt = DateTime.Now;
                item.ModifiedBy = AppConstant.LoggedInUserID;
                await estimateWindowService.Update(item);
            }

            var oldWinEstProdList = (await estimateWindowProductService.GetAll()).Where(x => x.EstimateId == estimateId && x.IsDelete == false).ToList();
            foreach (var item in oldWinEstProdList)
            {
                item.IsActive = false;
                item.IsDelete = true;
                item.UpdatedAt = DateTime.Now;
                item.ModifiedBy = AppConstant.LoggedInUserID;
                await estimateWindowProductService.Update(item);
            }

            var oldWinEstProdComList = (await estimateWindowProductComponentService.GetAll()).Where(x => x.EstimateId == estimateId && x.IsDelete == false).ToList();
            foreach (var item in oldWinEstProdComList)
            {
                item.IsActive = false;
                item.IsDelete = true;
                item.UpdatedAt = DateTime.Now;
                item.ModifiedBy = AppConstant.LoggedInUserID;
                await estimateWindowProductComponentService.Update(item);
            }

            var oldWinEstProdComMatList = (await estimateWindowProductComponentMaterialService.GetAll()).Where(x => x.EstimateId == estimateId && x.IsDelete == false).ToList();
            foreach (var item in oldWinEstProdComMatList)
            {
                item.IsActive = false;
                item.IsDelete = true;
                item.UpdatedAt = DateTime.Now;
                item.ModifiedBy = AppConstant.LoggedInUserID;
                await estimateWindowProductComponentMaterialService.Update(item);
            }
        }

        private async Task<bool> ValidateEstimateExists(string estimateName, Guid jobId, Guid estimateId)
        {
            FileLogger.Log($"Checking if estimate with name {estimateName} already exists...");
            var isEstimateExists = (await GetAll())
                .Where(c => (estimateId != Guid.Empty ? c.EstimateId != estimateId : true) && c.EstimateName == estimateName && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false && c.JobId == jobId)
                .ToList();
            if (isEstimateExists.Count > 0)
            {
                FileLogger.Log($"Estimate with name {estimateName} already exists.");
                return true;
            }
            return false;
        }

        private async Task<EstimateComponent> CreateEstimateRecord(CreateEstimate model)
        {
            FileLogger.Log("Creating new estimate...");

            decimal subtotal = 0.0M;
            decimal SumofSubtotalandAdditionalCost = 0.0M;
            decimal grandtotal = 0m;
            decimal additionalCostAsOptionPrice = 0m;
            decimal totalTaxAmount = 0.0m;
            Guid? taxRateId = null;
            Guid? discountId = null;
            var PricedProductsList = model.ProductsList.Where(x => x.ProductPrice != null && x.ProductPrice > 0).ToList();
            var windowproductdetails = await BuildEstimate(model.ProductsList.Select(x => x.WindowProductId).ToArray(), true);


            if (windowproductdetails.Data != null && windowproductdetails.Data.Any())
            {
                if (windowproductdetails.Data.Any(x => x.TotalPrice == null))
                {
                    var Ids = windowproductdetails.Data.Where(x => x.TotalPrice == null).Select(x => x.WindowProdcutId).Distinct().ToList();
                    subtotal = PricedProductsList
                        .Where(p => Ids.Contains(p.WindowProductId))
                        .Select(x => x.ProductPrice).Sum();
                }
            }
            subtotal += (decimal)windowproductdetails.Data.Where(x => x.TotalPrice != null).DistinctBy(x => x.WindowProdcutId).Sum(x => x.TotalPrice);
            additionalCostAsOptionPrice += (decimal)windowproductdetails.Data.Sum(x => x.AdditionalCost);


            SumofSubtotalandAdditionalCost = subtotal + additionalCostAsOptionPrice;



            if (string.IsNullOrEmpty((model.taxRateId).ToString()))
            {
                taxRateId = Guid.Empty;
            }
            else
            {
                taxRateId = model.taxRateId;
            }
            if (string.IsNullOrEmpty(model.DiscountId.ToString()) && string.IsNullOrEmpty(model.DiscountCoupon))
            {
                discountId = Guid.Empty;
            }
            else
            {
                discountId = model.DiscountId;
            }
            grandtotal = await CalculateGrandTotal(discountId, SumofSubtotalandAdditionalCost, taxRateId) ?? 0m;

            bool isTaxExemptionInContact = GetIsTaxExemptByWindowId(windowproductdetails.Data.Select(s => s.WindowId).FirstOrDefault());
            if (model.IsTaxApplicable == true && isTaxExemptionInContact)
            {
                var productTaxAmount = windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).Sum(x => x.TotalTaxAmount) ?? 0.0m;
                var optionTaxAmount = windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).SelectMany(x => x.OptionTaxDetails).Sum(x => x.TotalTaxAmount) ?? 0.0m;
                totalTaxAmount = optionTaxAmount + productTaxAmount;
                grandtotal += totalTaxAmount;
            }

            string EstimationCode = EstimationUniqueName().Result.Data;
            TblEstimate createEstimate = new TblEstimate
            {
                EstimateId = Guid.NewGuid(),
                EstimateName = model.EstimateName.Trim(),
                EstimateCode = EstimationCode,
                Notes = model.Notes,
                DiscountAmount = model.Discount,
                DiscountCoupon = model.DiscountCoupon,
                GrandTotal = grandtotal,
                JobId = model.JobId,
                TaxAmount = totalTaxAmount,
                TermsCondition = model.TermsCondition,
                CreatedBy = AppConstant.LoggedInUserID,
                CreatedAt = DateTime.Now,
                TenantId = AppConstant.LoggedInTenantId,
                IsActive = true,
                IsDelete = false,
                TotalPrice = SumofSubtotalandAdditionalCost,
                AdditionalCost = additionalCostAsOptionPrice,
                IsTaxApplied = model.IsTaxApplicable,
                TotaltaxAmount = totalTaxAmount,
                Status = EnumValue.GetEnumValue(Estimation.New),
            };
            var res = await Add(createEstimate);
            EstimateComponent estimateComponent = new EstimateComponent();
            estimateComponent.EstimateId = createEstimate.EstimateId;
            estimateComponent.WindowProdcutDetailsForEstimate = windowproductdetails.Data;
            return estimateComponent;
        }

        public async Task<decimal?> CalculateGrandTotal(Guid? discountId, decimal subtotal, Guid? taxId)
        {
            decimal discount = 0m;
            decimal grandTotal = 0m;

            // Fetch discount details based on discountId
            var discountdetails = await GetDiscountDetails(discountId);

            if (discountdetails != null)
            {
                // Apply discount based on type: Fixed or Percentage
                if (discountdetails.DiscountType == EnumValue.GetEnumValue(DiscountType.Fixed))
                {
                    // For "Fixed" discount type
                    discount = discountdetails.FixedAmount > subtotal
                        ? subtotal
                        : discountdetails.FixedAmount;
                }
                else if (discountdetails.DiscountType == EnumValue.GetEnumValue(DiscountType.Percentage))
                {
                    // For "Percentage" discount type
                    discount = ((subtotal) * discountdetails.PercentageDiscount) / 100;
                }
            }

            // Apply tax
            decimal? tax = await ApplyTax(taxId, subtotal, discount);

            // Calculate grand total after applying discount and tax
            grandTotal = Math.Max(0, (subtotal - discount) + tax.Value);

            return grandTotal;
        }

        private async Task<dynamic> GetDiscountDetails(Guid? discountId)
        {
            // Assuming you have a service or repository to fetch discount details.
            if (discountId == Guid.Empty)
            {
                return null;
            }

            return discountservice.GetAllQuerable()
                .Where(x => x.DiscountId == discountId && x.IsDelete == false)
                .FirstOrDefault();
        }

        private async Task<decimal?> ApplyTax(Guid? taxId, decimal subtotal, decimal discount)
        {
            // Calculate tax if taxId is provided
            if (taxId == Guid.Empty)
            {
                return 0m;
            }

            // Fetch the tax rate based on taxId
            var taxRate1 = taxRate.GetAllQuerable()
                .Where(x => x.TaxRateId == taxId && x.IsDelete == false)
                .Select(x => x.Rate)
                .FirstOrDefault();

            if (taxRate == null)
            {
                return 0m;
            }

            // Calculate tax based on subtotal, additional cost, and discount
            decimal taxApplied = (decimal)((subtotal - discount) * taxRate1) / 100;
            return taxApplied;
        }

        private async Task ProcessWindowProducts(List<ProductListing> windowProductIds, List<WindowProdcutDetailsForEstimate> windowProdcutDetailsForEstimates, Guid estimateId)
        {
            FileLogger.Log("Processing window products...");
            foreach (var product in windowProductIds)
            {
                var windowProductData = (await windowProductService.GetAll()).Where(x => x.WindowProductId == product.WindowProductId && x.IsDelete == false).FirstOrDefault();
                if (windowProductData == null)
                {
                    Message = "Window Product " + AppConstant.NoDataFound;
                    return; // Return early if window product not found
                }

                var windowData = (await windowService.GetAll()).Where(x => x.WindowId == windowProductData.WindowId && x.IsDelete == false).FirstOrDefault();
                if (windowData == null)
                {
                    Message = "Window " + AppConstant.NoDataFound;
                    return; // Return early if window not found
                }
                var wholesaleprice = windowProdcutDetailsForEstimates.Where(x => x.WindowProdcutId == product.WindowProductId).Select(x => x.WholesalePrice).FirstOrDefault() ?? 0.0m;
                var EstimateWindow = await AddWindowToEstimate(windowData, windowProductData, estimateId);
                var EstimateWindowProduct = await AddProductToEstimate(windowProductData, estimateId, product.ProductPrice, EstimateWindow, wholesaleprice);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }

                var windowComponentData = windowProdcutDetailsForEstimates.Where(x => x.WindowProdcutId == product.WindowProductId).FirstOrDefault();
                bool isComponentAvailable = (windowComponentData != null ? windowComponentData.IsComponentAvailable : false) ?? false;
                if (isComponentAvailable)
                {
                    await AddComponentsToEstimate((Guid)windowProductData.ProductId, (Guid)windowProductData.WindowId, estimateId,
                        windowProdcutDetailsForEstimates.Where(x => x.WindowProdcutId == product.WindowProductId).ToList(), EstimateWindowProduct);
                }
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
            }
        }

        private async Task<TblEstimateWindow> AddWindowToEstimate(TblWindow windowData, TblWindowProduct windowProductData, Guid estimateId)
        {
            FileLogger.Log($"Adding window data for window {windowData.WindowId} to estimate...");
            TblEstimateWindow tblEstimateWindow = new TblEstimateWindow
            {
                EstWindowId = Guid.NewGuid(),
                EstimateId = estimateId,
                WindowId = windowData.WindowId,
                IsActive = true,
                IsDelete = false,
                CreatedAt = DateTime.Now,
                CreatedBy = AppConstant.LoggedInUserID,
                TenantId = AppConstant.LoggedInTenantId,
                Label = windowData.Label,
                AddressId = windowData.AddressId,
                ArchHeight = windowData.ArchHeight,
                Height = windowData.Height,
                Width = windowData.Width,
                Quantity = windowData.Quantity,
                Notes = windowData.Notes,
                BuildHeight = windowProductData.BuildHeight,
                BuildWidth = windowProductData.BuildWidth,
                IsRated = windowData.IsRated,
            };

            await estimateWindowService.Add(tblEstimateWindow);
            return tblEstimateWindow;
        }

        private async Task<TblEstimateWindowProduct> AddProductToEstimate(TblWindowProduct windowProductData, Guid estimateId, decimal? ProductPrice, TblEstimateWindow tblEstimateWindow, decimal? wholesale)
        {
            FileLogger.Log($"Adding product data for product {windowProductData.ProductId} to estimate...");
            var productData = (await productService.GetAll()).Where(x => x.ProductId == windowProductData.ProductId && x.IsDelete == false).FirstOrDefault();
            if (productData == null)
            {
                Message = "Product " + AppConstant.NoDataFound;
                return null; // Return early if product data not found
            }
            var ProductTypeName = mstProductType.GetAllQuerable().Where(x => x.ProductId.ToString() == productData.ProductType).Select(x => x.ProductName).FirstOrDefault();
            Guid[] arr = { windowProductData.WindowProductId };
            var buildEstimate = await BuildEstimate(arr, false);

            var MunicipalityTax = (buildEstimate.Data.FirstOrDefault() != null ? buildEstimate.Data.FirstOrDefault().MunicipalityTax : 0.00M);
            var CityTax = (buildEstimate.Data.FirstOrDefault() != null ? buildEstimate.Data.FirstOrDefault().CityTax : 0.00M);
            var Statetax = (buildEstimate.Data.FirstOrDefault() != null ? buildEstimate.Data.FirstOrDefault().StateTax : 0.00M);

            //TblTaxRate taxDetailsInProductId= null;
            bool isTaxExemptionInContact = GetIsTaxExemptByWindowId(windowProductData.WindowId);
            //if(isTaxExemptionInContact)
            //{
            //     taxDetailsInProductId = GetTaxDetailsByProductId(productData.ProductId);
            //}


            TblEstimateWindowProduct tblEstimateWindowProduct = new TblEstimateWindowProduct
            {
                EstWindowId = tblEstimateWindow.EstWindowId,
                WindowId = (Guid)windowProductData.WindowId,
                ProductName = productData.ProductName,
                Height = windowProductData.BuildHeight,
                Width = windowProductData.BuildWidth,
                AdditionalWidth = productData.AdditionalWidth,
                Notes = productData.Notes,
                IsDelete = false,
                IsActive = true,
                Price = (buildEstimate.Data.FirstOrDefault() != null ? buildEstimate.Data.FirstOrDefault().TotalPrice : 0.00M),
                CreatedAt = DateTime.Now,
                CreatedBy = AppConstant.LoggedInUserID,
                ArchHeight = productData.ArchHeight,
                Area = productData.Area,
                Color = productData.Color,
                EstimateId = estimateId,
                Length = productData.Length,
                Markup = productData.Markup,
                ProductType = productData.ProductType,
                TenantId = AppConstant.LoggedInTenantId,
                ProductId = productData.ProductId,
                EstimateWindowProductId = Guid.NewGuid(),
                WindowProductId = windowProductData.WindowProductId,
                ProductTypeName = ProductTypeName,
                ProductPrice = ProductPrice,
                TaxId = productData.TaxId,
                TaxName = (buildEstimate.Data.FirstOrDefault() != null ? buildEstimate.Data.FirstOrDefault().TaxName : "") ?? "",
                Municipality = MunicipalityTax,
                City = CityTax,
                State = Statetax,
                WholeSalePrice = wholesale
            };

            await estimateWindowProductService.Add(tblEstimateWindowProduct);

            // Save OptionTaxDetails 
            var optionTaxDetails = buildEstimate.Data.FirstOrDefault()?.OptionTaxDetails;
            if (optionTaxDetails != null && optionTaxDetails.Any())
            {
                await SaveOptionTaxDetailsToEstimate(
                    tblEstimateWindowProduct.EstimateWindowProductId,
                    windowProductData.WindowProductId,
                    optionTaxDetails
                );
            }
            return tblEstimateWindowProduct;
        }


        private async Task SaveOptionTaxDetailsToEstimate(Guid estimateWindowProductId, Guid windowProductId, List<OptionTaxVM> optionTaxDetails)
        {
            if (optionTaxDetails == null || !optionTaxDetails.Any())
                return;

            List<TblEstimateWindowProductOption> estimateWindowOptions = new List<TblEstimateWindowProductOption>();

            foreach (var option in optionTaxDetails)
            {
                var optionEntity = new TblEstimateWindowProductOption
                {
                    Id = Guid.NewGuid(),
                    WindowProductId = windowProductId,
                    WindowOptionId = option.WindowOptionId,
                    EstimateWindowProductId = estimateWindowProductId,
                    TaxId = option.TaxId != Guid.Empty? option.TaxId:null,
                    CityTax = option.CityTaxAmount??0.0m,
                    StateTax = option.StateTaxAmount??0.0m,
                    MunicipilityTax = option.MunicipalityTaxAmount??0.0m,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TenantId = AppConstant.LoggedInTenantId,
                    CreatedBy = AppConstant.LoggedInUserID,
                    ModifiedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false,
                    OptionName = option.OptionName,
                    TaxName = option.TaxName,
                    OptionTypeid =Guid.TryParse(option.OptionTypeId, out var optionTypeId) && optionTypeId != Guid.Empty ? optionTypeId : null,
                    OptionTypeName = option.OptionTypeName,
                    Optionid = Guid.TryParse(option.OptionId, out var optionId) && optionId != Guid.Empty ? optionId : null,
                    OptionPrice = option.OptionPrice??0.0m,
                };

                estimateWindowOptions.Add(optionEntity);
            }

            dbContext.TblEstimateWindowProductOptions.AddRange(estimateWindowOptions);
            await dbContext.SaveChangesAsync();
        }



        private bool GetIsTaxExemptByWindowId(Guid? windowId)
        {
            var isTaxExempt = (from w in dbContext.TblWindows
                               join j in dbContext.TblJobs on w.JobId equals j.JobId
                               join c in dbContext.TblContacts on j.ContactId equals c.ContactId
                               where w.WindowId == windowId
                                     && w.IsDelete == false
                                     && j.IsDelete == false
                                     && c.IsDelete == false && c.TenantId==AppConstant.LoggedInTenantId
                               select c.IsTaxExemption).FirstOrDefault();

            return isTaxExempt ?? false;
        }


        private TblTaxRate GetTaxDetailsByProductId(Guid productId)
        {
            var productData = dbContext.TblProducts
                .Where(p => p.ProductId == productId && p.IsDelete == false && p.IsActive == true)
                .Select(p => new { p.TaxId, p.IsTaxApplied })
                .FirstOrDefault();

            if (productData == null || !productData.IsTaxApplied == false)
                return null;

            var taxDetails = dbContext.TblTaxRates
                .Where(t => t.TaxRateId == productData.TaxId && t.IsDelete == false && t.IsActive == true)
                .Select(t => new TblTaxRate
                {
                    TaxRateId = t.TaxRateId,
                    TaxName = t.TaxName,
                    Rate = t.Rate,
                    Municipality = t.Municipality,
                    City = t.City,
                    State = t.State,
                })
                .FirstOrDefault();

            return taxDetails;
        }


        private async Task AddComponentsToEstimate(Guid productId, Guid WindowId, Guid estimateId, List<WindowProdcutDetailsForEstimate> windowProdcutDetailsForEstimates, TblEstimateWindowProduct tblEstimateWindowProduct)
        {
            FileLogger.Log($"Adding components for product {productId} to estimate...");

            var productComponentData = (await productComponentService.GetAll()).Where(x => x.ProductId == productId && x.IsDelete == false).ToList();
            if (productComponentData == null || productComponentData.Count == 0)
            {
                //Message = "No Components " + AppConstant.NoDataFound;
                return; // Return early if no components found
            }
            foreach (var component in productComponentData)
            {
                var componentData = (await componentService.GetAll()).Where(x => x.ComponentId == component.ComponentId && x.IsDelete == false).FirstOrDefault();
                if (componentData == null)
                {
                    Message = "Components " + AppConstant.NoDataFound;
                    return; // Return early if component data not found
                }
                //expressionList.Add(Guid.Parse(componentData.PricingExpression ?? ""));
                var componentPrice = windowProdcutDetailsForEstimates.Where(x => x.ComponentId == component.ComponentId && x.WindowId == WindowId).Select(x => x.Price).FirstOrDefault();
                var EstimateWindowProductComponent = await AddComponentToEstimateWindow(componentData, WindowId, (Guid)component.ProductId, estimateId, componentPrice ?? 0.0m, tblEstimateWindowProduct.EstimateWindowProductId);
                await AddComponentMaterialsToEstimate(componentData.ComponentId, estimateId, WindowId, EstimateWindowProductComponent);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
            }
        }

        private async Task<TblEstimateWindowProductComponent> AddComponentToEstimateWindow(TblComponent componentData, Guid WindowId, Guid ProductId, Guid estimateId, decimal Price, Guid EstimateWindowProductId)
        {
            FileLogger.Log($"Adding component {componentData.ComponentName} to estimate...");
            var PricingExpressionId = dbContext.TblProductComponents.Where(x => x.ComponentId ==
            componentData.ComponentId && x.IsDelete == false).Select(x => x.PricingExpression).FirstOrDefault();
            var ExpressionFormulae = dbContext.TblExpressions.Where(x => x.Id.ToString() == PricingExpressionId)
                .Select(x => x.ExpressionFormulae).FirstOrDefault();
            var Wastefactor = dbContext.TblComponentMaterials.Where(x => x.ComponentId ==
            componentData.ComponentId && x.IsDelete == false).Select(x => x.WasteFactor).FirstOrDefault();
            var ProductName = productType.GetAllQuerable().Where(x => x.ProductId.ToString() == componentData.ProductType
            && x.IsDelete == false).Select(x => x.ProductName).FirstOrDefault();
            TblEstimateWindowProductComponent tblEstimateWindowProductComponent = new TblEstimateWindowProductComponent
            {
                EstimateWindowProductId = EstimateWindowProductId,
                EstimateWindowProductComponentId = Guid.NewGuid(),
                ProductId = ProductId,
                ComponentId = componentData.ComponentId,
                EstimateId = estimateId,
                ComponentName = componentData.ComponentName,
                IsDelete = false,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = AppConstant.LoggedInUserID,
                MarkupPercentage = componentData.MarkupPercentage,
                Notes = componentData.Notes,
                ProductType = componentData.ProductType,
                PricingExpression = ExpressionFormulae,
                TenantId = AppConstant.LoggedInTenantId,
                Wastefactor = Wastefactor,
                ProductTypeName = ProductName,
                ComponentPrice = Price,
                WindowId = WindowId,
            };

            await estimateWindowProductComponentService.Add(tblEstimateWindowProductComponent);
            return tblEstimateWindowProductComponent;
        }

        private async Task AddComponentMaterialsToEstimate(Guid componentId, Guid estimateId, Guid WindowId, TblEstimateWindowProductComponent tblEstimateWindowProductComponent)
        {
            FileLogger.Log($"Adding materials for component {componentId} to estimate...");
            var componentMaterialData = (await componentMaterialService.GetAll()).Where(x => x.ComponentId == componentId && x.IsDelete == false).ToList();
            if (componentMaterialData == null || componentMaterialData.Count == 0)
            {
                Message = "No material " + AppConstant.NoDataFound;
                return; // Return early if no materials found
            }

            foreach (var material in componentMaterialData)
            {
                var materialData = (await materialService.GetAll()).Where(x => x.RawMaterialId == material.RawMaterialId && x.IsDelete == false).FirstOrDefault();
                if (materialData == null)
                {
                    Message = "Material " + AppConstant.NoDataFound;
                    return; // Return early if material data not found
                }

                await AddMaterialToEstimate(materialData, (Guid)material.ComponentId, estimateId, WindowId, tblEstimateWindowProductComponent.EstimateWindowProductComponentId);

            }
        }

        private async Task AddMaterialToEstimate(TblRawMaterial materialData, Guid ComponentId, Guid estimateId, Guid WindowId, Guid EstimateWindowProductComponentId)
        {
            FileLogger.Log($"Adding material {materialData.MaterialName} to estimate...");
            TblEstimateWindowProductComponentMaterial tblEstimateWindowProductComponentMaterial = new TblEstimateWindowProductComponentMaterial
            {

                EstimateWindowProductComponentMaterialId = Guid.NewGuid(),
                EstimateWindowProductComponentId = EstimateWindowProductComponentId,
                ComponentId = ComponentId,
                IsDelete = false,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = AppConstant.LoggedInUserID,
                TenantId = AppConstant.LoggedInTenantId,
                EstimateId = estimateId,
                IsCutSheet = materialData.IsCutSheet,
                MaterialName = materialData.MaterialName,
                Notes = materialData.Notes,
                ProductType = materialData.ProductType,
                ReorderLevel = materialData.ReorderLevel,
                Sku = materialData.Sku,
                StockQuantity = materialData.StockQuantity,
                UnitCost = materialData.UnitCost,
                UnitOfMeasurement = materialData.UnitOfMeasurement,
                VendorId = materialData.VendorId,
                RawMaterialId = materialData.RawMaterialId,
                WindowId = WindowId
            };

            await estimateWindowProductComponentMaterialService.Add(tblEstimateWindowProductComponentMaterial);
        }

        public bool CanChangeStatus(Estimation currentStatus, Estimation newStatus)
        {
            if (currentStatus == Estimation.New)
            {
                return newStatus == Estimation.PendingForApproval || newStatus == Estimation.Approved || newStatus == Estimation.Rejected;
            }
            else if (currentStatus == Estimation.PendingForApproval)
            {
                return newStatus == Estimation.Approved || newStatus == Estimation.Rejected;
            }
            else if (currentStatus == Estimation.Approved)
            {
                return newStatus == Estimation.GenerateOrder;
            }
            else if (currentStatus == Estimation.Rejected)
            {
                return newStatus == Estimation.New;
            }
            else if (currentStatus == Estimation.GenerateOrder)
            {
                return false;
            }

            return false;
        }

        private async Task<IResponseVm<string>> EstimationChangeHistory(Guid EstimationId, Estimation Status)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                TblEstimateApproved approved = new TblEstimateApproved
                {
                    EstimationId = EstimationId,
                    CurrentDatetime = DateTime.Now,
                    Ipaddress = GetClientIpAddress(),
                    Status = EnumValue.GetEnumValue(Status),
                };

                await dbContext.TblEstimateApproveds.AddAsync(approved);
                await dbContext.SaveChangesAsync();

                FileLogger.Log($"Estimation change history added for EstimationId: {EstimationId}, Status: {Status}");
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
                FileLogger.Log($"Error adding change history for EstimationId: {EstimationId}. Error: {ex.Message}");
            }
            return responseVm;
        }

        public string GetClientIpAddress()
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return ipAddress;
        }

        private TblEstimate GetEstimationStatusById(Guid EstimationId)
        {
            // Fetch the Estimation status based on the EstimationId, ensuring we check IsDelete and IsActive flags
            return GetAllQuerable()
                    .Where(es => es.EstimateId == EstimationId && es.IsDelete == false && es.IsActive == true)
                    .FirstOrDefault();
        }

        //GenerateEstimationUniqueName
        public async Task<IResponseVm<string>> EstimationUniqueName()
        {
            IResponseVm<string> res = new ResponseVm<string>();
            try
            {

                var EstimationCount = GetAllQuerable().OrderByDescending(p => p.CreatedAt)
                   .Where(c => c.TenantId == AppConstant.LoggedInTenantId && c.CreatedAt.Value.Date == DateTime.Today.Date)
                   .ToList().Count();
                EstimationCount += 1;

                var EstimationCode = GenerateUniqueId.GenerateUniqueIds(ModuleName.Estimation, AppConstant.LoggedInCompanyCode, EstimationCount);
                res.IsSuccess = true;
                res.Data = EstimationCode;
                res.Message = "Estimation Code Created";
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        public async Task<OptionPriceResult> CalculateOptionPrice(
                 string[] optionTypeIds,
                 decimal? quantity,
                 decimal? height,
                 decimal? width,
                 bool? IsManageProduct)
        {
            var result = new OptionPriceResult();

            bool isTenant = await tenantService.AnyAsync(t =>
                t.TenantId == AppConstant.LoggedInTenantId &&
                (t.ParentTenantId == null || string.IsNullOrEmpty(t.ParentTenantId.ToString())) &&
                t.IsActive == true && t.IsDelete == false);

            foreach (var optionTypeId in optionTypeIds)
            {
                var option = optionsTypeService.GetAllQuerable()
                    .FirstOrDefault(x => x.OptionTypeId.ToString() == optionTypeId && x.IsDelete == false && x.IsActive == true);

                if (option == null) continue;

                var optionexp = optionService.GetAllQuerable()
                    .FirstOrDefault(x => x.OptionId == option.OptionId && x.IsActive == true && x.IsDelete == false);

                if (optionexp == null) continue;

                decimal baseCost = 0.0m;

                // Direct cost calculation if no expression exists
                //if (option.Unit == EnumValue.GetEnumValue(OptionUnit.PerUnit))
                //{
                //    baseCost = (decimal)(option.AdditionalCost * quantity);
                //}
                //else if (option.Unit == EnumValue.GetEnumValue(OptionUnit.EachJob))
                //{
                //    baseCost = (decimal)option.AdditionalCost;
                //}

                var model = new DimensionModel
                {
                    BuildHeight = height,
                    BuildWidth = width,
                    Quantity = quantity,
                    IsView = false,
                    Optionprice = option.AdditionalCost,
                    Unit = optionexp.Unit
                };

                var pricingExpression = expressionService.GetAllQuerable()
                    .Where(x => x.Id == optionexp.OptionExpression && x.IsActive == true && x.IsDelete == false)
                    .Select(x => x.ExpressionFormulae)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(pricingExpression))
                {
                    var expression = CalculateExpression(pricingExpression, model);
                    var res = expression.Evaluate();
                    baseCost = Convert.ToDecimal(res);
                }

                decimal OptionWholesale = Math.Round(baseCost, 2);
                decimal OptionMSRP = OptionWholesale;

                // Apply markup
                if (optionexp.Markup > 0)
                {
                    OptionMSRP = Math.Round((decimal)(OptionWholesale * (1 + optionexp.Markup / 100)), 2);
                }

                //if (IsManageProduct == true)
                //{
                result.OptionWholesale[optionTypeId] = OptionWholesale;
                result.OptionMSRP[optionTypeId] = OptionMSRP;

                //}


                // Apply tenant-specific logic
                if (!isTenant)
                {
                    OptionMSRP = ApplySubtenantMarkup(option.OptionTypeId, OptionMSRP);
                }

                result.TotalCost += OptionMSRP;
            }

            return result;
        }



        private decimal ApplySubtenantMarkup(Guid optionTypeId, decimal baseCost)
        {
            var markup = dbContext.TblManagePartnersProductsOptions
                .FirstOrDefault(x => x.OptionTypeId == optionTypeId &&
                                     x.TenantId == AppConstant.LoggedInTenantId &&
                                     x.IsDelete == false &&
                                     x.IsActive == true &&
                                     x.IsAvailable == true);

            if (markup == null)
                return baseCost;

            if (markup.MarkupOptionType == EnumValue.GetEnumValue(PriceType.Percentage))
            {
                return baseCost * (1 + (markup.PartnerSellingPrice ?? 0.0m) / 100);
            }
            else if (markup.MarkupOptionType == EnumValue.GetEnumValue(PriceType.Fixed))
            {
                return baseCost + (markup.PartnerSellingPrice ?? 0.0m);
            }
            //await ApplySubtenantTax(item, productMarkup.TaxId, (decimal)ComponentPriceWithProductMarkup);

            return baseCost;
        }


        private async Task<decimal> ApplySubtenantTax(WindowProdcutDetailsForEstimate item, Guid? taxId, decimal baseAmount)
        {
            decimal taxAmount = 0;

            if (taxId != null)
            {
                var subtenantTax = dbContext.TblTaxRates
                    .Where(x => x.TaxRateId == taxId && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                    .FirstOrDefault();

                if (subtenantTax != null)
                {
                    item.TaxRateId = subtenantTax.TaxRateId;
                    item.TaxName = subtenantTax.TaxName;
                    item.CityTax = subtenantTax.City ?? 0;
                    item.StateTax = subtenantTax.State ?? 0;
                    item.MunicipalityTax = subtenantTax.Municipality ?? 0;
                    item.IsTaxAvailable = true;

                    var taxRate = item.CityTax + item.StateTax + item.MunicipalityTax;
                    taxAmount = (decimal)(baseAmount * taxRate) / 100;
                    //item.TotalTaxAmount = taxAmount; // If needed, you can store it in the item too
                }
            }

            return taxAmount;
        }

    }
}
