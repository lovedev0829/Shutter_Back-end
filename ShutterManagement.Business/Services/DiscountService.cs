using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
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
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;

        public DiscountService(BtprojecShuttermanagementDevContext context,ITenantService tenantService, ITaxExemption taxExemptionService, ClaimsProperty claimsProperty )
        {
            _context = context;
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
        }

        private string GenerateCouponCode()
        {
            var tenantCode = dbContext.TblTenants
                                .Where(t => t.TenantId == AppConstant.LoggedInTenantId)
                                .Select(t => t.Code)
                                .FirstOrDefault();

            var tenantCodePrefix = tenantCode?.Substring(0, Math.Min(4, tenantCode.Length)) ?? "XXXX"; 

            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var randomCode = new string(Enumerable.Repeat(chars, 6)
                            .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{tenantCodePrefix}-{randomCode}";
        }


        public async Task<IResponseVm<string>> CreateDiscountAsync(CreateDiscount createDiscount)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (!IsTenantExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                if (createDiscount.PercentageDiscount <= 0 || createDiscount.PercentageDiscount > 100)
                {
                    responseVm.Message = "Percentage must be between 1 and 100.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                if (createDiscount.FixedAmount <= 0)
                {
                    responseVm.Message = "Fixed amount must be greater than 0.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var couponCode = GenerateCouponCode();
                //var IscouponCodeExit = await AnyAsync(x => x.IsActive == true
                //    && x.IsDelete == false && x.CouponCode == couponCode);
                //{

                //}
                if (createDiscount.Type == 1 || createDiscount.Type==2 && createDiscount.CustomerId != null && createDiscount.CustomerId.Length > 0)
                {
                    foreach (var customerId in createDiscount.CustomerId)
                    {
                        var discount = new TblDiscount
                        {
                            DiscountId = Guid.NewGuid(),
                            CouponCode = couponCode.Trim(),
                            CustomerId = customerId,
                            PercentageDiscount = createDiscount.PercentageDiscount.Value,
                            FixedAmount = createDiscount.FixedAmount.Value,
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
                else
                {
                    responseVm.Message = "No customers provided or invalid type.";
                    responseVm.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }


        public async Task<IResponseVm<string>> UpdateAsync(UpdateDiscount updateDiscount)
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

                var existingDiscount =  FirstOrDefault(x => x.DiscountId == updateDiscount.DiscountId && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                if (existingDiscount == null)
                {
                    responseVm.Message = "Record not found.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                existingDiscount.PercentageDiscount = updateDiscount.PercentageDiscount.Value;
                existingDiscount.FixedAmount = updateDiscount.FixedAmount.Value;
                existingDiscount.ExpiryDate = updateDiscount.ExpiryDate;
                existingDiscount.StartDate = updateDiscount.StartDate;
                existingDiscount.UpdatedAt = DateTime.Now;
                existingDiscount.ModifiedBy = AppConstant.LoggedInUserID;

                var existingCustomerDiscounts = GetAllQuerable().Where(x => x.DiscountId == updateDiscount.DiscountId).ToList();
                

                if (updateDiscount.CustomerId != null && updateDiscount.CustomerId.Length > 0)
                {
                    foreach (var customerId in updateDiscount.CustomerId)
                    {
                        var discountCustomer = new TblDiscount
                        {
                            DiscountId = existingDiscount.DiscountId,
                            CustomerId = customerId
                        };

                        await Add(discountCustomer);
                    }
                }

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


        public async Task<IResponseVm<string>> DeleteAsync(Guid discountId)
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

                var discount =  FirstOrDefault(x => x.DiscountId == discountId && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                if (discount == null)
                {
                    responseVm.Message = "Discount not found or already deleted.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                discount.IsDelete = true;
                discount.UpdatedAt = DateTime.Now;
                discount.ModifiedBy = AppConstant.LoggedInUserID;

                var relatedCustomers =  GetAllQuerable().Where(x => x.DiscountId == discountId).ToList();
                foreach (var relatedCustomer in relatedCustomers)
                {
                    relatedCustomer.IsDelete = true;
                    await Update(relatedCustomer);
                }


                responseVm.Message = AppConstant.DeleteSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
                responseVm.IsSuccess = false;
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
                    responseVm.Message = "Discount not found or already deleted.";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                discount.IsActive = false;
                discount.UpdatedAt = DateTime.Now;
                discount.ModifiedBy = AppConstant.LoggedInUserID;

                var relatedCustomers = dbContext.TblDiscounts.Where(x => x.DiscountId == discountId).ToList();
                foreach (var relatedCustomer in relatedCustomers)
                {
                    relatedCustomer.IsActive = false;
                    relatedCustomer.UpdatedAt = DateTime.Now;
                    relatedCustomer.ModifiedBy= AppConstant.LoggedInUserID;
                    await Update(relatedCustomer);
                }

               

                responseVm.Message = AppConstant.DeleteSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

       
    }
}
