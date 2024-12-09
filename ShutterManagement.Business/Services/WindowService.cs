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
                        foreach (var window in IsWindowInProduct)
                        {
                            window.IsActive = !window.IsActive;
                            window.ModifiedAt = DateTime.Now;
                            window.ModifiedBy = AppConstant.LoggedInUserID;
                            await windowProductService.Update(window);
                        }


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
                    responseVm.Message = AppConstant.AccessDenied;
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
                        foreach (var optionTypeId in items.OptionTypeId ?? Array.Empty<Guid>())
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

                            WindowOption windowOption = new WindowOption
                            {
                                WindowoptionId = Guid.NewGuid(),
                                OptionId = optionTypeId, 
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

                responseVm.IsSuccess = true;
                responseVm.Message = $"{AppConstant.InsertSuccessMsg}";
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
                        foreach (var window in IsWindowInProduct)
                        {
                            window.IsActive = false;
                            window.IsDelete = true;
                            window.ModifiedAt = DateTime.Now;
                            window.ModifiedBy = AppConstant.LoggedInUserID;
                            await windowProductService.Update(window);
                        }

                        IsWindowExists.IsActive = false;
                        IsWindowExists.IsDelete = true;
                        IsWindowExists.UpdatedAt = DateTime.Now;
                        IsWindowExists.ModifiedBy = AppConstant.LoggedInUserID;
                        await Update(IsWindowExists);
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
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<WindowListVm>>> GetAll_Window(Pagination filter)
        {
            IResponseVm<List<WindowListVm>> responseVm = new ResponseVm<List<WindowListVm>>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.IsDelete == false &&
                    x.IsActive == true);

                if (isTenantValid)
                {
                    var windowList = (from w in dbContext.TblWindows.OrderByDescending(x => x.CreatedAt)
                                      where w.IsDelete == false && w.TenantId == AppConstant.LoggedInTenantId
                                      select new WindowListVm
                                      {
                                          WindowId = w.WindowId,
                                          AddressId = w.AddressId,
                                          Label = w.Label,
                                          Width = w.Width,
                                          Height = w.Height,
                                          Quantity = w.Quantity,
                                          ArchHeight = w.ArchHeight,
                                          Photo = w.Photo,
                                          Notes = w.Notes,
                                          IsActive = w.IsActive,
                                          TenantId = w.TenantId,
                                          CreatedAt = w.CreatedAt,
                                          UpdatedAt = w.UpdatedAt
                                      }).ToList();

                    foreach (var item in windowList)
                    {
                        item.WindowProducts = (from wp in dbContext.TblWindowProducts.OrderByDescending(x => x.CreatedAt)
                                               join p in dbContext.TblProducts on wp.ProductId equals p.ProductId
                                               join wpt in dbContext.MstProductTypes on wp.ProductId equals wpt.ProductId
                                               join w in dbContext.TblWindows on wp.WindowId equals w.WindowId
                                               where wp.WindowId == item.WindowId && wp.IsDelete == false
                                               select new ShutterManagement.Core.ViewModels.Response.WindowProductVm
                                               {
                                                   ProductId = p.ProductId,
                                                   ProductName = p.ProductName,
                                                   IsActive = wp.IsActive,
                                                   ProductTypeId=wpt.ProductId,
                                                   ProductTypeName=wpt.ProductName,
                                                   BuildHeight=wp.BuildHeight,
                                                   BuildWidth=wp.BuildWidth,
                                                   Width=w.Width,
                                                   Height=w.Height,
                                                   Quantity=w.Quantity

                                               }).ToList();
                    }

                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = windowList
                        .Where(x =>
                            (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                            (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                (x.WindowId.ToString() == filter.SearchValue) ||
                                (x.AddressId.ToString() == filter.SearchValue) ||
                                (x.AddressId.ToString() == filter.SearchValue) ||
                                (x.Label ?? "").ToLower().Contains(filter.SearchValue) || 
                                (x.ArchHeight.ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                (x.WindowProducts.Select(x=>x.ProductName).ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
                                (x.WindowProducts.Select(x=>x.ProductTypeName).ToString() ?? "").ToLower().Contains(filter.SearchValue) ||
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

                var InsertRecords = model.WindowProducts.Where(a1 => !existingProducts.Any(a2 => model.WindowId == a2.WindowId && a1.ProductId == a2.ProductId && a1.ProductTypeId == a2.ProductTypeId 
                && a1.BuildWidth == a2.BuildWidth && a1.BuildHeight == a2.BuildHeight)).ToList();
                var DeleteRecords = existingProducts.Where(a1 => !model.WindowProducts.Any(a2 => model.WindowId == a2.WindowId && a1.ProductId == a2.ProductId && a1.ProductTypeId == a2.ProductTypeId)).ToList();

                foreach (var product in DeleteRecords)
                {
                    var data = windowProductService.GetAllQuerable().Where(x => x.WindowProductId == product.WindowProductId
                             && x.IsDelete == false).FirstOrDefault();
                    if (data != null)
                    {
                        data.IsDelete = true;
                        data.IsActive = false;
                        data.ModifiedBy = AppConstant.LoggedInUserID;
                        data.ModifiedAt = DateTime.Now;
                        await windowProductService.Update(data);
                    }
                }

                foreach (var product in InsertRecords)
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
