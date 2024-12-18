using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShutterManagement.Business.Services
{
    public class DiscountService : BaseRepository<BtprojecShuttermanagementDevContext, TblDiscount>, IDiscountService
    {
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;

        public DiscountService(ITenantService tenantService, ITaxExemption taxExemptionService, ClaimsProperty claimsProperty )
        {
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
        }

        private string GenerateCouponCode()
        {
            var tenantCode = AppConstant.LoggedInCompanyCode;

            var tenantCodePrefix = tenantCode?.Substring(0, Math.Min(4, tenantCode.Length)) ?? "XXXX"; 

            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var randomCode = new string(Enumerable.Repeat(chars, 6)
                            .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{tenantCodePrefix}{randomCode}";
        }

        public async Task<IResponseVm<string>> CreateDiscountCoupon(CreateDiscount createDiscount)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive==true);

                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                if (createDiscount.discountType == DiscountType.Percentage)
                {
                    if (createDiscount.PercentageDiscount == null || createDiscount.PercentageDiscount <= 0 || createDiscount.PercentageDiscount > 100)
                    {
                        responseVm.Message = "Percentage discount must be between 1 and 100.";
                        responseVm.IsSuccess = false;
                        return responseVm;
                    }
                }
                else if (createDiscount.discountType == DiscountType.Fixed)
                {
                    if (createDiscount.FixedAmount == null || createDiscount.FixedAmount <= 0)
                    {
                        responseVm.Message = "Fixed amount must be greater than 0.";
                        responseVm.IsSuccess = false;
                        return responseVm;
                    }
                }
                else
                {
                    responseVm.Message = "Invalid discount type.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var couponCode = GenerateCouponCode();

                if (createDiscount.AppliedFor == AppliedFor.Customers && createDiscount.CustomerId != null && createDiscount.CustomerId.Length > 0)
                {
                    foreach (var customerId in createDiscount.CustomerId)
                    {
                        var discount = new TblDiscount
                        {
                            DiscountId = Guid.NewGuid(),
                            CouponCode = couponCode.Trim(),
                            CustomerId = customerId,
                            PercentageDiscount = createDiscount.discountType == DiscountType.Percentage ? createDiscount.PercentageDiscount : null,
                            FixedAmount = createDiscount.discountType == DiscountType.Fixed ? createDiscount.FixedAmount : null,
                            ExpiryDate = createDiscount.ExpiryDate,
                            StartDate = createDiscount.StartDate,
                            CreatedAt = DateTime.Now,
                            Createdby = AppConstant.LoggedInUserID,
                            TenantId = AppConstant.LoggedInTenantId,
                            IsActive = true,
                            IsDelete = false
                        };

                        await Add(discount);
                    }

                    responseVm.Message = AppConstant.InsertSuccessMsg;
                    responseVm.IsSuccess = true;
                }
                else if (createDiscount.AppliedFor == AppliedFor.All)
                {
                    var discount = new TblDiscount
                    {
                        DiscountId = Guid.NewGuid(),
                        CouponCode = couponCode.Trim(),
                        PercentageDiscount = createDiscount.discountType == DiscountType.Percentage ? createDiscount.PercentageDiscount : null,
                        FixedAmount = createDiscount.discountType == DiscountType.Fixed ? createDiscount.FixedAmount : null,
                        ExpiryDate = createDiscount.ExpiryDate,
                        StartDate = createDiscount.StartDate,
                        CreatedAt = DateTime.Now,
                        Createdby = AppConstant.LoggedInUserID,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
                        IsDelete = false
                    };

                    await Add(discount);

                    responseVm.Message = AppConstant.InsertSuccessMsg;
                    responseVm.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateDiscountCoupon(UpdateDiscount updateDiscount)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                if (updateDiscount.PercentageDiscount <= 0 || updateDiscount.PercentageDiscount > 100)
                {
                    responseVm.Message = "Percentage must be between 1 and 100.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                if (updateDiscount.FixedAmount <= 0)
                {
                    responseVm.Message = "Fixed amount must be greater than 0.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var existingDiscount = GetAllQuerable()
                    .FirstOrDefault(x => x.DiscountId == updateDiscount.DiscountId
                                         && x.TenantId == AppConstant.LoggedInTenantId
                                         && x.IsDelete==false);
                if (existingDiscount == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                existingDiscount.CouponCode = updateDiscount.CouponCode;
                existingDiscount.PercentageDiscount = updateDiscount.discountType == DiscountType.Percentage ? updateDiscount.PercentageDiscount : null;
                existingDiscount.FixedAmount = updateDiscount.discountType == DiscountType.Fixed ? updateDiscount.FixedAmount : null;
                existingDiscount.ExpiryDate = updateDiscount.ExpiryDate;
                existingDiscount.StartDate = updateDiscount.StartDate;
                existingDiscount.UpdatedAt = DateTime.Now;
                existingDiscount.ModifiedBy = AppConstant.LoggedInUserID;

                await Update(existingDiscount);

                responseVm.Message = AppConstant.UpdateSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteDiscountCoupon(Guid discountId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var discount =  GetAllQuerable().Where(x => x.DiscountId == discountId && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                if (discount == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                discount.IsDelete = true;
                discount.IsActive = false;
                discount.UpdatedAt = DateTime.Now;
                discount.ModifiedBy = AppConstant.LoggedInUserID;

                await Update(discount);

                responseVm.Message = AppConstant.DeleteSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid discountId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
                if (!isTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var discount = FirstOrDefault(x => x.DiscountId == discountId && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                if (discount == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                discount.IsActive = false;
                discount.UpdatedAt = DateTime.Now;
                discount.ModifiedBy = AppConstant.LoggedInUserID;

               await Update(discount);

                responseVm.Message = AppConstant.UpdateSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<DiscoountVm>>> GetAll_Coupons(Pagination filter)
        {
            IResponseVm<List<DiscoountVm>> responseVm = new ResponseVm<List<DiscoountVm>>();
            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (isTenantExists)
                {
                    //var allCouponlList = (await GetAll())
                    //    .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                    //    .OrderByDescending(x => x.CreatedAt)
                    //    .ToList();

                    var allCouponlList = from t2 in dbContext.TblDiscounts.OrderByDescending(x => x.CreatedAt)
                              join t1 in dbContext.TblContacts on t2.CustomerId equals t1.ContactId
                              select new DiscoountVm
                              {
                                  CouponCode = t2.CouponCode,
                                  CustomerName=t1.Name,
                                  PercentageDiscount = t2.PercentageDiscount,
                                  FixedAmount = t2.FixedAmount,
                                  ExpiryDate = t2.ExpiryDate,
                                  StartDate = t2.StartDate,
                                  IsActive = t2.IsActive
                              };

                    var filteredRecords = allCouponlList
                        .Where(x =>
                            (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                            (string.IsNullOrEmpty(filter.SearchValue) ||
                                (x.CustomerName.ToString() == filter.SearchValue) ||
                                (x.CouponCode ?? "").Contains(filter.SearchValue.Trim())||
                                (x.PercentageDiscount.ToString() ?? "").Contains(filter.SearchValue.Trim()) ||
                                (x.FixedAmount.ToString() ?? "").Contains(filter.SearchValue.Trim()) ||
                                (x.ExpiryDate.ToString() ?? "").Contains(filter.SearchValue.Trim()) ||
                                (x.StartDate.ToString() ?? "").Contains(filter.SearchValue.Trim())
                            )
                        )
                        .ToList();

                    var paginatedRecords = filteredRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.IsSuccess = true;
                    responseVm.Data = paginatedRecords.Select(x => new DiscoountVm
                    {
                        CouponCode = x.CouponCode,
                        CustomerName = x.CustomerName,
                        PercentageDiscount = x.PercentageDiscount,
                        FixedAmount = x.FixedAmount,
                        ExpiryDate = x.ExpiryDate,
                        StartDate = x.StartDate,
                        IsActive = x.IsActive
                    }).ToList();

                    responseVm.TotalRecords = filteredRecords.Count;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                }
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
