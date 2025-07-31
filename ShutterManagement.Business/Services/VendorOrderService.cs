using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class VendorOrderService : BaseRepository<ShutterDBContext, TblVendorOrder>, IVendorOrderService
    {
        private readonly ITenantService _tenantService;
        private readonly IMaterial materialService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IVendorItemService vendorItemService;
        private readonly IVendorService vendorService;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly IJobService jobService;
        private readonly IConverter converter;


        public VendorOrderService(ITenantService tenantService, IMaterial materialService, ClaimsProperty claimsProperty,
            IVendorItemService vendorItemService ,IVendorService vendorService,
            ITenantBrandingService tenantBrandingService,IJobService jobService, IConverter converter)
        {
            _tenantService = tenantService;
            this.materialService = materialService;
            this.claimsProperty = claimsProperty;
            this.vendorItemService = vendorItemService;
            this.vendorService = vendorService;
            this.tenantBrandingService = tenantBrandingService;
            this.jobService = jobService;
            this.converter = converter;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid VendorOrderId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await _tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsVendorOrderExists = GetAllQuerable()
                       .Where(c => c.VendorOrderId == VendorOrderId
                                   && c.TenentId == claimsProperty.GetTenantId()
                                   && c.IsDelete == false)
                       .FirstOrDefault();
                    if (IsVendorOrderExists != null)
                    {
                        IsVendorOrderExists.IsActive = !IsVendorOrderExists.IsActive;
                        IsVendorOrderExists.UpdatedAt = DateTime.Now;
                        IsVendorOrderExists.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = Update(IsVendorOrderExists);

                        var vendorOrderItems = vendorItemService.GetAllQuerable()
                            .Where(x => x.IsDelete == false && x.VendorOrderId == VendorOrderId).ToList();

                        foreach (var items in vendorOrderItems)
                        {
                            items.UpdatedAt = DateTime.Now;
                            items.IsActive = IsVendorOrderExists.IsActive;
                            items.ModifiedBy = AppConstant.LoggedInUserID.ToString();
                            await vendorItemService.Update(items);
                        }
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
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_VendorOrder(CreateVendorOrder model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantValid = await _tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true
                    && x.IsDelete == false
                  );

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var isOrderNumberExist = await AnyAsync(x =>
                    x.IsDelete == false &&
                    x.IsActive == true &&
                    x.OrderNumber == model.OrderNumber.Trim() &&
                    x.TenentId == claimsProperty.GetTenantId());

                if (isOrderNumberExist)
                {
                    responseVm.Message = $"Order Number {AppConstant.AlreadyExists}";
                    return responseVm;
                }
                decimal? totalCost = model.VendorOrderItems.Sum(item => item.Quantity * item.UnitPrice);
                string arrayString = string.Join(", ", model.Photo).Trim();
                var vendorOrder = new TblVendorOrder
                {
                    VendorOrderId = Guid.NewGuid(),
                    VendorId = model.VendorId,
                    JobId = model.JobId,
                    OrderNumber = model.OrderNumber?.Trim(),
                    OrderDate = model.OrderDate,
                    ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                    ActualDeliveryDate = model.ActualDeliveryDate,
                    Status = EnumValue.GetEnumValue(model.Status),
                    Notes = model.Notes?.Trim(),
                    Photo= arrayString ?? "",
                    IsActive = true,
                    IsDelete = false,
                    TotalCost = totalCost,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    TenentId = claimsProperty.GetTenantId()
                };
                var res = await Add(vendorOrder);

                foreach (var item in model.VendorOrderItems)
                {
                    TblVendorOrderItem tblVendorOrderItem = new TblVendorOrderItem
                    {
                        VendorOrderItemId = Guid.NewGuid(),
                        VendorOrderId = res.VendorOrderId,
                        MaterialId = item.MaterialId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice,
                        CreatedAt = DateTime.Now,
                        IsDelete = false,
                        IsActive = true
                    };
                    await vendorItemService.Add(tblVendorOrderItem);
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

        public async Task<IResponseVm<string>> Update_VendorOrder(UpdateVendorOrder model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await _tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true
                    && x.IsDelete == false
                  );

                if (!IsTenant)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var existingOrder = await FirstOrDefaultAsync(x =>
                    x.IsDelete == false &&
                    x.VendorOrderId == model.VendorOrderId &&
                    x.TenentId == claimsProperty.GetTenantId());

                if (existingOrder == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                decimal? totalCost = model.VendorOrderItems.Sum(item => item.Quantity * item.UnitPrice);

                string arrayString = string.Join(", ", model.Photo);
                existingOrder.OrderNumber = model.OrderNumber?.Trim();
                existingOrder.OrderDate = model.OrderDate;
                existingOrder.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
                existingOrder.ActualDeliveryDate = model.ActualDeliveryDate;
                existingOrder.TotalCost = totalCost;
                existingOrder.Status = EnumValue.GetEnumValue(model.Status);
                existingOrder.Notes = model.Notes?.Trim();
                existingOrder.Photo = arrayString;
                existingOrder.UpdatedAt = DateTime.Now;
                existingOrder.ModifiedBy = AppConstant.LoggedInUserID;

                await Update(existingOrder);

                var vendorOrderItems = vendorItemService.GetAllQuerable()
                    .Where(x =>
                        x.IsActive == true &&
                        x.IsDelete == false &&
                        x.VendorOrderId == model.VendorOrderId)
                    .ToList();

                foreach (var item in vendorOrderItems)
                {
                    item.IsDelete = true;
                    item.IsActive = false;
                    item.UpdatedAt = DateTime.Now;
                    item.ModifiedBy = AppConstant.LoggedInUserID.ToString();
                    await vendorItemService.Update(item);
                }

                foreach (var newItem in model.VendorOrderItems)
                {
                    var tblVendorOrderItem = new TblVendorOrderItem
                    {
                        VendorOrderId = model.VendorOrderId,
                        MaterialId = newItem.MaterialId,
                        Quantity = newItem.Quantity,
                        UnitPrice = newItem.UnitPrice,
                        TotalPrice = newItem.Quantity * newItem.UnitPrice,
                        CreatedAt = DateTime.Now,
                        ModifiedBy = AppConstant.LoggedInUserID.ToString(),
                        IsDelete = false,
                        IsActive = true
                    };

                    await vendorItemService.Add(tblVendorOrderItem);
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

        public async Task<IResponseVm<string>> Delete_VendorOrder(Guid? vendorOrderId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                if (vendorOrderId != Guid.Empty)
                {
                    var IsTenant = await _tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false);
                    if (IsTenant)
                    {
                        var existingOrder = GetAllQuerable().Where(x =>
                            x.IsDelete == false &&
                            x.VendorOrderId == vendorOrderId &&
                            x.TenentId == claimsProperty.GetTenantId())
                            .FirstOrDefault();

                        if (existingOrder != null)
                        {
                            existingOrder.IsActive = false;
                            existingOrder.IsDelete = true;
                            existingOrder.UpdatedAt = DateTime.Now;
                            existingOrder.ModifiedBy = AppConstant.LoggedInUserID;

                            await Update(existingOrder);

                            var vendorOrderItems = vendorItemService.GetAllQuerable().Where(x =>
                           x.IsDelete == false && x.VendorOrderId == vendorOrderId).ToList();

                            foreach (var items in vendorOrderItems)
                            {
                                items.IsDelete = true;
                                items.UpdatedAt = DateTime.Now;
                                items.IsActive = false;
                                items.ModifiedBy = AppConstant.LoggedInUserID.ToString();
                                await vendorItemService.Update(items);
                            }
                            responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.DeleteSuccessMsg;

                        }
                        else
                        {
                            responseVm.Message = AppConstant.NoDataFound;
                            return responseVm;
                        }
                    }
                    else
                    {
                        responseVm.Message = AppConstant.TenantNotFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<VendorOrderListVm>>> GetAll_VendorOrder(Pagination filter)
        {
            var responseVm = new ResponseVm<List<VendorOrderListVm>>();
            try
            {
                var isTenantValid = await _tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true
                    && x.IsDelete == false
                  );
                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var vendorOrdersQuery = from vo in dbContext.TblVendorOrders
                                        join v in dbContext.TblVendors on vo.VendorId equals v.VendorId
                                        join j in dbContext.TblJobs on vo.JobId equals j.JobId
                                        where vo.IsDelete == false && vo.TenentId == claimsProperty.GetTenantId()
                                        orderby vo.CreatedAt descending
                                        select new VendorOrderListVm
                                        {
                                            VendorOrderId = vo.VendorOrderId,
                                            VendorId = vo.VendorId,
                                            JobId = vo.JobId,
                                            FirstName = v.FirstName ?? "",
                                            LastName = v.LastName ?? "",
                                            JobName = j.JobName ?? "",
                                            OrderNumber = vo.OrderNumber ?? "",
                                            OrderDate = vo.OrderDate,
                                            ExpectedDeliveryDate = vo.ExpectedDeliveryDate,
                                            ActualDeliveryDate = vo.ActualDeliveryDate,
                                            TotalAmount = vo.TotalCost,
                                            Notes = vo.Notes ?? "",
                                            Photo = vo.Photo != null ? vo.Photo.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray() : null,
                                            Status = vo.Status == EnumValue.GetEnumValue(VendorOrderStatus.Pending) ? 0 : 1,
                                            VendorOrderStatus = vo.Status,
                                            IsActive = vo.IsActive,
                                        };


                var filteredRecords = await vendorOrdersQuery.ToListAsync();

                 var queryable = filteredRecords.AsQueryable();

                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                foreach (var item in filteredRecords)
                {
                    var vendorOrderItems = await vendorItemService.GetAllQuerable()
                        .Where(x => x.IsDelete == false && x.VendorOrderId == item.VendorOrderId && x.MaterialId.HasValue)
                        .OrderBy(x => x.CreatedAt)
                        .ToListAsync();

                    item.VendorOrderItems = vendorOrderItems.Select(x => new ShutterManagement.Core.ViewModels.Response.VendorOrderItem
                    {
                        vendorOrderItemId = x.VendorOrderItemId,
                        MaterialId = x.MaterialId,
                        MaterialName = materialService.GetAllQuerable().Where(y=>y.RawMaterialId == x.MaterialId).Select(z=>z.MaterialName).FirstOrDefault(),
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        IsActive = x.IsActive,
                    }).ToList();
                }


                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                filteredRecords = filteredRecords
                    .Where(x =>
                        (filter.Status == 0 ? x.VendorOrderStatus == EnumValue.GetEnumValue(VendorOrderStatus.Pending) :
                        (filter.Status == 1 ? x.VendorOrderStatus == EnumValue.GetEnumValue(VendorOrderStatus.Approved) : true)) &&
                        (string.IsNullOrEmpty(filter.SearchValue) ? true :
                            (x.VendorId.ToString() == filter.SearchValue) ||
                            (x.VendorOrderId.ToString() == filter.SearchValue) ||
                            (x.JobId.ToString() == filter.SearchValue) ||
                            (x.OrderNumber?.ToLower() ?? "").Contains(filter.SearchValue.Trim()) ||
                            (x.JobName?.ToLower() ?? "").Contains(filter.SearchValue.Trim()) ||
                            ((x.FirstName.ToLower() + " " + x.LastName.ToLower()) ?? "").Contains(filter.SearchValue) ||
                            (x.FirstName.ToLower() ?? "").Contains(filter.SearchValue) ||
                            (x.LastName.ToLower() ?? "").Contains(filter.SearchValue) ||
                            (x.OrderDate.ToString() ?? "").Contains(filter.SearchValue.Trim()) ||
                            (x.VendorOrderItems.Any(item => item.MaterialId.ToString().Contains(filter.SearchValue))))
                    )
                    .ToList();


                var queryable2 = filteredRecords.AsQueryable();

                queryable2 = Paginate.ApplyColumnFilters(queryable2, filter.ColumnFilters);
                queryable2 = Paginate.ApplySorting(queryable2, filter.SortColumn, filter.SortDirection);


                var paginatedRecords = Paginate.Pagination(queryable2.ToList(), filter);

                responseVm.IsSuccess = true;
                responseVm.Data = paginatedRecords;
                responseVm.TotalRecords = filteredRecords.Count;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_VendorOrderItem(UpdateVendorOrderItem model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool isTenantValid = await _tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() &&
                    x.IsActive == true &&
                    x.IsDelete == false);
                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var existingOrderItem = await vendorItemService.FirstOrDefaultAsync(x =>
                    x.IsDelete == false &&
                    x.VendorOrderItemId == model.VendorOrderItemId &&
                    x.VendorOrderId == model.VendorOrderId );

                if (existingOrderItem == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                existingOrderItem.Quantity = model.Quantity;
                existingOrderItem.UnitPrice = model.UnitPrice;
                existingOrderItem.MaterialId=model.MaterialId;
                existingOrderItem.TotalPrice = model.Quantity * model.UnitPrice;
                existingOrderItem.UpdatedAt = DateTime.Now;
                existingOrderItem.ModifiedBy = AppConstant.LoggedInUserID.ToString();

                await vendorItemService.Update(existingOrderItem);

                var vendorOrderItems =  vendorItemService.GetAllQuerable()
                    .Where(x => x.VendorOrderId == model.VendorOrderId && x.IsDelete==false)
                    .ToList();

                // var totalOrderPrice = vendorOrderItems.Sum(x => x.TotalPrice);

                var total_price = model.Quantity * model.UnitPrice;

                var vendorOrderItemsList = vendorItemService.GetAllQuerable()
               .Where(x => x.VendorOrderId == model.VendorOrderId && x.IsDelete == false)
               .ToList();

                decimal? totalOrderPrice = 0;

                foreach (var vendorItem in vendorOrderItemsList)
                {
                    totalOrderPrice += vendorItem.TotalPrice;
                }

                var vendorOrder = await FirstOrDefaultAsync(x => x.VendorOrderId == model.VendorOrderId);
                if (vendorOrder != null)
                {
                    vendorOrder.TotalCost = totalOrderPrice;
                    vendorOrder.UpdatedAt = DateTime.Now;
                    vendorOrder.ModifiedBy = AppConstant.LoggedInUserID;
                    await Update(vendorOrder);
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Delete_VendorOrderItem(Guid? vendorOrderItemId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                if (vendorOrderItemId == Guid.Empty || !vendorOrderItemId.HasValue)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                bool isTenantValid = await _tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() &&
                    x.IsDelete == false);
                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var vendorOrderItem = await vendorItemService.FirstOrDefaultAsync(x =>
                    x.IsDelete == false &&
                    x.VendorOrderItemId == vendorOrderItemId);
                if (vendorOrderItem == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                vendorOrderItem.IsDelete = true;
                vendorOrderItem.IsActive = false;
                vendorOrderItem.UpdatedAt = DateTime.Now;
                vendorOrderItem.ModifiedBy = AppConstant.LoggedInUserID.ToString();

                await vendorItemService.Update(vendorOrderItem);

                var vendorOrderItems = await vendorItemService.GetAllQuerable()
                    .Where(x => x.VendorOrderId == vendorOrderItem.VendorOrderId && x.IsDelete==false)
                    .ToListAsync();

                var vendorOrder = await FirstOrDefaultAsync(x => x.VendorOrderId == vendorOrderItem.VendorOrderId);
                if (vendorOrder != null)
                {
                    vendorOrder.TotalCost = vendorOrder.TotalCost-vendorOrderItem.TotalPrice;
                    vendorOrder.UpdatedAt = DateTime.Now;
                    vendorOrder.ModifiedBy = AppConstant.LoggedInUserID;

                    await Update(vendorOrder);
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.DeleteSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> SendInvoiceAttachmentVendorOrder(GenerateInvoiceForVendorOrder model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                // Validate Tenant
                var isTenantValid = await _tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive==true && x.IsDelete==false);

                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                    return responseVm;
                }

                // Fetch Tenant Details
                var tenantDetails =  _tenantService.GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete==false);

                if (tenantDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Tenant {AppConstant.NoDataFound}";
                    return responseVm;
                }

                var tenantInfoDetails =  tenantBrandingService.GetAllQuerable()
                    .FirstOrDefault(x => x.Id == tenantDetails.BrandingInfo && x.IsActive == true);

                // Fetch Invoice Template
                Guid templateId = Guid.Parse("2D30573D-6753-4B04-935A-8CC1DE65D409");
                var invoiceTemplate =  dbContext.TblTemplates
                    .FirstOrDefault(c => c.TemplateId == templateId && c.IsDelete==false);

                if (invoiceTemplate == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                // Fetch Vendor Details
                var vendorDetails =  vendorService.GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() &&
                                              x.VendorId == model.VendorId &&
                                              x.IsDelete == false);

                if (vendorDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Vendor {AppConstant.NoDataFound}";
                    return responseVm;
                }

                // Fetch Order Details
                var orderDetails =  GetAllQuerable()
                    .FirstOrDefault(x => x.VendorOrderId == model.VendorOrderId && x.IsDelete == false);

                if (orderDetails == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"OrderDetails {AppConstant.NoDataFound}";
                    return responseVm;
                }

                // Fetch Job Details
                var jobDetails =  jobService.GetAllQuerable()
                    .FirstOrDefault(x => x.JobId == orderDetails.JobId && x.IsDelete == false);

                // Fetch User Details
                var userDetails =  dbContext.TblUsers
                    .FirstOrDefault(c => c.Id == orderDetails.CreatedBy &&
                                              c.TenantId == claimsProperty.GetTenantId() &&
                                              c.IsDelete==false);

                var materialList = await materialService.GetAllQuerable()
                    .Select(z => new { z.RawMaterialId, z.MaterialName })
                    .ToListAsync(); 

                // Fetch Vendor Order Items
                var vendorOrderItems = await vendorItemService.GetAllQuerable()
                    .Where(x => x.VendorOrderId == model.VendorOrderId && x.IsDelete==false)
                    .ToListAsync(); 

                // Map Vendor Order Items with Material Names
                var mappedVendorOrderItems = vendorOrderItems.Select(x => new VendorOrderItemModel
                {
                    MaterialName = materialList
                        .FirstOrDefault(m => m.RawMaterialId == x.MaterialId)?.MaterialName ?? "Unknown Material",
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalPrice = x.Quantity * x.UnitPrice
                }).ToList();



                // Build Vendor Order Template Data
                var vendorOrderData = new VendorOrderTemplateModel
                {
                    VendorId = vendorDetails.VendorCode ?? "",
                    VendorName = vendorDetails.FirstName +" "+vendorDetails.LastName??"",
                    JobName = jobDetails?.JobName ?? "N/A",
                    StreetAddress = vendorDetails.StreetAddress ?? "",
                    VendorCity = vendorDetails.City ?? "",
                    VendorState = dbContext.MstStates.Where(s => s.Id == vendorDetails.State).Select(y => y.StateName ?? "").FirstOrDefault(),
                    VendorZipCode = vendorDetails.ZipCode ?? "",
                    MobilePhone = vendorDetails.PhoneNumber ?? "",
                    EmailAddress = vendorDetails.Email ?? "",

                    OrderNo = orderDetails.OrderNumber ?? "",
                    OrderDate = orderDetails.OrderDate.ToString(),
                    ExpectedDeliveryDate = orderDetails.ExpectedDeliveryDate.ToString()??"",
                    ActualDeliveryDate = orderDetails.ActualDeliveryDate.ToString()??"",
                    VendorOrderStatus = orderDetails.Status ?? "",
                    TotalAmount = orderDetails.TotalCost,
                    Notes = orderDetails.Notes ?? "",

                    TenantCompanyName = tenantInfoDetails?.CompanyName ?? "",
                    CompanyStreetAddress = tenantInfoDetails?.StreetAddress ?? "",
                    CompanyCity = tenantInfoDetails?.City ?? "",
                    CompanyCountry = tenantInfoDetails?.Country ?? "",
                    CompanyZipCode = tenantInfoDetails?.ZipCode ?? "",

                    IssueDate = orderDetails.CreatedAt.ToString(),
                    CreatedBy = $"{userDetails?.FirstName} {userDetails?.LastName}".Trim(),
                    Logo = tenantInfoDetails?.Logo ?? "",

                    VendorOrderItems = GenerateProductListHtml(mappedVendorOrderItems)
                };

                // Generate HTML from Template
                var htmlContent = ReplaceTemplatePlaceholders(invoiceTemplate.TemplateValue, vendorOrderData);

                // Generate PDF
                var pdfBytes = GenerateInvoicePdf(htmlContent);

                // Save PDF to Disk
                var tenantId = claimsProperty.GetTenantId().ToString();
                var rootPath = Path.Combine(AppConstant.FileUploadPath, tenantId);
                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                var fileName = $"Invoice_{Guid.NewGuid()}.pdf";
                var savePath = Path.Combine(rootPath, fileName);

                await using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }

                // Prepare and Send Email
                var emailBody = $@"
 <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6; max-width: 600px;'>
    <p><strong>Dear {vendorDetails?.FirstName} {vendorDetails?.LastName},</strong></p>
    <p>Please find your Order invoice attached for your reference.</p>
   
    <p><strong>Thank you for your business!</strong></p>
    <p><img src='{tenantInfoDetails?.Logo}' alt='Company Logo' height='100px' 
             style='margin-top: 10px; display: block;'></p>
    <p>Best regards,</p>
    <p><strong>{tenantInfoDetails?.CompanyName}</strong></p>
    <p>Shutter Management and Estimation System</p>
</div>";

                var emailAttachment = new EmailAttachment
                {
                    FileName = $"{orderDetails.OrderNumber}.pdf",
                    Stream = new MemoryStream(pdfBytes),
                    MediaType = "application/pdf"
                };

                //var emailSent = EmailHelper.SendEmailWithAttachment(
                //    vendorDetails?.Email,
                //    "Your Order Invoice ",
                //    emailBody,
                //    new List<EmailAttachment> { emailAttachment }
                //);

                responseVm.IsSuccess = true;
                responseVm.Message = "Email Sent Successfully.";
                File.Delete(savePath);
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }


        private string GenerateProductListHtml(List<VendorOrderItemModel> items)
        {
            var itemslist = new StringBuilder();
            int i = 0;
            foreach (var product in items)
            {
                itemslist.Append($@"
    <tr style='background-color: #efefef;'>
        <td style='text-align: center; padding: 5px 5px;'>{++i}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.MaterialName}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.Quantity}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.UnitPrice}</td>
        <td style='text-align: center; padding: 5px 5px;'>{product.TotalPrice}</td>
    </tr>");
            }

            return $"<tbody>{itemslist}</tbody>";
        }

        private string ReplaceTemplatePlaceholders(string template, VendorOrderTemplateModel data)
        {
            return template
                .Replace("{{tenantCompanyName}}", data.TenantCompanyName ?? "")
                .Replace("{{companystreetaddress}}", data.CompanyStreetAddress ?? "")
                .Replace("{{companycity}}", data.CompanyCity ?? "")
                .Replace("{{companycountry}}", data.CompanyCountry ?? "")
                .Replace("{{companyzipcode}}", data.CompanyZipCode ?? "")
                .Replace("{{issuedate}}", Convert.ToDateTime(data.IssueDate).ToString("MM-dd-yyyy") ?? "")
                .Replace("{{createdby}}", data.CreatedBy ?? "")
                .Replace("{{vendorid}}", data.VendorId ?? "")
                .Replace("{{orderno}}", data.OrderNo ?? "")
                .Replace("{{vendorname}}", data.VendorName ?? "")
                .Replace("{{jobname}}", data.JobName ?? "")
                .Replace("{{orderdate}}", Convert.ToDateTime(data.OrderDate).ToString("MM-dd-yyyy") ?? "")
                .Replace("{{expecteddeliverydate}}", Convert.ToDateTime(data.ExpectedDeliveryDate).ToString("MM-dd-yyyy") ?? "")
                .Replace("{{actualdeliverydate}}", Convert.ToDateTime(data.ActualDeliveryDate).ToString("MM-dd-yyyy") ?? "")
                .Replace("{{vendorOrderStatus}}", data.VendorOrderStatus ?? "")
                .Replace("{{totalamount}}", data.TotalAmount.ToString() ?? "0.00")
                .Replace("{{balancedue}}", data.BalanceDue.ToString() ?? "0.00")
                .Replace("{{note}}", data.Notes ?? "")
                .Replace("{{status}}", data.VendorOrderStatus ?? "")
                .Replace("{{mobilephone}}", data.MobilePhone ?? "")
                .Replace("{{emailaddress}}", data.EmailAddress ?? "")
                .Replace("{{streetaddress}}", data.StreetAddress ?? "")
                .Replace("{{vendorcity}}", data.VendorCity ?? "")
                .Replace("{{vendorstate}}", data.VendorState ?? "")
                .Replace("{{vendorzipcode}}", data.VendorZipCode ?? "")
                .Replace("{{paymentlist}}", data.VendorOrderItems ?? "")
                .Replace("{{logo}}", data.Logo ?? "");
        }

        public byte[] GenerateInvoicePdf(string htmlContent)
        {
            var pdfDocument = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = DinkToPdf.ColorMode.Color,
                    Orientation = Orientation.Portrait,
                                          PaperSize = DinkToPdf.PaperKind.A4
                },
                Objects = { new ObjectSettings { HtmlContent = htmlContent } }
            };

            return converter.Convert(pdfDocument);
        }


    }
}
