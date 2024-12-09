using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class VendorOrderService : BaseRepository<BtprojecShuttermanagementDevContext, TblVendorOrder>, IVendorOrderService
    {
        private readonly ITenantService _tenantService;
        private readonly IVendorItemService vendorItemService;

        public ClaimsProperty ClaimsProperty { get; }

        public VendorOrderService(ITenantService tenantService, ClaimsProperty claimsProperty, IVendorItemService vendorItemService)
        {
            _tenantService = tenantService;
            ClaimsProperty = claimsProperty;
            this.vendorItemService = vendorItemService;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await _tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsVendorOrderExists = GetAllQuerable()
                       .Where(c => c.VendorOrderId == Id
                                   && c.TenentId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                       .FirstOrDefault();
                    if (IsVendorOrderExists != null)
                    {
                        IsVendorOrderExists.IsActive = !IsVendorOrderExists.IsActive;
                        IsVendorOrderExists.UpdatedAt = DateTime.Now;
                        IsVendorOrderExists.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = Update(IsVendorOrderExists);

                        var vendorOrderItems = vendorItemService.GetAllQuerable()
                            .Where(x => x.IsDelete == false && x.VendorOrderId == Id).ToList();

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
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.ParentTenantId == null);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.AccessDenied;
                    return responseVm;
                }
                var isOrderNumberExist = await AnyAsync(x =>
                    x.IsDelete == false &&
                    x.OrderNumber == model.OrderNumber.Trim() &&
                    x.TenentId == AppConstant.LoggedInTenantId);

                if (isOrderNumberExist)
                {
                    responseVm.Message = $"Order Number {AppConstant.AlreadyExists}";
                    return responseVm;
                }

                var vendorOrder = new TblVendorOrder
                {
                    VendorId = model.VendorId,
                    JobId = model.JobId,
                    OrderNumber = model.OrderNumber?.Trim(),
                    OrderDate = model.OrderDate,
                    ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                    ActualDeliveryDate = model.ActualDeliveryDate,
                    Status = model.Status?.Trim(),
                    Notes = model.Notes?.Trim(),
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    TenentId = AppConstant.LoggedInTenantId
                };

                if (model.VendorOrderItems != null && model.VendorOrderItems.Any())
                {
                    foreach (var item in model.VendorOrderItems)
                    {
                        vendorOrder.TblVendorOrderItems.Add(new TblVendorOrderItem
                        {
                            MaterialId = item.MaterialId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.Quantity * item.UnitPrice,
                            CreatedAt = DateTime.Now,
                            IsDelete = false,
                            IsActive = true
                        });
                    }
                }

                var result = await Add(vendorOrder);
                if (result?.VendorOrderId != Guid.Empty)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
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
                    x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);

                if (!IsTenant)
                {
                    responseVm.Message = AppConstant.AccessDenied;
                    return responseVm;
                }

                var existingOrder = await FirstOrDefaultAsync(x =>
                    x.IsDelete == false &&
                    x.IsActive == true &&
                    x.VendorOrderId == model.VendorOrderId &&
                    x.TenentId == AppConstant.LoggedInTenantId);

                if (existingOrder == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                existingOrder.VendorId = model.VendorId;
                existingOrder.JobId = model.JobId;
                existingOrder.OrderNumber = model.OrderNumber?.Trim();
                existingOrder.OrderDate = model.OrderDate;
                existingOrder.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
                existingOrder.ActualDeliveryDate = model.ActualDeliveryDate;
                existingOrder.Status = model.Status?.Trim();
                existingOrder.Notes = model.Notes?.Trim();
                existingOrder.UpdatedAt = DateTime.Now;
                existingOrder.ModifiedBy = AppConstant.LoggedInUserID;

                await Update(existingOrder);

                var vendorOrderItems =  vendorItemService.GetAllQuerable()
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

        public async Task<IResponseVm<string>> Delete_VendorOrder(string vendorOrderId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await _tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant) {
                    var existingOrder = GetAllQuerable().Where(x =>
                        x.IsDelete == false && x.IsActive == true &&
                        x.VendorOrderId.ToString() == vendorOrderId &&
                        x.TenentId == AppConstant.LoggedInTenantId)
                        .FirstOrDefault();

                    if (existingOrder != null)
                    {
                        existingOrder.IsActive = false;
                        existingOrder.IsDelete = true;
                        existingOrder.UpdatedAt = DateTime.Now;
                        existingOrder.ModifiedBy = AppConstant.LoggedInUserID;

                        await Update(existingOrder);

                        var vendorOrderItems = vendorItemService.GetAllQuerable().Where(x => x.IsActive == true
                        && x.IsDelete == false && x.VendorOrderId.ToString() == vendorOrderId).ToList();

                        foreach(var items in vendorOrderItems)
                        {
                            items.IsDelete=true;
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
                    responseVm.Message = AppConstant.AccessDenied;
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
            IResponseVm<List<VendorOrderListVm>> responseVm = new ResponseVm<List<VendorOrderListVm>>();
            try
            {
                var isTenantValid = await _tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId &&
                    x.ParentTenantId == null &&
                    x.IsActive == true &&
                    x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.AccessDenied;
                    return responseVm;
                }

                var vendorOrdersQuery = from vendorOrder in dbContext.TblVendorOrders
                                        where vendorOrder.IsDelete == false && vendorOrder.TenentId == AppConstant.LoggedInTenantId
                                        orderby vendorOrder.CreatedAt descending
                                        select new VendorOrderListVm
                                        {
                                            VendorId = vendorOrder.VendorId,
                                            JobId = vendorOrder.JobId,
                                            OrderNumber = vendorOrder.OrderNumber,
                                            OrderDate = vendorOrder.OrderDate,
                                            ExpectedDeliveryDate = vendorOrder.ExpectedDeliveryDate,
                                            ActualDeliveryDate = vendorOrder.ActualDeliveryDate,
                                            Status = vendorOrder.Status,
                                            Notes = vendorOrder.Notes,
                                            Job = dbContext.TblJobs
                                                  .Where(job => job.JobId == vendorOrder.JobId && job.TenantId == AppConstant.LoggedInTenantId)
                                                  .Select(job => job.JobName)
                                                  .FirstOrDefault(),
                                            Vendor = dbContext.TblVendors
                                                    .Where(vendor => vendor.VendorId == vendorOrder.VendorId && vendor.TenantId == AppConstant.LoggedInTenantId)
                                                    .Select(vendor => vendor.VendorName)
                                                    .FirstOrDefault(),
                                            Material = (from rawMaterial in dbContext.TblRawMaterials
                                                        join orderItem in dbContext.TblVendorOrderItems
                                                        on rawMaterial.RawMaterialId equals orderItem.MaterialId
                                                        where orderItem.VendorOrderId == vendorOrder.VendorOrderId
                                                        select rawMaterial.MaterialName)
                                                        .FirstOrDefault(),
                                        };

                var filteredRecords = vendorOrdersQuery.ToList();

                foreach (var item in filteredRecords)
                {

                    var vendorOrderItems = vendorItemService.GetAllQuerable().
                        Where(x => x.IsDelete == false && x.VendorOrderId == item.VendorId).OrderBy(x => x.CreatedAt)
                        .Select(x => new VendorOrderListVm
                        {
                           
                        })
                        .ToList();
                    item.VendorOrderItems = vendorOrderItems;
                }

                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    filteredRecords = filteredRecords.Where(x =>
                        (x.VendorId.ToString() == filter.SearchValue) ||
                        (x.OrderNumber?.Contains(filter.SearchValue.Trim()) ?? false) ||
                        (x.Job?.Contains(filter.SearchValue.Trim()) ?? false) ||
                        (x.Vendor?.Contains(filter.SearchValue.Trim()) ?? false) ||
                        (x.Material?.Contains(filter.SearchValue.Trim()) ?? false)

                    ).ToList();
                }

                var paginatedRecords = filteredRecords
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .Select(x => new VendorOrderListVm
                    {
                        OrderNumber = x.OrderNumber,
                        Vendor = x.Vendor
                    })
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = paginatedRecords;
                responseVm.TotalRecords = filteredRecords.Count;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }

        
    }
}
