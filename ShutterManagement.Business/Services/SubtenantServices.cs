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
using ShutterManagement.Services;
using ShutterManagement.DAL.Context;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core;
using Microsoft.AspNetCore.Http;
using ShutterManagement.Core.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ShutterManagement.Core.ViewModels.Response;
using System.Data.Entity.Core.Metadata.Edm;

namespace ShutterManagement.Business.Services
{
    public class SubtenantServices : BaseRepository<ShutterDBContext, TblTenant>, ISubtenantService
    {
        private readonly DataServices dataServices;
        private readonly IUserService userService;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly INotificationService notification;

        public SubtenantServices(DataServices dataServices, IUserService userService
            , ClaimsProperty claimsProperty, ITenantBrandingService tenantBrandingService,INotificationService notification
            )
        {
            this.dataServices = dataServices;
            this.userService = userService;
            this.claimsProperty = claimsProperty;
            this.tenantBrandingService = tenantBrandingService;
            this.notification = notification;
        }

        public async Task<IResponseVm<string>> SubTenantCreate(CreateTenant model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsCodeExists = await AnyAsync(x => x.IsActive == true
                    && x.IsDelete == false && x.Code == model.Code);
                //var IsCodeExists = await AnyAsync(x => x.IsActive == true
                //   && x.IsDelete == false && x.Code == model.Code);
                if (IsCodeExists)
                {
                    responseVm.Message = AppConstant.CodeAlreadyExists;
                    return responseVm;
                }

                bool IsParentExists = await AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId
                && x.IsDelete == false && x.IsActive == true);
                if (!IsParentExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
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
                if (model.InvoicePrefix != "")
                {
                    if (model.InvoicePrefix.Length != 4)
                    {
                        responseVm.Message = "4 characters is Mandatory.";
                        return responseVm;
                    }
                }
                //bool IsSubTenantEmailExists = await userService.AnyAsync(x => x.Email == model.Email && x.TenantId == AppConstant.LoggedInTenantId);
                //    if (IsSubTenantEmailExists)
                //    {
                //        responseVm.Message = "Email " + AppConstant.AlreadyExists;
                //        return responseVm;
                //    }
                var HashPassword = AesEncryptionHelper.Encrypt(model.Password ?? "");
                SqlParameter[] sqlParameters =
                {
                        new SqlParameter("@ParentTenantId", SqlDbType.UniqueIdentifier) { SqlValue = AppConstant.LoggedInTenantId },
                        new SqlParameter("@FirstName", SqlDbType.NVarChar) { SqlValue = model.FirstName },
                        new SqlParameter("@MiddleName", SqlDbType.NVarChar) { SqlValue = model.MiddleName },
                        new SqlParameter("@LastName", SqlDbType.NVarChar) { SqlValue = model.LastName },
                        new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = model.Code },
                        new SqlParameter("@URL", SqlDbType.NVarChar) { SqlValue = model.URL },
                        new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email },
                        new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber },
                        new SqlParameter("@Logo", SqlDbType.NVarChar) { SqlValue = model.Logo },
                        new SqlParameter("@CompanyName", SqlDbType.NVarChar) { SqlValue = model.CompanyName },
                        new SqlParameter("@PrimaryColor", SqlDbType.NVarChar) { SqlValue = model.PrimaryColor },
                        new SqlParameter("@StreetAddress", SqlDbType.NVarChar) { SqlValue = model.StreetAddress },
                        new SqlParameter("@City", SqlDbType.NVarChar) { SqlValue = model.City },
                       new SqlParameter("@Country", SqlDbType.NVarChar) { SqlValue = model.Country },
                       new SqlParameter("@ZipCode", SqlDbType.NVarChar) { SqlValue = model.ZipCode },
                      // new SqlParameter("@Markup", SqlDbType.NVarChar) { SqlValue = model.Markup },
                        new SqlParameter("@SecondaryColor", SqlDbType.NVarChar) { SqlValue = model.SecondaryColor },
                        new SqlParameter("@InvoicePrefix", SqlDbType.NVarChar) { SqlValue = model.InvoicePrefix },
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
                        Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.SubTenant),
                        Body = $"{AddedTenant.FirstName} {AddedTenant.LastName}({AddedTenant.Code}) Partner {AppConstant.Create_Notification}",
                        CreatedDate = DateTime.Now,
                        Type = Enum.GetName(typeof(NotificationModule), NotificationModule.SubTenant),
                        TypeId = AddedTenant.TenantId,
                    };
                    var result = await notification.CreateNotification(notification1);

                    var UserId = userService.GetAllQuerable().Where(x => x.TenantId == AddedTenant.TenantId && x.IsDelete == false).
                        Select(x => x.Id).FirstOrDefault();
                        CreateNotification notification2 = new CreateNotification()
                        {
                            UserId = UserId,
                            Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.SubTenant),
                            Body = $"{AppConstant.Welcome_Notification} {AddedTenant.FirstName}({AddedTenant.Code}) {AddedTenant.LastName}!",
                            CreatedDate = DateTime.Now,
                            Type = Enum.GetName(typeof(NotificationModule), NotificationModule.SubTenant),
                            TypeId = AddedTenant.TenantId,   

                        };

                        var result2 = await notification.CreateNotification(notification2);
                        string[] defaultRoleArray = ["SubTenant Admin", "Sales", "Installer"];
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

               // var Templateid = Guid.Parse("1916A280-109F-4829-8A70-0AFA94146A5F").ToString().ToLower();
                var Tenantcode =  dbContext.TblTenants.Where(x => x.IsDelete == false && x.TenantId==AppConstant.LoggedInTenantId).FirstOrDefault();
                var tenantInfoDetails = tenantBrandingService.GetAllQuerable().Where(x => x.Id == Tenantcode.BrandingInfo && x.IsActive == true).FirstOrDefault();

                WelcomeEmailModel welcomemodel = new WelcomeEmailModel
                {
                    Name = model.FirstName + " " + model.LastName ?? "",
                    UserName = model.Email ?? "",
                    Password = model.Password ?? "",
                    CompanyName = tenantInfoDetails?.CompanyName ?? "",
                    Logo = tenantInfoDetails?.Logo ?? "",
                    loginUrl = $"{AppConstant.UIBaseURL}{Tenantcode?.Code}"??"",
                    color = tenantInfoDetails?.PrimaryColor ?? ""

                };

                var WelcomeTemplate = dbContext.TblTemplates.Where(x => x.IsDelete == false && x.TemplateCategory == EnumValue.GetEnumValue(EmailTemplateType.WelcomeEmail)).FirstOrDefault();
                var emailContent = EmailHelper.ReplacePlaceholdersForlWelcomeEmail(WelcomeTemplate?.TemplateValue, welcomemodel);
                EmailHelper.WelcomeEmail(model.Email, emailContent);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> SubTenantUpdate(UpdateSubTenant model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsCodeExists = await AnyAsync(x => x.IsActive == true
                    && x.IsDelete == false && x.Code == model.Code && x.TenantId.ToString() != model.TenantId);
                if (!IsCodeExists)
                {
                    if (model.TenantId == null || model.TenantId == "")
                    {
                        responseVm.Message = AppConstant.TenantIdNull;
                        return responseVm;
                    }
                    bool IsParentExists = await AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId
                    && x.IsDelete == false && x.IsActive == true);
                    if (!IsParentExists)
                    {
                        responseVm.Message = AppConstant.TenantNotFound;
                        return responseVm;
                    }
                    //bool IsTenantExists = await AnyAsync(x => x.TenantId.ToString() == model.TenantId && x.ParentTenantId == AppConstant.LoggedInTenantId
                    //&& x.IsDelete == false);
                    //if (!IsTenantExists)
                    //{
                    //    responseVm.Message = AppConstant.TenantNotFound;
                    //    return responseVm;
                    //}
                    var TenantDetails = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId.ToString() != model.TenantId).Select(x => x.BrandingInfo).ToList();
                    bool IsTenantEmailExists = await tenantBrandingService.AnyAsync(x => x.Email == model.Email && x.IsDelete == false && TenantDetails.Contains(x.Id));
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

                    if (model.InvoicePrefix != "")
                    {
                        if (model.InvoicePrefix.Length != 4)
                        {
                            responseVm.Message = "4 characters is Mandatory.";
                            return responseVm;
                        }
                    }
                    var tenantid = Guid.Parse(model.TenantId);
                    var Parenttenantid = AppConstant.LoggedInTenantId;
                    var HashPassword = string.IsNullOrEmpty(model.Password) ? "" : AesEncryptionHelper.Encrypt(model.Password ?? "");

                    SqlParameter[] sqlParameters =
                    {
                    new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = tenantid},
                    new SqlParameter("@ParentTenantId", SqlDbType.UniqueIdentifier) { SqlValue = Parenttenantid },
                    new SqlParameter("@FirstName", SqlDbType.NVarChar) { SqlValue = model.FirstName },
                    new SqlParameter("@MiddleName", SqlDbType.NVarChar) { SqlValue = model.MiddleName },
                    new SqlParameter("@LastName", SqlDbType.NVarChar) { SqlValue = model.LastName },
                    new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = model.Code },
                    new SqlParameter("@URL", SqlDbType.NVarChar) { SqlValue = model.URL },
                    new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email },
                    new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber },
                    new SqlParameter("@Logo", SqlDbType.NVarChar) { SqlValue = model.Logo },
                    new SqlParameter("@CompanyName", SqlDbType.NVarChar) { SqlValue = model.CompanyName },
                    new SqlParameter("@PrimaryColor", SqlDbType.NVarChar) { SqlValue = model.PrimaryColor },
                    new SqlParameter("@SecondaryColor", SqlDbType.NVarChar) { SqlValue = model.SecondaryColor },
              //      new SqlParameter("@Markup", SqlDbType.NVarChar) { SqlValue = model.Markup },
                    new SqlParameter("@InvoicePrefix", SqlDbType.NVarChar) { SqlValue = model.InvoicePrefix },
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

        public async Task<IResponseVm<string>> SubTenantDelete(string TenantId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var TenantDetails = GetAllQuerable().Where(x => x.TenantId.ToString() == TenantId && x.ParentTenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
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

        public async Task<IResponseVm<string>> SubTenantActiveInactive(string TenantId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var TenantDetails = GetAllQuerable().Where(x => x.TenantId.ToString() == TenantId && x.ParentTenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
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

        public async Task<IResponseVm<dynamic>> SubTenantGetAll(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var TenantList = (await GetAll()).Where(x => x.IsDelete == false && x.ParentTenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).ToList();
                foreach (var item in TenantList)
                {
                    item.BrandingInfoNavigation = tenantBrandingService.GetAllQuerable().Where(x => x.Id == item.BrandingInfo).FirstOrDefault();
                }
                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var FilterRecords = TenantList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.TenantId != "" ?
                                    x.TenantId.ToString() == filter.TenantId?.ToString() : true)
                                    &&
                                    (filter.SearchValue != "" ?
                                    (x.TenantId.ToString() == filter.SearchValue) ||
                                    ((x.BrandingInfoNavigation?.Email ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.BrandingInfoNavigation?.PhoneNumber ?? "").ToLower().Contains(filter.SearchValue)
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
                        InvoicePrefix = x.BrandingInfoNavigation.InvoicePrefix,
                        StreetAddress = x.BrandingInfoNavigation.StreetAddress,
                        City = x.BrandingInfoNavigation.City,
                        Country = x.BrandingInfoNavigation.Country,
                        ZipCode = x.BrandingInfoNavigation.ZipCode,
                        Markup = x.BrandingInfoNavigation.Markup,
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
