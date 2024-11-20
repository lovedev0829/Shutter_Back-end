using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class VendorService : BaseRepository<ShutterDbContext, TblVendor>, IVendorService
    {
        private readonly IVendorService vendorService;
        private readonly ITenantService tenantService;
        public VendorService(IVendorService vendorService,ITenantService tenantService)
        {
            this.vendorService = vendorService;
            this.tenantService= tenantService;
        }
        public Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                var IsVendorExist = GetAllQuerable()
                      .Where(c => c.VendorId == Id
                                  && c.TenantId == AppConstant.LoggedInTenantId
                                  && c.IsActive == true
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                if (IsVendorExist != null)
                {
                    IsVendorExist.IsActive = false;
                    IsVendorExist.UpdatedAt = DateTime.Now;
                    IsVendorExist.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsVendorExist);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
                }
                else
                {
                    responseVm.Message = $"{AppConstant.NoDataFound}";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return Task.FromResult(responseVm);
        }

        public async Task<IResponseVm<string>> Create_Vendor(CreateVendor model)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                    TblVendor vendor = new TblVendor
                    {
                        VendorName = model.VendorName,
                        ContactInformation= model.ContactInformation,
                        PreferredVendor= model.PreferredVendor,
                        IsActive = true,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        TenantId = AppConstant.LoggedInTenantId
                    };

                    var res = await Add(vendor);
                    if (res.VendorId != Guid.Empty)
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

        public Task<IResponseVm<string>> Delete(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsVendorExist = GetAllQuerable()
                      .Where(c => c.VendorId == Id
                                  && c.TenantId == AppConstant.LoggedInTenantId
                                  && c.IsActive == true
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                if (IsVendorExist != null)
                {
                    IsVendorExist.IsActive = false;
                    IsVendorExist.IsDelete = true;
                    IsVendorExist.UpdatedAt = DateTime.Now;
                    IsVendorExist.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsVendorExist);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
                }
                else
                {
                    responseVm.Message = $"{AppConstant.NoDataFound}";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return Task.FromResult(responseVm);

        }

        public async Task<IResponseVm<dynamic>> GetAll_Vendor(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var IsComponentExists = (await GetAll())
                    .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                    .ToList();

                var FilterRecords = IsComponentExists
                    .Where(x => string.IsNullOrEmpty(filter.SearchValue) ||
                                (x.VendorName ?? "").Contains(filter.SearchValue.Trim()) ||
                                (x.Notes ?? "").Contains(filter.SearchValue.Trim()) ||
                                (x.ContactInformation ?? "").Contains(filter.SearchValue.Trim()))
                    .ToList();

                var TakenRecords = FilterRecords
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = TakenRecords;
                responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return await Task.FromResult(responseVm);
        }
        public async Task<IResponseVm<string>> Update_Vendor(UpdateVendor model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await  vendorService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (IsTenantExists)
                {
                    var vendor = await GetAllQuerable()
                        .FirstOrDefaultAsync(c => c.VendorId == model.VendorId && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsActive == true && c.IsDelete == false);

                    if (vendor != null)
                    {

                        vendor.VendorName = model.VendorName;
                        vendor.Notes = model.Notes;
                        vendor.ModifiedBy = AppConstant.LoggedInUserID;
                        vendor.UpdatedAt = DateTime.Now;

                        await Update(vendor);


                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
    }
}
