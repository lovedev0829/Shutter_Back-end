using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using static Azure.Core.HttpHeader;


namespace ShutterManagement.Business.Services
{
    public class OrderService : BaseRepository<ShutterDBContext, TblOrder>, IOrderService
    {
        private readonly ITenantService tenantService;
        private readonly IWindowProductService windowProductService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IWindowService windowService;
        private readonly IComponentMaterial componentMaterialService;
        private readonly IOrderWindowService orderWindowService;
        private readonly IMaterial materialService;
        private readonly IProductService productService;
        private readonly IOrderWindowProductService orderWindowProductService;
        private readonly IProductComponentService productComponentService;
        private readonly IComponentService componentService;
        private readonly IOrderWindowProductComponentService orderWindowProductComponentService;
        private readonly IOrderWindowProductComponentMaterialService orderWindowProductComponentMaterialService;
        private readonly IEstimateService estimateService;
        private readonly ShutterDBContext context;
        private readonly IWindowOptionService windowOptionService;
        private readonly IOptionsTypeService optionsTypeService;
        private readonly IMstProductType productType;
        private readonly IJobService jobService;
        private readonly IContactService contactService;
        private readonly IEstimateWindowProductService estimateWindowProduct;
        private readonly ITaxRate taxRate;
        private readonly IRegion region;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly IConverter converter;
        public Pagination filter = new Pagination();
        private string Message = "";
        public OrderService(ITenantService tenantService, IWindowProductService windowProductService, ClaimsProperty claimsProperty,
             IWindowService windowService, IComponentMaterial componentMaterialService,
             IOrderWindowService orderWindowService, IMaterial materialService,
             IProductService productService, IOrderWindowProductService orderWindowProductService,
             IProductComponentService productComponentService, IComponentService componentService,
             IOrderWindowProductComponentService orderWindowProductComponentService,
             IOrderWindowProductComponentMaterialService orderWindowProductComponentMaterialService,
             IEstimateService estimateService, ShutterDBContext context,
             IWindowOptionService windowOptionService, IOptionsTypeService optionsTypeService, IMstProductType productType,
             IJobService jobService, IContactService contactService, IEstimateWindowProductService estimateWindowProduct, ITaxRate taxRate,
            IRegion region,ITenantBrandingService tenantBrandingService,IConverter converter)
        {
            this.tenantService = tenantService;
            this.windowProductService = windowProductService;
            this.claimsProperty = claimsProperty;
            this.windowService = windowService;
            this.componentMaterialService = componentMaterialService;
            this.orderWindowService = orderWindowService;
            this.materialService = materialService;
            this.productService = productService;
            this.orderWindowProductService = orderWindowProductService;
            this.productComponentService = productComponentService;
            this.componentService = componentService;
            this.orderWindowProductComponentService = orderWindowProductComponentService;
            this.orderWindowProductComponentMaterialService = orderWindowProductComponentMaterialService;
            this.estimateService = estimateService;
            this.context = context;
            this.windowOptionService = windowOptionService;
            this.optionsTypeService = optionsTypeService;
            this.productType = productType;
            this.jobService = jobService;
            this.contactService = contactService;
            this.estimateWindowProduct = estimateWindowProduct;
            this.taxRate = taxRate;
            this.region = region;
            this.tenantBrandingService = tenantBrandingService;
            this.converter = converter;
        }


        public async Task<IResponseVm<string>> Create_Order_From_Estimate(Guid EstimateId, bool fromEstimate = false, UpdateOrder updateOrder = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log("Start: Create Order from Estimate...");

            try
            {
                var estimate = estimateService.GetAllQuerable()
                    .Where(e => e.EstimateId == EstimateId && e.IsDelete == false)
                    .FirstOrDefault();
                var version1 = "";
                var status1 = "";
                if (estimate == null)
                {
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }

                int Version = GetAllQuerable().Where(x => x.TenantId == claimsProperty.GetTenantId() && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
                     .Select(p => (p.OrderName.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.OrderName.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                Version += 1;
                var OrderNumber = GenerateUniqueId.GenerateUniqueIds(ModuleName.Order, claimsProperty.GetCompanyCode(), Version);
                var version = GetAllQuerable()
                .Where(x => x.EstimateId == EstimateId && x.IsDelete == false)
                .OrderByDescending(x => x.Version)
                .Select(x => x.Version)
                .FirstOrDefault();
                if (fromEstimate && estimate.Status.Equals(EnumValue.GetEnumValue(Estimation.GenerateOrder)))
                {
                    responseVm.Message = "Order already generated";
                    return responseVm;
                }
                if (estimate.Status.Equals(EnumValue.GetEnumValue(Estimation.Approved)))
                {
                    estimate.Status = EnumValue.GetEnumValue(Estimation.GenerateOrder);
                    await estimateService.Update(estimate);
                }
                if (version != null)
                {
                    if (int.TryParse(version, out int versionNumber))
                    {
                        status1 = EnumValue.GetEnumValue(Estimation.New);
                        version1 = (versionNumber + 1).ToString();
                    }
                    else
                    {
                        version1 = "1";
                    }
                }
                else
                {
                    status1 = EnumValue.GetEnumValue(Estimation.Approved);
                    version1 = "1";
                }
                TblOrder newOrder = new TblOrder();

                if (updateOrder != null && !fromEstimate)
                {
                    decimal subtotal = 0m;
                    decimal additionalCost = 0m;
                    decimal SumofSubtotalandAdditionalCost = 0.0m;
                    decimal grandtotal = 0m;
                    Guid? taxRateId = null;
                    Guid? discountId = null;
                    decimal totalTaxAmount = 0.0m;
                    var windowproductdetails = await estimateService.BuildEstimate(updateOrder.WindowProductId, false,false, updateOrder.EstimateId.ToString(), true);
                    if (windowproductdetails.Data != null && windowproductdetails.Data.Any())
                    {
                        additionalCost = windowproductdetails.Data.ToList().DistinctBy(x => x.AdditionalCost).FirstOrDefault().AdditionalCost ?? 0.0m;
                        subtotal = windowproductdetails.Data.ToList().FirstOrDefault().TotalPrice ?? 0.0m;
                        totalTaxAmount = windowproductdetails.Data.ToList().FirstOrDefault().TotalTaxAmount ?? 0.0m;
                        //SumofSubtotalandAdditionalCost = subtotal + additionalCost;
                    }
                    if (string.IsNullOrEmpty((updateOrder.taxRateId).ToString()))
                    {
                        taxRateId = Guid.Empty;
                    }
                    else
                    {
                        taxRateId = updateOrder.taxRateId;
                    }
                    if (string.IsNullOrEmpty(updateOrder.DiscountId.ToString()) && string.IsNullOrEmpty(updateOrder.DiscountCoupon))
                    {
                        discountId = Guid.Empty;
                    }
                    else
                    {
                        discountId = updateOrder.DiscountId;
                    }
                    grandtotal = await estimateService.CalculateGrandTotal(discountId, subtotal, taxRateId) ?? 0m;

                    bool isTaxExemptionInContact = GetIsTaxExemptByWindowId(windowproductdetails.Data.Select(s => s.WindowId).FirstOrDefault());
                    if (updateOrder.IsTaxApplicable == true && isTaxExemptionInContact)
                    {
                        grandtotal = grandtotal + totalTaxAmount;

                        //grandtotal = grandtotal + windowproductdetails.Data.SelectMany(x => x.OptionTaxDetails).Sum(x => x.TotalTaxAmount) ?? 0.0m;
                    }


                    newOrder.OrderId = Guid.NewGuid();
                    newOrder.EstimateId = (Guid)updateOrder.EstimateId;
                    newOrder.TenantId = estimate.TenantId;
                    newOrder.JobId = updateOrder.JobId;
                    newOrder.EstimateName = updateOrder.EstimateName;
                    newOrder.EstimateCode = updateOrder.EstimateCode;
                    newOrder.Notes = updateOrder.Notes;
                    newOrder.DiscountAmount = updateOrder.Discount;
                    newOrder.DiscountCoupon = updateOrder.DiscountCoupon;
                    newOrder.GrandTotal = grandtotal;
                    newOrder.TaxId = updateOrder.taxRateId;
                    newOrder.TaxAmount = updateOrder.Tax;
                    newOrder.TotalPrice = subtotal;
                    newOrder.AdditionalCost = additionalCost;
                    newOrder.TermsCondition = updateOrder.TermsCondition;
                    newOrder.Status = status1;
                    newOrder.CreatedAt = DateTime.Now;
                    newOrder.UpdatedAt = DateTime.Now;
                    newOrder.Version = version1;
                    newOrder.OrderName = OrderNumber;
                    newOrder.IsActive = true;
                    newOrder.IsDelete = false;
                    newOrder.IsTaxApplied = updateOrder.IsTaxApplicable;
                    newOrder.TotaltaxAmount = totalTaxAmount;
                    newOrder.CreatedBy = AppConstant.LoggedInUserID;
                    newOrder.ModifiedBy = AppConstant.LoggedInUserID;
                }
                else
                {
                    newOrder.OrderId = Guid.NewGuid();
                    newOrder.EstimateId = estimate.EstimateId;
                    newOrder.TenantId = estimate.TenantId;
                    newOrder.JobId = estimate.JobId;
                    newOrder.EstimateName = estimate.EstimateName;
                    newOrder.EstimateCode = estimate.EstimateCode;
                    newOrder.Notes = estimate.Notes;
                    newOrder.DiscountAmount = estimate.DiscountAmount;
                    newOrder.DiscountCoupon = estimate.DiscountCoupon;
                    newOrder.GrandTotal = estimate.GrandTotal;
                    newOrder.TaxId = estimate.TaxId;
                    newOrder.TaxAmount = estimate.TaxAmount;
                    newOrder.TotalPrice = estimate.TotalPrice;
                    newOrder.AdditionalCost = estimate.AdditionalCost;
                    newOrder.TermsCondition = estimate.TermsCondition;
                    newOrder.Status = status1;
                    newOrder.CreatedAt = DateTime.Now;
                    newOrder.UpdatedAt = DateTime.Now;
                    newOrder.Version = version1;
                    newOrder.OrderName = OrderNumber;
                    newOrder.IsActive = true;
                    newOrder.IsDelete = false;
                    newOrder.IsTaxApplied = estimate.IsTaxApplied;
                    newOrder.TotaltaxAmount = estimate.TotaltaxAmount;
                    newOrder.CreatedBy = AppConstant.LoggedInUserID;
                    newOrder.ModifiedBy = AppConstant.LoggedInUserID;
                };

                await Add(newOrder);
                var windowProductIds = dbContext.TblEstimateWindowProducts
                .Where(e => e.EstimateId == EstimateId)
                .Select(x => new ProductListing
                {
                    WindowProductId = (Guid)x.WindowProductId,
                    ProductPrice =(decimal)x.ProductPrice,
                })
                .ToList();



               // var windowProductIdsNonNullable = windowProductIds.Where(x => x.HasValue).Select(x => x.Value).ToList();

                await ProcessWindowProducts(windowProductIds, estimate.EstimateId, newOrder.OrderId, newOrder.Version);

                if (!string.IsNullOrEmpty(Message))
                {
                    responseVm.Message = Message;
                    return responseVm;
                }

                responseVm.IsSuccess = true;
                responseVm.Data = newOrder.OrderId.ToString();
                responseVm.Message = AppConstant.InsertSuccessMsg;
                FileLogger.Log("Order created successfully from estimate.");

            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred while creating order: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<OrderList>>> GetAll_Order(Pagination filter, Guid? TenantId = null)
        {
            IResponseVm<List<OrderList>> responseVm = new ResponseVm<List<OrderList>>();
            try
            {
                FileLogger.Log($"Start: Fetching estimates with filter parameters: TenantId = {filter.TenantId}, PageNo = {filter.PageNo}, RecordsPerPage = {filter.RecordPerPage}");

                var orders = await FetchOrders(filter,TenantId);
                if (orders == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                FileLogger.Log($"Fetched {orders.Count} estimates.");

                var filteredEstimates = ApplyFilters(orders, filter);

                var paginatedEstimates = Paginate.Pagination(filteredEstimates, filter);

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

        public async Task<IResponseVm<List<OrderList>>> GetLatestVersionOrders(Pagination filter)
        {
            IResponseVm<List<OrderList>> responseVm = new ResponseVm<List<OrderList>>();
            try
            {
                FileLogger.Log($"Start: Fetching latest version of orders with filter parameters: TenantId = {filter.TenantId}, PageNo = {filter.PageNo}, RecordsPerPage = {filter.RecordPerPage}");

                var TenantId = claimsProperty.GetTenantId();
                var orders = await FetchOrders(filter, TenantId);
                if (orders == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                FileLogger.Log($"Fetched {orders.Count} orders.");

                // Group orders by EstimateId and get the latest version for each group
                var latestVersionOrders = orders
                    .GroupBy(o => o.EstimateId)
                    .Select(g => g.OrderByDescending(o => o.Version)  // Order by Version to get the latest
                                .FirstOrDefault())  // Get the latest order in each group
                    .ToList();

                // Apply filters (like Status, SearchValue, etc.)
                var filteredOrders = ApplyFilters(latestVersionOrders, filter);

                // Apply pagination
                var paginatedOrders = Paginate.Pagination(filteredOrders, filter);

                var queryable = paginatedOrders.AsQueryable();

                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                responseVm.IsSuccess = true;
                if (paginatedOrders.Any())
                {
                    responseVm.Data = paginatedOrders;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = filteredOrders.Count;
                    FileLogger.Log($"Successfully fetched {paginatedOrders.Count} orders.");
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    FileLogger.Log("No data found after applying filters.");
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred while fetching orders: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_Order(UpdateOrder model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log("Start: Update Order...");

            try
            {
                var isTenantExists = await ValidateTenantExists();
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var isEstimationOrder = (await GetAll()).Where(c => c.OrderId == model.OrderId && c.TenantId == claimsProperty.GetTenantId()
                && c.IsDelete == false).ToList();
                if (isEstimationOrder != null)
                {
                    var latestRecord = isEstimationOrder
                      .OrderByDescending(c =>
                            int.TryParse(c.Version, out int versionNumber) ? versionNumber : 0)
                      .FirstOrDefault();
                    var latestId = latestRecord?.OrderId;
                    if (latestId == model.OrderId)
                    {
                        var orderExist = GetAllQuerable().Where(x => x.OrderId.ToString().ToLower() == model.OrderId.ToString().ToLower() && x.TenantId.ToString().ToLower() == claimsProperty.GetTenantId().ToString().ToLower() &&
                        x.IsDelete == false && x.Status != EnumValue.GetEnumValue(Estimation.Approved)).FirstOrDefault();
                        if (orderExist != null)
                        {
                            decimal subtotal = 0.0M;
                            decimal additionalCost = 0.0M;
                            decimal SumofSubtotalandAdditionalCost = 0.0M;
                            decimal grandtotal = 0.0M;
                            Guid? taxRateId = null;
                            Guid? discountId = null;
                            decimal totalTaxAmount = 0.0m;
                            var windowproductdetails = await estimateService.BuildEstimate(model.WindowProductId, false,false, model.EstimateId.ToString(), true);
                            if (windowproductdetails.Data != null && windowproductdetails.Data.Any())
                            {
                                additionalCost = windowproductdetails.Data.ToList().DistinctBy(x => x.AdditionalCost).FirstOrDefault().AdditionalCost??0.0m;
                                var ProductMarkup = windowproductdetails.Data.ToList().DistinctBy(x => x.ProductId).FirstOrDefault().ProductMarkup ?? 0.0m;
                                var ComponentMarkup = windowproductdetails.Data.ToList().DistinctBy(x => x.ProductId).FirstOrDefault().ComponentMarkup ??0.0m;
                                var OptionMarkup = windowproductdetails.Data.ToList().DistinctBy(x => x.ProductId).FirstOrDefault().OptionMarkup ??0.0m;
                                subtotal = windowproductdetails.Data.Sum(x => x.Price)??0.0m;
                                subtotal = (ProductMarkup + ComponentMarkup + OptionMarkup);
                                SumofSubtotalandAdditionalCost = subtotal + additionalCost;
                                totalTaxAmount += windowproductdetails.Data.DistinctBy(x => x.WindowProdcutId).Sum(x => x.TotalTaxAmount)??0.0m;
                            }
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
                            grandtotal = await estimateService.CalculateGrandTotal(discountId, SumofSubtotalandAdditionalCost, taxRateId ?? Guid.Empty) ?? 0m;

                            bool isTaxExemptionInContact = GetIsTaxExemptByWindowId(windowproductdetails.Data.Select(s => s.WindowId).FirstOrDefault());
                            if (model.IsTaxApplicable == true && isTaxExemptionInContact)
                            {
                                grandtotal = grandtotal + totalTaxAmount;

                                grandtotal = grandtotal + windowproductdetails.Data.SelectMany(x => x.OptionTaxDetails).Sum(x => x.TotalTaxAmount ?? 0.0m);
                            }

                            orderExist.Notes = model.Notes;
                            orderExist.DiscountAmount = model.Discount;
                            orderExist.DiscountCoupon = model.DiscountCoupon;
                            orderExist.GrandTotal = grandtotal;
                            orderExist.JobId = model.JobId;
                            orderExist.TaxId = model.taxRateId!=Guid.Empty? model.taxRateId:null;
                            orderExist.TaxAmount = model.Tax;
                            orderExist.TotalPrice = subtotal;
                            orderExist.AdditionalCost = additionalCost;
                            orderExist.TermsCondition = model.TermsCondition;
                            orderExist.ModifiedBy = AppConstant.LoggedInUserID;
                            orderExist.IsTaxApplied = model.IsTaxApplicable;
                            orderExist.TotaltaxAmount = totalTaxAmount;
                            orderExist.UpdatedAt = DateTime.Now;
                            await Update(orderExist);
                            responseVm.Message = AppConstant.UpdateSuccessMsg;
                            responseVm.IsSuccess = true;

                        }
                        else
                        {
                            var res = await SetOrderdetailsValues(model);
                            responseVm = res;
                            return responseVm;
                        }
                    }
                }
                else
                {
                    var res = await SetOrderdetailsValues(model);
                    responseVm = res;
                    return responseVm;
                }

            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred: {ex.Message}");
                responseVm.Message = "Error updating order: " + ex.Message;
            }

            return responseVm;
        }

        private bool GetIsTaxExemptByWindowId(Guid? windowId)
        {
            var isTaxExempt = (from w in dbContext.TblWindows
                               join j in dbContext.TblJobs on w.JobId equals j.JobId
                               join c in dbContext.TblContacts on j.ContactId equals c.ContactId
                               where w.WindowId == windowId
                                     && w.IsDelete == false
                                     && j.IsDelete == false
                                     && c.IsDelete == false
                               select c.IsTaxExemption).FirstOrDefault();

            return isTaxExempt ?? false;
        }

        public async Task<IResponseVm<string>> ChangeStatus(Guid OrderId, Estimation NewOrderStatus)
        {
            FileLogger.Log($"Processing status change for EstimationId: {OrderId}...");
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var OrderStatus = GetOrderStatusById(OrderId);

                if (OrderStatus == null)
                {
                    responseVm.Message = "Estimation not found.";
                    FileLogger.Log($"Estimation with ID {OrderId} not found.");
                    return responseVm;
                }

                // Safely parse the current status to Estimation enum
                if (!Enum.TryParse(OrderStatus.Status, out Estimation currentStatus))
                {
                    responseVm.Message = "Invalid status found in the system.";
                    FileLogger.Log($"Estimation with ID {OrderId} has an invalid status.");
                    return responseVm;
                }

                // Check if status change is allowed
                if (CanChangeStatus(currentStatus, NewOrderStatus))
                {
                    OrderStatus.Status = EnumValue.GetEnumValue(NewOrderStatus);
                    await Update(OrderStatus);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.Data = EnumValue.GetEnumValue(NewOrderStatus);

                }
                else
                {
                    responseVm.Message = $"You can't change status from {currentStatus} to {NewOrderStatus}.";
                    responseVm.Data = currentStatus.ToString();
                    FileLogger.Log($"Status change from {currentStatus} to {NewOrderStatus} is not allowed.");
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
                FileLogger.Log($"Error processing status change for EstimationId: {OrderId}. Error: {ex.Message}");
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> ActiveInActiveOrder(Guid OrderId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                // Log the start of the operation
                FileLogger.Log($"Start: Toggling Active/Inactive status for Estimate with ID: {OrderId}");

                var order = GetAllQuerable()
                    .Where(c => c.OrderId == OrderId
                    && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false)
                    .FirstOrDefault();

                if (order == null)
                {
                    // Log failure when estimate is not found
                    FileLogger.Log($"Estimate with ID {OrderId} not found.");
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }

                // Toggle the active status of the estimate
                order.IsActive = !order.IsActive;
                order.UpdatedAt = DateTime.Now;
                order.ModifiedBy = AppConstant.LoggedInUserID;

                // Log the status change
                FileLogger.Log($"Estimate with ID {OrderId} status changed to: {(order.IsActive ?? false ? "Active" : "Inactive")}.");

                await Update(order);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

                // Log the successful completion
                FileLogger.Log($"Estimate with ID {OrderId} status update completed successfully.");
            }
            catch (Exception ex)
            {
                // Log exception if an error occurs
                FileLogger.Log($"Error occurred while updating Estimate with ID {OrderId}: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteOrder(Guid OrderId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                // Log the start of the operation
                FileLogger.Log($"Start: Toggling Active/Inactive status for Estimate with ID: {OrderId}");

                var order = GetAllQuerable()
                    .Where(c => c.OrderId == OrderId
                    && c.TenantId == claimsProperty.GetTenantId() && c.IsDelete == false)
                    .FirstOrDefault();

                if (order == null)
                {
                    // Log failure when estimate is not found
                    FileLogger.Log($"Estimate with ID {OrderId} not found.");
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }

                // Toggle the active status of the estimate
                order.IsActive = false;
                order.IsDelete = true;
                order.UpdatedAt = DateTime.Now;
                order.ModifiedBy = AppConstant.LoggedInUserID;

                // Log the status change
                FileLogger.Log($"Estimate with ID {OrderId} status changed to: {(order.IsActive ?? false ? "Active" : "Inactive")}.");

                await Update(order);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

                // Log the successful completion
                FileLogger.Log($"Estimate with ID {OrderId} status update completed successfully.");
            }
            catch (Exception ex)
            {
                // Log exception if an error occurs
                FileLogger.Log($"Error occurred while updating Estimate with ID {OrderId}: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> SendForApproval(Guid OrderId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {


                var IsOrderExists = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId() &&
                x.OrderId == OrderId && x.Status != EnumValue.GetEnumValue(Estimation.Approved)).
                    OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                if (IsOrderExists == null)
                {
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }
                if (IsOrderExists.Status == EnumValue.GetEnumValue(Estimation.Approved))
                {
                    Create_Order_From_Estimate(IsOrderExists.EstimateId);
                    return responseVm;
                }

                IsOrderExists.Status = EnumValue.GetEnumValue(Estimation.PendingForApproval);
                IsOrderExists.UpdatedAt = DateTime.Now;
                IsOrderExists.ModifiedBy = AppConstant.LoggedInUserID;
                await Update(IsOrderExists);

                var jobData = jobService.GetAllQuerable().Where(x => x.JobId == IsOrderExists.JobId && x.IsDelete == false).FirstOrDefault();
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
                        //emailHelper.SendMail(sendEmail);
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

        public async Task<IResponseVm<dynamic>> ViewHistory(Guid EstimateId)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();

            try
            {
                Pagination pagination = new Pagination
                {
                    SearchValue = EstimateId.ToString()
                };

                var allVersionsOrder = await GetAll_Order(pagination);

                if (allVersionsOrder != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Data = allVersionsOrder.Data;
                    responseVm.Message = "History fetched successfully.";
                    responseVm.TotalRecords = allVersionsOrder.Data.Count;
                }
                else
                {
                    responseVm.Message = "No history found for the provided EstimateId.";
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error fetching history for EstimateId: {EstimateId}. Error: {ex.Message}");
                responseVm.Message = "An error occurred while fetching the history. Please try again later.";
            }

            return responseVm;
        }

        public async Task<IResponseVm<dynamic>> ViewOrder(Guid OrderId)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();

            try
            {
                var order = GetAllQuerable()
                                    .Where(e => e.OrderId == OrderId && e.IsDelete == false)
                                    .FirstOrDefault();

                Pagination pagination = new Pagination
                {
                    SearchValue = OrderId.ToString()
                };
                var allVersionsOrder = await GetAll_Order(pagination, order.TenantId);

                if (allVersionsOrder != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Data = allVersionsOrder.Data;
                    responseVm.Message = "History fetched successfully.";
                    responseVm.TotalRecords = allVersionsOrder.Data.Count;
                }
                else
                {
                    responseVm.Message = "No history found for the provided EstimateId.";
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error fetching history for EstimateId: {OrderId}. Error: {ex.Message}");
                responseVm.Message = "An error occurred while fetching the history. Please try again later.";
            }

            return responseVm;
        }

        #region Helper methods
        private bool CanChangeStatus(Estimation currentStatus, Estimation newStatus)
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
                return false;
            }
            else if (currentStatus == Estimation.Rejected)
            {
                return newStatus == Estimation.New;
            }

            return false;
        }

        private async Task<IResponseVm<string>> SetOrderdetailsValues(UpdateOrder model)
        {
            var res = await Create_Order_From_Estimate((Guid)model.EstimateId, false, model);
            return res;
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

        private TblOrder GetOrderStatusById(Guid OrderId)
        {
            return GetAllQuerable()
                    .Where(es => es.OrderId == OrderId && es.IsDelete == false && es.IsActive == true)
                    .FirstOrDefault();
        }
        public async Task<List<TblOrder>> FetchOrders(Pagination filter, Guid? TenantId = null)
        {
            if (!string.IsNullOrEmpty(TenantId.ToString()) && TenantId != Guid.Empty)
            {
                // Log and fetch estimates for the logged-in tenant
                FileLogger.Log("No TenantId provided, fetching for the logged-in tenant.");
                return (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == TenantId)
                                       .OrderByDescending(x => x.CreatedAt)
                                       .ToList();
            }
            if (string.IsNullOrEmpty(filter.TenantId))
            {
                // Log and fetch estimates for the logged-in tenant
                FileLogger.Log("No TenantId provided, fetching for the logged-in tenant.");
                return (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId())
                                       .OrderByDescending(x => x.CreatedAt)
                                       .ToList();
            }

            // Validate TenantId if provided
            var isRelatedTenant = tenantService.GetAllQuerable()
                .Where(x => x.TenantId.ToString() == filter.TenantId && x.ParentTenantId == claimsProperty.GetTenantId() && x.IsDelete == false)
                .FirstOrDefault();

            if (isRelatedTenant == null)
            {
                FileLogger.Log($"Tenant with ID {filter.TenantId} is not related or not found.");
                return null; // Return null to indicate tenant validation failure
            }
            
            // Fetch estimates for the provided tenant
            FileLogger.Log($"Tenant with ID {filter.TenantId} validated successfully.");
            return (await GetAll()).Where(x => x.IsDelete == false && x.TenantId.ToString() == filter.TenantId)
                                   .OrderByDescending(x => x.CreatedAt)
                                   .ToList();
        }

        private List<OrderList> ApplyFilters(List<TblOrder> orders, Pagination filter)
        {
            var orderlist = new List<OrderList>();

            foreach (var order in orders)
            {
                var e = new OrderList
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderName,
                    Version = order.Version,
                    EstimateId = order.EstimateId,
                    CreatedDate = order.CreatedAt,
                    JobId = order.JobId,
                    taxRateId = order.TaxId,
                    Status = order.Status,
                    IsActive = order.IsActive,
                    Discount = order.DiscountAmount,
                    DiscountCoupon = order.DiscountCoupon,
                    EstimateName = order.EstimateName,
                    EstimateCode = order.EstimateCode,
                    SubTotal = order.TotalPrice,
                    additionalCost = order.AdditionalCost,
                    GrandTotal = order.GrandTotal,
                    Notes = order.Notes,
                    Tax = order.TaxAmount,
                    TotaltaxAmount = order.TotaltaxAmount,
                    IsTaxApplicable = order.IsTaxApplied,
                    TermsCondition = order.TermsCondition,
                };
                var jobDetails = jobService.GetAllQuerable().Where(x => x.JobId == order.JobId).Select(x => x.ContactId).FirstOrDefault();

                var contactDetails = contactService.GetAllQuerable().Where(x => x.ContactId == jobDetails).FirstOrDefault();
                e.FirstContactName = (contactDetails?.FirstName ?? "");
                e.LastContactName = (contactDetails?.LastName ?? "");
                e.Suffix = (contactDetails?.Suffix ?? "");
                e.ContactId = (contactDetails?.ContactId);

                var userdetails = dbContext.TblUsers
                    .Where(x => x.Id == order.CreatedBy && x.IsDelete == false)
                    .FirstOrDefault();
                if (userdetails != null)
                {
                    e.CreatedBy = userdetails.FirstName + " " + userdetails.LastName;
                }
                else
                {
                    e.CreatedBy = "User not found";
                }

                var Tenant = dbContext.TblTenants
                    .Where(e => e.TenantId == order.TenantId && e.IsDelete == false)
                    .FirstOrDefault();
                var tenantdetails = dbContext.TblTenantBrandingInfos
                 .Where(tb => tb.Id == Tenant.BrandingInfo)
                    .FirstOrDefault();
                e.primarycolor = tenantdetails.PrimaryColor;
                e.CompanyName = tenantdetails.CompanyName;
                var taxdetails = taxRate.GetAllQuerable()
                   .Where(x => x.TaxRateId == order.TaxId && x.IsDelete == false)
                   .FirstOrDefault();

                if (taxdetails != null)
                {
                    var region1 = region.GetAllQuerable()
                        .Where(x => x.Id == taxdetails.Region && x.IsDelete == false)
                        .Select(pt => pt.RegionName)
                        .FirstOrDefault();

                    e.TaxRate = taxdetails.Rate;
                    e.Region = region1 ?? "";
                }
                else
                {
                    e.TaxRate = 0;
                    e.Region = "";
                }
                AddEstimateProducts(order, e);
                orderlist.Add(e);
            }

            var filteredRecords = orderlist
                .Where(x =>
                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                    (string.IsNullOrEmpty(filter.JobStatus.ToLower()) ? true : x.jobDetails?.JobStatusId.ToLower() == filter.JobStatus.ToLower()) &&
                    (string.IsNullOrEmpty(filter.EstimationId.ToLower()) ? true : x.EstimateId.ToString().ToLower() == filter.EstimationId.ToLower()) &&
                    (string.IsNullOrEmpty(filter.CustomerId.ToLower()) ? true : x.jobDetails?.ContactId.ToString().ToLower() == filter.CustomerId.ToLower()) &&
                    (string.IsNullOrWhiteSpace(filter.SearchValue) ||
                     ((x.Notes ?? "").Contains(filter.SearchValue)
                      || (x.JobId.ToString() ?? "").Contains(filter.SearchValue)
                      || (x.FirstContactName?.ToLower() ?? "").Contains(filter.SearchValue.Trim().ToLower())
                      || ((x.FirstContactName?.ToLower() ?? "") + " " + (x.LastContactName?.ToLower() ?? "")).Contains(filter.SearchValue.Trim().ToLower())
                      || (x.LastContactName?.ToLower() ?? "").Contains(filter.SearchValue.Trim().ToLower())
                      || (x.OrderNumber ?? "").Contains(filter.SearchValue)
                      || (x.OrderId.ToString() ?? "").Contains(filter.SearchValue)
                      || (x.Status ?? "").Contains(filter.SearchValue)
                      || (x.EstimateId.ToString()).Contains(filter.SearchValue)
                      || (x.ContactId?.ToString()).Contains(filter.SearchValue)
                      || (x.EstimateCode ?? "").Contains(filter.SearchValue)
                      || (x.EstimateName ?? "").ToLower().Contains(filter.SearchValue)
                      || (x.TermsCondition ?? "").Contains(filter.SearchValue)
                      || (x.DiscountCoupon ?? "").Contains(filter.SearchValue)
                      || (x.GrandTotal.ToString() ?? "").Contains(filter.SearchValue)))
                ).ToList();

            return filteredRecords;
        }

        private void AddJobDetails(OrderList e)
        {
            var jobdetails1 = (from j1 in dbContext.TblJobs
                               join j2 in dbContext.TblOrders on j1.JobId equals j2.JobId into order
                               from j2 in order.DefaultIfEmpty()
                               join j3 in dbContext.TblAddresses on j1.AddressId equals j3.AddressId into address
                               from j3 in address.DefaultIfEmpty()
                               join j4 in dbContext.MstJobTypes on j1.JobType equals j4.Id into JobType
                               from j4 in JobType.DefaultIfEmpty()
                               join j5 in dbContext.TblUsers on j1.AssignedPersonnel equals j5.Id into Users
                               from j5 in Users.DefaultIfEmpty()
                               join c in dbContext.TblContacts on j1.ContactId equals c.ContactId into contact
                               from c in contact.DefaultIfEmpty()
                               join j6 in dbContext.MstJobStatuses on j1.JobStatus equals j6.JobStatusId into JobStatus
                               from j6 in JobStatus.DefaultIfEmpty()
                               where j1.IsDelete == false && c.IsDelete == false && j1.TenantId == claimsProperty.GetTenantId() &&
                               j1.JobId == e.JobId && j1.IsActive == true && j1.IsDelete == false
                               select new JobDetail
                               {
                                   JobName = string.IsNullOrWhiteSpace(j1.JobName) ? j1.JobName : "",
                                   JobNotes = string.IsNullOrWhiteSpace(j1.Notes) ? j1.Notes : "",
                                   JobType = j1.JobType != null ? j4.JobTypeName : null,
                                   AssignedPersonnel = j5 != null ? (j5.FirstName + " " + j5.LastName) : "",
                                   Address = j1.AddressId != null || j1.AddressId != Guid.Empty ? (j3.StreetAddress + " " + j3.City + " " + j3.State +
                                   " " + j3.ZipCode) : "",
                                   AddressLabel = j1.AddressId != null || j1.AddressId != Guid.Empty ? j3.Label : "",
                                   ScheduleTime = j1.ScheduledDate != null ? j1.ScheduledDate : null,
                                   JobStatusId = j1.JobStatus != null ? j1.JobStatus.ToString().ToLower() : "",
                                   JobStatus = j1.JobStatus != null ? j6.JobStatusName : "",
                                   ContactId = j1.ContactId != null ? j1.ContactId.ToString() : "",
                                   ContactFirstName = string.IsNullOrWhiteSpace(c.FirstName) ? c.FirstName : "",
                                   ContactLastName = string.IsNullOrWhiteSpace(c.LastName) ? c.LastName : "",
                                   CreatedAt = j1.CreatedAt != null ? j1.CreatedAt : null,
                                   UpdatedAt = j1.UpdatedAt != null ? j1.UpdatedAt : null,
                                   JobIsActive = j1.IsActive != null ? j1.IsActive : false
                               }).FirstOrDefault();

            e.jobDetails = jobdetails1;
        }
        private async void AddEstimateProducts(TblOrder order, OrderList e)
        {
            var productList = orderWindowProductService.GetAllQuerable()
                .Where(x => x.OrderId == order.OrderId && x.IsDelete == false)
                .Select(x => new
                {
                    x.OrderWindowProductId,
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
                    x.ProductMarkup,
                    x.ComponentMarkup,
                    x.OptionMarkup
                }).ToList();

            e.WindowProductId = productList.DistinctBy(x=>x.WindowProductId).Select(p => (Guid)p.WindowProductId).ToArray();
            foreach (var p in productList.DistinctBy(x => x.WindowProductId))
            {
                if (p == null)
                    continue;
                var Options = (from orderOption in dbContext.TblOrderWindowProductOptions
                               //join optionType in dbContext.TblOptionsTypes on orderOption.OptionTypeid equals optionType.OptionTypeId
                               //join option in dbContext.TblOptions on optionType.OptionId equals option.OptionId
                               where orderOption.OrderWindowProductId == p.OrderWindowProductId 
                               select new OptionTaxVM
                               {
                                   TaxId = orderOption.TaxId,
                                   TaxName = orderOption.TaxName ?? "",
                                   MunicipalityTax = orderOption.MunicipilityTax,
                                   StateTax = orderOption.StateTax,
                                   CityTax = orderOption.CityTax,
                                   OptionPrice = orderOption.OptionPrice,
                                   OptionName = orderOption.OptionName ?? "",
                                   OptionId = orderOption.Optionid.ToString(),
                                   OptionTypeId = orderOption.OptionTypeid.ToString(),
                                   OptionTypeName = Convert.ToString(orderOption.OptionName + " - " + orderOption.OptionTypeName),
                               }).ToList();
                //var Options = (from windowOption in dbContext.WindowOptions
                //               join optionType in dbContext.TblOptionsTypes on windowOption.OptionTypeId equals optionType.OptionTypeId
                //               join options in dbContext.TblOptions on optionType.OptionId equals options.OptionId
                //               where windowOption.WindowProductId == p.WindowProductId
                //               select new
                //               {
                //                   OptionName = Convert.ToString(options.OptionName + " - " + optionType.OptionType)
                //               }).ToList();
                var WindowDetails = orderWindowService.GetAllQuerable()
                    .Where(x => x.OrderId == order.OrderId && x.IsDelete == false)
                    .ToList();

                var WindowQty_Label = windowService.GetAllQuerable()
                       .Where(w => w.WindowId == p.WindowId && w.IsDelete == false)
                       .FirstOrDefault();


                EstimateProductLists estimateProduct = new EstimateProductLists
                {
                    EstimateWindowProductId = p.OrderWindowProductId,
                    Label = WindowQty_Label?.Label?? "",
                    WindowQty = WindowQty_Label?.Quantity ??0.0m,
                    ProductType = p.ProductType,
                    ProductTypeName = p.ProductTypeName,
                    Width = p.Width,
                    Height = p.Height,
                    ArchHeight = p.ArchHeight,
                    ProductName = p.ProductName,
                    additionalCost = Options.Select(x => x.OptionPrice).Sum(),
                    WindowProductId = p.WindowProductId,
                    Price = p.Price,
                    ProductPrice = p.ProductPrice,
                    ProductMarkup = p.ProductMarkup,
                    ComponentMarkup = p.ComponentMarkup,
                    OptionMarkup = p.OptionMarkup,
                    Options = Options.Select(x => x.OptionName).ToArray(),
                    WholesalePrice = p.WholeSalePrice,
                    EstimateWindowDetails = WindowDetails.Where(x => x.WindowId == p.WindowId).Select(x => new EstimateWindowLists
                    {
                        ArchHeight = x.ArchHeight,
                        Height = x.Height,
                        Width = x.Width
                    }).FirstOrDefault()

                };
                e.EstimateProductList.Add(estimateProduct);
            }
            AddJobDetails(e);
            e.IsTaxApplied = await estimateService.IsTaxApplied((Guid)e.JobId);
        }
        private async Task ProcessWindowProducts(List<ProductListing> windowProductIds, Guid estimateId, Guid OrderId, string Version)
        {
            FileLogger.Log("Processing window products...");
            foreach (var product in windowProductIds)
            {
                var windowProductData = (await windowProductService.GetAll()).Where(x => x.WindowProductId == product.WindowProductId && x.IsDelete == false).FirstOrDefault();
                if (windowProductData == null)
                {
                    Message = "Window Product " + AppConstant.NoDataFound;
                    return;
                }

                var windowData = (await windowService.GetAll()).Where(x => x.WindowId == windowProductData.WindowId && x.IsDelete == false).FirstOrDefault();
                if (windowData == null)
                {
                    Message = "Window " + AppConstant.NoDataFound;
                    return;
                }

                await AddWindowToOrder(windowData, windowProductData, OrderId, estimateId, Version);
                await AddProductToOrder(windowProductData, estimateId, OrderId, Version,product.ProductPrice);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
                if (product.ProductPrice == null)
                {
                    await AddComponentsToOrder((Guid)windowProductData.ProductId, estimateId, OrderId, Version);
                }
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
            }
        }

        private async Task AddWindowToOrder(TblWindow windowData, TblWindowProduct windowProductData, Guid OrderId, Guid estimateId, string Version)
        {
            FileLogger.Log($"Adding window data for window {windowData.WindowId} to order...");
            TblOrderWindow tblOrderWindow = new TblOrderWindow
            {
                OrderWindowId = Guid.NewGuid(),
                OrderId = OrderId,
                Version = Version,
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
            };

            await orderWindowService.Add(tblOrderWindow);
        }

        private async Task AddProductToOrder(TblWindowProduct windowProductData, Guid estimateId, Guid OrderId, string Version,
            decimal? ProductPrice)
        {
            FileLogger.Log($"Adding product data for product {windowProductData.ProductId} to order...");
            var productData = (await productService.GetAll()).Where(x => x.ProductId == windowProductData.ProductId && x.IsDelete == false).FirstOrDefault();
            var estimatePrice = estimateWindowProduct.GetAllQuerable().Where(x => x.EstimateId.ToString() == estimateId.ToString() && x.WindowId.ToString() == windowProductData.WindowId.ToString() && x.ProductId.ToString() == windowProductData.ProductId.ToString() && x.IsDelete == false).Select(x => x.Price).FirstOrDefault();
            if (productData == null)
            {
                Message = "Product " + AppConstant.NoDataFound;
                return;
            }
            var estimateWindowProductDetails = estimateWindowProduct.GetAllQuerable().Where(x => x.EstimateId == estimateId && x.ProductId.ToString() == windowProductData.ProductId.ToString()).FirstOrDefault();
            var estimateWindowProductOptionDetails = dbContext.TblEstimateWindowProductOptions.Where(s => s.EstimateWindowProductId == estimateWindowProductDetails.EstimateWindowProductId).ToList();
            TblOrderWindowProduct tblOrderWindowProduct = new TblOrderWindowProduct
            {
                OrderWindowProductId = Guid.NewGuid(),
                OrderId = OrderId,
                Version = Version,
                WindowId = (Guid)windowProductData.WindowId,
                ProductName = productData.ProductName,
                Height = productData.Height,
                Width = productData.Width,
                AdditionalWidth = productData.AdditionalWidth,
                Notes = productData.Notes,
                IsDelete = false,
                IsActive = true,
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
                Price = estimatePrice,
                WindowProductId = windowProductData.WindowProductId,
                ProductTypeName = estimateWindowProductDetails?.ProductTypeName??"",
                ProductPrice = ProductPrice,
                TaxId = productData.TaxId,
                TaxName = estimateWindowProductDetails?.TaxName?? "",
                Municipality = estimateWindowProductDetails.Municipality,
                City = estimateWindowProductDetails.City,
                State = estimateWindowProductDetails.State,
                WholeSalePrice = estimateWindowProductDetails.WholeSalePrice,
                ProductMarkup = estimateWindowProductDetails.ProductMarkup,
                ComponentMarkup = estimateWindowProductDetails.ComponentMarkup,
                OptionMarkup = estimateWindowProductDetails.OptionMarkup,
            };

            await orderWindowProductService.Add(tblOrderWindowProduct);

            // Save OptionTaxDetails 
            var optionTaxDetails = estimateWindowProductOptionDetails.ToList();
            if (optionTaxDetails != null && optionTaxDetails.Any())
            {
                await SaveOptionTaxDetailsToOrder(
                    tblOrderWindowProduct.OrderWindowProductId,
                    windowProductData.WindowProductId,
                    optionTaxDetails
                );
            }
        }


        private async Task SaveOptionTaxDetailsToOrder(Guid OrderWindowProductId, Guid windowProductId, List<TblEstimateWindowProductOption> optionTaxDetails)
        {
            if (optionTaxDetails == null || !optionTaxDetails.Any())
                return;

            List<TblOrderWindowProductOption> estimateWindowOptions = new List<TblOrderWindowProductOption>();

            foreach (var option in optionTaxDetails)
            {
                var optionEntity = new TblOrderWindowProductOption
                {
                    Id = Guid.NewGuid(),
                    WindowProductId = windowProductId,
                    WindowOptionId = option.WindowOptionId,
                    OrderWindowProductId = OrderWindowProductId,
                    TaxId = option.TaxId!=Guid.Empty? option.TaxId:null,
                    CityTax = option.CityTax,
                    StateTax = option.StateTax,
                    MunicipilityTax = option.MunicipilityTax,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = AppConstant.LoggedInUserID,
                    ModifiedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false,
                    OptionTypeid = option.OptionTypeid,
                    Optionid = option.Optionid,
                    OptionName = option.OptionName,
                    OptionTypeName = option.OptionTypeName,
                    OptionPrice = option.OptionPrice,
                    TaxName = option.TaxName,
                };

                estimateWindowOptions.Add(optionEntity);
            }
            await dbContext.TblOrderWindowProductOptions.AddRangeAsync(estimateWindowOptions);
          //  await dbContext.TblOrderWindowProductOption.AddRangeAsync(estimateWindowOptions);
            await dbContext.SaveChangesAsync();
        }


        private async Task AddComponentsToOrder(Guid productId, Guid estimateId, Guid OrderId, string Version)
        {
            FileLogger.Log($"Adding components for product {productId} to estimate...");
            var productComponentData = (await productComponentService.GetAll()).Where(x => x.ProductId == productId && x.IsDelete == false).ToList();
            if (productComponentData == null || productComponentData.Count == 0)
            {
                Message = "No Components " + AppConstant.NoDataFound;
                return;
            }

            foreach (var component in productComponentData)
            {
                var componentData = (await componentService.GetAll()).Where(x => x.ComponentId == component.ComponentId && x.IsDelete == false).FirstOrDefault();
                if (componentData == null)
                {
                    Message = "Components " + AppConstant.NoDataFound;
                    return;
                }

                await AddComponentToOrderWindow(componentData, estimateId, OrderId, Version);
                await AddComponentMaterialsToOrder(componentData.ComponentId, estimateId, OrderId, Version);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
            }
        }

        private async Task AddComponentMaterialsToOrder(Guid componentId, Guid estimateId, Guid OrderId, string Version)
        {
            FileLogger.Log($"Adding materials for component {componentId} to estimate...");

            var componentMaterialData = (await componentMaterialService.GetAll())
             .Where(x => x.ComponentId == componentId && x.IsDelete == false)
              .AsQueryable()
              .AsNoTracking()
              .ToList();


            if (componentMaterialData == null || componentMaterialData.Count == 0)
            {
                Message = "No material " + AppConstant.NoDataFound;
                return;
            }

            foreach (var material in componentMaterialData)
            {
                var materialData = (await materialService.GetAll())
                 .AsQueryable()
                 .Where(x => x.RawMaterialId == material.RawMaterialId && x.IsDelete == false)
                 .AsNoTracking()
                 .FirstOrDefault();


                if (materialData == null)
                {
                    Message = "Material " + AppConstant.NoDataFound;
                    return;
                }

                await AddMaterialToOrder(materialData, estimateId, OrderId, Version);
            }
        }

        private async Task AddMaterialToOrder(TblRawMaterial materialData, Guid estimateId, Guid OrderId, string Version)
        {
            FileLogger.Log($"Adding material {materialData.MaterialName} to order...");
            TblOrderWindowProductComponentMaterial tblEstimateWindowProductComponentMaterial = new TblOrderWindowProductComponentMaterial
            {
                OrderId = OrderId,
                Version = Version,
                OrderWindowProductComponentMaterialId = Guid.NewGuid(),
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
            };

            await orderWindowProductComponentMaterialService.Add(tblEstimateWindowProductComponentMaterial);
        }

        private async Task AddComponentToOrderWindow(TblComponent componentData, Guid estimateId, Guid OrderId, string Version)
        {
            FileLogger.Log($"Adding component {componentData.ComponentName} to estimate...");
            TblOrderWindowProductComponent tblOrderWindowProductComponent = new TblOrderWindowProductComponent
            {
                OrderWindowProductComponentId = Guid.NewGuid(),
                OrderId = OrderId,
                Version = Version,
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
                PricingExpression = componentData.PricingExpression,
                TenantId = claimsProperty.GetTenantId(),
            };

            await orderWindowProductComponentService.Add(tblOrderWindowProductComponent);
        }


        #endregion


        public async Task<IResponseVm<string>> GenerateOrderSheet(Guid? orderId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {

                 var template = dbContext.TblTemplates
                    .FirstOrDefault(c => c.TemplateName == EnumValue.GetEnumValue(EstimateDocumentType.OrderSheet) && c.IsDelete==false);

                if (template == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Order Sheet Template " + AppConstant.NoDataFound;
                    return responseVm;
                }


                var orderDetails = dbContext.TblOrders
                    .FirstOrDefault(o => o.OrderId == orderId && o.TenantId == claimsProperty.GetTenantId() && o.IsDelete==false);

                if (orderDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Order Details " + AppConstant.NoDataFound;
                    return responseVm;
                }


                var jobDetails = dbContext.TblJobs.FirstOrDefault(j => j.JobId == orderDetails.JobId && j.IsDelete == false);
                var customerDetails = dbContext.TblContacts
                    .FirstOrDefault(x => x.ContactId == jobDetails.ContactId && x.IsActive==true && x.IsDelete ==false);


                var tenantDetails = tenantService.GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive==true && x.IsDelete == false);

                if (tenantDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Tenant " + AppConstant.NoDataFound;
                    return responseVm;
                }

                var brandingInfo = tenantBrandingService.GetAllQuerable()
                    .FirstOrDefault(x => x.Id == tenantDetails.BrandingInfo && x.IsActive == true && x.IsDelete == false);

                var newestimate = estimateService.GetAll_Estimate(filter);
                var EstimateRes = newestimate?.Result?.Data?.FirstOrDefault(x => x.EstimateId == orderDetails.EstimateId);

                if (newestimate?.Result?.Data == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }

                var firstProduct = EstimateRes?.EstimateProductList?.FirstOrDefault();
                var optionName = firstProduct?.Options?.FirstOrDefault() ?? "";

                var model = new OrderSheetModel
                {
                    CustomerName = $"{customerDetails?.FirstName} {customerDetails?.LastName}",
                    JobName = orderDetails?.OrderName,
                    Notes = EstimateRes?.Notes ?? "",
                    CurrentDate = DateTime.Now.ToString("MM/dd/yyyy"),
                    CompanyName = brandingInfo?.CompanyName ?? "",
                    PhoneNo = brandingInfo?.PhoneNumber ?? "",
                    Position = tenantDetails?.Suffix ?? "",
                    CompanyLogo = brandingInfo?.Logo ?? "",
                    OptionName = optionName??"NA",
                };


                var finalHtml = ReplaceOrderSheetPlaceholders(template?.TemplateValue, model);

  

                var pdfBytes = GenerateInvoicePdf(finalHtml);

                var rootPath = Path.Combine(AppConstant.FileUploadPath, orderDetails.OrderName);
                FileLogger.Log($"Order Sheet File Path For Generation : {rootPath}");
                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }
                var savePath = Path.Combine(rootPath, orderDetails.OrderName);

                await using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }
                var pdfBase64 = Convert.ToBase64String(pdfBytes);

                responseVm.IsSuccess = true;
                responseVm.Data = pdfBase64;
                responseVm.Message = "Order sheet generated successfully.";
                File.Delete(savePath);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred while generating order sheet: {ex.Message}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public string ReplaceOrderSheetPlaceholders(string template, OrderSheetModel model)
        {
            if (string.IsNullOrWhiteSpace(template)) return string.Empty;

            return template
                .Replace("{{CustomerName}}", model.CustomerName ?? "")
                .Replace("{{JobName}}", model.JobName ?? "")
                .Replace("{{Notes}}", model.Notes ?? "")
                .Replace("{{CurrentDate}}", model.CurrentDate ?? "")
                .Replace("{{CompanyName}}", model.CompanyName ?? "")
                .Replace("{{PhoneNo}}", model.PhoneNo ?? "")
                .Replace("{{Position}}", model.Position ?? "")
                .Replace("{{logo}}", model.CompanyLogo ?? "")
                .Replace("{{OptionName}}", model.OptionName ?? "");
        }

        public byte[] GenerateInvoicePdf(string htmlContent)
        {
            var pdfDocument = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10 } 
                },
                Objects = {
            new ObjectSettings
            {
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8", LoadImages = true }
            }
        }
            };

            return converter.Convert(pdfDocument);
        }




    }
}


