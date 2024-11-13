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
using System.Data;
using System.Threading.Tasks;

namespace ShutterManagement.Services
{
    public class UserService : BaseRepository<ShutterDbContext, TblUser>, IUserService
    {
        private readonly DataServices _dataServices;
        private readonly ClaimsProperty claimsProperty;
        private readonly IMapper mapper;

        public UserService(DataServices dataServices
            , ClaimsProperty claimsProperty
            , IMapper mapper)
        {
            _dataServices = dataServices;
            this.claimsProperty = claimsProperty;
            this.mapper = mapper;
        }

        public async Task<IResponseVm<string>> Create(UserDetails UserDetails)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                var IsUserExists = GetAllQuerable().Where(x => x.IsActive == true && x.IsDelete == false &&
                (x.Username == UserDetails.Email || x.Phone == UserDetails.PhoneNumber)
                && x.TenantId == AppConstant.LoggedInTenantId
                );
                if (IsUserExists.Count() == 0)
                {
                    TblUser tblUser = new TblUser();
                    mapper.Map(UserDetails, tblUser);
                    tblUser.CreatedBy = AppConstant.LoggedInUserID;
                    tblUser.CreatedAt = DateTime.Now;
                    var res = await Add(tblUser);
                    if (res.Id != Guid.Empty)
                    {
                        result.IsSuccess = true;
                        result.Message = AppConstant.InsertSuccessMsg;
                    }
                    else
                    {
                        result.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    result.Message = AppConstant.AlreadyExists;
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
                var IsUserExists = GetAllQuerable().Where(x => x.IsActive == true && x.IsDelete == false &&
                x.Id == Guid.Parse(UserDetails.UserId)
                && x.TenantId == AppConstant.LoggedInTenantId
                );
                if (IsUserExists.Count() == 1)
                {
                    var UpdateUserDetails = IsUserExists.FirstOrDefault();
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
                var IsUserExists = GetAllQuerable().Where(x => x.IsActive == true && x.IsDelete == false &&
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
                var IsUserExists = GetAllQuerable().Where(x => x.IsActive == true && x.IsDelete == false &&
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
                var UserList = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).ToList();
                var FilterRecords = UserList
                                    .Where(x =>
                                    (filter.SearchValue != "" ?
                                    ((x.Email ?? "").Contains(filter.SearchValue)
                                    || (x.Phone ?? "").Contains(filter.SearchValue)
                                    || (x.FirstName ?? "").Contains(filter.SearchValue) 
                                    || (x.MiddleName ?? "").Contains(filter.SearchValue)
                                    || (x.LastName ?? "").Contains(filter.SearchValue)
                                    || (x.Username ?? "").Contains(filter.SearchValue)
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

    }
}
