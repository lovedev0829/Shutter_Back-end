using Microsoft.EntityFrameworkCore;
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

        public async Task<IResponseVm<string>> ActiveInActive(Guid VendorOrderId)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await _tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IsVendorOrderExists = GetAllQuerable()
                       .Where(c => c.VendorOrderId == VendorOrderId
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
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true
                    && x.IsDelete == false
                  );

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
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
                    VendorOrderId = Guid.NewGuid(),
                    VendorId = model.VendorId,
                    JobId = model.JobId,
                    OrderNumber = model.OrderNumber?.Trim(),
                    OrderDate = model.OrderDate,
                    ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                    ActualDeliveryDate = model.ActualDeliveryDate,
                    Status = EnumValue.GetEnumValue(model.Status),
                    Notes = model.Notes?.Trim(),
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    TenentId = AppConstant.LoggedInTenantId
                };
                var res = await Add(vendorOrder);

                foreach (var item in model.VendorOrderItems)
                {
                    TblVendorOrderItem tblVendorOrderItem = new TblVendorOrderItem
                    {
                        VendorOrderItemId = Guid.NewGuid(),
                        VendorOrderId=res.VendorOrderId,
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
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true
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
                    x.TenentId == AppConstant.LoggedInTenantId);

                if (existingOrder == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                existingOrder.OrderNumber = model.OrderNumber?.Trim();
                existingOrder.OrderDate = model.OrderDate;
                existingOrder.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
                existingOrder.ActualDeliveryDate = model.ActualDeliveryDate;
                existingOrder.Status = EnumValue.GetEnumValue(model.Status);
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

        public async Task<IResponseVm<string>> Delete_VendorOrder(Guid vendorOrderId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                if(vendorOrderId!=Guid.Empty)
                {
                    var IsTenant = await _tenantService.AnyAsync(x =>x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                    if (IsTenant)
                    {
                        var existingOrder = GetAllQuerable().Where(x =>
                            x.IsDelete == false &&
                            x.VendorOrderId== vendorOrderId &&
                            x.TenentId == AppConstant.LoggedInTenantId)
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
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true
                    && x.IsDelete == false
                  );
                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var vendorOrdersQuery = from vo in dbContext.TblVendorOrders
                                        where vo.IsDelete == false && vo.TenentId==AppConstant.LoggedInTenantId
                                        orderby vo.CreatedAt descending
                                        select new VendorOrderListVm
                                        {
                                            VendorOrderId = vo.VendorOrderId,
                                            VendorId = vo.VendorId,
                                            JobId = vo.JobId,
                                            OrderNumber = vo.OrderNumber,
                                            OrderDate = vo.OrderDate,
                                            ExpectedDeliveryDate = vo.ExpectedDeliveryDate,
                                            ActualDeliveryDate = vo.ActualDeliveryDate,
                                            Notes = vo.Notes,
                                            Status=vo.Status== EnumValue.GetEnumValue(VendorOrderStatus.Pending) ? 0 : 1,
                                            VendorOrderStatus= vo.Status,
                                            IsActive = vo.IsActive,
                                        };

                var filteredRecords = await vendorOrdersQuery.ToListAsync();

                foreach (var item in filteredRecords)
                {
                    item.VendorOrderItems = await vendorItemService.GetAllQuerable()
                        .Where(x => x.IsDelete == false && x.VendorOrderId == item.VendorOrderId)
                        .OrderBy(x => x.CreatedAt)
                        .Select(x => new ShutterManagement.Core.ViewModels.Response.VendorOrderItem
                        {
                            MaterialId = x.MaterialId,
                            Quantity = x.Quantity,
                            UnitPrice = x.UnitPrice,
                            TotalPrice = x.TotalPrice,
                            IsActive = x.IsActive
                        })
                        .ToListAsync();
                }
                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                filteredRecords = filteredRecords
                    .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                        (string.IsNullOrEmpty(filter.SearchValue) ? true :
                            (x.VendorId.ToString() == filter.SearchValue) ||
                            (x.JobId.ToString() == filter.SearchValue) ||
                            (x.OrderNumber ?? "").Contains(filter.SearchValue.Trim()) ||
                            (x.OrderDate.ToString() ?? "").Contains(filter.SearchValue.Trim()) ||
                            (x.VendorOrderItems.Any(item => item.MaterialId.ToString().Contains(filter.SearchValue))))
                    )
                    .ToList();

                var paginatedRecords = filteredRecords
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

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

    }
}
