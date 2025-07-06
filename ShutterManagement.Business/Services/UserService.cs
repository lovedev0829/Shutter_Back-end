using AutoMapper;
using Azure;
using MailKit;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Bcpg.OpenPgp;
using ShutterManagement.Business;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Threading.Tasks;

namespace ShutterManagement.Services
{
    public class UserService : BaseRepository<ShutterDBContext, TblUser>, IUserService
    {
        private readonly DataServices _dataServices;
        private readonly IRoleService roleService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IUserRoleService userRoleService;
        private readonly IMapper mapper;
        private readonly INotificationService notification;
        private readonly ITenantBrandingService tenantBrandingService;
       // private readonly GoogleAuthService googleAuthService;

        public UserService(DataServices dataServices, IRoleService roleService
            , ClaimsProperty claimsProperty, IUserRoleService userRoleService
            , IMapper mapper,INotificationService notification,ITenantBrandingService tenantBrandingService/*,GoogleAuthService googleAuthService*/)
        {
            _dataServices = dataServices;
            this.roleService = roleService;
            this.claimsProperty = claimsProperty;
            this.userRoleService = userRoleService;
            this.mapper = mapper;
            this.notification = notification;
            this.tenantBrandingService = tenantBrandingService;
            //this.googleAuthService = googleAuthService;
        }

        public async Task<IResponseVm<string>> Create(CreateUser UserDetails)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                var IsUserExists = new List<TblUser>();
                var AllTenantSubTenants = new List<string>();
                if (string.IsNullOrEmpty(claimsProperty.GetParentTenantId()))
                {
                    AllTenantSubTenants = dbContext.TblTenants.Where(x => x.IsActive == true && x.IsDelete == false
                   && (x.ParentTenantId == claimsProperty.GetTenantId() || x.TenantId == claimsProperty.GetTenantId())).Select(x => x.TenantId.ToString()).ToList();
                }
                else
                {
                    AllTenantSubTenants = dbContext.TblTenants.Where(x => x.IsActive == true && x.IsDelete == false
                  && (x.ParentTenantId.ToString() == claimsProperty.GetParentTenantId() || x.TenantId.ToString() == claimsProperty.GetParentTenantId())).Select(x => x.TenantId.ToString()).ToList();
                }

                //bool IsPhoneExists = await AnyAsync(x => x.IsDelete == false &&
                //  (x.Phone == UserDetails.Phone)
                //  //&& AllTenantSubTenants.Contains(x.TenantId.ToString())
                //  );
                //if (IsPhoneExists)
                //{
                //    result.Message = "Phone " + AppConstant.AlreadyExists;
                //    return result;
                //}

                bool IsEmailExists = await AnyAsync(x => x.IsDelete == false && 
                  (x.Email == UserDetails.Email)
                  && x.TenantId == claimsProperty.GetTenantId()
                  );
                if (IsEmailExists)
                {
                    result.Message = "Email " + AppConstant.AlreadyExists;
                    return result;
                }
                bool IsRoleExists = await roleService.AnyAsync(x => x.Id == UserDetails.RoleId
                && x.IsShow == false && x.IsActive == true && x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId());
                if (IsRoleExists)
                {
                    var HashPassword = AesEncryptionHelper.Encrypt(UserDetails.Password ?? "");
                    TblUser tblUser = new TblUser
                    {
                        Id = Guid.NewGuid(),
                        FirstName = UserDetails.FirstName,
                        MiddleName = UserDetails.MiddleName,
                        LastName = UserDetails.LastName,
                        Username = UserDetails.Email,
                        GhlUserId = UserDetails.GhlUserId??"",
                        Suffix=UserDetails.Suffix??"",
                        Email = UserDetails.Email,
                        Pwd = HashPassword,
                        JobTitleId = string.IsNullOrEmpty(UserDetails.JobTitle) ? null : Guid.Parse(UserDetails.JobTitle),
                        Phone = UserDetails.Phone,
                        CreatedBy = AppConstant.LoggedInUserID,
                        CreatedAt = DateTime.Now,
                        TenantId = claimsProperty.GetTenantId(),
                        IsActive = true,
                        IsDelete = false,
                        IsTenantAdmin = false
                    };
                    var res = await Add(tblUser);
                    if (res.Id != Guid.Empty)
                    {
                        CreateNotification notification2 = new CreateNotification()
                        {
                            UserId = tblUser.Id,
                            Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.User),
                            Body = $"{tblUser.FirstName} {tblUser.LastName} {AppConstant.Create_Notification}",
                            CreatedDate = DateTime.Now,
                            Type = Enum.GetName(typeof(NotificationModule),NotificationModule.User),
                            TypeId = tblUser.Id,
                        };
                        var result1 = notification.CreateNotification(notification2);
                        CreateNotification notification3 = new CreateNotification()
                        {
                            UserId = AppConstant.LoggedInUserID,
                            Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.User),
                            Body = $"{tblUser.FirstName} {tblUser.LastName} {AppConstant.Create_Notification}",
                            CreatedDate = DateTime.Now,
                            Type = Enum.GetName(typeof(NotificationModule), NotificationModule.User),
                            TypeId = AppConstant.LoggedInUserID,
                        };
                        var result3 = notification.CreateNotification(notification3);
                        TblUserRole userRole = new TblUserRole
                        {
                            UserId = res.Id,
                            RoleId = UserDetails.RoleId,
                            CreatedBy = AppConstant.LoggedInUserID,
                            CreatedAt = DateTime.Now,
                            IsActive = true,
                            IsDelete = false
                        };
                        await userRoleService.Add(userRole);

                        var Tenantcode = dbContext.TblTenants.Where(x => x.IsDelete == false && x.TenantId==claimsProperty.GetTenantId()).FirstOrDefault();
                        var tenantInfoDetails = tenantBrandingService.GetAllQuerable().Where(x => x.Id == Tenantcode.BrandingInfo && x.IsActive == true).FirstOrDefault();

                        WelcomeEmailModel welcomemodel = new WelcomeEmailModel
                        {
                            Name = UserDetails.FirstName + " " + UserDetails.LastName ?? "" + UserDetails.Suffix ?? "",
                            UserName = UserDetails.Email ?? "",
                            Password = UserDetails.Password ?? "",
                            CompanyName = tenantInfoDetails?.CompanyName ?? "",
                            Logo = tenantInfoDetails?.Logo ?? "",
                            loginUrl = $"{AppConstant.UIBaseURL}{Tenantcode?.Code ?? ""}" ??"",
                            color = tenantInfoDetails?.PrimaryColor ?? ""
                        };

                        var WelcomeTemplate = dbContext.TblTemplates.Where(x => x.IsDelete == false && x.TemplateCategory.ToLower() == EnumValue.GetEnumValue(EmailTemplateType.WelcomeEmail)).FirstOrDefault();
                        var emailContent = EmailHelper.ReplacePlaceholdersForlWelcomeEmail(WelcomeTemplate?.TemplateValue, welcomemodel);
                        //await googleAuthService.WelcomeEmailWithOauth(UserDetails.Email, emailContent);

                        result.IsSuccess = true;
                        result.Message = AppConstant.InsertSuccessMsg;
                    }
                    else
                    {
                        result.Message = AppConstant.ExternalError;
                    }
                }
                else
                {
                    result.Message = "Role " + AppConstant.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                result.Message = AppConstant.ExceptionError + ex.Message;
            }
            return result;
        }

        public async Task<IResponseVm<string>> Update(UpdateUser UserDetails)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                if(UserDetails.UserId == AppConstant.LoggedInUserID.ToString())
                {
                    result.Message = AppConstant.AccessDenied;
                    return result;
                }
                var UserExistsDetails = GetAllQuerable().Where(x => x.IsDelete == false &&
                x.Id == Guid.Parse(UserDetails.UserId)
                && x.TenantId == claimsProperty.GetTenantId()
                );
                if (UserExistsDetails.Count() == 1)
                {
                    var AllTenantSubTenants = new List<string>();
                    if (string.IsNullOrEmpty(claimsProperty.GetParentTenantId()))
                    {
                        AllTenantSubTenants = dbContext.TblTenants.Where(x => x.IsActive == true && x.IsDelete == false
                       && (x.ParentTenantId == claimsProperty.GetTenantId() || x.TenantId == claimsProperty.GetTenantId())).Select(x => x.TenantId.ToString()).ToList();
                    }
                    else
                    {
                        AllTenantSubTenants = dbContext.TblTenants.Where(x => x.IsActive == true && x.IsDelete == false
                      && (x.ParentTenantId.ToString() == claimsProperty.GetParentTenantId() || x.TenantId.ToString() == claimsProperty.GetParentTenantId())).Select(x => x.TenantId.ToString()).ToList();
                    }


                  //  bool IsPhoneExists = await AnyAsync(x => x.IsDelete == false &&
                  //(x.Phone == UserDetails.Phone) && x.Id != Guid.Parse(UserDetails.UserId)
                  ////&& AllTenantSubTenants.Contains(x.TenantId.ToString())
                  //);
                  //  if (IsPhoneExists)
                  //  {
                  //      result.Message = "Phone " + AppConstant.AlreadyExists;
                  //      return result;
                  //  }

                    bool IsEmailExists = await AnyAsync(x => x.IsDelete == false &&
                      (x.Email == UserDetails.Email) && x.Id != Guid.Parse(UserDetails.UserId)
                      && x.TenantId == claimsProperty.GetTenantId()
                      );
                    if (IsEmailExists)
                    {
                        result.Message = "Email " + AppConstant.AlreadyExists;
                        return result;
                    }
                    bool IsRoleExists = await roleService.AnyAsync(x => x.Id == UserDetails.RoleId
                        && x.IsShow == false && x.IsActive == true && x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId());
                    if (!IsRoleExists)
                    {
                        result.Message = "Role " + AppConstant.NoDataFound;
                        return result;
                    }
                    var UpdateUserDetails = UserExistsDetails.FirstOrDefault();
                    if (UpdateUserDetails != null)
                    {
                        UpdateUserDetails.FirstName = UserDetails.FirstName;
                        UpdateUserDetails.MiddleName = UserDetails.MiddleName;
                        UpdateUserDetails.LastName = UserDetails.LastName;
                        UpdateUserDetails.Username = UserDetails.Email;
                        UpdateUserDetails.GhlUserId = UserDetails.GhlUserId;
                        UpdateUserDetails.Suffix = UserDetails.Suffix??"";
                        UpdateUserDetails.Email = UserDetails.Email;
                        UpdateUserDetails.JobTitleId = string.IsNullOrEmpty(UserDetails?.JobTitle) ? (Guid?)null: (Guid.TryParse(UserDetails.JobTitle, out var parsedGuid) ? parsedGuid : (Guid?)null);
                        UpdateUserDetails.Phone = UserDetails.Phone;
                        UpdateUserDetails.ModifiedAt = DateTime.Now;
                        UpdateUserDetails.ModifiedBy = AppConstant.LoggedInUserID;
                        UpdateUserDetails.IsTenantAdmin = UpdateUserDetails.IsTenantAdmin != null ? UpdateUserDetails.IsTenantAdmin : false ;
                        if (!string.IsNullOrEmpty(UserDetails.Password))
                        {
                            UpdateUserDetails.Pwd = AesEncryptionHelper.Encrypt(UserDetails.Password);
                        };
                        var res = await Update(UpdateUserDetails);

                        var UserRoleDeatils = (await userRoleService.GetAll()).Where(x => x.UserId == Guid.Parse(UserDetails.UserId) && x.IsDelete == false).FirstOrDefault();
                        if (UserRoleDeatils != null)
                        {
                            UserRoleDeatils.RoleId = UserDetails.RoleId;
                            UserRoleDeatils.ModifiedBy = AppConstant.LoggedInUserID;
                            UserRoleDeatils.ModifiedAt = DateTime.Now;
                            await userRoleService.Update(UserRoleDeatils);
                        }
                        else
                        {
                            TblUserRole userRole = new TblUserRole
                            {
                                UserId = res.Id,
                                RoleId = UserDetails.RoleId,
                                CreatedBy = AppConstant.LoggedInUserID,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                IsDelete = false
                            };
                            await userRoleService.Add(userRole);
                        }
                        result.IsSuccess = true;
                        result.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        result.Message = AppConstant.NoUserFound;
                    }
                }
                else
                {
                    result.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex)
            {
                result.Message = AppConstant.ExceptionError + ex.Message;
            }
            return result;
        }

        public async Task<IResponseVm<string>> Delete(string UserId)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                if (UserId == AppConstant.LoggedInUserID.ToString())
                {
                    result.Message = AppConstant.AccessDenied;
                    return result;
                }
                        
                var IsUserExists = GetAllQuerable().Where(x => x.IsDelete == false && x.IsTenantAdmin == false &&
                x.Id == Guid.Parse(UserId)
                && x.TenantId == claimsProperty.GetTenantId()
                );
                if (IsUserExists.Count() == 1)
                {
                    var UpdateUserDetails = IsUserExists.FirstOrDefault();
                    if (UpdateUserDetails != null)
                    {
                        UpdateUserDetails.IsActive = false;
                        UpdateUserDetails.IsDelete = true;
                        UpdateUserDetails.ModifiedAt = DateTime.Now;
                        UpdateUserDetails.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = await Update(UpdateUserDetails);

                        result.IsSuccess = true;
                        result.Message = AppConstant.DeleteSuccessMsg;
                    }
                    else
                    {
                        result.Message = AppConstant.NoUserFound;
                    }
                }
                else
                {
                    result.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex)
            {
                result.Message = AppConstant.ExceptionError + ex.Message;
            }
            return result;
        }

        public async Task<IResponseVm<string>> ActiveInactive(string UserId)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                if (UserId == AppConstant.LoggedInUserID.ToString())
                {
                    result.Message = AppConstant.AccessDenied;
                    return result;
                }
                var IsUserExists = GetAllQuerable().Where(x => x.IsDelete == false && x.IsTenantAdmin == false &&
                x.Id == Guid.Parse(UserId)
                && x.TenantId == claimsProperty.GetTenantId()
                );
                if (IsUserExists.Count() == 1)
                {
                    var UpdateUserDetails = IsUserExists.FirstOrDefault();
                    if (UpdateUserDetails != null)
                    {
                        UpdateUserDetails.IsActive = !UpdateUserDetails.IsActive;
                        UpdateUserDetails.ModifiedAt = DateTime.Now;
                        UpdateUserDetails.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = await Update(UpdateUserDetails);
                        result.IsSuccess = true;
                        result.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        result.Message = AppConstant.NoUserFound;
                    }
                }
                else
                {
                    result.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex)
            {
                result.Message = AppConstant.ExceptionError + ex.Message;
            }
            return result;
        }

        public async Task<IResponseVm<List<UsersList>>> GetAll(Pagination filter)
        {
            IResponseVm<List<UsersList>> responseVm = new ResponseVm<List<UsersList>>();
            try
            {
                var Users = new List<TblUser>();
                List<UsersList> UsersList = new List<UsersList>();
                if (filter.TenantId != "")
                {
                    var IsRealtedTenant = dbContext.TblTenants.Where(x => x.TenantId.ToString() == filter.TenantId && x.ParentTenantId == claimsProperty.GetTenantId()).FirstOrDefault();
                    if (IsRealtedTenant == null)
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                        return responseVm;
                    }
                    else
                    {
                        Users = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId.ToString() == filter.TenantId).OrderByDescending(x => x.CreatedAt).ToList();
                    }
                }
                else
                {
                    Users = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId() /*&& x.IsTenantAdmin == false*/).OrderByDescending(x => x.CreatedAt).ToList();
                }
              //  Users = Users.Where(x => x.Id != AppConstant.LoggedInUserID ).ToList();
                foreach (var user in Users)
                {
                    UsersList users = new UsersList
                    {
                        FirstName = user.FirstName,
                        MiddleName = user.MiddleName,
                        LastName = user.LastName,
                        GhlUserId=user.GhlUserId,
                        Suffix=user.Suffix??"",
                        Email = user.Email,
                        UserId = user.Id,
                        UserName = user.Username,
                        Phone = user.Phone,
                        JobTitle = user.JobTitleId,
                        TenantId = user.TenantId,
                        IsActive = user.IsActive,
                        IsTenantAdmin = user.IsTenantAdmin
                    };
                    users.JobTitleName = dbContext.MstJobTitles
                        .Where(x => x.Id == users.JobTitle
                        && x.IsActive == true && x.IsDelete == false)
                        .Select(x => x.JobTitleName).FirstOrDefault();
                    var UserRoleId = userRoleService.GetAllQuerable()
                        .Where(x => x.UserId == users.UserId).Select(x => x.RoleId).FirstOrDefault();
                    users.RoleId = UserRoleId;
                    users.RoleName = dbContext.TblRoles
                        .Where(x => x.Id == UserRoleId
                        && x.IsActive == true && x.IsDelete == false)
                        .Select(x => x.RoleName).FirstOrDefault();
                    var GhlUserName = dbContext.TblGhlUsers
                        .Where(x => x.GhlUserId == users.GhlUserId).Select(x => x.GhlUserName).FirstOrDefault();
                    users.GhlUserName = GhlUserName;

                    UsersList.Add(users);
                }
                filter.SearchValue = (filter.SearchValue.Trim() ?? "").ToLower();
                var FilterRecords = UsersList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (string.IsNullOrEmpty(filter.SearchValue) ||
                                    string.IsNullOrEmpty(filter.TenantId) || filter.TenantId == x.TenantId.ToString()) &&
                                    (filter.SearchValue != "" ?
                                    (x.UserId.ToString() == filter.SearchValue) ||
                                    ((x.Email ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.Phone ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.TenantId.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.FirstName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.MiddleName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.RoleName ?? "").ToLower().Contains(filter.SearchValue)
                                    || ((x.FirstName.ToLower() + " " + x.LastName.ToLower()) ?? "").Contains(filter.SearchValue)
                                    || (x.LastName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.UserName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.GhlUserName ?? "").ToLower().Contains(filter.SearchValue)
                                    ) : true)).ToList();


                var queryable = FilterRecords.AsQueryable();

                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords;
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



        public async Task<IResponseVm<List<UsersList>>> GetAllUserList()
        {
            IResponseVm<List<UsersList>> responseVm = new ResponseVm<List<UsersList>>();
            try
            {
              

                var tenantDetails = dbContext.TblTenants.Where(u => (u.ParentTenantId == claimsProperty.GetTenantId() || u.TenantId== claimsProperty.GetTenantId()) && u.IsDelete == false && u.IsActive == true).Select(x=> x.TenantId).ToList();
                var users = GetAllQuerable().Where(u =>tenantDetails.Contains((Guid)u.TenantId) && u.IsDelete == false && u.IsActive == true).ToList();
                
                var usersList = users.Select(user => new UsersList
                {
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    GhlUserId = user.GhlUserId,
                    Suffix = user.Suffix ?? "",
                    Email = user.Email,
                    UserId = user.Id,
                    UserName = user.Username,
                    Phone = user.Phone,
                    JobTitle = user.JobTitleId,
                    TenantId = user.TenantId,
                    IsActive = user.IsActive
                }).ToList();

                responseVm.Data = usersList;
                responseVm.IsSuccess = true;
                responseVm.Message = "User list fetched successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = "Error: " + ex.Message;
            }

            return responseVm;
        }


        public async Task<string> GetTenantIdByUserEmailAndPwd(string Username, string Password)
        {
            var user = GetAllQuerable().Where(u => u.Username == Username && u.Pwd == Password 
                && u.IsActive == true && u.IsDelete == false)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (user == null)
            {
                Console.WriteLine("No user found with all criteria");
                return "";
            }

            Console.WriteLine($"User found, TenantId: {user.TenantId}");
            return user.TenantId.ToString();
        }

        public async Task PasswordEnc()
        {
            var users = GetAllQuerable().Where(x => x.IsActive == true).ToList();
            foreach (var user in users)
            {
                user.Pwd = AesEncryptionHelper.Encrypt(user.Pwd);
                await Update(user);
            }
        }

    }
}
