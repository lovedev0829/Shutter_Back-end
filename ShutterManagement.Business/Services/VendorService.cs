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
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class VendorService : BaseRepository<BtprojecShuttermanagementDevContext, TblVendor>, IVendorService
    {
        private readonly ITenantService tenantService;
        public VendorService(ITenantService tenantService)
        {
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
                                
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                if (IsVendorExist != null)
                {
                    IsVendorExist.IsActive = !IsVendorExist.IsActive;
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
                var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Vendor, AppConstant.LoggedInCompanyCode);
                bool IsVendorNameExist = await AnyAsync(x => x.IsDelete == false &&
                                        x.VendorName == model.VendorName.Trim() && x.TenantId == AppConstant.LoggedInTenantId);

                if (IsVendorNameExist)
                {
                    responseVm.Message = "Vendor Name " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                bool IsVendorEmailExists = await AnyAsync(x => x.IsDelete == false &&
                     (x.Email == model.Email.Trim())
                     && x.TenantId == AppConstant.LoggedInTenantId);
                if (IsVendorEmailExists)
                {
                    responseVm.Message = "Vendor Email " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                bool IsVendorPhoneExists = await AnyAsync(x => x.IsDelete == false &&
                                        (x.PhoneNumber == model.PhoneNumber.Trim())
                                        && x.TenantId == AppConstant.LoggedInTenantId);

                if (IsVendorPhoneExists)
                {
                    responseVm.Message = "Vendor Phone " + AppConstant.AlreadyExists;
                    return responseVm;
                }

                //bool isvendorcodeexists = await anyasync(x => x.isdelete == false && (x.vendorcode == model.vendorcode.trim())
                //&& x.tenantid == appconstant.loggedintenantid);
                //if (isvendorcodeexists)
                //{
                //    responsevm.message = "vendor code " + appconstant.alreadyexists;
                //    return responsevm;
                //}

                TblVendor vendor = new TblVendor
                {
                    VendorName = model.VendorName.Trim(),
                    Email = model.Email.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    State = model.State.Trim(),
                    City = model.City.Trim(),
                    ZipCode = model.ZipCode.Trim(),
                    StreetAddress = model.StreetAddress.Trim(),
                    VendorCode = generateUnique,
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

        public async Task<IResponseVm<List<VendorListVm>>> GetAll_Vendor(Pagination filter)
        {
            IResponseVm<List<VendorListVm>> responseVm = new ResponseVm<List<VendorListVm>>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    var IsComponentExists = (await GetAll())
                    .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList();

                    var FilterRecords = IsComponentExists
                        .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                            (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                    (x.VendorId.ToString() == filter.SearchValue) ||
                                    (x.VendorName ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.Email ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.PhoneNumber ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.City ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.State ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.StreetAddress ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.VendorCode ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.ZipCode ?? "").Contains(filter.SearchValue.Trim())
                                    ))
                        .ToList();
                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.IsSuccess = true;
                    responseVm.Data = TakenRecords.Select(x => new VendorListVm
                    {
                        VendorId = x.VendorId,
                        VendorName = x.VendorName,
                        City = x.City,
                        State = x.State,
                        ZipCode = x.ZipCode,
                        Email = x.Email,
                        StreetAddress = x.StreetAddress,
                        PhoneNumber = x.PhoneNumber,
                        TenantId = x.TenantId,
                        IsActive = x.IsActive,
                        VendorCode = x.VendorCode,
                    }).ToList();
                    responseVm.TotalRecords = FilterRecords.Count;
                    responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                }
                else
                 {
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
        public   async Task<IResponseVm<string>> Update_Vendor(UpdateVendor model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists =   tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (IsTenantExists!=null)
                {
                    var vendor =  GetAllQuerable()
                        .Where(c => c.VendorId == model.VendorId && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false).FirstOrDefault();
                    bool IsVendorEmail =  Any(x => x.IsDelete == false &&(x.Email == model.Email.Trim())
                                           && x.TenantId == AppConstant.LoggedInTenantId && x.VendorId!=model.VendorId );

                    
                    if (IsVendorEmail)
                    {
                        responseVm.Message = "Vendor Email " + AppConstant.AlreadyExists;
                        return (responseVm);
                    }
                    bool IsVendorPhoneExists =  Any(x => x.IsDelete == false &&
                                        (x.PhoneNumber == model.PhoneNumber.Trim())
                                        && x.TenantId == AppConstant.LoggedInTenantId && x.VendorId != model.VendorId);

                    if (IsVendorPhoneExists)
                    {
                        responseVm.Message = "Vendor Phone " + AppConstant.AlreadyExists;
                        return (responseVm);
                    }
                   bool IsVendorNameEXists = Any(x => x.IsDelete == false &&
                                        (x.VendorName == model.VendorName.Trim())
                                        && x.TenantId == AppConstant.LoggedInTenantId && x.VendorId != model.VendorId);
                    if (IsVendorNameEXists)
                    
                        {
                         responseVm.Message="Vendor Name"+ AppConstant.AlreadyExists;
                        return (responseVm);
                        }
                    //bool IsVendorCodeExists = Any(x => x.IsDelete == false &&
                    //                   (x.VendorCode == model.VendorCode.Trim())
                    //                   && x.TenantId == AppConstant.LoggedInTenantId && x.VendorId != model.VendorId);
                    //if (IsVendorCodeExists)

                    //{
                    //    responseVm.Message = "Vendor Code" + AppConstant.AlreadyExists;
                    //    return (responseVm);
                    //}
                    if (vendor != null)
                    {
                        vendor.VendorName = model.VendorName;
                        vendor.Email = model.Email;
                        vendor.PhoneNumber = model.PhoneNumber;
                        vendor.StreetAddress = model.StreetAddress;
                        vendor.City = model.City;
                        vendor.State = model.State;
                        vendor.ZipCode = model.ZipCode;
                        vendor.ModifiedBy = AppConstant.LoggedInUserID;
                        vendor.UpdatedAt = DateTime.Now;
                         Update(vendor);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                   
                }
              
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return (responseVm); 

        }

    }
}
