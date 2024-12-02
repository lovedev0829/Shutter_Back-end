using AutoMapper;
using Microsoft.Data.SqlClient;
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
using System.Threading.Tasks;

namespace ShutterManagement.Services
{
    public class UserService : BaseRepository<ShutterDbContext, TblUser>, IUserService
    {
        private readonly DataServices _dataServices;
        private readonly IRoleService roleService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IUserRoleService userRoleService;
        private readonly IMapper mapper;

        public UserService(DataServices dataServices, IRoleService roleService
            , ClaimsProperty claimsProperty, IUserRoleService userRoleService
            , IMapper mapper)
        {
            _dataServices = dataServices;
            this.roleService = roleService;
            this.claimsProperty = claimsProperty;
            this.userRoleService = userRoleService;
            this.mapper = mapper;
        }

        public async Task<IResponseVm<string>> Create(CreateUser UserDetails)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                var IsUserExists = new List<TblUser>();
                var AllTenantSubTenants = new List<string>();
                if (string.IsNullOrEmpty(AppConstant.LoggedInParentTenantId))
                {
                    AllTenantSubTenants = dbContext.TblTenants.Where(x => x.IsActive == true && x.IsDelete == false
                   && (x.ParentTenantId == AppConstant.LoggedInTenantId || x.TenantId == AppConstant.LoggedInTenantId)).Select(x => x.TenantId.ToString()).ToList();
                }
                else
                {
                    AllTenantSubTenants = dbContext.TblTenants.Where(x => x.IsActive == true && x.IsDelete == false
                  && (x.ParentTenantId.ToString() == AppConstant.LoggedInParentTenantId || x.TenantId.ToString() == AppConstant.LoggedInParentTenantId)).Select(x => x.TenantId.ToString()).ToList();
                }

                bool IsPhoneExists = await AnyAsync(x => x.IsDelete == false &&
                  (x.Phone == UserDetails.Phone)
                  && AllTenantSubTenants.Contains(x.TenantId.ToString()));
                if (IsPhoneExists)
                {
                    result.Message = "Phone " + AppConstant.AlreadyExists;
                    return result;
                }

                bool IsEmailExists = await AnyAsync(x => x.IsDelete == false &&
                  (x.Email == UserDetails.Email)
                  && AllTenantSubTenants.Contains(x.TenantId.ToString()));
                if (IsEmailExists)
                {
                    result.Message = "Email " + AppConstant.AlreadyExists;
                    return result;
                }


                bool IsRoleExists = await roleService.AnyAsync(x => x.Id == UserDetails.RoleId
                && x.IsShow == false && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
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
                        Email = UserDetails.Email,
                        Pwd = HashPassword,
                        JobTitleId = string.IsNullOrEmpty(UserDetails.JobTitle) ? null : Guid.Parse(UserDetails.JobTitle),
                        Phone = UserDetails.Phone,
                        CreatedBy = AppConstant.LoggedInUserID,
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
                        IsDelete = false,
                    };
                    var res = await Add(tblUser);
                    if (res.Id != Guid.Empty)
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
                && x.TenantId == AppConstant.LoggedInTenantId
                );
                if (UserExistsDetails.Count() == 1)
                {
                    var AllTenantSubTenants = new List<string>();
                    if (string.IsNullOrEmpty(AppConstant.LoggedInParentTenantId))
                    {
                        AllTenantSubTenants = dbContext.TblTenants.Where(x => x.IsActive == true && x.IsDelete == false
                       && (x.ParentTenantId == AppConstant.LoggedInTenantId || x.TenantId == AppConstant.LoggedInTenantId)).Select(x => x.TenantId.ToString()).ToList();
                    }
                    else
                    {
                        AllTenantSubTenants = dbContext.TblTenants.Where(x => x.IsActive == true && x.IsDelete == false
                      && (x.ParentTenantId.ToString() == AppConstant.LoggedInParentTenantId || x.TenantId.ToString() == AppConstant.LoggedInParentTenantId)).Select(x => x.TenantId.ToString()).ToList();
                    }


                    bool IsPhoneExists = await AnyAsync(x => x.IsDelete == false &&
                  (x.Phone == UserDetails.Phone) && x.Id != Guid.Parse(UserDetails.UserId)
                  && AllTenantSubTenants.Contains(x.TenantId.ToString()));
                    if (IsPhoneExists)
                    {
                        result.Message = "Phone " + AppConstant.AlreadyExists;
                        return result;
                    }

                    bool IsEmailExists = await AnyAsync(x => x.IsDelete == false &&
                      (x.Email == UserDetails.Email) && x.Id != Guid.Parse(UserDetails.UserId)
                      && AllTenantSubTenants.Contains(x.TenantId.ToString()));
                    if (IsEmailExists)
                    {
                        result.Message = "Email " + AppConstant.AlreadyExists;
                        return result;
                    }
                    bool IsRoleExists = await roleService.AnyAsync(x => x.Id == UserDetails.RoleId
                        && x.IsShow == false && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
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
                        UpdateUserDetails.Email = UserDetails.Email;
                        UpdateUserDetails.JobTitleId = string.IsNullOrEmpty(UserDetails?.JobTitle) ? (Guid?)null: (Guid.TryParse(UserDetails.JobTitle, out var parsedGuid) ? parsedGuid : (Guid?)null);
                        UpdateUserDetails.Phone = UserDetails.Phone;
                        UpdateUserDetails.ModifiedAt = DateTime.Now;
                        UpdateUserDetails.ModifiedBy = AppConstant.LoggedInUserID;
                        if (string.IsNullOrEmpty(UserDetails.Password))
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
                var IsUserExists = GetAllQuerable().Where(x => x.IsDelete == false &&
                x.Id == Guid.Parse(UserId)
                && x.TenantId == AppConstant.LoggedInTenantId
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
                var IsUserExists = GetAllQuerable().Where(x => x.IsDelete == false &&
                x.Id == Guid.Parse(UserId)
                && x.TenantId == AppConstant.LoggedInTenantId
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
                    var IsRealtedTenant = dbContext.TblTenants.Where(x => x.TenantId.ToString() == filter.TenantId && x.ParentTenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
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
                    Users = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).ToList();
                }
                Users = Users.Where(x => x.Id != AppConstant.LoggedInUserID ).ToList();
                foreach (var user in Users)
                {
                    UsersList users = new UsersList
                    {
                        FirstName = user.FirstName,
                        MiddleName = user.MiddleName,
                        LastName = user.LastName,
                        Email = user.Email,
                        UserId = user.Id,
                        UserName = user.Username,
                        Phone = user.Phone,
                        JobTitle = user.JobTitleId,
                        IsActive = user.IsActive
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

                    UsersList.Add(users);
                }
                filter.SearchValue = (filter.SearchValue.Trim() ?? "").ToLower();
                var FilterRecords = UsersList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.SearchValue != "" ?
                                    (x.UserId.ToString() == filter.SearchValue) ||
                                    ((x.Email ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.Phone ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.FirstName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.MiddleName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.LastName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.UserName ?? "").ToLower().Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
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
