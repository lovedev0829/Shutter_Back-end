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
        private readonly IContactService contactService;
        private readonly ClaimsProperty claimsProperty;

        public DiscountService(ITenantService tenantService,IContactService contactService,  ClaimsProperty claimsProperty )
        {
            this.tenantService = tenantService;
            this.contactService = contactService;
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
            FileLogger.Log($"Start: Creating Discount Coupon for TenantID: {AppConstant.LoggedInTenantId}");

            try
            {
                if (!await ValidateTenantAsync())
                    return SetFailureResponse(responseVm, AppConstant.TenantNotFound);

                if (!ValidateDiscountType(createDiscount, out string validationMessage))
                    return SetFailureResponse(responseVm, validationMessage);

                var couponCode = GenerateCouponCode();
                var discountRecords = BuildDiscountRecords(createDiscount, couponCode);

                foreach (var discount in discountRecords)
                {
                    await Add(discount);
                }

                FileLogger.Log($"Success: Discount Coupon created successfully.");
                return SetSuccessResponse(responseVm, AppConstant.InsertSuccessMsg);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error: Exception in CreateDiscountCoupon - {ex.Message}");
                return SetFailureResponse(responseVm, $"An error occurred: {AppConstant.ExceptionError}");
            }
        }

        public async Task<IResponseVm<string>> UpdateDiscountCoupon(UpdateDiscount updateDiscount)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log($"Start: Updating Discount Coupon ID: {updateDiscount.DiscountId}");

            try
            {
                if (!await ValidateTenantAsync())
                    return SetFailureResponse(responseVm, AppConstant.TenantNotFound);

                if (!ValidateUpdateDiscount(updateDiscount, out string validationMessage))
                    return SetFailureResponse(responseVm, validationMessage);

                var existingDiscount = await FindExistingDiscountAsync((Guid)updateDiscount.DiscountId);
                if (existingDiscount == null)
                    return SetFailureResponse(responseVm, AppConstant.NoDataFound);
                var existingCoupon = GetAllQuerable()
                .FirstOrDefault(t => t.CouponCode == updateDiscount.CouponCode);

                UpdateDiscountRecord(existingDiscount, updateDiscount);

                await Update(existingDiscount);

                FileLogger.Log($"Success: Discount Coupon ID: {updateDiscount.DiscountId} updated successfully.");
                return SetSuccessResponse(responseVm, AppConstant.UpdateSuccessMsg);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error: Exception in UpdateDiscountCoupon - {ex.Message}");
                return SetFailureResponse(responseVm, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IResponseVm<string>> DeleteDiscountCoupon(Guid discountId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log($"Start: Deleting Discount Coupon ID: {discountId}");

            try
            {
                if (!await ValidateTenantAsync())
                    return SetFailureResponse(responseVm, AppConstant.TenantNotFound);

                var discount = await FindExistingDiscountAsync(discountId);
                if (discount == null)
                    return SetFailureResponse(responseVm, AppConstant.NoDataFound);

                MarkAsDeleted(discount);
                await Update(discount);

                FileLogger.Log($"Success: Discount Coupon ID: {discountId} marked as deleted.");
                return SetSuccessResponse(responseVm, AppConstant.DeleteSuccessMsg);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error: Exception in DeleteDiscountCoupon - {ex.Message}");
                return SetFailureResponse(responseVm, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid discountId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            FileLogger.Log($"Start: Toggling Active/Inactive status for Discount with ID: {discountId}");

            try
            {
                if (!await ValidateTenantAsync())
                    return SetFailureResponse(responseVm, AppConstant.TenantNotFound);

                var discount = await GetActiveDiscountAsync(discountId);
                if (discount == null)
                    return SetFailureResponse(responseVm, AppConstant.NoDataFound);

                ToggleActiveStatus(discount);
                await Update(discount);

                FileLogger.Log($"Success: Discount ID: {discountId} status toggled to {(discount.IsActive ?? false ? "Active" : "Inactive")}.");
                return SetSuccessResponse(responseVm, AppConstant.UpdateSuccessMsg);
            }     
            catch (Exception ex)
            {
                FileLogger.Log($"Error: Exception in ActiveInActive - {ex.Message}");
                return SetFailureResponse(responseVm, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IResponseVm<List<DiscoountVm>>> GetAll_Coupons(Pagination filter)
        {
            IResponseVm<List<DiscoountVm>> responseVm = new ResponseVm<List<DiscoountVm>>();
            FileLogger.Log("Start: Fetching all coupons.");

            try
            {
                if (!await ValidateTenantAsync())
                    return SetFailureResponse(responseVm, AppConstant.AccessDenied);

                var DiscountList = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).Select(
                x => new DiscoountVm
                {
                    DiscountId = x.DiscountId,
                    CustomerId = x.CustomerId,
                    ApplyforName = x.ApplyFor,
                    AppliedFor = x.ApplyFor == EnumValue.GetEnumValue(AppliedFor.All) ? 0 : 1,
                    DiscountType = x.DiscountType == EnumValue.GetEnumValue(DiscountType.Percentage) ? 0 : 1,
                    DiscountTypeName = x.DiscountType,
                    CouponCode = x.CouponCode,
                    PercentageDiscount = x.PercentageDiscount,
                    FixedAmount = x.FixedAmount,
                    ExpiryDate = x.ExpiryDate,
                    StartDate = x.StartDate,
                    IsActive = x.IsActive,
                }
            ).ToList();
                foreach (var item in DiscountList)
                {
                    var customerData = contactService.GetAllQuerable().Where(x => x.ContactId == item.CustomerId && x.IsDelete == false).Select(x => x.Name).FirstOrDefault();
                    item.CustomerName = customerData;
                }


                var filteredRecords = DiscountList.Where(x =>
                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                    (filter.StartDate != null ? x.StartDate >= filter.StartDate : true) &&
                    (filter.ExpiryDate != null ? x.ExpiryDate <= filter.ExpiryDate : true) &&
                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                    (string.IsNullOrEmpty(filter.CustomerId) ? true : x.CustomerId.ToString() == filter.CustomerId || x.CustomerId == null) &&
                    (string.IsNullOrEmpty(filter.SearchValue) ? true :
                        (x.CustomerName?.ToLower() ?? "").Contains(filter.SearchValue.Trim().ToLower()) ||
                        (x.CouponCode?.ToLower() ?? "").Contains(filter.SearchValue.Trim().ToLower()) ||
                        (x.PercentageDiscount.ToString() ?? "").Contains(filter.SearchValue.Trim()) ||
                        (x.FixedAmount.ToString() ?? "").Contains(filter.SearchValue.Trim()) ||
                        (x.ExpiryDate.ToString() ?? "").Contains(filter.SearchValue.Trim()) ||
                        (x.StartDate.ToString() ?? "").Contains(filter.SearchValue.Trim()))
                );

                var pagination = Paginate.Pagination(filteredRecords, filter);


                responseVm.IsSuccess = true;
                responseVm.Data = pagination;
                responseVm.TotalRecords = DiscountList.Count;
                responseVm.Message = AppConstant.FetchSuccessMsg;

                FileLogger.Log("Success: Coupons fetched successfully.");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Error: Exception in GetAll_Coupons - {ex.Message}");
                return SetFailureResponse(responseVm, $"Error: {ex.Message}");
            }

            return responseVm;
        }

        #region Helper Methods

        private async Task<bool> ValidateTenantAsync()
        {
            return await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);
        }

        private async Task<TblDiscount> GetActiveDiscountAsync(Guid discountId)
        {
            return await Task.FromResult(FirstOrDefault(x =>
                x.DiscountId == discountId &&
                x.TenantId == AppConstant.LoggedInTenantId &&
                x.IsDelete == false));
        }

        private void ToggleActiveStatus(TblDiscount discount)
        {
            discount.IsActive = !discount.IsActive;
            discount.UpdatedAt = DateTime.Now;
            discount.ModifiedBy = AppConstant.LoggedInUserID;
        }

        private IResponseVm<T> SetFailureResponse<T>(IResponseVm<T> responseVm, string message)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = message;
            FileLogger.Log($"Failure: {message}");
            return responseVm;
        }

        private IResponseVm<T> SetSuccessResponse<T>(IResponseVm<T> responseVm, string message)
        {
            responseVm.IsSuccess = true;
            responseVm.Message = message;
            FileLogger.Log($"Success: {message}");
            return responseVm;
        }

        private bool ValidateDiscountType(CreateDiscount discount, out string message)
        {
            DateTime expiryDate = Convert.ToDateTime(discount.ExpiryDate);
            DateTime startDate = Convert.ToDateTime(discount.StartDate);

            if (expiryDate < startDate)
            {
                message = "Expiry Date must be greater than Start Date.";
                return false;
            }

            if (discount.discountType == DiscountType.Percentage)
            {
                if (discount.PercentageDiscount is null or <= 0 or > 100)
                {
                    message = "Percentage discount must be between 1 and 100.";
                    return false;
                }
            }
            else if (discount.discountType == DiscountType.Fixed)
            {
                if (discount.FixedAmount is null or <= 0)
                {
                    message = "Fixed amount must be greater than 0.";
                    return false;
                }
            }
            else
            {
                message = "Invalid discount type.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private bool ValidateUpdateDiscount(UpdateDiscount discount, out string message)
        {
            if ( discount.PercentageDiscount > 100)
            {
                message = "Percentage must be between 0 to 100.";
                return false;
            }
            if (discount.FixedAmount <= 0)
            {
                message = "Fixed amount must be greater than 0.";
                return false;
            }
            if (discount.StartDate.HasValue && discount.ExpiryDate.HasValue)
            {
                if (discount.StartDate >= discount.ExpiryDate)
                {
                    message = "Start date must be earlier than expiry date.";
                    return false;
                }
            }
            else
            {
                message = "Both start date and expiry date are required.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private IEnumerable<TblDiscount> BuildDiscountRecords(CreateDiscount createDiscount, string couponCode)
        {
            var discountList = new List<TblDiscount>();

            if (createDiscount.AppliedFor == AppliedFor.Customers && createDiscount.CustomerId?.Length > 0)
            {
                foreach (var customerId in createDiscount.CustomerId)
                {
                    discountList.Add(BuildSingleDiscount(createDiscount, couponCode, customerId));
                }
            }
            else if (createDiscount.AppliedFor == AppliedFor.All)
            {
                discountList.Add(BuildSingleDiscount(createDiscount, couponCode));
            }

            return discountList;
        }

        private TblDiscount BuildSingleDiscount(CreateDiscount createDiscount, string couponCode, Guid? customerId = null)
        {
            return new TblDiscount
            {
                DiscountId = Guid.NewGuid(),
                CouponCode = couponCode.Trim().ToUpper(),
                ApplyFor = EnumValue.GetEnumValue(createDiscount.AppliedFor),
                DiscountType = EnumValue.GetEnumValue(createDiscount.discountType),
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
        }

        private async Task<TblDiscount> FindExistingDiscountAsync(Guid discountId)
        {
            return (await GetAll()).Where(x => x.DiscountId == discountId &&
                                                                              x.TenantId == AppConstant.LoggedInTenantId &&
                                                                              !(bool)x.IsDelete).FirstOrDefault();
        }
         
        private void UpdateDiscountRecord(TblDiscount existing, UpdateDiscount updated)
        {
            existing.CouponCode = updated.CouponCode?.ToUpper();
            existing.DiscountType = EnumValue.GetEnumValue(updated.discountType);
            existing.PercentageDiscount = updated.discountType == DiscountType.Percentage ? updated.PercentageDiscount : null;
            existing.FixedAmount = updated.discountType == DiscountType.Fixed ? updated.FixedAmount : null;
            existing.ExpiryDate = updated.ExpiryDate;
            existing.StartDate = updated.StartDate;
            existing.UpdatedAt = DateTime.Now;
            existing.ModifiedBy = AppConstant.LoggedInUserID;
        }

        private void MarkAsDeleted(TblDiscount discount)
        {
            discount.IsDelete = true;
            discount.IsActive = false;
            discount.UpdatedAt = DateTime.Now;
            discount.ModifiedBy = AppConstant.LoggedInUserID;
        }

        private IResponseVm<string> SetFailureResponse(IResponseVm<string> responseVm, string message)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = message;
            FileLogger.Log($"Failure: {message}");
            return responseVm;
        }

        private IResponseVm<string> SetSuccessResponse(IResponseVm<string> responseVm, string message)
        {
            responseVm.IsSuccess = true;
            responseVm.Message = message;
            FileLogger.Log($"Success: {message}");
            return responseVm;
        }

        #endregion



    }
}
