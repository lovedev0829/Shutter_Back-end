using System.Data.Entity;
using System.Text.RegularExpressions;
using Antlr.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NCalc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

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
                    && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false)
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

                var estimatewindowproductcomponent = estimateWindowProductComponentService.GetAllQuerable().
                    Where(x => x.EstimateId == EstimateId && x.IsDelete == false && x.IsActive == true).ToList();
                if (estimatewindowproductcomponent != null)
                {
                    foreach (var res in estimatewindowproductcomponent)
                    {
                        res.IsDelete = true;
                        res.IsActive = false;
                        res.UpdatedAt = DateTime.Now;
                        res.ModifiedBy = AppConstant.LoggedInUserID;
                        await estimateWindowProductComponentService.Update(res);
                    }
                }
                var estimate = GetAllQuerable()
                    .Where(c => c.EstimateId == EstimateId
                    && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false)
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

        public async Task<IResponseVm<List<EstimateList>>> GetAll_Estimate(Pagination filter, Guid? TenantId = null)
        {
            IResponseVm<List<EstimateList>> responseVm = new ResponseVm<List<EstimateList>>();
            try
            {
                FileLogger.Log($"Start: Fetching estimates with filter parameters: TenantId = {filter.TenantId}, PageNo = {filter.PageNo}, RecordsPerPage = {filter.RecordPerPage}");

                // Validate Tenant if provided, else use the logged-in tenant
                var estimates = await FetchEstimates(filter,TenantId);
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
                if (model.ProductsList.Any(x => x.ProductPrice < 0))
                {
                    responseVm.Message = "Product Price can't be negative.";
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
                    responseVm.Data = createEstimate?.EstimateId?.ToString() ?? "";
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
                //var a = new NCalc.Expression(formula);
                var validate = CalculateExpression(formula, model);
                //var parser = new Core.Common.Parser(model.CalculateExpression ?? "");
                //var result = parser.ParseExpression();

                //if (!result.IsValid) { 
                //    responseVm.Message = ("Syntax Errors:");
                //    foreach (var error in result.Errors)
                //    responseVm.Message += "--"+ error;
                //    return responseVm; // Return early if there are syntax errors
                //}

                if (validate.HasErrors())
                {
                    string errorMessgae = validate.Error;
                    const string pattern = @"at line \d+:\d+";
                    var matches = Regex.Matches(errorMessgae ?? string.Empty, pattern);
                    foreach (Match match in matches)
                    {
                        errorMessgae = errorMessgae.Replace(match.Value, "");
                    }
                    responseVm.Message = "There is some issue in Expression. " + errorMessgae;
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
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> BuildEstimate(
        Guid[] windowProductIds,
        bool IsBreakdown,
        bool IsMaterialBreakdown = false,
        string EstimateId = "",
        bool IsView = false,
        bool AfterbuildEstimate = false,
        string? DiscountId = null,
        List<ProductListing> products = null)
        {
            var responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();
            FileLogger.Log("Start: Calculate Estimate price...");

            try
            {
                if (windowProductIds == null || windowProductIds.Length == 0)
                {
                    responseVm.Message = "Incorrect data.";
                    return responseVm;
                }

                if (IsView)
                {
                    var data = new List<WindowProdcutDetailsForEstimate>();

                    foreach (var windowProductId in windowProductIds)
                    {
                        var result = await componentPriceService.GetProductDataForView(windowProductId, EstimateId);
                        if (IsBreakdown && IsMaterialBreakdown)
                        {
                            data.AddRange(result.Data.OrderBy(x => x.MaterialName));
                        }
                        else
                        {
                            data.AddRange(result.Data.DistinctBy(x => x.ComponentId).OrderBy(x => x.ComponentName));
                        }
                    }

                    responseVm.IsSuccess = true;
                    responseVm.Data = data;
                    responseVm.TotalRecords = data.Count;
                    return responseVm;
                }

                if (!await ValidateTenantExists())
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var allProductData = new List<WindowProdcutDetailsForEstimate>();

                foreach (var windowProductId in windowProductIds)
                {
                    var result = await componentPriceService.GetProductData(windowProductId);

                    if (!result.IsSuccess || result.Data == null) continue;

                    var data = result.Data;

                    var itemsWithoutComponentPricing = data.Where(x => x.PricingExpression == null).ToList();
                    itemsWithoutComponentPricing.ForEach(x => x.IsMaterialBreakdown = true);

                    var itemsWithoutPricing = data.Where(x => x.ComponentId == null).ToList();
                    itemsWithoutPricing.ForEach(x => x.IsComponentAvailable = false);

                    if(AfterbuildEstimate)
                    {
                        var DiscountValue = dbContext.TblDiscounts.Where(x => x.DiscountId.ToString() == DiscountId && x.IsDelete == false && x.IsActive == true && x.TenantId == claimsProperty.GetTenantId())
                            .FirstOrDefault();
                        data.ForEach(x =>
                        {
                            x.DiscountId = DiscountId;
                            x.DiscountType = DiscountValue.DiscountType;
                            x.DiscountValue = DiscountValue.DiscountType == EnumValue.GetEnumValue(DiscountType.Percentage) ? 
                                DiscountValue.PercentageDiscount : DiscountValue.FixedAmount;

                        });
                    }
                    var wholesale = await CalculateEstimate(data, true, products);
                    var componentPrice = await CalculateEstimate(data, false, products);

                    
                    foreach (var item in data)
                    {
                        item.WholesalePrice = Math.Round(wholesale, 2);
                        item.TotalPrice = componentPrice > 0.0m ? Math.Round(componentPrice, 2) : null;
                        item.ProductPrice = item.TotalPrice;
                        
                    }

                    allProductData.AddRange(data);
                }

                var jobIds = allProductData.Select(x => x.JobId).Distinct().OfType<Guid>().ToList();
                var jobTaxStatus = new Dictionary<Guid, bool>();
                foreach (var jobId in jobIds)
                {
                    jobTaxStatus[jobId] = await IsTaxApplied(jobId);
                }

                // To get the calculated discounted value of all products
                decimal subTotalOfAllProduct = allProductData.DistinctBy(x => x.ComponentId).Sum(x => (x.TotalPrice ?? 0m) + (x.ProductMarkup ?? 0m));
                var disconutDetails = allProductData.Select(x => new { x.DiscountType, x.DiscountValue, x.DiscountId }).Distinct().Where(x => x.DiscountId != null).FirstOrDefault();
                var discountedPercentage = disconutDetails != null ? GetDiscountValue(subTotalOfAllProduct, disconutDetails.DiscountType, disconutDetails.DiscountValue) : 0.0m;
                // Calculate the subtotal of all products by summing ProductPrice and ProductMarkup for each item
                var AllProductTotalTaxAmount = 0.0m;
                foreach (var item in allProductData.DistinctBy(y => y.WindowProdcutId))
                {
                    decimal subTotalOfProduct = (item.TotalPrice ?? 0m) + (item.ProductMarkup ?? 0m);
                    var discountedValue = disconutDetails != null ? ApplyDiscount(subTotalOfProduct, discountedPercentage) : 0.0m;
                    var discountedsubTotalOfProduct = subTotalOfProduct - discountedValue;
                    var isTaxExempt = GetIsTaxExemptByWindowId(item.WindowId);
                    var isJobTaxable = item.JobId != null && jobTaxStatus.ContainsKey((Guid)item.JobId) && jobTaxStatus[(Guid)item.JobId];
                    var productTaxAmount =  item.IsTaxAvailable == true && isTaxExempt
                        ? CalculateTotalTaxAmount(discountedsubTotalOfProduct, item.MunicipalityTax, item.CityTax, item.StateTax)
                        : 0.0m;
                    var OptionTaxAmount = item.OptionTaxDetails.Sum(x => x.TotalTaxAmount);
                    item.TotalTaxAmount = Math.Round((decimal)(productTaxAmount + OptionTaxAmount),2);
                }

                if ((bool)IsMaterialBreakdown && IsBreakdown)
                {
                    responseVm.Data = allProductData
                        //.DistinctBy(x => x.WindowProdcutId)
                        .OrderBy(x => x.MaterialName)
                        .ToList();
                    
                    responseVm.TotalRecords = responseVm.Data.Count;
                }
                else if (!IsBreakdown)
                {
                    responseVm.Data = allProductData
                        .DistinctBy(x => x.WindowProdcutId)
                        .OrderBy(x => x.ComponentName)
                        .ToList();
                    responseVm.TotalRecords = responseVm.Data.Count;
                }
                else
                {
                    responseVm.Data = allProductData
                        .DistinctBy(x => x.ComponentId)
                        .OrderBy(x => x.ComponentName)
                        .ToList();
                    responseVm.TotalRecords = responseVm.Data.Count;
                }

                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred: {ex.Message}\n{ex.StackTrace}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
        private decimal ApplyDiscount(decimal? totalPrice, decimal? discountValue)
        {
            decimal basePrice = totalPrice ?? 0.0m;
            decimal discount = (basePrice * (discountValue ?? 0.0m) / 100);
            return Math.Round(discount,2);
        }
        private decimal GetDiscountValue(decimal? totalPrice, string? discountType, decimal? discountValue)
        {
            decimal basePrice = totalPrice ?? 0.0m;
            decimal discount = discountType == EnumValue.GetEnumValue(DiscountType.Percentage)
                  ? discountValue ?? 0.0m
                  : discountValue < totalPrice ? ((discountValue * 100) / totalPrice) ?? 0.0m : 100;

            return Math.Round(discount, 2);
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

        private void TaxDetailsApplied(WindowProdcutDetailsForEstimate item, TblTaxRate taxDetails)
        {
            if (taxDetails != null)
            {
                item.TaxName = taxDetails.TaxName ?? "";
                item.MunicipalityTax = taxDetails.Municipality ?? 0;
                item.StateTax = taxDetails.State ?? 0;
                item.CityTax = taxDetails.City ?? 0;
            }
            else
            {
                item.TaxName = "";
                item.MunicipalityTaxAmount = 0;
                item.StateTaxAmount = 0;
                item.CityTaxAmount = 0;
                item.TotalTaxAmount = 0;
                item.MunicipalityTax = 0;
                item.StateTax = 0;
                item.CityTax = 0;
            }
        }

        private void TaxAppliedWithMarkup(WindowProdcutDetailsForEstimate item,decimal productPrice, decimal componentOwnMarkup,decimal productOwnMarkup)
        {
            if (GetIsTaxExemptByWindowId(item.WindowId) && item.IsTaxAvailable == true)
            {
                var subtotal = productPrice + componentOwnMarkup + productOwnMarkup;
                item.DiscountValue = item.DiscountType == EnumValue.GetEnumValue(DiscountType.Percentage) 
                    ? (subtotal * (item.DiscountValue ?? 0.0m) / 100) 
                    : item.DiscountValue;
                subtotal = subtotal - (item.DiscountValue ?? 0.0m);
                item.MunicipalityTaxAmount = ((subtotal) * (item.MunicipalityTax ?? 0.0m) / 100);
                item.StateTaxAmount = ((subtotal) * (item.StateTax ?? 0.0m) / 100);
                item.CityTaxAmount = ((subtotal) * (item.CityTax ?? 0.0m) / 100);
                item.TotalTaxAmount = item.MunicipalityTaxAmount + item.StateTaxAmount + item.CityTaxAmount;
            }
           
        }

        private async Task<decimal> CalculateEstimate(List<WindowProdcutDetailsForEstimate> data, bool? iswholesale = false, List<ProductListing> products = null)
        {
            decimal componentSubTotal = 0.0m;
            decimal componentMarkup = 0.0m;
            decimal productMarkup = 0.0m;
            decimal wholesale = 0.0m;

            var isSubtenant = (await tenantService.GetAll())
                .Any(t => (t.TenantId == claimsProperty.GetTenantId() && t.ParentTenantId != null) && t.IsActive == true && t.IsDelete == false);

            var distinctComponents = data.DistinctBy(x => x.ComponentId).ToList();
            foreach (var item in distinctComponents)
            {
                var formula = item.ExpressionFormulae ?? string.Empty;
                decimal basePrice = 0.0m;
                decimal componentOwnMarkup = 0.0m;
                decimal productOwnMarkup = 0.0m;

                //when component have pricing expression
                if (!string.IsNullOrEmpty(formula))
                {
                    var expression = CalculateExpression(formula, item);
                    basePrice = Convert.ToDecimal(expression.Evaluate());

                    if (iswholesale == true)
                    {
                        if (isSubtenant)
                        {
                            var markup = dbContext.TblManagePartnersProducts
                                .FirstOrDefault(x => x.ProductId == item.ProductId && x.TenantId == claimsProperty.GetTenantId() && x.IsAvailable == true);
                            basePrice = markup?.MarkupType switch
                            {
                                "Percentage" => basePrice * (1 + (markup.WholeSalePrice ?? 0) / 100),
                                "Fixed" => basePrice + (markup.WholeSalePrice ?? 0),
                                _ => basePrice
                            };
                        }
                        wholesale += Math.Round(basePrice, 2);
                        continue;
                    }

                    componentOwnMarkup = ((basePrice * item.MarkupPercentage) / 100) ?? 0.0m;
                    basePrice += componentOwnMarkup;
                    //componentMarkup += componentOwnMarkup;

                    //productOwnMarkup = (((basePrice + componentOwnMarkup) * item.Markup) / 100) ?? 0.0m;
                    productOwnMarkup = ((basePrice * item.Markup) / 100) ?? 0.0m;

                    if (isSubtenant)
                    {
                        var partnerMarkup = dbContext.TblManagePartnersProducts
                            .FirstOrDefault(x => x.ProductId == item.ProductId && x.TenantId == claimsProperty.GetTenantId());

                        productOwnMarkup += partnerMarkup?.MarkupType switch
                        {
                            "Percentage" => (basePrice + productOwnMarkup) * (partnerMarkup.PartnerSellingPrice ?? 0) / 100,
                            "Fixed" => (basePrice + productOwnMarkup + (partnerMarkup.PartnerSellingPrice ?? 0)),
                            _ => 0.0m
                        };
                        //productOwnMarkup += partnerMarkup?.MarkupType switch
                        //{
                        //    "Percentage" => (basePrice + componentOwnMarkup + productOwnMarkup) * (partnerMarkup.PartnerSellingPrice ?? 0) / 100,
                        //    "Fixed" => (basePrice + componentOwnMarkup + productOwnMarkup + (partnerMarkup.PartnerSellingPrice ?? 0)),
                        //    _ => 0.0m
                        //};

                        var taxId = partnerMarkup?.TaxId;
                        var taxDetails = dbContext.TblTaxRates
                            .FirstOrDefault(x => x.TaxRateId == taxId && x.TenantId == claimsProperty.GetTenantId());

                        TaxDetailsApplied(item, taxDetails);
                    }

                    productMarkup += productOwnMarkup;
                    item.Price = Math.Round(basePrice, 2);
                    //TaxAppliedWithMarkup(item,item.Price ?? 0.0m, componentOwnMarkup, productOwnMarkup);

                    componentSubTotal += Math.Round(basePrice, 2);
                }
                //when material have pricing expression
                else if (string.IsNullOrEmpty(formula))
                {
                    var distinctComponentMaterials = data
                        .Where(x => x.ComponentId == item.ComponentId && x.RawMaterialId != null)
                        .DistinctBy(x => x.RawMaterialId).ToList();

                    foreach (var materialItem in distinctComponentMaterials)
                    {
                        var materialFormula = materialItem.MaterialExpressionFormulae ?? string.Empty;
                        var materialPrice = 0.0m;

                        //when material dont have any pricing expression
                        if (string.IsNullOrEmpty(materialFormula))
                            materialPrice = Convert.ToDecimal(materialItem.UnitCost);

                        // when material have expression
                        else
                            materialPrice = Convert.ToDecimal(CalculateExpression(materialFormula, materialItem).Evaluate());

                        //apply waste factor on matrial final price after formula calculation or unit price
                        if (materialItem.WasteFactor != null && materialItem.WasteFactor > 0m)
                            materialPrice = (Convert.ToDecimal(materialPrice) * (100 + (materialItem.WasteFactor ?? 0)) / 100);

                        basePrice += materialPrice;
                        materialItem.MaterialPrice = Math.Round(materialPrice, 2);
                    }

                    

                    if (iswholesale == true)
                    {
                        if (isSubtenant)
                        {
                            var markup = dbContext.TblManagePartnersProducts
                                .FirstOrDefault(x => x.ProductId == item.ProductId && x.TenantId == claimsProperty.GetTenantId() && x.IsAvailable == true);
                            basePrice = markup?.MarkupType switch
                            {
                                "Percentage" => basePrice * (1 + (markup.WholeSalePrice ?? 0) / 100),
                                "Fixed" => basePrice + (markup.WholeSalePrice ?? 0),
                                _ => basePrice
                            };
                        }

                        wholesale += Math.Round(basePrice, 2);
                        continue;
                    }

                    componentOwnMarkup = ((basePrice * item.MarkupPercentage) / 100) ?? 0.0m;
                    basePrice += componentOwnMarkup;
                    //componentMarkup += componentOwnMarkup;

                    //productOwnMarkup = (((basePrice + componentOwnMarkup) * item.Markup) / 100) ?? 0.0m;
                    productOwnMarkup = ((basePrice * item.Markup) / 100) ?? 0.0m;
                    if (isSubtenant)
                    {
                        var partnerMarkup = dbContext.TblManagePartnersProducts
                            .FirstOrDefault(x => x.ProductId == item.ProductId && x.TenantId == claimsProperty.GetTenantId() && x.IsAvailable == true);

                        productOwnMarkup += partnerMarkup?.MarkupType switch
                        {
                            "Percentage" => (basePrice + productOwnMarkup) * (partnerMarkup.PartnerSellingPrice ?? 0) / 100,
                            "Fixed" => (basePrice + productOwnMarkup + (partnerMarkup.PartnerSellingPrice ?? 0)),
                            _ => 0.0m
                        };
                        //productOwnMarkup += partnerMarkup?.MarkupType switch
                        //{
                        //    "Percentage" => (basePrice + componentOwnMarkup + productOwnMarkup) * (partnerMarkup.PartnerSellingPrice ?? 0) / 100,
                        //    "Fixed" => (basePrice + componentOwnMarkup + productOwnMarkup + (partnerMarkup.PartnerSellingPrice ?? 0)),
                        //    _ => 0.0m
                        //};

                        var taxId = partnerMarkup?.TaxId;
                        var taxDetails = dbContext.TblTaxRates
                            .FirstOrDefault(x => x.TaxRateId == taxId && x.TenantId == claimsProperty.GetTenantId());

                        TaxDetailsApplied(item, taxDetails);

                        
                    }
                    item.Price = Math.Round(basePrice, 2);
                    productMarkup += productOwnMarkup;
                    componentSubTotal += Math.Round(basePrice, 2);
                }

                //Product price calculation for product without component
                if (item.ComponentId == null || item.ComponentId == Guid.Empty)
                {
                    var productPrice = products?.FirstOrDefault(x => x.WindowProductId == item.WindowProdcutId)?.ProductPrice ?? 0.0m;
                    item.Price = Math.Round(productPrice, 2);
                    productOwnMarkup = (productPrice * item.Markup / 100) ?? 0.0m;

                    if (isSubtenant)
                    {
                        var markup = dbContext.TblManagePartnersProducts
                            .FirstOrDefault(x => x.ProductId == item.ProductId && x.TenantId == claimsProperty.GetTenantId());

                        productOwnMarkup += markup?.MarkupType switch
                        {
                            "Percentage" => (productPrice + productOwnMarkup) * (markup.PartnerSellingPrice ?? 0) / 100,
                            "Fixed" => (productPrice + productOwnMarkup + (markup.PartnerSellingPrice ?? 0)),
                            _ => 0.0m
                        };
                    }

                    productMarkup += productOwnMarkup;
                    componentSubTotal += Math.Round(productPrice, 2);

                   //TaxAppliedWithMarkup(item, productPrice, componentOwnMarkup, productOwnMarkup);

                }
                else
                {
                  // TaxAppliedWithMarkup(item, item.Price ?? 0.0m, 0, productOwnMarkup);
                }
               //TaxAppliedWithMarkup(item, item.Price ?? 0.0m, componentOwnMarkup, productOwnMarkup);


                

                //Option price calculation
                var optionMSRP = await CalculateOptionPrice(item.OptionsTypeIds, item.Quantity ?? 0, item.BuildHeight ?? 0, item.BuildWidth ?? 0, false);
                item.AdditionalCost = optionMSRP.TotalCost;
                item.OptionMarkup = optionMSRP.OptionMarkup;

                var partnerOptionPrice = 0.0m;
                var optionTax = data.FirstOrDefault(x => x.OptionTaxDetails?.Any() == true && x.ProductId == item.ProductId && x.WindowProdcutId == item.WindowProdcutId);
                if (optionTax != null)
                {
                    foreach (var x in optionTax.OptionTaxDetails)
                    {
                        optionMSRP.OptionMSRP.TryGetValue(x.OptionTypeId, out var price);
                        x.OptionPrice = price;
                        if (isSubtenant)
                        {
                            var adjusted = ApplySubtenantMarkup(x.OptionTypeId, x.OptionPrice ?? 0, x.OptionMarkup ?? 0);
                            x.OptionPrice = adjusted;
                            partnerOptionPrice += adjusted;
                        }
                    }
                }

                if (partnerOptionPrice > 0)
                    item.AdditionalCost = partnerOptionPrice;
                //Calculation of tax on OptionPrice with option markup
                if (item.OptionTaxDetails != null)
                {
                    foreach (var x in item.OptionTaxDetails)
                    {
                        if (GetIsTaxExemptByWindowId(item.WindowId))
                        {
                            decimal? optionMarkupAmt = !isSubtenant ? (x.OptionPrice * (x.OptionMarkup ?? 0.0m) / 100) ?? 0.0m : 0.0m;
                            x.MunicipalityTaxAmount = ((x.OptionPrice + optionMarkupAmt) * x.MunicipalityTax / 100);
                            x.StateTaxAmount = ((x.OptionPrice + optionMarkupAmt) * x.StateTax / 100);
                            x.CityTaxAmount = ((x.OptionPrice + optionMarkupAmt) * x.CityTax / 100);
                            x.TotalTaxAmount = x.MunicipalityTaxAmount + x.StateTaxAmount + x.CityTaxAmount;
                        }
                        else
                        {
                            x.MunicipalityTaxAmount = 0;
                            x.StateTaxAmount = 0;
                            x.CityTaxAmount = 0;
                            x.TotalTaxAmount = 0;
                        }
                    }
                }

                var matchingItems = data.Where(x => x.ProductId == item.ProductId).ToList();
                foreach (var match in matchingItems)
                {
                    match.AdditionalCost = item.AdditionalCost;
                    match.ProductMarkup = productMarkup;
                    match.ComponentMarkup = componentMarkup;
                    if (isSubtenant)
                        match.OptionMarkup = 0.0m;
                }
            }

            return iswholesale == true ? wholesale : componentSubTotal;
        }


        public NCalc.Expression CalculateExpression(string formula, dynamic values)
        {
            const string pattern = "{{.*?}}";

            var matches = Regex.Matches(formula ?? string.Empty, pattern);

            foreach (Match match in matches)
            {
                var placeholder = match.Value;
                var property = placeholder.Replace("{{", "").Replace("}}", "");
                object val = GetProperty.GetColumnValue(values, property);

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
                            var material = materialService.GetAllQuerable()
                                .FirstOrDefault(x => x.ProductType.ToString() == info[1] &&
                                                     (x.MaterialName == info[2] || x.RawMaterialId.ToString() == info[2]) &&
                                                     x.IsActive == true &&
                                                     (x.TenantId == claimsProperty.GetTenantId() ||
                                                      x.TenantId.ToString() == claimsProperty.GetParentTenantId()));

                            val = material?.UnitCost;

                            object componentId = GetProperty.GetColumnValue(values, "ComponentId");
                            if (componentId != null && material != null)
                            {
                                if (!string.IsNullOrEmpty(values.PricingExpression))
                                {
                                    var wasteFactor = componentMaterialService.GetAllQuerable()
                                                                        .Where(x => x.RawMaterialId == material.RawMaterialId &&
                                                                                    x.ComponentId.ToString() == componentId.ToString() &&
                                                                                    !x.IsDelete == true &&
                                                                                    (x.TenantId == claimsProperty.GetTenantId() ||
                                                                                     x.TenantId.ToString() == claimsProperty.GetParentTenantId()))
                                                                        .Select(x => x.WasteFactor)
                                                                        .FirstOrDefault();

                                    if (wasteFactor != null)
                                        val = (Convert.ToDecimal(val) * (100 + (wasteFactor ?? 0)) / 100);
                                }
                                //var wasteFactor = componentMaterialService.GetAllQuerable()
                                //    .Where(x => x.RawMaterialId == material.RawMaterialId &&
                                //                x.ComponentId.ToString() == componentId.ToString() &&
                                //                !x.IsDelete == true &&
                                //                (x.TenantId == claimsProperty.GetTenantId() ||
                                //                 x.TenantId.ToString() == claimsProperty.GetParentTenantId()))
                                //    .Select(x => x.WasteFactor)
                                //    .FirstOrDefault();

                            }
                        }
                        else if (info.Length == 4 && info[0] == "Material" && info[3] == "Unit")
                        {
                            var materialUnit = materialService.GetAllQuerable()
                                .Where(x => x.ProductType.ToString() == info[1] &&
                                            (x.MaterialName == info[2] || x.RawMaterialId.ToString() == info[2]) &&
                                            x.IsActive == true &&
                                            (x.TenantId == claimsProperty.GetTenantId() ||
                                             x.TenantId.ToString() == claimsProperty.GetParentTenantId()))
                                .Select(x => x.UnitOfMeasurement)
                                .FirstOrDefault();

                            formula = formula?.Replace(placeholder, $"' {materialUnit} '");
                            continue;
                        }
                    }
                }

                string replacement = property switch
                {
                    "Unit" => $"'{val}'",
                    "OptionPrice" => Convert.ToDecimal(values.Optionprice).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    _ => Convert.ToDecimal(val).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                };

                formula = formula?.Replace(placeholder, " " + replacement + " ");
            }
            //values.CalculateExpression = formula;
            try
            {
                return new NCalc.Expression(formula);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Expression parsing error. Formula: '{formula}', Error: {ex.Message}");
                throw new InvalidOperationException($"Invalid expression formula: '{formula}'. Error: {ex.Message}", ex);
            }
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
                    .Where(c => c.EstimateId == model.EstimateId && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false)
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
                decimal additionalCostAsOptionPrice = 0.0M;
                decimal subtotal = 0.0M;
                decimal grandtotal = 0.0M;
                decimal totalTaxAmount = 0.0m;
                decimal discountedValue = 0.0m;
                Guid? taxRateId = null;
                var PricedProductsList = model.ProductsList.Where(x => x.ProductPrice != null && x.ProductPrice > 0).ToList();
                var windowproductdetails = await BuildEstimate(model.ProductsList.Select(x => x.WindowProductId).ToArray(), false,false, "", false, true,model.DiscountId.ToString(), model.ProductsList);
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
                //var ComponentMarkup = (decimal)windowproductdetails.Data.Where(x => x.ComponentMarkup != null).DistinctBy(x => x.WindowProdcutId).Sum(x => x.ComponentMarkup);
                var ProductMarkup = (decimal)windowproductdetails.Data.Where(x => x.ProductMarkup != null).DistinctBy(x => x.WindowProdcutId).Sum(x => x.ProductMarkup);
                var OptionMarkup = (decimal)windowproductdetails.Data.Where(x => x.OptionMarkup != null).DistinctBy(x => x.WindowProdcutId).Sum(x => x.OptionMarkup);
                additionalCostAsOptionPrice += (decimal)windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).Sum(x => x.AdditionalCost);
                subtotal += (ProductMarkup);
                if (!string.IsNullOrEmpty(model.DiscountId.ToString()))
                {
                    var discountType = windowproductdetails.Data.Select(x => x.DiscountType).FirstOrDefault();
                    var discountValue = (decimal)windowproductdetails.Data.Select(x => x.DiscountValue).FirstOrDefault();
                    var discountPer = GetDiscountValue(subtotal, discountType, discountValue);
                    discountedValue = ApplyDiscount(subtotal, discountPer);
                    discountedValue = subtotal - discountedValue;
                }

                subtotal += (OptionMarkup);
                discountedValue += OptionMarkup;
                subtotal = subtotal + additionalCostAsOptionPrice;
                discountedValue = discountedValue + additionalCostAsOptionPrice;


                bool isTaxExemptionInContact = GetIsTaxExemptByWindowId(windowproductdetails.Data.Select(s => s.WindowId).FirstOrDefault());
                if (model.IsTaxApplicable == true && isTaxExemptionInContact)
                {
                    totalTaxAmount = windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).DistinctBy(x => x.ProductId).Sum(x => x.TotalTaxAmount) ?? 0.0m;
                    grandtotal = (!string.IsNullOrEmpty(model.DiscountId.ToString()) ? discountedValue : subtotal) + totalTaxAmount;
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
                estimate.TotalPrice = subtotal;
                estimate.AdditionalCost = additionalCostAsOptionPrice;
                estimate.TermsCondition = model.TermsCondition;
                estimate.ModifiedBy = AppConstant.LoggedInUserID;
                estimate.IsTaxApplied = model.IsTaxApplicable;
                estimate.TotaltaxAmount = totalTaxAmount;
                estimate.PreparedBy = model.PreparedBy ?? Guid.Empty;
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
                responseVm.Data = estimate?.EstimateId.ToString() ?? "";
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

               

                Pagination filterValue = new Pagination
                {
                    EstimationId = EstimateId.ToString()
                };
                var estimateListFiltered = await GetAll_Estimate(filterValue, estimate.TenantId);

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


        private async Task<List<TblEstimate>> FetchEstimates(Pagination filter, Guid? TenantId = null)
        {
            var isFilteringByEstimationId = !string.IsNullOrEmpty(filter.EstimationId);
            var estimationId = filter.EstimationId;

            IQueryable<TblEstimate> query = GetAllQuerable().Where(x => !x.IsDelete == true);
            // Apply estimation ID filter if provided
            if (isFilteringByEstimationId)
            {
                query = query.Where(x => x.EstimateId.ToString() == estimationId);
            }

            if (!string.IsNullOrEmpty(TenantId.ToString()) && TenantId != Guid.Empty)
            {
                // Default to logged-in tenant
                FileLogger.Log("No TenantId provided, fetching for the logged-in tenant.");
                query = query.Where(x => x.TenantId == TenantId);
            }
            else if (string.IsNullOrEmpty(filter.TenantId))
            {
                // Default to logged-in tenant
                FileLogger.Log("No TenantId provided, fetching for the logged-in tenant.");
                query = query.Where(x => x.TenantId == claimsProperty.GetTenantId());
            }
            else
            {
                // Validate related tenant
                var isRelatedTenant = await tenantService.GetAllQuerable()
                    .AnyAsync(x =>
                        x.TenantId.ToString() == filter.TenantId &&
                        x.ParentTenantId == claimsProperty.GetTenantId() &&
                        !x.IsDelete == true);

                if (!isRelatedTenant)
                {
                    FileLogger.Log($"Tenant with ID {filter.TenantId} is not related or not found.");
                    return null;
                }

                FileLogger.Log($"Tenant with ID {filter.TenantId} validated successfully.");
                query = query.Where(x => x.TenantId.ToString() == filter.TenantId);
            }

            return query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }


        private async Task<List<EstimateList>> ApplyFilters(List<TblEstimate> estimates, Pagination filter)
        {
            var estimateIds = estimates.Select(e => e.EstimateId).ToList();
            var userIds = estimates.Select(e => e.CreatedBy).Distinct().ToList();
            var tenantIds = estimates.Select(e => e.TenantId).Distinct().ToList();

            // Batch load users
            var users = dbContext.TblUsers
                .Where(u => userIds.Contains(u.Id) && !u.IsDelete == true)
                .ToDictionary(u => u.Id);

            // Batch load tenants and branding info
            var tenants = dbContext.TblTenants
                .Where(t => tenantIds.Contains(t.TenantId) && !t.IsDelete == true)
                .ToList();

            var brandingInfoIds = tenants
                .Where(t => t.BrandingInfo.HasValue)
                .Select(t => t.BrandingInfo.Value)
                .Distinct()
                .ToList();

            var brandingInfos = dbContext.TblTenantBrandingInfos
                .Where(b => brandingInfoIds.Contains(b.Id))
                .ToDictionary(b => b.Id);

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
                    GrandTotal = estimate.GrandTotal,
                    Notes = estimate.Notes,
                    Tax = estimate.TotaltaxAmount,
                    TermsCondition = estimate.TermsCondition,
                    IsTaxApplicable = estimate.IsTaxApplied,
                    PreparedBy = estimate.PreparedBy?? estimate.CreatedBy,
                    PreparedByName = dbContext.TblUsers.Where(s => s.Id == (estimate.PreparedBy?? estimate.CreatedBy)).Select(s=>s.FirstName +" "+ s.LastName +" "+ s.Suffix).FirstOrDefault(),
                    PreparedByEmail = dbContext.TblUsers.Where(s => s.Id == (estimate.PreparedBy ?? estimate.CreatedBy)).Select(s => s.Email).FirstOrDefault(),
                    jobDetails = new JobDetails()
                };

                // Set user info
                if (users.TryGetValue(estimate.CreatedBy ?? Guid.Empty, out var user))
                {
                    estimateDetails.CreatedBy = $"{user.FirstName} {user.LastName}";
                    estimateDetails.jobDetails.FirstName = user.FirstName;
                    estimateDetails.jobDetails.LastName = user.LastName;
                }

                // Set tenant branding info
                var tenant = tenants.FirstOrDefault(t => t.TenantId == estimate.TenantId);
                if (tenant != null && tenant.BrandingInfo.HasValue && brandingInfos.TryGetValue(tenant.BrandingInfo.Value, out var branding))
                {
                    estimateDetails.primarycolor = branding.PrimaryColor;
                    estimateDetails.CompanyName = branding.CompanyName;
                }

                // Fetch and add related products and options
                await AddEstimateProducts(estimate, estimateDetails);

                estimateList.Add(estimateDetails);
            }

            // Apply filtering
            var filteredRecords = estimateList.Where(x =>
                (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                (string.IsNullOrWhiteSpace(filter.JobStatus) || string.Equals(x.jobDetails?.JobStatus, filter.JobStatus, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(filter.JobId) || string.Equals(x.JobId?.ToString(), filter.JobId, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(filter.EstimationId) || string.Equals(x.EstimateId.ToString(), filter.EstimationId, StringComparison.OrdinalIgnoreCase)) &&
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
            var TenantId = GetAllQuerable().Where(x => x.EstimateId == e.EstimateId && x.IsDelete == false && x.IsActive == true).Select
                (y => y.TenantId).FirstOrDefault();
            var jobdetails1 = (from j1 in dbContext.TblJobs
                               join j2 in dbContext.TblEstimates on j1.JobId equals j2.JobId
                               join con in dbContext.TblContacts on j1.ContactId equals con.ContactId
                               join j3 in dbContext.TblAddresses on j1.AddressId equals j3.AddressId
                               join j4 in dbContext.MstJobTypes on j1.JobType equals j4.Id into JobType
                               from j4 in JobType.DefaultIfEmpty()
                               join j5 in dbContext.TblUsers on j1.AssignedPersonnel equals j5.Id into Users
                               from j5 in Users.DefaultIfEmpty()
                               join j6 in dbContext.TblContacts on j1.ContactId equals j6.ContactId
                               join j7 in dbContext.MstPowderCoatColors on j1.PowderCoatColorId equals j7.Id into PowderCoatColors
                               from j7 in PowderCoatColors.DefaultIfEmpty()
                               where j1.TenantId == TenantId && j1.IsDelete == false && j1.JobId == e.JobId
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
                                   Address = j3.StreetAddress + " " + j3.City + " " + (dbContext.MstStates.Where(s=>s.Id==j3.State).Select(state=>state.StateName).FirstOrDefault())??"" + " " + j3.ZipCode,
                                   AddressLabel = j3.Label,
                                   ScheduleTime = j1.ScheduledDate,
                                   ContactId = j6.ContactId,
                                   JobIsActive = j1.IsActive,
                                   PowderCoatColor = j7.ColorName
                               }).FirstOrDefault();

            e.jobDetails = jobdetails1;

        }

        private async Task AddEstimateProducts(TblEstimate estimate, EstimateList e)
        {
            try
            {
                var windowProducts = estimateWindowProductService.GetAllQuerable()
                    .Where(x => x.EstimateId == estimate.EstimateId && x.IsDelete == false)
                    .ToList();

                var productIds = windowProducts.Select(x => x.EstimateWindowProductId).ToList();
                var windowIds = windowProducts.Select(x => x.WindowId).Distinct().ToList();

                var allOptions = (
                    from estimateOption in dbContext.TblEstimateWindowProductOptions
                    join optionType in dbContext.TblOptionsTypes on estimateOption.OptionTypeid equals optionType.OptionTypeId
                    join option in dbContext.TblOptions on optionType.OptionId equals option.OptionId
                    where productIds.Contains(estimateOption.EstimateWindowProductId ?? Guid.Empty)
                    select new
                    {
                        estimateOption.EstimateWindowProductId,
                        Option = new OptionTaxVM
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
                            hidePreview = optionType.HidePreview??false,
                            OptionTypeName = estimateOption.OptionName + " - " + estimateOption.OptionTypeName
                        }
                    }).ToList();

                Console.WriteLine(allOptions);
                var windowDetails = estimateWindowService.GetAllQuerable()
                    .Where(x => x.EstimateId == estimate.EstimateId && x.IsDelete == false)
                    .ToList();

                var labelQtyMap = windowService.GetAllQuerable()
                    .Where(w => windowIds.Contains(w.WindowId) && w.IsDelete == false)
                    .ToDictionary(w => w.WindowId);

                var componentFlags = estimateWindowProductComponentService.GetAllQuerable()
                    .Where(x => x.EstimateId == estimate.EstimateId && x.IsDelete == false)
                    .Select(x => new { x.WindowId, x.ProductId })
                    .ToList();

                e.WindowProductId = windowProducts.Select(p => (Guid)p.WindowProductId).Distinct().ToArray();

                foreach (var p in windowProducts)
                {
                    var options = allOptions
                        .Where(o => o.EstimateWindowProductId == p.EstimateWindowProductId)
                        .Select(o => o.Option)
                        .ToList();
                    
                    labelQtyMap.TryGetValue(p.WindowId, out var labelQty);

                    var isComponentAvailable = componentFlags.Any(x =>
                        x.WindowId == p.WindowId && x.ProductId == p.ProductId);

                    var estimateProduct = new EstimateProductList
                    {
                        EstimateWindowProductId = p.EstimateWindowProductId,
                        Label = labelQty?.Label,
                        WindowQty = labelQty?.Quantity ?? 0,
                        ProductType = p.ProductType,
                        Width = p.Width,
                        Height = p.Height,
                        ArchHeight = p.ArchHeight,
                        ProductName = p.ProductName,
                        ProductTypeName = p.ProductTypeName,
                        WindowProductId = p.WindowProductId,
                        TotalPrice = p.Price,
                        additionalCost = options.Sum(x => x.OptionPrice),
                        ProductPrice = p.ProductPrice,
                        Options = options.Select(x => x.OptionTypeName).ToArray(),
                        hidePreviews = options.Select(x => x.hidePreview).ToArray(),
                        IsComponentAvailable = isComponentAvailable,
                        WholesalePrice = p.WholeSalePrice,
                        ProductMarkup = p.ProductMarkup,
                        ComponentMarkup = p.ComponentMarkup,
                        OptionMarkup = p.OptionMarkup,
                        EstimateWindowDetails = windowDetails
                            .Where(w => w.WindowId == p.WindowId)
                            .Select(w => new EstimateWindowList
                            {
                                ArchHeight = w.ArchHeight,
                                Height = w.Height,
                                Width = w.Width
                            }).FirstOrDefault()
                    };

                    e.EstimateProductList.Add(estimateProduct);
                }

                await AddJobDetails(e);
                e.IsTaxApplied = await IsTaxApplied((Guid)e.JobId);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred in AddEstimateProducts: {ex.Message}\n{ex.StackTrace}");
            }
        }


        private async Task<bool> ValidateTenantExists()
        {
            FileLogger.Log($"Checking if tenant with ID {claimsProperty.GetTenantId()} exists...");
            var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
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
                .Where(c => (estimateId != Guid.Empty ? c.EstimateId != estimateId : true) && c.EstimateName == estimateName && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false && c.JobId == jobId)
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
            decimal discountedValue = 0.0m;
            Guid? taxRateId = null;
            Guid? discountId = null;
            bool IsAfterCreateEstimate = false;
            if(!string.IsNullOrEmpty(model.DiscountId.ToString()))
            {
                IsAfterCreateEstimate = true;
            }
            var PricedProductsList = model.ProductsList.Where(x => x.ProductPrice != null && x.ProductPrice > 0).ToList();
            var windowproductdetails = await BuildEstimate(model.ProductsList.Select(x => x.WindowProductId).ToArray(), false, false,"", false, IsAfterCreateEstimate, model.DiscountId.ToString(), model.ProductsList);


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
            //var ComponentMarkup = (decimal)windowproductdetails.Data.Where(x => x.ComponentMarkup != null).DistinctBy(x => x.WindowProdcutId).Sum(x => x.ComponentMarkup);
            var ProductMarkup = (decimal)windowproductdetails.Data.Where(x => x.ProductMarkup != null).DistinctBy(x => x.WindowProdcutId).Sum(x => x.ProductMarkup);
            var OptionMarkup = (decimal)windowproductdetails.Data.Where(x => x.OptionMarkup != null).DistinctBy(x => x.WindowProdcutId).Sum(x => x.OptionMarkup);
            additionalCostAsOptionPrice += (decimal)windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).Sum(x => x.AdditionalCost);
            subtotal += (ProductMarkup);
            if (!string.IsNullOrEmpty(model.DiscountId.ToString()))
            {
                var discountType = windowproductdetails.Data.Select(x => x.DiscountType).FirstOrDefault();
                var discountValue = (decimal)windowproductdetails.Data.Select(x => x.DiscountValue).FirstOrDefault();
                var discountPer = GetDiscountValue(subtotal, discountType, discountValue);
                discountedValue = ApplyDiscount(subtotal, discountPer);
                discountedValue = subtotal - discountedValue;
            }
           
            subtotal += (OptionMarkup);
            discountedValue += OptionMarkup;
            subtotal = subtotal + additionalCostAsOptionPrice;
            discountedValue = discountedValue + additionalCostAsOptionPrice;


            bool isTaxExemptionInContact = GetIsTaxExemptByWindowId(windowproductdetails.Data.Select(s => s.WindowId).FirstOrDefault());
            if (model.IsTaxApplicable == true && isTaxExemptionInContact)
            {
               totalTaxAmount = windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).DistinctBy(x => x.ProductId).Sum(x => x.TotalTaxAmount) ?? 0.0m;
                grandtotal = (!string.IsNullOrEmpty(model.DiscountId.ToString()) ? discountedValue : subtotal) + totalTaxAmount;
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
                TenantId = claimsProperty.GetTenantId(),
                IsActive = true,
                IsDelete = false,
                TotalPrice = subtotal,
                AdditionalCost = additionalCostAsOptionPrice,
                IsTaxApplied = model.IsTaxApplicable,
                TotaltaxAmount = totalTaxAmount,
                PreparedBy = model.PreparedBy ?? Guid.Empty,
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
            decimal taxApplied = 0.0m;
            // Fetch the tax rate based on taxId
            var taxRate1 = taxRate.GetAllQuerable()
                .Where(x => x.TaxRateId == taxId && x.IsDelete == false)
                .Select(x => x.Rate)
                .FirstOrDefault();

            if (taxRate == null)
            {
                return 0m;
            }

            if ((subtotal - discount) > 0)
            {
                // Calculate tax based on subtotal, additional cost, and discount
                taxApplied = (decimal)((subtotal - discount) * taxRate1) / 100;
            }
            else
            {
                taxApplied = 0m;
            }
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
                var ComponentMarkup = windowProdcutDetailsForEstimates.Where(x => x.WindowProdcutId == product.WindowProductId).Select(x => x.ComponentMarkup).FirstOrDefault() ?? 0.0m;
                var ProductMarkup = windowProdcutDetailsForEstimates.Where(x => x.WindowProdcutId == product.WindowProductId).Select(x => x.ProductMarkup).FirstOrDefault() ?? 0.0m;
                var OptionMarkup = windowProdcutDetailsForEstimates.Where(x => x.WindowProdcutId == product.WindowProductId).Select(x => x.OptionMarkup).FirstOrDefault() ?? 0.0m;
                var DiscountId = windowProdcutDetailsForEstimates.Where(x => x.WindowProdcutId == product.WindowProductId).Select(x => x.DiscountId).FirstOrDefault() ?? null;
                var EstimateWindow = await AddWindowToEstimate(windowData, windowProductData, estimateId);
                var EstimateWindowProduct = await AddProductToEstimate(windowProductData, estimateId, product.ProductPrice, EstimateWindow, wholesaleprice
                    , OptionMarkup, ProductMarkup, ComponentMarkup);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }

                var windowComponentData = windowProdcutDetailsForEstimates.Where(x => x.WindowProdcutId == product.WindowProductId).FirstOrDefault();
                bool isComponentAvailable = (windowComponentData != null ? windowComponentData.IsComponentAvailable : false) ?? false;
                if (isComponentAvailable)
                {
                    Guid[] arr = { product.WindowProductId };
                    var windowproductdetail1s = await BuildEstimate(arr, true, false, "", false,false, DiscountId,null);
                    await AddComponentsToEstimate((Guid)windowProductData.ProductId, (Guid)windowProductData.WindowId, estimateId,
                        windowproductdetail1s.Data.Where(x => x.WindowProdcutId == product.WindowProductId).ToList(), EstimateWindowProduct);
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
                TenantId = claimsProperty.GetTenantId(),
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

        private async Task<TblEstimateWindowProduct> AddProductToEstimate(TblWindowProduct windowProductData, Guid estimateId, decimal? ProductPrice, TblEstimateWindow tblEstimateWindow,
            decimal? wholesale, decimal? OptionMarkup, decimal? ProductMarkup, decimal? ComponentMarkup)
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
                TenantId = claimsProperty.GetTenantId(),
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
                WholeSalePrice = wholesale,
                ComponentMarkup = ComponentMarkup,
                ProductMarkup = ProductMarkup,
                OptionMarkup = OptionMarkup,
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
                    TaxId = option.TaxId != Guid.Empty ? option.TaxId : null,
                    CityTax = option.CityTaxAmount ?? 0.0m,
                    StateTax = option.StateTaxAmount ?? 0.0m,
                    MunicipilityTax = option.MunicipalityTaxAmount ?? 0.0m,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = AppConstant.LoggedInUserID,
                    ModifiedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false,
                    OptionName = option.OptionName,
                    TaxName = option.TaxName,
                    OptionTypeid = Guid.TryParse(option.OptionTypeId, out var optionTypeId) && optionTypeId != Guid.Empty ? optionTypeId : null,
                    OptionTypeName = option.OptionTypeName,
                    Optionid = Guid.TryParse(option.OptionId, out var optionId) && optionId != Guid.Empty ? optionId : null,
                    OptionPrice = option.OptionPrice ?? 0.0m,
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
                                     && c.IsDelete == false && c.TenantId == claimsProperty.GetTenantId()
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
                bool IsMaterialBreakdown = (bool)windowProdcutDetailsForEstimates.Where(x => x.ComponentId == component.ComponentId).Select(x => x.IsMaterialBreakdown ?? false).FirstOrDefault();
                var windowproductId = windowProdcutDetailsForEstimates.Select(x => x.WindowProdcutId ?? Guid.Empty).FirstOrDefault();
                var EstimateWindowProductComponent = await AddComponentToEstimateWindow(componentData, WindowId, (Guid)component.ProductId, estimateId, componentPrice ?? 0.0m, tblEstimateWindowProduct.EstimateWindowProductId,(bool)IsMaterialBreakdown);
                await AddComponentMaterialsToEstimate(componentData.ComponentId, estimateId, WindowId, EstimateWindowProductComponent, windowproductId);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
            }
        }

        private async Task<TblEstimateWindowProductComponent> AddComponentToEstimateWindow(TblComponent componentData, Guid WindowId, Guid ProductId, Guid estimateId, decimal Price, Guid EstimateWindowProductId,bool? IsMaterialBreakdown)
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
                TenantId = claimsProperty.GetTenantId(),
                Wastefactor = Wastefactor,
                ProductTypeName = ProductName,
                ComponentPrice = Price,
                WindowId = WindowId,
                IsMaterialBreakdown = IsMaterialBreakdown,
            };

            await estimateWindowProductComponentService.Add(tblEstimateWindowProductComponent);
            return tblEstimateWindowProductComponent;
        }

        private async Task AddComponentMaterialsToEstimate(Guid componentId, Guid estimateId, Guid WindowId, TblEstimateWindowProductComponent tblEstimateWindowProductComponent,Guid? WindowProductId)
        {
            FileLogger.Log($"Adding materials for component {componentId} to estimate...");
            var componentMaterialData = (await componentMaterialService.GetAll()).Where(x => x.ComponentId == componentId && x.IsDelete == false).ToList();
            if (componentMaterialData == null || componentMaterialData.Count == 0)
            {
                Message = "No material " + AppConstant.NoDataFound;
                return; // Return early if no materials found
            }
            Guid[] arr = { WindowProductId ?? Guid.Empty };
            var buildEstimate = await BuildEstimate(arr, true,true);
            foreach (var material in componentMaterialData)
            {
                var materialData = (await materialService.GetAll()).Where(x => x.RawMaterialId == material.RawMaterialId && x.IsDelete == false).FirstOrDefault();
                if (materialData == null)
                {
                    Message = "Material " + AppConstant.NoDataFound;
                    return; // Return early if material data not found
                }
                var materialPrice = buildEstimate.Data.Where(x => x.RawMaterialId == material.RawMaterialId && x.IsMaterialBreakdown == true).Select(y=> y.MaterialPrice ?? 0.0m).FirstOrDefault();
                var MaterialExpressionFormulae = buildEstimate.Data.Where(x => x.RawMaterialId == material.RawMaterialId && x.IsMaterialBreakdown == true).Select(y=> y.MaterialExpressionFormulae ?? "").FirstOrDefault();
                await AddMaterialToEstimate(materialData, (Guid)material.ComponentId, estimateId, WindowId, tblEstimateWindowProductComponent.EstimateWindowProductComponentId, materialPrice,MaterialExpressionFormulae ?? "");

            }
        }

        private async Task AddMaterialToEstimate(TblRawMaterial materialData, Guid ComponentId, Guid estimateId, Guid WindowId, Guid EstimateWindowProductComponentId,
            decimal? materialPrice,string? MaterialExpressionFormulae)
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
                TenantId = claimsProperty.GetTenantId(),
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
                WindowId = WindowId,
                MaterialPrice = materialPrice,
                MaterialPricingExpression = MaterialExpressionFormulae,
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
                   .Where(c => c.TenantId == claimsProperty.GetTenantId() && c.CreatedAt.Value.Date == DateTime.Today.Date)
                   .ToList().Count();
                EstimationCount += 1;

                var EstimationCode = GenerateUniqueId.GenerateUniqueIds(ModuleName.Estimation, claimsProperty.GetCompanyCode(), EstimationCount);
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
                t.TenantId == claimsProperty.GetTenantId() &&
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
                    result.OptionMarkup += Math.Round((decimal)(OptionWholesale * optionexp.Markup / 100), 2);
                }
                //if (IsManageProduct == true)
                //{
                result.OptionWholesale[optionTypeId] = OptionWholesale;
                result.OptionMSRP[optionTypeId] = OptionMSRP;
                //}
                // Apply tenant-specific logic
                //if (!isTenant)
                //{
                //    result.OptionMarkup = ApplySubtenantMarkup(option.OptionTypeId, result.OptionMarkup ?? 0.0m);
                //}
                result.TotalCost += OptionMSRP;
            }
            return result;
        }


        private decimal ApplySubtenantMarkup(string optionTypeId, decimal baseCost, decimal OptionMarkup)
        {
            var markup = dbContext.TblManagePartnersProductsOptions
                .FirstOrDefault(x => x.OptionTypeId.ToString() == optionTypeId &&
                                     x.TenantId == claimsProperty.GetTenantId() &&
                                     x.IsDelete == false &&
                                     x.IsActive == true &&
                                     x.IsAvailable == true);
         
            if (OptionMarkup > 0)
            {
                baseCost = baseCost * (1 + (OptionMarkup) / 100);
            }
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
            //await ApplySubtenantTax(distinctItem, productMarkup.TaxId, (decimal)ComponentPriceWithProductMarkup);
            return baseCost;
        }

        private async Task<decimal> ApplySubtenantTax(WindowProdcutDetailsForEstimate item, Guid? taxId, decimal baseAmount)
        {
            decimal taxAmount = 0;

            if (taxId != null)
            {
                var subtenantTax = dbContext.TblTaxRates
                    .Where(x => x.TaxRateId == taxId && x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId())
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
                    //distinctItem.TotalTaxAmount = taxAmount; // If needed, you can store it in the distinctItem too
                }
            }
            return taxAmount;
        }

        public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> BuildEstimateAfterDiscountApplied(string? DiscountId = null,
        List<ProductListing> products = null)
        {
            IResponseVm<List<WindowProdcutDetailsForEstimate>> response = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();
            try
            {
                var windowproductdetails = await BuildEstimate(products.Select(x => x.WindowProductId).ToArray(), false, false, "", false, true, DiscountId, products);
                if (windowproductdetails.IsSuccess)
                {
                    response.IsSuccess = true;
                    response.Message = windowproductdetails.Message;
                    response.Data = windowproductdetails.Data;
                }
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;

        }

    }
}
