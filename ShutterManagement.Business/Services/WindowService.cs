using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class WindowService : BaseRepository<BtprojecShuttermanagementDevContext, TblWindow>, IWindowService
    {
        private readonly ITenantService tenantService;
        private readonly IWindowProductService windowProductService;
        private readonly IWindowOptionService windowOptionService;

        public WindowService(ITenantService tenantService, IWindowProductService windowProductService,IWindowOptionService windowOptionService)
        {
            this.tenantService = tenantService;
            this.windowProductService = windowProductService;
            this.windowOptionService = windowOptionService;
        }

        public async Task<IResponseVm<string>> ActiveInActiveWindow(Guid WindowId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {
                    var IsWindowExists = GetAllQuerable()
                        .Where(c => c.WindowId == WindowId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();

                    if (IsWindowExists != null)
                    {
                        var IsWindowInProduct = windowProductService
                         .GetAllQuerable().Where(x => x.WindowId == WindowId &&
                                         x.IsDelete == false &&
                                         x.TenantId == AppConstant.LoggedInTenantId)
                         .ToList();
                        if (IsWindowInProduct != null && IsWindowInProduct.Count > 0)
                        {
                           
                            var IsWindowOption = windowOptionService
                            .GetAllQuerable().Where(x => x.WindowId == WindowId &&
                                         x.IsDelete == false &&
                                         x.TenantId == AppConstant.LoggedInTenantId)
                            .ToList();
                            if (IsWindowOption != null && IsWindowOption.Count > 0)
                            {
                                //Window options status change
                                foreach (var option in IsWindowOption)
                                {
                                    option.IsActive = !option.IsActive;
                                    option.ModifiedAt = DateTime.Now;
                                    option.ModifiedBy = AppConstant.LoggedInUserID;
                                    await windowOptionService.Update(option);
                                }
                            }
                            //Window product status change
                            foreach (var product in IsWindowInProduct)
                            {
                                product.IsActive = !product.IsActive;
                                product.ModifiedAt = DateTime.Now;
                                product.ModifiedBy = AppConstant.LoggedInUserID;
                                await windowProductService.Update(product);
                            }
                        }

                        //Window status change
                        IsWindowExists.IsActive = !IsWindowExists.IsActive;
                        IsWindowExists.UpdatedAt = DateTime.Now;
                        IsWindowExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsWindowExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
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
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Window(CreateWindow model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenant = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (!isTenant)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                TblWindow window = new TblWindow
                {
                    WindowId = Guid.NewGuid(),
                    Notes = model.Notes,
                    Label = model.Label,
                    Width = model.Width,
                    Quantity = model.Quantity,
                    Height = model.Height,
                    ArchHeight = model.ArchHeight,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false,
                    JobId = model.JobId,
                    TenantId = AppConstant.LoggedInTenantId
                };

                var res = await Add(window);

                foreach (var items in model.WindowProducts)
                {
                    var isWindowProductExists = await windowProductService.AnyAsync(x =>
                        x.WindowProductId == items.ProductId &&
                        x.ProductTypeId == items.ProductTypeId &&
                        x.WindowId == window.WindowId &&
                        x.IsDelete == false &&
                        x.TenantId == AppConstant.LoggedInTenantId);

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
                            IsActive = true,
                            IsDelete = false,
                            TenantId = AppConstant.LoggedInTenantId
                        };

                        await windowProductService.Add(windowProduct);

                        if (items.OptionTypeId != null)
                        {
                            foreach (var optionId in items.OptionTypeId)
                            {
                                WindowOption windowOption = new WindowOption
                                {
                                    WindowoptionId = Guid.NewGuid(),
                                    OptionId = optionId,
                                    WindowProductId = windowProduct.WindowProductId,
                                    WindowId = window.WindowId,
                                    IsActive = true,
                                    IsDelete = false,
                                    CreatedBy = AppConstant.LoggedInUserID,
                                    TenantId = AppConstant.LoggedInTenantId,
                                    AdditionalCost = items.AdditionalCost
                                };

                                await windowOptionService.Add(windowOption);

                            }
                        }
                    }
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteWindow(Guid WindowId)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {
                    var IsWindowExists = GetAllQuerable()
                        .Where(c => c.WindowId == WindowId
                                    && c.TenantId == AppConstant.LoggedInTenantId
                                    && c.IsDelete == false)
                        .FirstOrDefault();
                    if (IsWindowExists != null)
                    {
                        var IsWindowInProduct = windowProductService
                         .GetAllQuerable().Where(x => x.WindowId == WindowId &&
                                         x.IsDelete == false &&
                                         x.TenantId == AppConstant.LoggedInTenantId)
                         .ToList();
                        if (IsWindowInProduct != null && IsWindowInProduct.Count > 0)
                        {

                            var IsWindowOption = windowOptionService
                            .GetAllQuerable().Where(x => x.WindowId == WindowId &&
                                         x.IsDelete == false &&
                                         x.TenantId == AppConstant.LoggedInTenantId)
                            .ToList();
                            if (IsWindowOption != null && IsWindowOption.Count > 0)
                            {
                                //Window options status change
                                foreach (var option in IsWindowOption)
                                {
                                    option.IsActive = false;
                                    option.IsDelete = true;
                                    option.ModifiedAt = DateTime.Now;
                                    option.ModifiedBy = AppConstant.LoggedInUserID;
                                    await windowOptionService.Update(option);
                                }
                            }
                            //Window product status change
                            foreach (var product in IsWindowInProduct)
                            {
                                product.IsActive = false;
                                product.IsDelete = true;
                                product.ModifiedAt = DateTime.Now;
                                product.ModifiedBy = AppConstant.LoggedInUserID;
                                await windowProductService.Update(product);
                            }
                        }

                        //Window status change
                        IsWindowExists.IsActive = false;
                        IsWindowExists.IsDelete = true;
                        IsWindowExists.UpdatedAt = DateTime.Now;
                        IsWindowExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsWindowExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
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
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<WindowsList>>> GetAll_Window(Pagination filter)
        {
            IResponseVm<List<WindowsList>> responseVm = new ResponseVm<List<WindowsList>>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (isTenantValid)
                {

                   var WindowsData = (from w in dbContext.TblWindows.OrderBy(x=>x.Label)
                                     join wp in dbContext.TblWindowProducts on w.WindowId equals wp.WindowId
                                     where w.IsDelete == false && w.TenantId == AppConstant.LoggedInTenantId
                                     select new WindowsList
                                     {
                                         WindowId = w.WindowId,
                                         Label = w.Label,
                                         Quantity = w.Quantity,
                                         ArchHeight = w.ArchHeight,
                                         Height = w.Height,
                                         Width = w.Width,
                                         Notes = w.Notes,
                                         Photo = w.Photo,
                                         ProductId = (Guid)wp.ProductId,
                                         BuildHeight = wp.BuildHeight,
                                         BuildWidth = wp.BuildWidth,
                                         ProductName = wp.Product.ProductName,
                                         ProductTypeId = (Guid)wp.ProductTypeId,
                                         ProductTypeName = wp.ProductType.ProductName,
                                         IsActive = w.IsActive
                                     }).ToList();

                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = WindowsData
                        .Where(x =>
                            (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                            (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                (x.WindowId.ToString() == filter.SearchValue) ||
                                (x.AddressId.ToString() == filter.SearchValue) ||
                                (x.AddressId.ToString() == filter.SearchValue) ||
                                (x.Label ?? "").ToLower().Contains(filter.SearchValue) || 
                                (x.ArchHeight.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                (x.ProductName?.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                (x.ProductTypeName?.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                (x.Notes ?? "").ToLower().Contains(filter.SearchValue) ||
                                (x.Width.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                (x.Height.ToString() ?? "").ToLower().Contains(filter.SearchValue))
                        ).ToList();

                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.IsSuccess = true;
                    if (TakenRecords != null && TakenRecords.Count > 0)
                    {
                        responseVm.Data = TakenRecords;
                        responseVm.Message = AppConstant.FetchSuccessMsg;
                        responseVm.TotalRecords = FilterRecords.Count;
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
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_Window(UpdateWindow model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var existingWindow = GetAllQuerable()
                    .FirstOrDefault(c =>
                        c.WindowId == model.WindowId &&
                        c.TenantId == AppConstant.LoggedInTenantId &&
                        c.IsDelete == false);

                if (existingWindow == null)
                {
                    responseVm.Message = "Window " + AppConstant.NoDataFound;
                    return responseVm;
                }

                existingWindow.Label = model.Label;
                existingWindow.Height = model.Height;
                existingWindow.Photo = model.Photo;
                existingWindow.Quantity = model.Quantity;
                existingWindow.Width = model.Width;
                existingWindow.Notes = model.Notes;
                existingWindow.ArchHeight = model.ArchHeight;
                existingWindow.JobId = model.JobId;
                existingWindow.UpdatedAt = DateTime.Now;
                existingWindow.ModifiedBy = AppConstant.LoggedInUserID;

                await Update(existingWindow);

                var existingProducts = windowProductService.GetAllQuerable()
                    .Where(x => x.WindowId == model.WindowId && x.IsDelete == false)
                    .ToList();

                var insertProducts = model.WindowProducts.Where(a1 =>
                    !existingProducts.Any(a2 =>
                        a1.ProductId == a2.ProductId &&
                        a1.ProductTypeId == a2.ProductTypeId &&
                        a1.BuildWidth == a2.BuildWidth &&
                        a1.BuildHeight == a2.BuildHeight))
                    .ToList();

                var deleteProducts = existingProducts.Where(a1 =>
                    !model.WindowProducts.Any(a2 =>
                        a1.ProductId == a2.ProductId &&
                        a1.ProductTypeId == a2.ProductTypeId &&
                        a1.BuildWidth == a2.BuildWidth &&
                        a1.BuildHeight == a2.BuildHeight))
                    .ToList();

                foreach (var product in deleteProducts)
                {
                    product.IsDelete = true;
                    product.IsActive = false;
                    product.ModifiedBy = AppConstant.LoggedInUserID;
                    product.ModifiedAt = DateTime.Now;
                    await windowProductService.Update(product);

                    var existingOptions = windowOptionService.GetAllQuerable()
                        .Where(x => x.WindowProductId == product.WindowProductId && x.IsDelete == false)
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

                foreach (var product in insertProducts)
                {
                    var newProduct = new TblWindowProduct
                    {
                        WindowProductId = Guid.NewGuid(),
                        WindowId = model.WindowId,
                        ProductId = product.ProductId,
                        ProductTypeId = product.ProductTypeId,
                        CreatedAt = DateTime.Now,
                        BuildHeight = product.BuildHeight,
                        BuildWidth = product.BuildWidth,
                        CreatedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false,
                        TenantId = AppConstant.LoggedInTenantId
                    };

                    await windowProductService.Add(newProduct);

                    foreach (var optionId in product.OptionTypeId ?? new List<Guid>())
                    {
                        var newOption = new WindowOption
                        {
                            WindowoptionId = Guid.NewGuid(),
                            OptionId = optionId,
                            WindowProductId = newProduct.WindowProductId,
                            WindowId = model.WindowId,
                            IsActive = true,
                            IsDelete = false,
                            CreatedBy = AppConstant.LoggedInUserID,
                            TenantId = AppConstant.LoggedInTenantId,
                            AdditionalCost = product.AdditionalCost
                        };

                        await windowOptionService.Add(newOption);
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
    }
}
