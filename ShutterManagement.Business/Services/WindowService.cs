using Microsoft.Extensions.Logging;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class WindowService : BaseRepository<ShutterDBContext, TblWindow>, IWindowService
    {
        private readonly ITenantService tenantService;
        private readonly IWindowProductService windowProductService;
        private readonly IWindowOptionService windowOptionService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IEstimateWindowService estimateWindowService;
  

        public WindowService(ITenantService tenantService, 
            IWindowProductService windowProductService, IWindowOptionService windowOptionService, ClaimsProperty claimsProperty,
            IEstimateWindowService estimateWindowService)
        {
            this.tenantService = tenantService;
            this.windowProductService = windowProductService;
            this.windowOptionService = windowOptionService;
            this.claimsProperty = claimsProperty;
            this.estimateWindowService = estimateWindowService;
        
        }

        public async Task<IResponseVm<string>> ActiveInActiveWindow(Guid WindowId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log("ActiveInActiveWindow process started.");

            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && !(bool)x.IsDelete  && (bool)x.IsActive);

                if (IsTenant)
                {
                    FileLogger.Log($"Tenant validation passed for TenantId: {claimsProperty.GetTenantId()}");

                    var IsWindowExists = GetAllQuerable()
                        .FirstOrDefault(c => c.WindowId == WindowId && c.TenantId == claimsProperty.GetTenantId() && !(bool)c.IsDelete);

                    if (IsWindowExists != null)
                    {
                        FileLogger.Log($"Window found with ID: {WindowId}");

                        var IsWindowInProduct = windowProductService
                            .GetAllQuerable()
                            .Where(x => x.WindowId == WindowId && !(bool)x.IsDelete  && x.TenantId == claimsProperty.GetTenantId())
                            .ToList();

                        if (IsWindowInProduct?.Count > 0)
                        {
                            var IsWindowOption = windowOptionService
                                .GetAllQuerable()
                                .Where(x => x.WindowId == WindowId && !(bool)x.IsDelete  && x.TenantId == claimsProperty.GetTenantId())
                                .ToList();

                            // Update window options status
                            if (IsWindowOption?.Count > 0)
                            {
                                foreach (var option in IsWindowOption)
                                {
                                    option.IsActive = !option.IsActive;
                                    option.ModifiedAt = DateTime.Now;
                                    option.ModifiedBy = AppConstant.LoggedInUserID;
                                    await windowOptionService.Update(option);
                                    FileLogger.Log($"Updated WindowOption ID: {option.WindowoptionId}, New Status: {option.IsActive}");
                                }
                            }

                            // Update window products status
                            foreach (var product in IsWindowInProduct)
                            {
                                product.IsActive = !product.IsActive;
                                product.ModifiedAt = DateTime.Now;
                                product.ModifiedBy = AppConstant.LoggedInUserID;
                                await windowProductService.Update(product);
                                FileLogger.Log($"Updated WindowProduct ID: {product.WindowProductId}, New Status: {product.IsActive}");
                            }
                        }

                        // Update window status
                        IsWindowExists.IsActive = !IsWindowExists.IsActive;
                        IsWindowExists.UpdatedAt = DateTime.Now;
                        IsWindowExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsWindowExists);

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                        FileLogger.Log($"Window status updated successfully. Window ID: {WindowId}, New Status: {IsWindowExists.IsActive}");
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                        FileLogger.Log($"Window not found with ID: {WindowId}");
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    FileLogger.Log($"Tenant validation failed for TenantId: {claimsProperty.GetTenantId()}");
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                FileLogger.Log("Exception occurred in ActiveInActiveWindow process.", ex);
            }

            FileLogger.Log("ActiveInActiveWindow process ended.");
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Window(CreateWindow model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log("Create Window process started.");

            try
            {
                var isTenant = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && !(bool)x.IsDelete  && (bool)x.IsActive);

                if (!isTenant)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    FileLogger.Log("Tenant validation failed in Create_Window.");
                    return responseVm;
                }
                if(model.AddressId == null || model.AddressId == Guid.Empty)
                {
                    responseVm.Message = "AddressId is required.Please update job detail with address";
                    FileLogger.Log("AddressId is required in Create_Window.");
                    return responseVm;
                }

                var IsLabelExists = await AnyAsync(x => x.Label == model.Label.Trim() && x.JobId == model.JobId  && !(bool)x.IsDelete  && x.TenantId == claimsProperty.GetTenantId());

                if (IsLabelExists)
                {
                    responseVm.Message = "Label" + AppConstant.AlreadyExists;
                    FileLogger.Log($"Label already exists: {model.Label}");
                    return responseVm;
                }
                string arrayString = string.Join(", ", model.Photo).Trim();
                TblWindow window = new TblWindow
                {
                    WindowId = Guid.NewGuid(),
                    AddressId = model.AddressId,
                    Notes = model.Notes,
                    Label = model.Label.Trim(),
                    Width = model.Width,
                    Quantity = model.Quantity,
                    Height = model.Height,
                    ArchHeight = model.ArchHeight,
                    Photo = arrayString,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false,
                    JobId = model.JobId,
                    IsRated = true,
                    TenantId = claimsProperty.GetTenantId()
                };

                await Add(window);
                FileLogger.Log($"New window created with ID: {window.WindowId}");

                foreach (var items in model.WindowProducts)
                {
                    var isWindowProductExists = await windowProductService.AnyAsync(x =>
                        x.WindowProductId == items.ProductId &&
                        x.ProductTypeId == items.ProductTypeId &&
                        x.WindowId == window.WindowId &&
                        !(bool)x.IsDelete  &&
                        x.TenantId == claimsProperty.GetTenantId());

                    if (!isWindowProductExists)
                    {
                        TblWindowProduct windowProduct = new TblWindowProduct
                        {
                            WindowProductId = Guid.NewGuid(),
                            WindowId = window.WindowId,
                            ProductId = items.ProductId,
                            ProductTypeId = items.ProductTypeId,
                            BuildWidth = items.BuildWidth,
                            BuildHeight = items.BuildHeight,
                            CreatedAt = DateTime.Now,
                            CreatedBy = AppConstant.LoggedInUserID,
                            SortOrder=items.SortOrder,
                            IsActive = true,
                            IsDelete = false,
                            TenantId = claimsProperty.GetTenantId()
                        };

                        await windowProductService.Add(windowProduct);
                        FileLogger.Log($"New window product added with ID: {windowProduct.WindowProductId}");

                        if (items.OptionTypeId != null && items.OptionTypeId.Count > 0)
                        {
                            foreach (var optionId in items.OptionTypeId)
                            {
                                Guid.TryParse(optionId, out var optionTypeId);
                                if (optionTypeId != Guid.Empty)
                                {
                                    WindowOption windowOption = new WindowOption
                                    {
                                        WindowoptionId = Guid.NewGuid(),
                                        OptionTypeId = optionTypeId,
                                        WindowProductId = windowProduct.WindowProductId,
                                        WindowId = window.WindowId,
                                        IsActive = true,
                                        IsDelete = false,
                                        CreatedBy = AppConstant.LoggedInUserID,
                                        TenantId = claimsProperty.GetTenantId(),
                                        AdditionalCost = items.AdditionalCost
                                    };

                                    await windowOptionService.Add(windowOption);
                                    FileLogger.Log($"New window option added with ID: {windowOption.WindowoptionId}");
                                }
                            }
                        }
                    }
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;
                FileLogger.Log($"Create_Window process completed successfully for Window ID: {window.WindowId}");
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
                FileLogger.Log("Exception occurred in Create_Window process.", ex);
            }

            FileLogger.Log("Create_Window process ended.");
            return responseVm;
        }

        public async Task<IResponseVm<List<WindowsList>>> GetAll_Window(Pagination filter)
        {
            IResponseVm<List<WindowsList>> responseVm = new ResponseVm<List<WindowsList>>();

            try
            {
                FileLogger.Log("Fetching all windows started.");

                // Tenant validation
                if (!await IsTenantValid())
                {
                    FileLogger.Log("Tenant validation failed.");
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                FileLogger.Log("Tenant validated successfully.");

                // Fetch data
                var windowsData = FetchWindowsData();
                FileLogger.Log($"Fetched total {windowsData.Count} windows before applying filters.");

                // Apply filters
                var filteredWindows = ApplyFilters(windowsData, filter);
                FileLogger.Log($"Total {filteredWindows.Count} windows after applying search and status filters.");

                // Paginate results
                var paginatedRecords = Paginate.Pagination(filteredWindows, filter);
                FileLogger.Log($"Total {paginatedRecords.Count} windows after pagination. PageNo: {filter.PageNo}, RecordsPerPage: {filter.RecordPerPage}");

                // Prepare response
                responseVm.IsSuccess = true;
                responseVm.Data = paginatedRecords;
                responseVm.TotalRecords = filteredWindows.Count;
                responseVm.Message = paginatedRecords.Any() ? AppConstant.FetchSuccessMsg : AppConstant.NoDataFound;

                FileLogger.Log("Fetching windows operation completed successfully.");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception occurred while fetching windows. Error: {ex}");
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }


        public async Task<IResponseVm<string>> DeleteWindow(Guid WindowId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                FileLogger.Log($"Delete window process started for WindowId: {WindowId}.");

                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false && (bool)x.IsActive == true);
                List<TblEstimate> Estimatestatus = new List<TblEstimate>();
                var WindowUsedInEstimation = estimateWindowService.GetAllQuerable().Where(x => x.WindowId == WindowId && x.IsDelete == false).FirstOrDefault();
                if (WindowUsedInEstimation != null)
                {
                    Estimatestatus = dbContext.TblEstimates.Where(x => x.EstimateId == WindowUsedInEstimation.EstimateId && x.IsDelete == false).ToList();
                    if (Estimatestatus.Any())
                    {
                        bool isInGenerateOrderStatus = Estimatestatus
                            .Any(x => x.Status == EnumValue.GetEnumValue(Estimation.GenerateOrder));

                        if (isInGenerateOrderStatus)
                        {
                            responseVm.Message = "Window used in Estimation. It's not allowed to delete.";
                            return responseVm;
                        }
                        foreach (var item in Estimatestatus)
                        {
                                item.IsActive = false;
                                item.IsDelete = true;
                                item.UpdatedAt = DateTime.Now;
                                item.ModifiedBy = claimsProperty.GetTenantId();
                                dbContext.TblEstimates.Update(item);
                        }
                    }
                }
                if (IsTenant)
                {
                    var IsWindowExists = GetAllQuerable()
                        .Where(c => c.WindowId == WindowId
                                    && c.TenantId == claimsProperty.GetTenantId()
                                    && c.IsDelete == false)
                        .FirstOrDefault();
                    if (IsWindowExists != null)
                    {
                        FileLogger.Log($"Window found with WindowId: {WindowId}.");

                        var IsWindowInProduct = windowProductService
                            .GetAllQuerable().Where(x => x.WindowId == WindowId &&
                                            x.IsDelete == false &&
                                            x.TenantId == claimsProperty.GetTenantId())
                            .ToList();

                        if (IsWindowInProduct != null && IsWindowInProduct.Count > 0)
                        {
                            FileLogger.Log($"Found {IsWindowInProduct.Count} products for this window.");

                            var IsWindowOption = windowOptionService
                                .GetAllQuerable().Where(x => x.WindowId == WindowId &&
                                             x.IsDelete == false &&
                                             x.TenantId == claimsProperty.GetTenantId())
                                .ToList();

                            if (IsWindowOption != null && IsWindowOption.Count > 0)
                            {
                                FileLogger.Log($"Found {IsWindowOption.Count} options for this window.");

                                // Window options status change
                                foreach (var option in IsWindowOption)
                                {
                                    option.IsActive = false;
                                    option.IsDelete = true;
                                    option.ModifiedAt = DateTime.Now;
                                    option.ModifiedBy = AppConstant.LoggedInUserID;
                                    await windowOptionService.Update(option);
                                }
                            }

                            // Window product status change
                            foreach (var product in IsWindowInProduct)
                            {
                                product.IsActive = false;
                                product.IsDelete = true;
                                product.ModifiedAt = DateTime.Now;
                                product.ModifiedBy = AppConstant.LoggedInUserID;
                                await windowProductService.Update(product);
                            }
                        }
                        var Estimation = estimateWindowService.GetAllQuerable().Where(x => x.WindowId == WindowId &&
                        x.IsActive == true && x.IsDelete == false).ToList();
                        if (Estimation != null && Estimation.Count > 0)
                        {
                            foreach (var item in Estimation)
                            {
                                item.IsActive = false;
                                item.IsDelete = true;
                                item.UpdatedAt = DateTime.Now;
                                item.ModifiedBy = AppConstant.LoggedInUserID;
                                dbContext.Update(item);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                        // Window status change
                        IsWindowExists.IsActive = false;
                        IsWindowExists.IsDelete = true;
                        IsWindowExists.UpdatedAt = DateTime.Now;
                        IsWindowExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsWindowExists);

                        FileLogger.Log($"Window {WindowId} marked for deletion.");

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.DeleteSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception during delete window process for WindowId: {WindowId}. Error: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_Window(UpdateWindow model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                FileLogger.Log("Window update process started.");

                // Trim and validate input
                model.Label = model.Label?.Trim();

                // Step 1: Validate Tenant
                if (!await IsTenantValid())
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                // Step 2: Check for duplicate Job Name
                if (await IsWindowExists(model))
                {
                    responseVm.Message = $"Window LabelName {AppConstant.AlreadyExists}";
                    return responseVm;
                }

                // Step 3: Fetch existing window
                var existingWindow = await GetExistingWindow(model.WindowId);
                if (existingWindow == null)
                {
                    responseVm.Message = $"Window {AppConstant.NoDataFound}";
                    return responseVm;
                }

                // Step 4: Update Window
                UpdateWindowData(existingWindow, model);
                await Update(existingWindow);

                // Step 5: Update Window Products
                await UpdateWindowProducts(model);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
                FileLogger.Log("Window update process completed successfully.");
            }
            catch (Exception ex)
            {
                FileLogger.Log("Exception in window update process: ", ex);
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }


        #region private methods
        private List<WindowsList> FetchWindowsData()
        {
            return dbContext.TblWindows
                .Where(w => w.IsDelete == false && w.TenantId == claimsProperty.GetTenantId())
                .GroupJoin(dbContext.TblWindowProducts,
                    w => w.WindowId,
                    wp => wp.WindowId,
                    (w, windowProducts) => new { w, windowProducts })
                .SelectMany(
                    x => x.windowProducts.Where(x=>x.IsDelete == false).OrderBy(x=>x.SortOrder).DefaultIfEmpty(),
                    (x, wp) => new { x.w, wp })
                .GroupJoin(dbContext.WindowOptions,
                    x => x.wp.WindowProductId,
                    wop => wop.WindowProductId,
                    (x, windowOptions) => new { x.w, x.wp, windowOptions })
                .SelectMany(
                    x => x.windowOptions.Where(x=>x.IsDelete == false).DefaultIfEmpty(),
                    (x, wop) => new { x.w, x.wp, wop })
                //.Join(dbContext.TblOptionsTypes,
                //    x => x.wop.OptionTypeId,
                //    op => op.OptionTypeId,
                //    (x, op) => new { x.w, x.wp, x.wop, op })
                .GroupBy(x => x.w.WindowId)
                .Select(g => new WindowsList
                {
                    WindowId = g.Key,
                    AddressId = g.FirstOrDefault().w.AddressId,
                    JobId = g.FirstOrDefault().w.JobId,
                    Label = g.FirstOrDefault().w.Label,
                    CreatedAt = g.FirstOrDefault().w.CreatedAt,
                    Quantity = g.FirstOrDefault().w.Quantity,
                    ArchHeight = g.FirstOrDefault().w.ArchHeight,
                    Height = g.FirstOrDefault().w.Height,
                    Width = g.FirstOrDefault().w.Width,
                    Notes = g.FirstOrDefault().w.Notes,
                    IsRated = g.FirstOrDefault().w.IsRated,
                    Photo = g.FirstOrDefault().w.Photo.Trim().Split( "," , StringSplitOptions.RemoveEmptyEntries).ToArray(),
                    TenantId = g.FirstOrDefault().w.TenantId,
                    IsActive = g.FirstOrDefault().w.IsActive,
                    ProductDetails = g.GroupBy(x => x.wp.WindowProductId)
                        .Select(p => new ProductList
                        {
                            WindowProductId = p.Key,
                            ProductTypeId = p.FirstOrDefault().wp.ProductTypeId,
                            ProductTypeName = p.FirstOrDefault().wp.ProductType.ProductName ?? "",
                            ProductId = (Guid)p.FirstOrDefault().wp.ProductId,
                            ProductName = p.FirstOrDefault().wp.Product.ProductName ?? "",
                            BuildHeight = p.FirstOrDefault().wp.BuildHeight,
                            BuildWidth = p.FirstOrDefault().wp.BuildWidth,
                            SortOrder=p.FirstOrDefault().wp.SortOrder,
                            
                            
                            WindowOptions = p.Select(o => new WindowOptions
                            {
                                WindowProductId = o.wp.WindowProductId,
                                OptionTypeId = o.wop.OptionTypeId.ToString(),
                                //OptionName = o.op.OptionType,
                                //AdditionalCost = o.op.AdditionalCost.GetValueOrDefault(),
                            }).ToList()
                        }).ToList()
                }).OrderByDescending(s=>s.CreatedAt)

                .ToList();
        }

        private List<WindowsList> ApplyFilters(List<WindowsList> windowsData, Pagination filter)
        {
            filter.SearchValue = filter.SearchValue?.Trim().ToLower();
            bool isStatusFilterActive = filter.Status == 1;
            bool isStatusFilterInactive = filter.Status == 0;

            return windowsData.Where(x =>
                // Status filter
                (isStatusFilterInactive ? !(bool)x.IsActive : (isStatusFilterActive ? (bool)x.IsActive : true)) &&
                // Search filter
                (string.IsNullOrEmpty(filter.SearchValue) ||
                 IsWindowMatched(x, filter.SearchValue) ||
                 x.ProductDetails.Any(p => IsProductMatched(p, filter.SearchValue))
                )
            ).ToList();
        }

        private async Task<bool> IsTenantValid()
        {
            FileLogger.Log("Validating Tenant...");
            return await tenantService.AnyAsync(x =>
                x.TenantId == claimsProperty.GetTenantId() &&
                !(bool)x.IsDelete &&
                (bool)x.IsActive);
        }

        private async Task<bool> IsWindowExists(UpdateWindow model)
        {
            FileLogger.Log("Checking for duplicate Window Name...");
            return await AnyAsync(x =>
                x.Label == model.Label &&
                x.JobId == model.JobId &&
                x.WindowId != model.WindowId &&
                !(bool)x.IsDelete &&
                x.TenantId == claimsProperty.GetTenantId());
        }

        private async Task<TblWindow> GetExistingWindow(Guid windowId)
        {
            FileLogger.Log("Fetching existing window...");
            return (await GetAll())
                .FirstOrDefault(c =>
                    c.WindowId == windowId &&
                    c.TenantId == claimsProperty.GetTenantId() &&
                    !(bool)c.IsDelete);
        }

        private void UpdateWindowData(TblWindow existingWindow, UpdateWindow model)
        {
            string arrayString = string.Join(", ", model.Photo);
            FileLogger.Log("Updating window data...");
            existingWindow.Label = model.Label;
            existingWindow.Height = model.Height;
            existingWindow.Photo = arrayString;
            existingWindow.Quantity = model.Quantity;
            existingWindow.Width = model.Width;
            existingWindow.Notes = model.Notes;
            existingWindow.ArchHeight = model.ArchHeight;
            existingWindow.JobId = model.JobId;
            existingWindow.UpdatedAt = DateTime.Now;
            existingWindow.IsRated = true;
            existingWindow.ModifiedBy = AppConstant.LoggedInUserID;
        }

        private async Task UpdateWindowProducts(UpdateWindow model)
        {
            FileLogger.Log("Updating window products...");

            // Fetch existing products
            var existingProducts = windowProductService.GetAllQuerable()
                .Where(x => x.WindowId == model.WindowId && !(bool)x.IsDelete)
                .ToList();

            // Determine products to insert and delete
            var productsToInsert = model.WindowProducts;
              

            var productsToDelete = existingProducts
                .Where(e => e.WindowId == model.WindowId && e.IsDelete == false)
                .ToList();

            // Delete products and their options
            foreach (var product in productsToDelete)
            {
                await DeleteProductWithOptions(product);
            }

            // Insert new products
            foreach (var product in productsToInsert)
            {
                await InsertProductWithOptions(model.WindowId, product);
            }

            FileLogger.Log("Window products updated successfully.");
        }

        private async Task DeleteProductWithOptions(TblWindowProduct product)
        {
            FileLogger.Log($"Deleting product: {product.WindowProductId}");

            product.IsDelete = true;
            product.IsActive = false;
            product.ModifiedBy = AppConstant.LoggedInUserID;
            product.ModifiedAt = DateTime.Now;
            await windowProductService.Update(product);

            var existingOptions = windowOptionService.GetAllQuerable()
                .Where(x => x.WindowProductId == product.WindowProductId && !(bool)x.IsDelete)
                .ToList();

            foreach (var option in existingOptions)
            {
                option.IsDelete = true;
                option.IsActive = false;
                option.ModifiedBy = AppConstant.LoggedInUserID;
                option.ModifiedAt = DateTime.Now;
                await windowOptionService.Update(option);
            }
        }

        private async Task InsertProductWithOptions(Guid windowId, Core.ViewModels.Request.WindowProductVm product)
        {
            FileLogger.Log("Inserting new product...");

            var newProduct = new TblWindowProduct
            {
                WindowProductId = Guid.NewGuid(),
                WindowId = windowId,
                ProductId = product.ProductId,
                ProductTypeId = product.ProductTypeId,
                BuildHeight = product.BuildHeight,
                BuildWidth = product.BuildWidth,
                SortOrder = product.SortOrder,
                CreatedAt = DateTime.Now,
                CreatedBy = AppConstant.LoggedInUserID,
                IsActive = true,
                IsDelete = false,
                TenantId = claimsProperty.GetTenantId()
            };

            await windowProductService.Add(newProduct);
            if (product.OptionTypeId != null && product.OptionTypeId.Count > 0)
            {
                foreach (var optionId in product.OptionTypeId)
                {
                    Guid.TryParse(optionId, out var optionTypeId);
                    if (optionTypeId != Guid.Empty)
                    {
                        var newOption = new WindowOption
                        {
                            WindowoptionId = Guid.NewGuid(),
                            OptionTypeId = optionTypeId,
                            WindowProductId = newProduct.WindowProductId,
                            WindowId = windowId,
                            CreatedBy = AppConstant.LoggedInUserID,
                            TenantId = claimsProperty.GetTenantId(),
                            IsActive = true,
                            IsDelete = false,
                            AdditionalCost = product.AdditionalCost
                        };

                        await windowOptionService.Add(newOption);
                    }
                }
            }
        }

        bool IsWindowMatched(WindowsList window, string searchValue)
        {
            return window.WindowId.ToString() == searchValue ||
                   (window.JobId?.ToString() ?? "").Contains(searchValue) ||
                   (window.Label?.ToLower().Contains(searchValue) ?? false) ||
                   (window.Notes?.ToLower().Contains(searchValue) ?? false) ||
                   (window.Width.ToString() ?? "").Contains(searchValue) ||
                   (window.Height.ToString() ?? "").Contains(searchValue);
        }

        bool IsProductMatched(ProductList product, string searchValue)
        {
            return product.ProductId.ToString() == searchValue ||
                   (product.ProductName?.ToLower().Contains(searchValue) ?? false) ||
                   (product.ProductTypeName?.ToLower().Contains(searchValue) ?? false) ||
                   (product.ProductTypeId.ToString() ?? "").Contains(searchValue) ||
                   product.WindowProductId.ToString() == searchValue ||
                   product.WindowOptions.Any(o =>
                       (o.OptionTypeId?.ToLower().Contains(searchValue) ?? false) ||
                       (o.OptionName?.ToLower().Contains(searchValue) ?? false)
                   );
        }

        #endregion
    }
}
