using Newtonsoft.Json;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Services;
using ShutterManagement.DAL.Context;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core;
using AutoMapper;
using ShutterManagement.Core.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ShutterManagement.Business.Services
{
    public class TenantServices : BaseRepository<ShutterDBContext, TblTenant>, ITenantService
    {
        private readonly DataServices dataServices;
        private readonly IMapper mapper;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IUserService userService;

        public TenantServices(DataServices dataServices, IMapper mapper, ITenantBrandingService tenantBrandingService
            , ClaimsProperty claimsProperty, IUserService userService
            )
        {
            this.dataServices = dataServices;
            this.mapper = mapper;
            this.tenantBrandingService = tenantBrandingService;
            this.claimsProperty = claimsProperty;
            this.userService = userService;
        }

        public async Task<IResponseVm<string>> TenantCreate(CreateTenant model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsCodeExists = await AnyAsync(x => x.IsActive == true
                    && x.IsDelete == false && x.Code == model.Code);
                if (IsCodeExists)
                {
                    responseVm.Message = AppConstant.CodeAlreadyExists;
                    return responseVm;
                }

                var TenantDetails = (await GetAll()).Where(x => x.IsDelete == false).Select(x => x.BrandingInfo).ToList();
                bool IsTenantEmailExists = await tenantBrandingService.AnyAsync(x => x.Email == model.Email && x.IsDelete == false && TenantDetails.Contains(x.Id));
                if (IsTenantEmailExists)
                {
                    responseVm.Message = "Email " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                bool IsTenantCompanynameExists = await tenantBrandingService.AnyAsync(x => x.CompanyName == model.CompanyName.Trim() && x.IsDelete == false);
                if (IsTenantCompanynameExists)
                {
                    responseVm.Message = "CompanyName " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                bool IsUserEmailExists = await userService.AnyAsync(x => x.Email == model.UserEmail && x.IsDelete == false);
                if (IsUserEmailExists)
                {
                    responseVm.Message = "User Email " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                var HashPassword = AesEncryptionHelper.Encrypt(model.Password ?? "");
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@FirstName", SqlDbType.NVarChar) { SqlValue = model.FirstName },
                    new SqlParameter("@MiddleName", SqlDbType.NVarChar) { SqlValue = model.MiddleName },
                    new SqlParameter("@LastName", SqlDbType.NVarChar) { SqlValue = model.LastName },
                    new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = model.Code.Trim() },
                    new SqlParameter("@URL", SqlDbType.NVarChar) { SqlValue = model.URL },
                    new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email },
                    new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber },
                    new SqlParameter("@Logo", SqlDbType.NVarChar) { SqlValue = model.Logo },
                    new SqlParameter("@CompanyName", SqlDbType.NVarChar) { SqlValue = model.CompanyName.Trim() },
                    new SqlParameter("@PrimaryColor", SqlDbType.NVarChar) { SqlValue = model.PrimaryColor },
                    new SqlParameter("@SecondaryColor", SqlDbType.NVarChar) { SqlValue = model.SecondaryColor },
                    new SqlParameter("@StreetAddress", SqlDbType.NVarChar) { SqlValue = model.StreetAddress },
                    new SqlParameter("@City", SqlDbType.NVarChar) { SqlValue = model.City },
                    new SqlParameter("@Country", SqlDbType.NVarChar) { SqlValue = model.Country },
                    new SqlParameter("@ZipCode", SqlDbType.NVarChar) { SqlValue = model.ZipCode },
                    new SqlParameter("@UserFirstName", SqlDbType.NVarChar) { SqlValue = model.UserFirstName },
                    new SqlParameter("@UserMiddleName", SqlDbType.NVarChar) { SqlValue = model.UserMiddleName },
                    new SqlParameter("@UserLastName", SqlDbType.NVarChar) { SqlValue = model.UserLastName },
                    new SqlParameter("@UserEmail", SqlDbType.NVarChar) { SqlValue = model.UserEmail },
                    new SqlParameter("@UserPhoneNumber", SqlDbType.NVarChar) { SqlValue = model.UserPhoneNumber },
                    new SqlParameter("@Password", SqlDbType.NVarChar) { SqlValue = HashPassword },
                    new SqlParameter("@LoggedInUser", SqlDbType.UniqueIdentifier) { SqlValue = AppConstant.LoggedInUserID },
                    new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Create" },
                };

                await dataServices.CallSP("sp_TenantCRUD", null, sqlParameters);

                var AddedTenant = GetAllQuerable().Where(x => x.FirstName == model.FirstName && x.Code == model.Code).FirstOrDefault();
                if (AddedTenant != null)
                {
                    string[] defaultRoleArray = ["Tenant Admin", "Sales", "Installer"];
                    var DefaultRoles = dbContext.TblRoles.Where(x => x.IsShow == true && x.IsActive == true && x.IsDelete == false
                    && defaultRoleArray.Contains(x.RoleName)).ToList();
                    foreach (var item in DefaultRoles)
                    {
                        var DefaultRolesPermission = dbContext.TblRolesPermissions.Where(x => x.RoleId == item.Id).ToList();
                        
                        TblRole role = new TblRole();
                        role.RoleName = item.RoleName;
                        role.IsShow = false;
                        role.CreatedAt = DateTime.Now;
                        role.CreatedBy = AppConstant.LoggedInUserID;
                        role.IsActive = true;
                        role.IsDelete = false;
                        role.TenantId = AddedTenant.TenantId;
                        role.Id = Guid.NewGuid();
                        var res = await dbContext.TblRoles.AddAsync(role);
                        dbContext.SaveChanges();
                        if (DefaultRolesPermission != null)
                        {
                            foreach (var permission in DefaultRolesPermission)
                            {
                                TblRolesPermission rolesPermission = new TblRolesPermission
                                {
                                    Id = Guid.NewGuid(),
                                    RoleId = role.Id,
                                    ModulePermissionId = permission.ModulePermissionId,
                                };
                                dbContext.TblRolesPermissions.Add(rolesPermission);
                                dbContext.SaveChanges();
                            }
                        }
                    }
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

        public async Task<IResponseVm<string>> TenantUpdate(UpdateTenant model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                if (string.IsNullOrEmpty(model.TenantId))
                {
                    responseVm.Message = "Tenant Id is required";
                    return responseVm;
                }
                var tenantid = Guid.Parse(model.TenantId);
                //var braindingInfo = GetAllQuerable().Where(x => x.TenantId.ToString() == model.TenantId && x.IsDelete == false).Select(x => x.BrandingInfo).FirstOrDefault();
                var TenantDetails = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId.ToString() != model.TenantId).Select(x => x.BrandingInfo).ToList();
                bool IsTenantEmailExists = await tenantBrandingService.AnyAsync(x => x.Email == model.Email  && x.IsDelete == false && TenantDetails.Contains(x.Id));
                if (IsTenantEmailExists)
                {
                    responseVm.Message = "Email " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                bool IsTenantCompanynameExists = await tenantBrandingService.AnyAsync(x => x.CompanyName == model.CompanyName.Trim() && x.IsDelete == false &&
                TenantDetails.Contains(x.Id));
                if (IsTenantCompanynameExists)
                {
                    responseVm.Message = "CompanyName " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                var IsCodeExists = await AnyAsync(x => x.IsActive == true
                    && x.IsDelete == false && x.Code == model.Code.Trim() && x.TenantId != tenantid);
                if (!IsCodeExists)
                {

                    var HashPassword = string.IsNullOrEmpty(model.Password) ? "" : AesEncryptionHelper.Encrypt(model.Password ?? "");

                    SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = tenantid},
                    new SqlParameter("@FirstName", SqlDbType.NVarChar) { SqlValue = model.FirstName },
                    new SqlParameter("@MiddleName", SqlDbType.NVarChar) { SqlValue = model.MiddleName },
                    new SqlParameter("@LastName", SqlDbType.NVarChar) { SqlValue = model.LastName },
                    new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = model.Code?.Trim() },
                    new SqlParameter("@URL", SqlDbType.NVarChar) { SqlValue = model.URL },
                    new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email?.Trim() },
                    new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber },
                    new SqlParameter("@Logo", SqlDbType.NVarChar) { SqlValue = model.Logo },
                    new SqlParameter("@CompanyName", SqlDbType.NVarChar) { SqlValue = model.CompanyName.Trim() },
                    new SqlParameter("@PrimaryColor", SqlDbType.NVarChar) { SqlValue = model.PrimaryColor },
                    new SqlParameter("@SecondaryColor", SqlDbType.NVarChar) { SqlValue = model.SecondaryColor },
                    new SqlParameter("@StreetAddress", SqlDbType.NVarChar) { SqlValue = model.StreetAddress },
                    new SqlParameter("@City", SqlDbType.NVarChar) { SqlValue = model.City },
                    new SqlParameter("@Country", SqlDbType.NVarChar) { SqlValue = model.Country },
                    new SqlParameter("@ZipCode", SqlDbType.NVarChar) { SqlValue = model.ZipCode },
                    new SqlParameter("@UserFirstName", SqlDbType.NVarChar) { SqlValue = model.UserFirstName },
                    new SqlParameter("@UserMiddleName", SqlDbType.NVarChar) { SqlValue = model.UserMiddleName },
                    new SqlParameter("@UserLastName", SqlDbType.NVarChar) { SqlValue = model.UserLastName },
                    new SqlParameter("@UserEmail", SqlDbType.NVarChar) { SqlValue = model.UserEmail },
                    new SqlParameter("@UserPhoneNumber", SqlDbType.NVarChar) { SqlValue = model.UserPhoneNumber },
                    new SqlParameter("@Password", SqlDbType.NVarChar) { SqlValue = HashPassword },
                    new SqlParameter("@LoggedInUser", SqlDbType.UniqueIdentifier) { SqlValue = AppConstant.LoggedInUserID },

                    new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Update" },
                };

                    await dataServices.CallSP("sp_TenantCRUD", null, sqlParameters);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;

                }
                else
                {
                    responseVm.Message = AppConstant.CodeAlreadyExists;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> TenantDelete(string TenantId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var TenantDetails = GetAllQuerable().Where(x => x.TenantId.ToString() == TenantId && x.TenantId != AppConstant.LoggedInTenantId && x.ParentTenantId == null).FirstOrDefault();
                if (TenantDetails != null)
                {
                    TenantDetails.IsDelete = true;
                    TenantDetails.IsActive = false;
                    TenantDetails.ModifiedBy = AppConstant.LoggedInUserID;
                    TenantDetails.UpdatedAt = DateTime.Now;

                    var brandingInfo = tenantBrandingService.GetAllQuerable().Where(x => x.Id == TenantDetails.BrandingInfo && x.IsDelete == false).FirstOrDefault();
                    if (brandingInfo != null)
                    {
                        brandingInfo.IsActive = false;
                        brandingInfo.IsDelete = true;   
                    }
                    var TenantUser = userService.GetAllQuerable().Where(x => x.TenantId.ToString() == TenantId).ToList();

                    foreach (var item in TenantUser)
                    {
                        item.IsActive = false;
                        item.IsDelete = true;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = AppConstant.LoggedInUserID;
                        await userService.Update(item);
                    }
                    await Update(TenantDetails);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
                }
                else
                {
                    responseVm.Message = AppConstant.NoUserFound;
                }

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;

        }

        public async Task<IResponseVm<string>> TenantActiveInactive(string TenantId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var TenantDetails = GetAllQuerable().Where(x => x.TenantId.ToString() == TenantId && x.TenantId != AppConstant.LoggedInTenantId && x.ParentTenantId == null).FirstOrDefault();
                if (TenantDetails != null)
                {
                    TenantDetails.IsActive = !TenantDetails.IsActive;
                    TenantDetails.ModifiedBy = AppConstant.LoggedInUserID;
                    TenantDetails.UpdatedAt = DateTime.Now;


                    var brandingInfo = tenantBrandingService.GetAllQuerable().Where(x => x.Id == TenantDetails.BrandingInfo && x.IsDelete == false).FirstOrDefault();
                    if (brandingInfo != null)
                    {
                        brandingInfo.IsActive = !brandingInfo.IsActive;
                        //brandingInfo.IsDelete = true;
                    }

                    var TenantUser = userService.GetAllQuerable().Where(x => x.TenantId.ToString() == TenantId).ToList();

                    foreach (var item in TenantUser)
                    {
                        item.IsActive = !item.IsActive;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = AppConstant.LoggedInUserID;

                        await userService.Update(item);
                    }
                    await Update(TenantDetails);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
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

        public async Task<IResponseVm<List<GetAllTenant>>> TenantGetAll(Pagination filter)
        {
            IResponseVm<List<GetAllTenant>> responseVm = new ResponseVm<List<GetAllTenant>>();
            try
            {
                var TenantList = (await GetAll()).Where(x => x.IsDelete == false && x.ParentTenantId == null && x.TenantId != AppConstant.LoggedInTenantId).ToList().OrderByDescending(x => x.CreatedAt);
                foreach (var item in TenantList)
                {
                    item.BrandingInfoNavigation = tenantBrandingService.GetAllQuerable().Where(x => x.Id == item.BrandingInfo).FirstOrDefault();
                }
                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var FilterRecords = TenantList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.TenantId != "" ?
                                    x.TenantId.ToString() == filter.TenantId?.ToString() : true) &&
                                    (filter.SearchValue != "" ?
                                    (x.TenantId.ToString() == filter.SearchValue) ||
                                    ((x.BrandingInfoNavigation?.Email ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.BrandingInfoNavigation?.PhoneNumber ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.BrandingInfoNavigation?.CompanyName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.FirstName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.LastName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.MiddleName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.Code ?? "").ToLower().Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    var data = TakenRecords.Select(x => new GetAllTenant
                    {
                        TenantId = x.TenantId,
                        FirstName = x.FirstName,
                        MiddleName = x.MiddleName,
                        LastName = x.LastName,
                        Code = x.Code,
                        URL = x.Url,
                        Email = x.BrandingInfoNavigation.Email,
                        PhoneNumber = x.BrandingInfoNavigation.PhoneNumber,
                        Logo = x.BrandingInfoNavigation.Logo,
                        CompanyName = x.BrandingInfoNavigation.CompanyName,
                        PrimaryColor = x.BrandingInfoNavigation.PrimaryColor,
                        SecondaryColor = x.BrandingInfoNavigation.SecondaryColor,
                        StreetAddress = x.BrandingInfoNavigation.StreetAddress,
                        City = x.BrandingInfoNavigation.City,
                        Country = x.BrandingInfoNavigation.Country,
                        ZipCode = x.BrandingInfoNavigation.ZipCode,
                        IsActive = x.IsActive
                    }).ToList();
                    for (int i = 0; i < data.Count; i++)
                    {
                        var UserDetails = (await userService.GetAll()).
                            Where(x => x.TenantId == data[i].TenantId).OrderBy(x => x.CreatedAt).FirstOrDefault();
                        if (UserDetails != null)
                        {
                            data[i].UserFirstName = UserDetails.FirstName;
                            data[i].UserMiddleName = UserDetails.MiddleName;
                            data[i].UserLastName = UserDetails.LastName;
                            data[i].UserPhoneNumber = UserDetails.Phone;
                            data[i].UserEmail = UserDetails.Email;
                        }
                    }
                    responseVm.Data = data;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count;
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
    }
}
