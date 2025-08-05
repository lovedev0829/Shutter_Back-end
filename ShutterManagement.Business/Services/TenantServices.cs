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
using ShutterManagement.Core.SPModel.SP_Responses;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace ShutterManagement.Business.Services
{
    public class TenantServices : BaseRepository<ShutterDBContext, TblTenant>, ITenantService
    {
        private readonly DataServices dataServices;
        private readonly IMapper mapper;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IUserService userService;
        private readonly INotificationService notification;
        private readonly IGHLConfiguration ghlConfiguration;
        private readonly EmailSendHelperService emailSendHelperService;

        // private readonly GoogleAuthService googleAuthService;

        public TenantServices(DataServices dataServices, IMapper mapper, ITenantBrandingService tenantBrandingService
            , ClaimsProperty claimsProperty, IUserService userService,INotificationService notification,
            IGHLConfiguration ghlConfiguration ,EmailSendHelperService emailSendHelperService /*,GoogleAuthService googleAuthService*/
            )
        {
            this.dataServices = dataServices;
            this.mapper = mapper;
            this.tenantBrandingService = tenantBrandingService;
            this.claimsProperty = claimsProperty;
            this.userService = userService;
            this.notification = notification;
            this.ghlConfiguration = ghlConfiguration;
            this.emailSendHelperService = emailSendHelperService;
            //this.googleAuthService = googleAuthService;
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

                //var TenantDetails = (await GetAll()).Where(x => x.IsDelete == false).Select(x => x.BrandingInfo).ToList();
                //bool IsTenantEmailExists = await tenantBrandingService.AnyAsync(x => x.Email == model.Email && x.IsDelete == false && TenantDetails.Contains(x.Id));
                //if (IsTenantEmailExists)
                //{
                //    responseVm.Message = "Email " + AppConstant.AlreadyExists;
                //    return responseVm;
                //}
                bool IsTenantCompanynameExists = await tenantBrandingService.AnyAsync(x => x.CompanyName == model.CompanyName.Trim() && x.IsDelete == false);
                if (IsTenantCompanynameExists)
                {
                    responseVm.Message = "CompanyName " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                bool IsUserEmailExists = await userService.AnyAsync(x => x.Email == model.UserEmail && x.IsDelete == false && x.IsTenantAdmin == true );
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
               //     new SqlParameter("@Markup", SqlDbType.NVarChar) { SqlValue = model.Markup },
                    new SqlParameter("@InvoicePrefix", SqlDbType.NVarChar) { SqlValue = model.InvoicePrefix },
                    new SqlParameter("@Suffix", SqlDbType.NVarChar) { SqlValue = model.Suffix??"" },
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
                    CreateNotification notification1 = new CreateNotification()
                    {
                        UserId = AppConstant.LoggedInUserID,
                        Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Tenant),
                        Body = $"{AddedTenant.FirstName} {AddedTenant.LastName} Tenant {AppConstant.Create_Notification}",
                        CreatedDate = DateTime.Now,
                        Type = Enum.GetName(typeof(NotificationModule), NotificationModule.Tenant),
                        TypeId = AddedTenant.TenantId
                    };

                    var result = await notification.CreateNotification(notification1);
                    var UserId = userService.GetAllQuerable().Where(x => x.TenantId == AddedTenant.TenantId && x.IsDelete == false).
                       Select(x => x.Id).FirstOrDefault();
                    CreateNotification notification2 = new CreateNotification()
                    {
                        UserId = UserId,
                        Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Tenant),
                        Body =  $"{AppConstant.Welcome_Notification} {AddedTenant.FirstName} {AddedTenant.LastName}!",
                        CreatedDate = DateTime.Now,
                        Type = Enum.GetName(typeof(NotificationModule), NotificationModule.Tenant),
                        TypeId = AddedTenant.TenantId

                    };

                    var result2 = await notification.CreateNotification(notification2);
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

                var mailResult = await SendWelcomeEmail(model);
                if (!mailResult.IsSuccess)
                    responseVm.Message += " " + mailResult.Message;
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
                bool IsUserEmailExists = await userService.AnyAsync(x => x.Email == model.UserEmail && x.TenantId.ToString() != model.TenantId && x.IsDelete == false && x.IsTenantAdmin == false);
                if (IsUserEmailExists)
                {
                    responseVm.Message = "User Email " + AppConstant.AlreadyExists;
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
                    new SqlParameter("@Suffix", SqlDbType.NVarChar) { SqlValue = model.Suffix },
                    new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = model.Code?.Trim() },
                    new SqlParameter("@URL", SqlDbType.NVarChar) { SqlValue = model.URL },
                    new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email?.Trim() },
                    new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber },
                    new SqlParameter("@Logo", SqlDbType.NVarChar) { SqlValue = model.Logo },
                    new SqlParameter("@CompanyName", SqlDbType.NVarChar) { SqlValue = model?.CompanyName?.Trim() },
                    new SqlParameter("@InvoicePrefix", SqlDbType.NVarChar) { SqlValue = model?.InvoicePrefix?.Trim() },
                    new SqlParameter("@PrimaryColor", SqlDbType.NVarChar) { SqlValue = model?.PrimaryColor },
                    new SqlParameter("@SecondaryColor", SqlDbType.NVarChar) { SqlValue = model?.SecondaryColor },
                    new SqlParameter("@StreetAddress", SqlDbType.NVarChar) { SqlValue = model?.StreetAddress },
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
                var TenantDetails = GetAllQuerable().Where(x => x.TenantId.ToString() == TenantId && x.TenantId != claimsProperty.GetTenantId() && x.ParentTenantId == null).FirstOrDefault();
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
                var TenantDetails = GetAllQuerable().Where(x => x.TenantId.ToString() == TenantId && x.TenantId != claimsProperty.GetTenantId() && x.ParentTenantId == null).FirstOrDefault();
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

        public async Task<IResponseVm<List<TenantList>>> GetTenantsListByUsername(string UserName)
        {
            IResponseVm<List<TenantList>> responseVm = new ResponseVm<List<TenantList>>();

            try
            {
                var users = userService.GetAllQuerable().Where(u => u.Username == UserName && u.IsDelete == false && u.IsActive == true).ToList();
                var tenantIds = users.Select(u => u.TenantId).Distinct().ToList();

                var tenants = GetAllQuerable().Where(t => tenantIds.Contains(t.TenantId) && t.IsDelete == false).ToList();
                
                var tenantList = new List<TenantList>();
                
                foreach (var tenant in tenants)
                {
                    var brandingInfo = tenantBrandingService.GetAllQuerable().Where(x => x.Id == tenant.BrandingInfo && x.IsDelete == false).FirstOrDefault();
                    
                    tenantList.Add(new TenantList
                    {
                        TenantId = tenant.TenantId,
                        FirstName = tenant.FirstName,
                        MiddleName = tenant.MiddleName,
                        LastName = tenant.LastName,
                        Code = tenant.Code,
                        URL = tenant.Url,
                        Email = brandingInfo?.Email,
                        PhoneNumber = brandingInfo?.PhoneNumber,
                        Logo = brandingInfo?.Logo,
                        PrimaryColor = brandingInfo?.PrimaryColor,
                        SecondaryColor = brandingInfo?.SecondaryColor,
                        IsActive = tenant.IsActive,
                        IsDelete = tenant.IsDelete
                    });
                }

                responseVm.Data = tenantList;
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch(Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<UserLoginDetails>> GetUserDataForTenantSwitch(string UserName, string TenantId)
        {
            IResponseVm<UserLoginDetails> responseVm = new ResponseVm<UserLoginDetails>();

            try
            {
                var user = userService.GetAllQuerable()
                    .Where(u => u.Username == UserName && u.TenantId.ToString() == TenantId && u.IsDelete == false && u.IsActive == true)
                    .FirstOrDefault();

                if (user == null)
                {
                    responseVm.Message = "User not found for the selected tenant";
                    return responseVm;
                }

                var tenant = GetAllQuerable().Where(t => t.TenantId.ToString() == TenantId && t.IsDelete == false).FirstOrDefault();
                if (tenant == null)
                {
                    responseVm.Message = "Tenant not found";
                    return responseVm;
                }

                var brandingInfo = tenantBrandingService.GetAllQuerable().Where(x => x.Id == tenant.BrandingInfo && x.IsDelete == false).FirstOrDefault();

                // Get user roles and permissions for this tenant using stored procedure
                var userRoles = await dataServices.CallSPtoRead<UserRoles>("Sp_GetLoggedinUserRoles", reader =>
                {
                    return new UserRoles
                    {
                        PermissionId = reader.GetGuid(reader.GetOrdinal("Id")),
                        PermissionName = reader.GetString(reader.GetOrdinal("Name")),
                        PermissionDisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                        ModuleDisplayName = reader.GetString(reader.GetOrdinal("ModuleDisplayName")),
                        Icon = reader.GetString(reader.GetOrdinal("Icon")),
                        Url = reader.GetString(reader.GetOrdinal("Url"))
                    };
                },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { SqlValue = user.Id });

                // Create login details object for token generation
                var loginDetail = new LoginDetails
                {
                    UserId = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    suffix = user.Suffix,
                    Email = user.Email,
                    Phone = user.Phone,
                    TenantId = tenant.TenantId,
                    ParentTenantId = tenant.ParentTenantId?.ToString(),
                    Code = tenant.Code,
                    Logo = brandingInfo?.Logo ?? "",
                    PrimaryColor = brandingInfo?.PrimaryColor ?? "",
                    SecondaryColor = brandingInfo?.SecondaryColor ?? "",
                    CompanyName = brandingInfo?.CompanyName ?? $"{tenant.FirstName} {tenant.LastName}",
                    IsGHLConfigSameAsTenant = tenant.IsGhlconfigSameAsTenant ?? false
                };

                // Create new JWT token with updated roles
                var token = CreateAuthenticationToken(loginDetail, userRoles);
                
                var loginDetails = BuildLoginResponse(loginDetail, userRoles, token);

                responseVm.Data = loginDetails;
                responseVm.IsSuccess = true;
                responseVm.Message = "Tenant switched successfully with updated permissions.";

                await ghlConfiguration.GetGHLConfiguration(loginDetail.TenantId.ToString());

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
                var TenantList = (await GetAll()).Where(x => x.IsDelete == false && x.ParentTenantId == null && x.TenantId != claimsProperty.GetTenantId()).ToList().OrderByDescending(x => x.CreatedAt);
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
                
                    var data = FilterRecords.Select(x => new GetAllTenant
                    {
                        TenantId = x.TenantId,
                        FirstName = x.FirstName,
                        MiddleName = x.MiddleName,
                        LastName = x.LastName,
                        Suffix = x.Suffix??"",
                        InvoicePrefix=x.BrandingInfoNavigation?.InvoicePrefix ??"",
                        Code = x.Code,
                        URL = x.Url,
                        Email = x.BrandingInfoNavigation?.Email,
                        PhoneNumber = x.BrandingInfoNavigation?.PhoneNumber,
                        Logo = x.BrandingInfoNavigation?.Logo,
                        CompanyName = x.BrandingInfoNavigation?.CompanyName,
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

                var queryable = data.AsQueryable();

                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                responseVm.Data = TakenRecords;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count;
                


            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;

        }

        private string CreateAuthenticationToken(LoginDetails userDetail, List<UserRoles> userRoles)
        {
            var claims = new ClaimsIdentity();
            
            // Add role claims
            foreach (var role in userRoles)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, role.PermissionName));
            }

            // Add user claims
            var isTenant = string.IsNullOrEmpty(userDetail.ParentTenantId);
            
            claims.AddClaim(new Claim("UserId", userDetail.UserId.ToString().ToUpper()));
            claims.AddClaim(new Claim("Username", userDetail.Username ?? string.Empty));
            claims.AddClaim(new Claim(ClaimTypes.Name, $"{userDetail.FirstName} {userDetail.MiddleName} {userDetail.LastName}".Trim()));
            claims.AddClaim(new Claim(ClaimTypes.Email, userDetail.Email ?? string.Empty));
            claims.AddClaim(new Claim("Phone", userDetail.Phone ?? string.Empty));
            claims.AddClaim(new Claim("TenantId", userDetail.TenantId.ToString().ToUpper()));
            claims.AddClaim(new Claim("ParentTenantId", userDetail.ParentTenantId?.ToUpper() ?? string.Empty));
            claims.AddClaim(new Claim("CompanyCode", userDetail.Code));
            claims.AddClaim(new Claim("CompanyName", userDetail.CompanyName));
            claims.AddClaim(new Claim("IsTenant", isTenant.ToString()));
               
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(AppConstant.JwtExpiryTime)),
                Issuer = AppConstant.JwtIssuer,
                Audience = AppConstant.JwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private UserLoginDetails BuildLoginResponse(LoginDetails userDetail, List<UserRoles> userRoles, string token)
        {
            var loginDetails = mapper.Map<UserLoginDetails>(userDetail);
            
            loginDetails.Roles = userRoles.Select(x => x.PermissionName).ToArray();
            loginDetails.Menu = userRoles
                .Select(x => new LoginRoles
                {
                    RoleName = RoleNameHelper.CleanRoleName(x.PermissionName),
                    RoleDisplayName = x.ModuleDisplayName,
                    Icon = x.Icon,
                    Url = x.Url
                })
                .DistinctBy(x => x.RoleName)
                .Where(x => !string.IsNullOrWhiteSpace(x.RoleName))
                .ToArray();
             
            loginDetails.IsGHLConfigSameAsTenant = userDetail.IsGHLConfigSameAsTenant;
            loginDetails.Token = token;
            loginDetails.IsTenant = string.IsNullOrEmpty(userDetail.ParentTenantId);

            return loginDetails;
        }

        private async Task<IResponseVm<string>> SendWelcomeEmail(CreateTenant model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                WelcomeEmailModel welcomemodel = new WelcomeEmailModel
                {
                    Name = $"{model.FirstName} {model.LastName}" ?? "",
                    UserName = model.Email ?? "",
                    Password = model.Password ?? "",
                    CompanyName = model.CompanyName ?? "",
                    Logo = model.Logo ?? "",
                    loginUrl = $"{AppConstant.UIBaseURL}",
                    color = model.PrimaryColor ?? "",
                    Email = model.Email
                };
                responseVm = await emailSendHelperService.SendEmailWithReplacedContent(EmailTemplateType.WelcomeEmail, welcomemodel,AppConstant.Sub_WelcomeEmail);
            }
            catch (Exception ex)
            {
                responseVm.Message = ("Welcome email failed: " + ex.Message);
            }
            return responseVm;
        }

        public List<Guid> GetAccessibleTenantIds(Guid? loggedInTenantId)
        {
            if (loggedInTenantId!=Guid.Empty)
            {
                return GetAllQuerable()
                    .Where(t => t.ParentTenantId == loggedInTenantId || t.TenantId == loggedInTenantId)
                    .Select(t => t.TenantId)
                    .ToList();
            }

            return new List<Guid> { loggedInTenantId?? Guid.Empty };
        }

    }
}
