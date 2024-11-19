using AutoMapper;
using Microsoft.Data.SqlClient;
using ShutterManagement.Business;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
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

        public async Task<IResponseVm<string>> Create(UserDetails UserDetails)
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

                bool IsPhoneExists = await AnyAsync(x => x.IsActive == true && x.IsDelete == false &&
                  (x.Phone == UserDetails.Phone)
                  && AllTenantSubTenants.Contains(x.TenantId.ToString()));
                if (IsPhoneExists)
                {
                    result.Message = "Phone " + AppConstant.AlreadyExists;
                    return result;
                }

                bool IsEmailExists = await AnyAsync(x => x.IsActive == true && x.IsDelete == false &&
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
                    TblUser tblUser = new TblUser();
                    mapper.Map(UserDetails, tblUser);
                    tblUser.Id = Guid.NewGuid();
                    tblUser.Pwd = HashPassword;
                    tblUser.Username = UserDetails.Email;
                    tblUser.JobTitleId = UserDetails.JobTitle;
                    tblUser.CreatedBy = AppConstant.LoggedInUserID;
                    tblUser.CreatedAt = DateTime.Now;
                    tblUser.TenantId = AppConstant.LoggedInTenantId;
                    var res = await Add(tblUser);
                    if (res.Id != Guid.Empty)
                    {
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

                var UserExistsDetails = GetAllQuerable().Where(x => x.IsActive == true && x.IsDelete == false &&
                x.Id == Guid.Parse(UserDetails.UserId)
                && x.TenantId == AppConstant.LoggedInTenantId
                );
                if (UserExistsDetails.Count() == 1)
                {
                    bool IsPhoneExists = await AnyAsync(x => x.IsActive == true && x.IsDelete == false &&
                  (x.Phone == UserDetails.Phone) && x.Id != Guid.Parse(UserDetails.UserId)
                  && AllTenantSubTenants.Contains(x.TenantId.ToString()));
                    if (IsPhoneExists)
                    {
                        result.Message = "Phone " + AppConstant.AlreadyExists;
                        return result;
                    }

                    bool IsEmailExists = await AnyAsync(x => x.IsActive == true && x.IsDelete == false &&
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
                    mapper.Map(UserDetails, UpdateUserDetails);
                    if (UpdateUserDetails != null)
                    {
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

        public async Task<IResponseVm<string>> Delete(string UserId)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
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
                        result.Message = AppConstant.IncorrectDetails;
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
                        result.Message = AppConstant.IncorrectDetails;
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

        public async Task<IResponseVm<dynamic>> GetAll(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var UserList = (await GetAll()).Where(x => x.IsDelete == false && x.Id != AppConstant.LoggedInUserID && x.TenantId == AppConstant.LoggedInTenantId).ToList();
                filter.SearchValue= (filter.SearchValue.Trim() ?? "").ToLower();
                var FilterRecords = UserList
                                    .Where(x =>
                                    (filter.SearchValue != "" ?
                                    ((x.Email ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.Phone ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.FirstName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.MiddleName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.LastName ?? "").ToLower().Contains(filter.SearchValue)
                                    || (x.Username ?? "").ToLower().Contains(filter.SearchValue)
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
