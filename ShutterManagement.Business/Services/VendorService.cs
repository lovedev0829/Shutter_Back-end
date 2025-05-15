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
    public class VendorService : BaseRepository<ShutterDBContext, TblVendor>, IVendorService
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
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
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
                //bool IsVendorNameExist = await AnyAsync(x => x.IsDelete == false &&
                //                        x.VendorName == model.VendorName.Trim() && x.TenantId == AppConstant.LoggedInTenantId);

                //if (IsVendorNameExist)
                //{
                //    responseVm.Message = "Vendor Name " + AppConstant.AlreadyExists;
                //    return responseVm;
                //}
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

                int Version1 = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.CreatedAt.Value.Date == DateTime.Today.Date).OrderByDescending(p => p.CreatedAt)
                .Select(p => (p.VendorCode.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(p.VendorCode.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0)).FirstOrDefault();
                Version1 += 1;
                var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Vendor, AppConstant.LoggedInCompanyCode, Version1);

                TblVendor vendor = new TblVendor
                {
                    FirstName = model?.FirstName?.Trim()??"",
                    LastName = model?.LastName?.Trim() ?? "",
                    Email = model.Email.Trim()??"",
                    PhoneNumber = model.PhoneNumber.Trim() ?? "",
                    State = model.StateId != null ? model.StateId : null,
                    City = model.City.Trim() ?? "",
                    ZipCode = model.ZipCode.Trim()??"",
                    StreetAddress = model.StreetAddress.Trim() ?? "",
                    VendorCode = generateUnique??"",
                    CompanyName= model.CompanyName.Trim()??"",
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
                                    ((x.FirstName.ToLower() + " " + x.LastName.ToLower()) ?? "").Contains(filter.SearchValue) ||
                                    (x.FirstName.ToLower() ?? "").Contains(filter.SearchValue) ||
                                    (x.CompanyName.ToLower() ?? "").Contains(filter.SearchValue) ||
                                    (x.LastName.ToLower() ?? "").Contains(filter.SearchValue) ||
                                    (x.Email ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.PhoneNumber ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.City ?? "").Contains(filter.SearchValue.Trim()) ||
                                    //(x.sta ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.StreetAddress ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.VendorCode ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.ZipCode ?? "").Contains(filter.SearchValue.Trim())
                                    ))
                        .ToList();
                    var queryable = FilterRecords.AsQueryable();

                    queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                    queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                    var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                    responseVm.IsSuccess = true;
                    responseVm.Data = TakenRecords.Select(x => new VendorListVm
                    {
                        VendorId = x.VendorId,
                        FirstName = x.FirstName??"",
                        LastName = x.LastName ?? "",
                        City = x.City??"",
                        StateName= dbContext.MstStates.Where(s => s.Id == x.State).Select(y => y.StateName ?? "").FirstOrDefault()??"",
                        StateId=x.State,
                        ZipCode = x.ZipCode ?? "",  
                        Email = x.Email ?? "",
                        StreetAddress = x.StreetAddress ??"",
                        PhoneNumber = x.PhoneNumber ?? "",
                        TenantId = x.TenantId,
                        IsActive = x.IsActive,
                        VendorCode = x.VendorCode,
                        CompanyName=x.CompanyName
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
                   //bool IsVendorNameEXists = Any(x => x.IsDelete == false &&
                   //                     (x.VendorName == model.VendorName.Trim())
                   //                     && x.TenantId == AppConstant.LoggedInTenantId && x.VendorId != model.VendorId);
                   // if (IsVendorNameEXists)
                    
                   //     {
                   //      responseVm.Message="Vendor Name"+ AppConstant.AlreadyExists;
                   //     return (responseVm);
                   //     }
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
                        vendor.FirstName = model.FirstName??"";
                        vendor.LastName = model.LastName??"";
                        vendor.Email = model.Email ?? "";
                        vendor.PhoneNumber = model.PhoneNumber??"";
                        vendor.StreetAddress = model.StreetAddress ?? "";
                        vendor.City = model.City??"";
                        vendor.State = model.StateId != null ? model.StateId : null;
                        vendor.ZipCode = model.ZipCode ?? "";
                        vendor.CompanyName = model.CompanyName ?? "";
                        vendor.ModifiedBy = AppConstant.LoggedInUserID;
                        vendor.UpdatedAt = DateTime.Now;
                        await Update(vendor);
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
