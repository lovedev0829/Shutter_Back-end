using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private string Message = "";
        public OrderService(ITenantService tenantService, IWindowProductService windowProductService,
             IWindowService windowService, IComponentMaterial componentMaterialService,
             IOrderWindowService orderWindowService, IMaterial materialService,
             IProductService productService, IOrderWindowProductService orderWindowProductService,
             IProductComponentService productComponentService, IComponentService componentService,
             IOrderWindowProductComponentService orderWindowProductComponentService,
             IOrderWindowProductComponentMaterialService orderWindowProductComponentMaterialService,
             IEstimateService estimateService,BtprojecShuttermanagementDevContext context,
             IWindowOptionService windowOptionService,IOptionsTypeService optionsTypeService,IMstProductType productType )
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
        }


        public async Task<IResponseVm<string>> Create_Order_From_Estimate(Guid EstimateId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log("Start: Create Order from Estimate...");

            try
            {
                // Fetch the estimate using EstimateId
                var estimate = estimateService.GetAllQuerable()
                    .Where(e => e.EstimateId == EstimateId && e.IsDelete == false)
                    .FirstOrDefault();

                if (estimate == null)
                {
                    responseVm.Message = "Estimate " + AppConstant.NoDataFound;
                    return responseVm;
                }
                var OrderNumber =  GenerateUniqueId.GenerateUniqueIds(ModuleName.Order, AppConstant.LoggedInCompanyCode);

                TblOrder newOrder = new TblOrder
                {
                    OrderId = Guid.NewGuid(), 
                    EstimateId = estimate.EstimateId,  
                    TenantId = estimate.TenantId,
                    JobId = estimate.JobId,
                    EstimateName = estimate.EstimateName,
                    EstimateCode = estimate.EstimateCode,
                    Notes = estimate.Notes,
                    DiscountAmount = estimate.DiscountAmount,
                    DiscountCoupon = estimate.DiscountCoupon,
                    GrandTotal = estimate.GrandTotal,
                    TaxAmount = estimate.TaxAmount,
                    TotalPrice = estimate.TotalPrice,
                    TermsCondition = estimate.TermsCondition,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Version = "1",  
                    OrderName = OrderNumber,
                    IsActive = true,
                    IsDelete = false,
                    CreatedBy = AppConstant.LoggedInUserID,
                    ModifiedBy = AppConstant.LoggedInUserID
                };

                await Add(newOrder);
                var windowProductIds = dbContext.TblEstimateWindowProducts
           .Where(e => e.EstimateId == EstimateId)
           .Select(e => e.WindowProductId)
           .ToList();

                // Convert List<Guid?> to List<Guid>
                var windowProductIdsNonNullable = windowProductIds.Where(x => x.HasValue).Select(x => x.Value).ToList();

                // Process the window products for the order
                await ProcessWindowProducts(windowProductIdsNonNullable, newOrder.OrderId, estimate.EstimateId, newOrder.Version);

                if (!string.IsNullOrEmpty(Message))
                {
                    responseVm.Message = Message;
                    return responseVm;
                }

                responseVm.IsSuccess = true;
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

        private UpdateOrder MapCreateOrderToUpdateOrder(CreateOrder model)
        {
            var updateOrder = new UpdateOrder
            {
                EstimateName = model.EstimateName,
                EstimateCode = model.EstimateCode,
                Notes = model.Notes,
                Discount = model.Discount,
                DiscountCoupon = model.DiscountCoupon,
                GrandTotal = model.GrandTotal,
                Tax = model.Tax,
                TermsCondition = model.TermsCondition,
                SubTotal = model.SubTotal,
                Status = model.Status,
            };

            return updateOrder;
        }
        private async Task ProcessWindowProducts(List<Guid> windowProductIds, Guid estimateId,Guid OrderId,string Version)
        {
            FileLogger.Log("Processing window products...");
            foreach (var product in windowProductIds)
            {
                var windowProductData = (await windowProductService.GetAll()).Where(x => x.WindowProductId == product && x.IsDelete == false).FirstOrDefault();
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
            FileLogger.Log($"Adding window data for window {windowData.WindowId} to estimate...");
            TblOrderWindow tblOrderWindow = new TblOrderWindow
            {
                OrderId = OrderId,
                Version = Version,
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
            };

            await orderWindowService.Add(tblOrderWindow);
        }

        private async Task AddProductToOrder(TblWindowProduct windowProductData, Guid estimateId, Guid OrderId, string Version)
        {
            FileLogger.Log($"Adding product data for product {windowProductData.ProductId} to estimate...");
            var productData = (await productService.GetAll()).Where(x => x.ProductId == windowProductData.ProductId && x.IsDelete == false).FirstOrDefault();
            if (productData == null)
            {
                Message = "Product " + AppConstant.NoDataFound;
                return; // Return early if product data not found
            }

            TblOrderWindowProduct tblOrderWindowProduct = new TblOrderWindowProduct
            {
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
                EstimateWindowProductId = Guid.NewGuid(),
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
                EstimateWindowProductComponentMaterialId = Guid.NewGuid(),
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
                OrderId = OrderId,
                Version = Version,
                EstimateWindowProductComponentId = Guid.NewGuid(),
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

        public async Task<IResponseVm<List<OrderList>>> GetAll_Order(Pagination filter)
        {
            IResponseVm<List<OrderList>> responseVm = new ResponseVm<List<OrderList>>();
            try
            {
                FileLogger.Log($"Start: Fetching estimates with filter parameters: TenantId = {filter.TenantId}, PageNo = {filter.PageNo}, RecordsPerPage = {filter.RecordPerPage}");

                // Validate Tenant if provided, else use the logged-in tenant
                var orders = await FetchOrders(filter);
                if (orders == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                // Log the number of estimates fetched
                FileLogger.Log($"Fetched {orders.Count} estimates.");

                // Get product types and filter records based on search and status
                var filteredEstimates = ApplyFilters(orders, filter);

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

        private async Task<List<TblOrder>> FetchOrders(Pagination filter)
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

            // Loop through the 'orders' list (not 'orderlist')
            foreach (var order in orders)  // We are now iterating over TblOrder items
            {
                var e = new OrderList
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderName,
                    Version = order.Version,
                    EstimateId = order.EstimateId,
                    CreatedDate = order.CreatedAt,
                    JobId = order.JobId,
                    Status = order.Status,
                    IsActive = order.IsActive,
                    Discount = order.DiscountAmount,
                    DiscountCoupon = order.DiscountCoupon,
                    EstimateName = order.EstimateName,
                    EstimateCode = order.EstimateCode,
                   // SubTotal = order.SubTotal,
                    GrandTotal = order.GrandTotal,
                    Notes = order.Notes,
                    Tax = order.TaxAmount,
                    TermsCondition = order.TermsCondition,
                };

                // Fetch User details
                var userdetails = dbContext.TblUsers
                    .Where(x => x.Id == order.CreatedBy && x.IsDelete == false)
                    .FirstOrDefault();
                if (userdetails != null)
                {
                    e.CreatedBy = userdetails.FirstName + " " + userdetails.LastName;
                }
                else
                {
                    // Handle the case when userdetails is null
                    e.CreatedBy = "User not found"; // You can set a default message or handle it in another way
                }

                // Fetch Tenant details
                var Tenant = dbContext.TblTenants
                    .Where(e => e.TenantId == order.TenantId && e.IsDelete == false)
                    .FirstOrDefault();
                e.primarycolor = dbContext.TblTenantBrandingInfos
                    .Where(tb => tb.Id == Tenant.BrandingInfo)
                    .Select(tb => tb.PrimaryColor)
                    .FirstOrDefault();

                // Fetch and add related products and options
                AddEstimateProducts(order, e);  // Pass the individual TblOrder to the method
                orderlist.Add(e);  // Add the populated OrderList view model to the list
            }

            // Filter records based on search value and status
            var filteredRecords = orderlist
                .Where(x =>
                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
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


        private void AddEstimateProducts(TblOrder order, OrderList e)
        {
            var productList = orderWindowProductService.GetAllQuerable()
                .Where(x => x.EstimateId == order.EstimateId && x.IsDelete == false)
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
                    x.WindowProductId
                }).ToList();

            e.WindowProductId = productList.Select(p => (Guid)p.WindowProductId).ToArray();
            foreach (var p in productList)
            {
                var optionIds = windowOptionService.GetAllQuerable()
                    .Where(x => x.IsDelete == false && x.WindowProductId == p.WindowProductId)
                    .ToList();
                var optionDetails = optionsTypeService.GetAllQuerable()
                    .Where(x => x.IsDelete == false && optionIds.Select(y => y.OptionTypeId).Contains(x.OptionTypeId))
                    .Select(x => new OptionLists { OptionName = x.OptionType })
                    .ToList();
                var WindowDetails = orderWindowService.GetAllQuerable()
                    .Where(x => x.EstimateId == order.EstimateId && x.IsDelete == false)
                    .ToList();
                EstimateProductLists estimateProduct = new EstimateProductLists
                {
                    EstimateWindowProductId = p.EstimateWindowProductId,
                    Label = windowService.GetAllQuerable()
                        .Where(w => w.WindowId == p.WindowId && w.IsDelete == false)
                        .Select(pt => pt.Label)
                        .FirstOrDefault(),
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
                    Options = optionDetails,
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


        private void AddJobDetails(OrderList e)
        {

            var jobdetails1 = (from j1 in dbContext.TblJobs
                               join j2 in dbContext.TblEstimates on j1.JobId equals j2.JobId
                               join j3 in dbContext.TblAddresses on j1.AddressId equals j3.AddressId
                               join j4 in dbContext.MstJobTypes on j1.JobType equals j4.Id
                               join j5 in dbContext.TblUsers on j1.AssignedPersonnel equals j5.Id
                               where j1.IsDelete == false
                               select new JobDetail
                               {
                                   JobName = j1.JobName,
                                   JobNotes = j1.Notes,
                                   JobType = j4.JobTypeName,
                                   AssignedPersonnel = j5.FirstName + " " + j5.LastName,
                                   Address = j3.StreetAddress + " " + j3.City + " " + j3.State + " " + j3.ZipCode,
                                   AddressLabel = j3.Label,
                                   ScheduleTime = j1.ScheduledDate
                               }).FirstOrDefault();

            e.jobDetails = jobdetails1;
        }
        //public async Task<IResponseVm<string>> Update_Order(UpdateOrder model)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    FileLogger.Log("Start: Update Order...");

        //    try
        //    {
        //        var isTenantExists = await ValidateTenantExists();
        //        if (!isTenantExists)
        //        {
        //            responseVm.Message = AppConstant.TenantNotFound;
        //            return responseVm;
        //        }
        //        var existingOrder = await ValidateOrderExists((Guid)model.EstimationId);

        //        if (existingOrder == null)
        //        {
        //            CreateOrderRecord(model);
        //            return responseVm;
        //        }
        //        var isOrderExist = (await GetAll())
        //        .Where(c => c.EstimateId == model.EstimationId && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false && c.JobId == model.JobId)
        //        .FirstOrDefault();

        //        string newVersion = IncrementVersion(isOrderExist.Version);
        //        TblOrder updatedOrder = new TblOrder
        //        {
        //            OrderId = Guid.NewGuid(),
        //            Version = newVersion,
        //            EstimateId = (Guid)model.EstimationId,
        //            EstimateName = model.EstimateName,
        //            EstimateCode = model.EstimateCode,
        //            Notes = model.Notes,
        //            DiscountAmount = model.Discount,
        //            DiscountCoupon = model.DiscountCoupon,
        //            GrandTotal = model.GrandTotal,
        //            JobId = model.JobId,
        //            TaxAmount = model.Tax,
        //            TermsCondition = model.TermsCondition,
        //            CreatedBy = AppConstant.LoggedInUserID,
        //            CreatedAt = DateTime.Now,
        //            TenantId = AppConstant.LoggedInTenantId,
        //            IsActive = true,
        //            IsDelete = false,
        //            TotalPrice = model.SubTotal,
        //            Status = EnumValue.GetEnumValue(Estimation.New),
        //        };

        //        var res = await Add(updatedOrder);
        //        await ProcessWindowProducts(model.WindowProductId.ToList(), (Guid)model.EstimationId, updatedOrder.OrderId, newVersion);
        //        responseVm.IsSuccess = true;
        //        responseVm.Message = "Order updated successfully with new version.";
        //    }
        //    catch (Exception ex)
        //    {
        //        FileLogger.Log($"Error occurred: {ex.Message}");
        //        responseVm.Message = "Error updating order: " + ex.Message;
        //    }

        //    return responseVm;
        //}

        //private string IncrementVersion(string currentVersion)
        //{
        //    if (string.IsNullOrEmpty(currentVersion))
        //    {
        //        return "v1";
        //    }
        //    int versionNumber = int.Parse(currentVersion.TrimStart('v'));
        //    versionNumber++;
        //    return "v" + versionNumber.ToString();
        //}

        //private async Task<TblOrder> CreateOrderRecord(EstimateDetails estimatedetails)
        //{
        //    FileLogger.Log("Creating new Order...");
        //    TblOrder createOrder = new TblOrder
        //    {
        //        OrderId = Guid.NewGuid(),
        //        Version = "v1",
        //        EstimateId = estimatedetails.EstimateId,
        //        EstimateName = estimatedetails.EstimateName,
        //        EstimateCode = estimatedetails.EstimateCode,
        //        Notes = estimatedetails.Notes,
        //        DiscountAmount = estimatedetails.DiscountAmount,
        //        DiscountCoupon = estimatedetails.DiscountCoupon,
        //        GrandTotal = estimatedetails.GrandTotal,
        //        JobId = estimatedetails.JobId,
        //        TaxAmount = estimatedetails.TaxAmount,
        //        TermsCondition = estimatedetails.TermsCondition,
        //        CreatedBy = AppConstant.LoggedInUserID,
        //        CreatedAt = DateTime.Now,
        //        TenantId = AppConstant.LoggedInTenantId,
        //        IsActive = true,
        //        IsDelete = false,
        //        TotalPrice = estimatedetails.TotalPrice,
        //        Status = EnumValue.GetEnumValue(Estimation.New),
        //    };

        //    var res = await Add(createOrder); 
        //    return res;
        //}

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

        private async Task<bool> ValidateOrderExists(Guid estimateId)
        {
            FileLogger.Log($"Checking if Order with this estimation {estimateId} already exists...");
            var isOrderExist = (await GetAll())
                .Where(c => c.EstimateId == estimateId && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false)
                .ToList();
            if (isOrderExist.Count > 0)
            {
                FileLogger.Log($"Order with this estimation {estimateId} already exists.");
                return true;
            }
            return false;
        }


        //public async Task<EstimateDetails> GetEstimateDetails(Guid EstimateId)
        //{
        //    var orderDetails = await (from t1 in context.TblEstimates
        //                              join t2 in context.TblEstimateWindows on t1.EstimateId equals t2.EstimateId
        //                              join t3 in context.TblEstimateWindowProducts on t1.EstimateId equals t3.EstimateId
        //                              join t4 in context.TblEstimateWindowProductComponents on t1.EstimateId equals t4.EstimateId
        //                              join t5 in context.TblEstimateWindowProductComponentMaterials on t1.EstimateId equals t5.EstimateId
        //                              where t1.EstimateId == EstimateId && t1.TenantId == AppConstant.LoggedInTenantId && t1.IsDelete == false
        //                              select new EstimateDetails
        //                              {
        //                                  EstimateId = t1.EstimateId,
        //                                  JobId = t1.JobId,
        //                                  EstimateName = t1.EstimateName,
        //                                  EstimateCode = t1.EstimateCode,
        //                                  TotalPrice = t1.TotalPrice,
        //                                  DiscountAmount = t1.DiscountAmount,
        //                                  TaxAmount = t1.TaxAmount,
        //                                  GrandTotal = t1.GrandTotal,
        //                                  Status = t1.Status,
        //                                  TermsCondition = t1.TermsCondition,
        //                                  Notes = t1.Notes,
        //                                  DiscountCoupon = t1.DiscountCoupon,
        //                                  EstWindowId = t2.EstWindowId,
        //                                  AddressId = t2.AddressId,
        //                                  Label = t2.Label,
        //                                  Width = t2.Width,
        //                                  Height = t2.Height,
        //                                  ArchHeight = t2.ArchHeight,
        //                                  Photo = t2.Photo,
        //                                  OWNotes = t2.Notes,
        //                                  WindowNotes = t2.Notes,
        //                                  Quantity = t2.Quantity,
        //                                  WindowId = t2.WindowId,
        //                                  EstWindowProductId = t3.EstimateWindowProductId,
        //                                  ProductId = t3.ProductId,
        //                                  ProductName = t3.ProductName,
        //                                  ProductType = t3.ProductType,
        //                                  WPWidth = t3.Width,
        //                                  WPHeight = t3.Height,
        //                                  Length = t3.Length,
        //                                  Area = t3.Area,
        //                                  AdditionalWidth = t3.AdditionalWidth,
        //                                  WPArchHeight = t3.ArchHeight,
        //                                  Color = t3.Color,
        //                                  WPNotes = t3.Notes,
        //                                  Markup = t3.Markup,
        //                                  WindowProductId = t3.WindowProductId,
        //                                  EstWindowProductComponent = t4.EstimateWindowProductComponentId,
        //                                  ComponentId = t4.ComponentId,
        //                                  ComponentName = t4.ComponentName,
        //                                  WCProductType = t4.ProductType,
        //                                  MarkupPercentage = t4.MarkupPercentage,
        //                                  PricingExpression = t4.PricingExpression,
        //                                  WCNotes = t4.Notes,
        //                                  EstWindowProductComponentMaterialId = t5.EstimateWindowProductComponentMaterialId,
        //                                  RawMaterialId = t5.RawMaterialId,
        //                                  MaterialName = t5.MaterialName,
        //                                  UnitCost = t5.UnitCost,
        //                                  UnitofMeasurement = t5.UnitOfMeasurement,
        //                                  StockQuantity = t5.StockQuantity,
        //                                  ReOrderlevel = t5.ReorderLevel,
        //                                  VendorId = t5.VendorId,
        //                                  WRNotes = t5.Notes,
        //                                  SKU = t5.Sku,
        //                                  IsCutSheet = t5.IsCutSheet,
        //                                  WRProductType = t5.ProductType
        //                              }).FirstOrDefaultAsync();

        //    return orderDetails;
        //}

        //public async Task<IResponseVm<string>> Create_Order(Guid EstimateId)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        var Estimatedetails = await GetEstimateDetails(EstimateId);

        //        if (Estimatedetails != null)
        //        {
        //            TblOrder order = new TblOrder
        //            {
        //                OrderId = Guid.NewGuid(),
        //                Version = "v1",
        //                EstimateId = EstimateId,
        //                JobId = Estimatedetails.JobId,
        //                EstimateName = Estimatedetails.EstimateName,
        //                EstimateCode = Estimatedetails.EstimateCode,
        //                TotalPrice = Estimatedetails.TotalPrice,
        //                DiscountAmount = Estimatedetails.DiscountAmount,
        //                TaxAmount = Estimatedetails.TaxAmount,
        //                GrandTotal = Estimatedetails.GrandTotal,
        //                Status = Estimatedetails.Status,
        //                CreatedAt = DateTime.Now,
        //                IsActive = true,
        //                IsDelete = false,
        //                CreatedBy = AppConstant.LoggedInUserID,
        //                TenantId = AppConstant.LoggedInTenantId,
        //                Notes = Estimatedetails.Notes,
        //                TermsCondition = Estimatedetails.TermsCondition,
        //                DiscountCoupon = Estimatedetails.DiscountCoupon
        //            };
        //            var res = await Add(order);

        //            TblOrderWindow orderWindow = new TblOrderWindow
        //            {
        //                OrderId = order.OrderId,
        //                Version = order.Version,
        //                EstWindowId = Estimatedetails.EstimateId,
        //                EstimateId = Estimatedetails.EstimateId,
        //                AddressId = Estimatedetails.AddressId,
        //                Label = Estimatedetails.Label,
        //                Width = Estimatedetails.Width,
        //                Height = Estimatedetails.Height,
        //                ArchHeight = Estimatedetails.ArchHeight,
        //                Photo = Estimatedetails.Photo,
        //                Notes = Estimatedetails.OWNotes,
        //                CreatedAt = DateTime.Now,
        //                CreatedBy = AppConstant.LoggedInUserID,
        //                IsActive = true,
        //                IsDelete = false,
        //                TenantId = AppConstant.LoggedInTenantId,
        //                Quantity = Estimatedetails.Quantity,
        //                WindowId = Estimatedetails.WindowId
        //            };

        //            await orderWindowService.Add(orderWindow);

        //            TblOrderWindowProduct orderWindowProduct = new TblOrderWindowProduct
        //            {
        //                OrderId = order.OrderId,
        //                Version = order.Version,
        //                EstimateWindowProductId = (Guid)Estimatedetails.EstWindowProductId,
        //                EstimateId = Estimatedetails.EstimateId,
        //                WindowId = (Guid)Estimatedetails.WindowId,
        //                ProductId = (Guid)Estimatedetails.ProductId,
        //                ProductName = Estimatedetails.ProductName,
        //                ProductType = Estimatedetails.ProductType,
        //                Width = Estimatedetails.WPWidth,
        //                Height = Estimatedetails.WPHeight,
        //                Length = Estimatedetails.Length,
        //                Area = Estimatedetails.Area,
        //                AdditionalWidth = Estimatedetails.AdditionalWidth,
        //                ArchHeight = Estimatedetails.WPArchHeight,
        //                Color = Estimatedetails.Color,
        //                Notes = Estimatedetails.WPNotes,
        //                Markup = Estimatedetails.Markup,
        //                WindowProductId = Estimatedetails.WindowProductId,
        //                CreatedAt = DateTime.Now,
        //                CreatedBy = AppConstant.LoggedInUserID,
        //                IsActive = true,
        //                IsDelete = false,
        //                TenantId = AppConstant.LoggedInTenantId
        //            };

        //            await orderWindowProductService.Add(orderWindowProduct);

        //            TblOrderWindowProductComponent orderWindowProductComponent = new TblOrderWindowProductComponent
        //            {
        //                OrderId = order.OrderId,
        //                Version = order.Version,
        //                EstimateWindowProductComponentId = (Guid)Estimatedetails.EstWindowProductComponent,
        //                EstimateId = Estimatedetails.EstimateId,
        //                ComponentId = (Guid)Estimatedetails.ComponentId,
        //                ComponentName = Estimatedetails.ComponentName,
        //                ProductType = Estimatedetails.WCProductType,
        //                MarkupPercentage = Estimatedetails.MarkupPercentage,
        //                PricingExpression = Estimatedetails.PricingExpression,
        //                Notes = Estimatedetails.WCNotes,
        //                CreatedAt = DateTime.Now,
        //                CreatedBy = AppConstant.LoggedInUserID,
        //                IsActive = true,
        //                IsDelete = false,
        //                TenantId = AppConstant.LoggedInTenantId
        //            };

        //            await orderWindowProductComponentService.Add(orderWindowProductComponent);

        //            TblOrderWindowProductComponentMaterial windowProductComponentMaterial = new TblOrderWindowProductComponentMaterial
        //            {
        //                OrderId = order.OrderId,
        //                Version = order.Version,
        //                EstimateWindowProductComponentMaterialId = (Guid)Estimatedetails.EstWindowProductComponentMaterialId,
        //                EstimateId = Estimatedetails.EstimateId,
        //                RawMaterialId = Estimatedetails.RawMaterialId,
        //                MaterialName = Estimatedetails.MaterialName,
        //                UnitCost = Estimatedetails.UnitCost,
        //                UnitOfMeasurement = Estimatedetails.UnitofMeasurement,
        //                StockQuantity = Estimatedetails.StockQuantity,
        //                ReorderLevel = Estimatedetails.ReOrderlevel,
        //                VendorId = Estimatedetails.VendorId,
        //                Notes = Estimatedetails.WRNotes,
        //                ProductType = Estimatedetails.WRProductType,
        //                Sku = Estimatedetails.SKU,
        //                IsCutSheet = Estimatedetails.IsCutSheet,
        //                TenantId = AppConstant.LoggedInTenantId,
        //                IsActive = true,
        //                IsDelete = false,
        //                CreatedAt = DateTime.Now,
        //                CreatedBy = AppConstant.LoggedInUserID
        //            };
        //            await orderWindowProductComponentMaterialService.Add(windowProductComponentMaterial);

        //            responseVm.IsSuccess = true;
        //            responseVm.Message = AppConstant.InsertSuccessMsg;
        //        }

        //    }
        //    catch(Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }
        //    return responseVm;

        //}

    }
}

