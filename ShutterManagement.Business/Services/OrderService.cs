using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Microsoft.EntityFrameworkCore;
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
    public class OrderService : BaseRepository<BtprojecShuttermanagementDevContext, TblOrder>, IOrderService
    {
        private readonly ITenantService tenantService;
        private readonly IWindowProductService windowProductService;
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
        private readonly BtprojecShuttermanagementDevContext context;
        private readonly IWindowOptionService windowOptionService;
        private readonly IOptionsTypeService optionsTypeService;
        private readonly IMstProductType productType;
        private readonly IJobService jobService;
        private readonly IContactService contactService;
        private readonly IEstimateWindowProductService estimateWindowProduct;
        private readonly ITaxRate taxRate;
        private readonly IRegion region;
        private string Message = "";
        public OrderService(ITenantService tenantService, IWindowProductService windowProductService,
             IWindowService windowService, IComponentMaterial componentMaterialService,
             IOrderWindowService orderWindowService, IMaterial materialService,
             IProductService productService, IOrderWindowProductService orderWindowProductService,
             IProductComponentService productComponentService, IComponentService componentService,
             IOrderWindowProductComponentService orderWindowProductComponentService,
             IOrderWindowProductComponentMaterialService orderWindowProductComponentMaterialService,
             IEstimateService estimateService, BtprojecShuttermanagementDevContext context,
             IWindowOptionService windowOptionService, IOptionsTypeService optionsTypeService, IMstProductType productType,
             IJobService jobService, IContactService contactService, IEstimateWindowProductService estimateWindowProduct, ITaxRate taxRate,
            IRegion region)
        {
            this.tenantService = tenantService;
            this.windowProductService = windowProductService;
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


                var OrderNumber = GenerateUniqueId.GenerateUniqueIds(ModuleName.Order, AppConstant.LoggedInCompanyCode);
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
                    newOrder.OrderId = Guid.NewGuid();
                    newOrder.EstimateId = (Guid)updateOrder.EstimateId;
                    newOrder.TenantId = estimate.TenantId;
                    newOrder.JobId = updateOrder.JobId;
                    newOrder.EstimateName = updateOrder.EstimateName;
                    newOrder.EstimateCode = updateOrder.EstimateCode;
                    newOrder.Notes = updateOrder.Notes;
                    newOrder.DiscountAmount = updateOrder.Discount;
                    newOrder.DiscountCoupon = updateOrder.DiscountCoupon;
                    newOrder.GrandTotal = updateOrder.GrandTotal;
                    newOrder.TaxId = updateOrder.taxRateId;
                    newOrder.TaxAmount = updateOrder.Tax;
                    newOrder.TotalPrice = updateOrder.SubTotal;
                    newOrder.TermsCondition = updateOrder.TermsCondition;
                    newOrder.Status = status1;
                    newOrder.CreatedAt = DateTime.Now;
                    newOrder.UpdatedAt = DateTime.Now;
                    newOrder.Version = version1;
                    newOrder.OrderName = OrderNumber;
                    newOrder.IsActive = true;
                    newOrder.IsDelete = false;
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
                    newOrder.TermsCondition = estimate.TermsCondition;
                    newOrder.Status = status1;
                    newOrder.CreatedAt = DateTime.Now;
                    newOrder.UpdatedAt = DateTime.Now;
                    newOrder.Version = version1;
                    newOrder.OrderName = OrderNumber;
                    newOrder.IsActive = true;
                    newOrder.IsDelete = false;
                    newOrder.CreatedBy = AppConstant.LoggedInUserID;
                    newOrder.ModifiedBy = AppConstant.LoggedInUserID;
                };

                await Add(newOrder);
                var windowProductIds = dbContext.TblEstimateWindowProducts
                .Where(e => e.EstimateId == EstimateId)
                .Select(e => e.WindowProductId)
                .ToList();

                var windowProductIdsNonNullable = windowProductIds.Where(x => x.HasValue).Select(x => x.Value).ToList();

                await ProcessWindowProducts(windowProductIdsNonNullable, estimate.EstimateId, newOrder.OrderId, newOrder.Version);

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

        public async Task<IResponseVm<List<OrderList>>> GetAll_Order(Pagination filter)
        {
            IResponseVm<List<OrderList>> responseVm = new ResponseVm<List<OrderList>>();
            try
            {
                FileLogger.Log($"Start: Fetching estimates with filter parameters: TenantId = {filter.TenantId}, PageNo = {filter.PageNo}, RecordsPerPage = {filter.RecordPerPage}");

                var orders = await FetchOrders(filter);
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

                var orders = await FetchOrders(filter);
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
                var isEstimationOrder = (await GetAll()).Where(c => c.OrderId == model.OrderId && c.TenantId == AppConstant.LoggedInTenantId
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
                        var orderExist = GetAllQuerable().Where(x => x.OrderId == model.OrderId && x.TenantId == AppConstant.LoggedInTenantId &&
                        x.IsDelete == false && x.Status != EnumValue.GetEnumValue(Estimation.Approved)).FirstOrDefault();
                        if (orderExist != null)
                        {

                            orderExist.Notes = model.Notes;
                            orderExist.DiscountAmount = model.Discount;
                            orderExist.DiscountCoupon = model.DiscountCoupon;
                            orderExist.GrandTotal = model.GrandTotal;
                            orderExist.JobId = model.JobId;
                            orderExist.TaxId = model.taxRateId;
                            orderExist.TaxAmount = model.Tax;
                            orderExist.TotalPrice = model.SubTotal;
                            orderExist.TermsCondition = model.TermsCondition;
                            orderExist.ModifiedBy = AppConstant.LoggedInUserID;
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
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false)
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
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false)
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


                var IsOrderExists = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId &&
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
                            FromEmail = contactEmail,
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

                AppConstant.LoggedInTenantId = order.TenantId;
                Pagination pagination = new Pagination
                {
                    SearchValue = OrderId.ToString()
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
            FileLogger.Log($"Checking if tenant with ID {AppConstant.LoggedInTenantId} exists...");
            var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
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
        public async Task<List<TblOrder>> FetchOrders(Pagination filter)
        {
            if (string.IsNullOrEmpty(filter.TenantId))
            {
                // Log and fetch estimates for the logged-in tenant
                FileLogger.Log("No TenantId provided, fetching for the logged-in tenant.");
                return (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
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
                    GrandTotal = order.GrandTotal,
                    Notes = order.Notes,
                    Tax = order.TaxAmount,
                    TermsCondition = order.TermsCondition,
                };
                var jobDetails = jobService.GetAllQuerable().Where(x => x.JobId == order.JobId).Select(x => x.ContactId).FirstOrDefault();

                var contactDetails = contactService.GetAllQuerable().Where(x => x.ContactId == jobDetails).FirstOrDefault();
                e.Contact = contactDetails.Name;

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
                    (string.IsNullOrEmpty(filter.JobStatus.ToLower()) ? true : x.jobDetails.JobStatusId.ToLower() == filter.JobStatus.ToLower()) &&
                    (string.IsNullOrWhiteSpace(filter.SearchValue) ||
                     ((x.Notes ?? "").Contains(filter.SearchValue)
                      || (x.JobId.ToString() ?? "").Contains(filter.SearchValue)
                      || (x.OrderNumber ?? "").Contains(filter.SearchValue)
                      || (x.OrderId.ToString() ?? "").Contains(filter.SearchValue)
                      || (x.Status ?? "").Contains(filter.SearchValue)
                      || (x.EstimateId.ToString()).Contains(filter.SearchValue)
                      || (x.EstimateCode ?? "").Contains(filter.SearchValue)
                      || (x.EstimateName ?? "").Contains(filter.SearchValue)
                      || (x.TermsCondition ?? "").Contains(filter.SearchValue)
                      || (x.DiscountCoupon ?? "").Contains(filter.SearchValue)
                      || (x.GrandTotal.ToString() ?? "").Contains(filter.SearchValue)))
                ).ToList();

            return filteredRecords;
        }

        private void AddJobDetails(OrderList e)
        {

            var jobdetails1 = (from j1 in dbContext.TblJobs
                               join j2 in dbContext.TblOrders on j1.JobId equals j2.JobId
                               join j3 in dbContext.TblAddresses on j1.AddressId equals j3.AddressId
                               join j4 in dbContext.MstJobTypes on j1.JobType equals j4.Id
                               join j5 in dbContext.TblUsers on j1.AssignedPersonnel equals j5.Id
                               join c in dbContext.TblContacts on j1.ContactId equals c.ContactId
                               join j6 in dbContext.MstJobStatuses on j1.JobStatus equals j6.JobStatusId 
                               where j1.IsDelete == false && c.IsDelete == false
                               select new JobDetail
                               {
                                   JobName = j1.JobName,
                                   JobNotes = j1.Notes,
                                   JobType = j4.JobTypeName,
                                   AssignedPersonnel = j5.FirstName + " " + j5.LastName,
                                   Address = j3.StreetAddress + " " + j3.City + " " + j3.State + " " + j3.ZipCode,
                                   AddressLabel = j3.Label,
                                   ScheduleTime = j1.ScheduledDate,
                                   JobStatusId = j1.JobStatus.ToString().ToLower(),
                                   JobStatus = j6.JobStatusName,
                                   ContactId = j1.ContactId.ToString(),
                                   ContactName = c.Name,
                                   CreatedAt = j1.CreatedAt,
                                   UpdatedAt = j1.UpdatedAt
                               }).FirstOrDefault();

            e.jobDetails = jobdetails1;
        }
        private void AddEstimateProducts(TblOrder order, OrderList e)
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
                    x.Price
                }).ToList();

            e.WindowProductId = productList.Select(p => (Guid)p.WindowProductId).ToArray();
            foreach (var p in productList)
            {
                var Options = (from windowOption in dbContext.WindowOptions
                               join optionType in dbContext.TblOptionsTypes on windowOption.OptionTypeId equals optionType.OptionTypeId
                               join options in dbContext.TblOptions on optionType.OptionId equals options.OptionId
                               where windowOption.WindowProductId == p.WindowProductId
                               select new
                               {
                                   OptionName = Convert.ToString(options.OptionName + " - " + optionType.OptionType)
                               }).ToList();
                var WindowDetails = orderWindowService.GetAllQuerable()
                    .Where(x => x.OrderId == order.OrderId && x.IsDelete == false)
                    .ToList();

                var WindowQty_Label = windowService.GetAllQuerable()
                       .Where(w => w.WindowId == p.WindowId && w.IsDelete == false)
                       .FirstOrDefault();
               
                   
                    EstimateProductLists estimateProduct = new EstimateProductLists
                    {
                    EstimateWindowProductId = p.OrderWindowProductId,
                    Label = WindowQty_Label.Label,
                    WindowQty = WindowQty_Label.Quantity,
                    ProductType = p.ProductType,
                    ProductTypeName = productType.GetAllQuerable()
                        .Where(pt => pt.ProductId.ToString() == p.ProductType && pt.IsDelete == false)
                        .Select(pt => pt.ProductName)
                        .FirstOrDefault(),
                    Width = p.Width,
                    Height = p.Height,
                    ArchHeight = p.ArchHeight,
                    ProductName = p.ProductName,
                    WindowProductId = p.WindowProductId,
                    Price = p.Price,
                    Options = Options.Select(x => x.OptionName).ToArray(),
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
        }
        private async Task ProcessWindowProducts(List<Guid> windowProductIds, Guid estimateId, Guid OrderId, string Version)
        {
            FileLogger.Log("Processing window products...");
            foreach (var product in windowProductIds)
            {
                var windowProductData = (await windowProductService.GetAll()).Where(x => x.WindowProductId == product && x.IsDelete == false).FirstOrDefault();
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
                await AddProductToOrder(windowProductData, estimateId, OrderId, Version);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
                await AddComponentsToOrder((Guid)windowProductData.ProductId, estimateId, OrderId, Version);
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
                TenantId = AppConstant.LoggedInTenantId,
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

        private async Task AddProductToOrder(TblWindowProduct windowProductData, Guid estimateId, Guid OrderId, string Version)
        {
            FileLogger.Log($"Adding product data for product {windowProductData.ProductId} to order...");
            var productData = (await productService.GetAll()).Where(x => x.ProductId == windowProductData.ProductId && x.IsDelete == false).FirstOrDefault();
            var estimatePrice = estimateWindowProduct.GetAllQuerable().Where(x => x.EstimateId == estimateId && x.IsDelete == false).Select(x => x.Price).FirstOrDefault();
            if (productData == null)
            {
                Message = "Product " + AppConstant.NoDataFound;
                return;
            }

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
                TenantId = AppConstant.LoggedInTenantId,
                ProductId = productData.ProductId,
                Price = estimatePrice,
                WindowProductId = windowProductData.WindowProductId
            };

            await orderWindowProductService.Add(tblOrderWindowProduct);
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
                TenantId = AppConstant.LoggedInTenantId,
            };

            await orderWindowProductComponentService.Add(tblOrderWindowProductComponent);
        }


        #endregion

    }
}

