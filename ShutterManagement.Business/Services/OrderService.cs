using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;


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
        private string Message = "";
        public OrderService(ITenantService tenantService,IWindowProductService windowProductService,
             IWindowService windowService, IComponentMaterial componentMaterialService,
             IOrderWindowService orderWindowService, IMaterial materialService,
             IProductService productService ,IOrderWindowProductService orderWindowProductService,
             IProductComponentService productComponentService, IComponentService componentService,
             IOrderWindowProductComponentService orderWindowProductComponentService,
             IOrderWindowProductComponentMaterialService orderWindowProductComponentMaterialService)
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
        }
      

        public async Task<IResponseVm<string>> Create_Order(CreateOrder model)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log("Start: Create Order...");

            try
            {
                var isTenantExists = await ValidateTenantExists();
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsOrderExists = await ValidateOrderExists((Guid)model.EstimationId, (Guid)model.JobId);
                if (IsOrderExists)
                {
                    var updateOrder = MapCreateOrderToUpdateOrder(model);
                    await Update_Order(updateOrder);
                }

                if (model.WindowProductId == null || model.WindowProductId.Length == 0)
                {
                    responseVm.Message = "Atleast one window is required.";
                    return responseVm;
                }
                TblOrder createOrder = await CreateOrderRecord(model);
                if (createOrder.OrderId != Guid.Empty)
                {
                    await ProcessWindowProducts(model.WindowProductId.ToList(),createOrder.EstimateId, createOrder.OrderId,createOrder.Version);
                    if (!string.IsNullOrEmpty(Message))
                    {
                        responseVm.Message = Message;
                        return responseVm;
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                    FileLogger.Log("Order created and all related data added successfully.");
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
                var existingOrder = await ValidateOrderExists((Guid)model.EstimationId, (Guid)model.JobId);

                if (existingOrder == null)
                {
                    await CreateOrderRecord(model);
                    return responseVm;
                }
                var isOrderExist = (await GetAll())
                .Where(c => c.EstimateId == model.EstimationId && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false && c.JobId == model.JobId)
                .FirstOrDefault();

                string newVersion = IncrementVersion(isOrderExist.Version);
                TblOrder updatedOrder = new TblOrder
                {
                    OrderId = Guid.NewGuid(),
                    Version = newVersion,
                    EstimateId = (Guid)model.EstimationId,
                    EstimateName = model.EstimateName,
                    EstimateCode = model.EstimateCode,
                    Notes = model.Notes,
                    DiscountAmount = model.Discount,
                    DiscountCoupon = model.DiscountCoupon,
                    GrandTotal = model.GrandTotal,
                    JobId = model.JobId,
                    TaxAmount = model.Tax,
                    TermsCondition = model.TermsCondition,
                    CreatedBy = AppConstant.LoggedInUserID,
                    CreatedAt = DateTime.Now,
                    TenantId = AppConstant.LoggedInTenantId,
                    IsActive = true,
                    IsDelete = false,
                    TotalPrice = model.SubTotal,
                    Status = EnumValue.GetEnumValue(Estimation.New),
                };

                var res = await Add(updatedOrder);
                await ProcessWindowProducts(model.WindowProductId.ToList(), (Guid)model.EstimationId, updatedOrder.OrderId, newVersion);
                responseVm.IsSuccess = true;
                responseVm.Message = "Order updated successfully with new version.";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error occurred: {ex.Message}");
                responseVm.Message = "Error updating order: " + ex.Message;
            }

            return responseVm;
        }



        #region Helper Methods
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

                await AddWindowToOrder(windowData, windowProductData,OrderId, estimateId,Version);
                await AddProductToOrder(windowProductData, estimateId, OrderId, Version);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
                await AddComponentsToOrder((Guid)windowProductData.ProductId, estimateId,OrderId,Version);
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
            }
        }

        private async Task AddWindowToOrder(TblWindow windowData, TblWindowProduct windowProductData, Guid OrderId ,Guid estimateId, string Version)
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

        private async Task AddProductToOrder(TblWindowProduct windowProductData, Guid estimateId,Guid OrderId, string Version)
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

        private async Task AddComponentsToOrder(Guid productId, Guid estimateId,Guid OrderId, string Version)
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

                await AddComponentToOrderWindow(componentData, estimateId,OrderId,Version);
                await AddComponentMaterialsToOrder(componentData.ComponentId, estimateId, OrderId, Version   );
                if (!string.IsNullOrEmpty(Message))
                {
                    return;
                }
            }
        }

        private async Task AddComponentMaterialsToOrder(Guid componentId, Guid estimateId, Guid OrderId, string Version )
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

                await AddMaterialToOrder(materialData, estimateId,OrderId,Version);

            }
        }

        private async Task AddMaterialToOrder(TblRawMaterial materialData, Guid estimateId,Guid OrderId , string Version)
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

        private async Task AddComponentToOrderWindow(TblComponent componentData, Guid estimateId,Guid OrderId,string Version)
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
       

        private string IncrementVersion(string currentVersion)
        {
            if (string.IsNullOrEmpty(currentVersion))
            {
                return "v1"; 
            }
            int versionNumber = int.Parse(currentVersion.TrimStart('v'));
            versionNumber++;
            return "v" + versionNumber.ToString();
        }

        private async Task<TblOrder> CreateOrderRecord(CreateOrder model)
        {
            FileLogger.Log("Creating new Order...");
            TblOrder createEstimate = new TblOrder
            {
                OrderId = Guid.NewGuid(),
                Version = "v1",
                EstimateId = (Guid)model.EstimationId,
                EstimateName = model.EstimateName,
                EstimateCode = model.EstimateCode,
                Notes = model.Notes,
                DiscountAmount = model.Discount,
                DiscountCoupon = model.DiscountCoupon,
                GrandTotal = model.GrandTotal,
                JobId = model.JobId,
                TaxAmount = model.Tax,
                TermsCondition = model.TermsCondition,
                CreatedBy = AppConstant.LoggedInUserID,
                CreatedAt = DateTime.Now,
                TenantId = AppConstant.LoggedInTenantId,
                IsActive = true,
                IsDelete = false,
                TotalPrice = model.SubTotal,
                Status = EnumValue.GetEnumValue(Estimation.New),
            };

            var res = await Add(createEstimate);
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

        private async Task<bool> ValidateOrderExists(Guid estimateId, Guid jobId)
        {
            FileLogger.Log($"Checking if Order with this estimation {estimateId} already exists...");
            var isOrderExist = (await GetAll())
                .Where(c => c.EstimateId == estimateId && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false && c.JobId == jobId)
                .ToList();
            if (isOrderExist.Count > 0)
            {
                FileLogger.Log($"Order with this estimation {estimateId} already exists.");
                return true;
            }
            return false;
        }

        #endregion
    }
}
