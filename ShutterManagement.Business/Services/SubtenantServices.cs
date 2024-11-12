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

namespace ShutterManagement.Business.Services
{
    public class SubtenantServices :  BaseRepository<ShutterDbContext, TblTenant>, ISubtenantService
    {
        private readonly DataServices dataServices;
        private readonly IUserService userService;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantBrandingService tenantBrandingService;

        public SubtenantServices(DataServices dataServices, IUserService userService
            , ClaimsProperty claimsProperty, ITenantBrandingService tenantBrandingService
            )
        {
            this.dataServices = dataServices;
            this.userService = userService;
            this.claimsProperty = claimsProperty;
            this.tenantBrandingService = tenantBrandingService;
        }
        
        public async Task<IResponseVm<string>> SubTenantCreate(CreateSubTenant model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                if(model.ParentTenantId == null || model.ParentTenantId == "")
                {
                    responseVm.Message = AppConstant.ParentTenantIdNull;
                    return responseVm;
                }
                bool IsParentExists = await AnyAsync(x => x.TenantId.ToString() == model.ParentTenantId 
                && x.IsDelete == false && x.IsActive == true && x.Code == model.Code);
                if (!IsParentExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@ParentTenantId", SqlDbType.UniqueIdentifier) { SqlValue = model.ParentTenantId },
                    new SqlParameter("@FirstName", SqlDbType.NVarChar) { SqlValue = model.FirstName },
                    new SqlParameter("@MiddleName", SqlDbType.NVarChar) { SqlValue = model.MiddleName },
                    new SqlParameter("@LastName", SqlDbType.NVarChar) { SqlValue = model.LastName },
                    new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = model.Code },
                    new SqlParameter("@URL", SqlDbType.NVarChar) { SqlValue = model.URL },
                    new SqlParameter("@Email", SqlDbType.NVarChar) { SqlValue = model.Email },
                    new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { SqlValue = model.PhoneNumber },
                    new SqlParameter("@Logo", SqlDbType.NVarChar) { SqlValue = model.Logo },
                    new SqlParameter("@PrimaryColor", SqlDbType.NVarChar) { SqlValue = model.PrimaryColor },
                    new SqlParameter("@SecondaryColor", SqlDbType.NVarChar) { SqlValue = model.SecondaryColor },
                    new SqlParameter("@Password", SqlDbType.NVarChar) { SqlValue = model.Password },
                    new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Create" },
                };

                await dataServices.CallSP("sp_TenantCRUD", null, sqlParameters);
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
                if (model.TenantId == null || model.TenantId == "")
                {
                    responseVm.Message = AppConstant.TenantIdNull;
                    return responseVm;
                }
                if (model.ParentTenantId == null || model.ParentTenantId == "")
                {
                    responseVm.Message = AppConstant.ParentTenantIdNull;
                    return responseVm;
                }
                bool IsParentExists = await AnyAsync(x => x.TenantId.ToString() == model.ParentTenantId
                && x.IsDelete == false && x.IsActive == true && x.Code == model.Code);
                if (!IsParentExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                bool IsTenantExists = await AnyAsync(x => x.TenantId.ToString() == model.TenantId
                && x.IsDelete == false && x.IsActive == true && x.Code == model.Code);
                if (!IsParentExists)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var tenantid = Guid.Parse(model.TenantId);
                var Parenttenantid = Guid.Parse(model.ParentTenantId);
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@TenantId", SqlDbType.UniqueIdentifier) { SqlValue = tenantid},
                    new SqlParameter("@ParentTenantId", SqlDbType.UniqueIdentifier) { SqlValue = Parenttenantid },
                    new SqlParameter("@FirstName", SqlDbType.VarChar) { SqlValue = model.FirstName },
                    new SqlParameter("@MiddleName", SqlDbType.VarChar) { SqlValue = model.MiddleName },
                    new SqlParameter("@LastName", SqlDbType.VarChar) { SqlValue = model.LastName },
                    new SqlParameter("@Code", SqlDbType.VarChar) { SqlValue = model.Code },
                    new SqlParameter("@URL", SqlDbType.VarChar) { SqlValue = model.URL },
                    new SqlParameter("@Email", SqlDbType.VarChar) { SqlValue = model.Email },
                    new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { SqlValue = model.PhoneNumber },
                    new SqlParameter("@Logo", SqlDbType.VarChar) { SqlValue = model.Logo },
                    new SqlParameter("@PrimaryColor", SqlDbType.VarChar) { SqlValue = model.PrimaryColor },
                    new SqlParameter("@SecondaryColor", SqlDbType.VarChar) { SqlValue = model.SecondaryColor },
                    new SqlParameter("@Password", SqlDbType.VarChar) { SqlValue = model.Password },
                    new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Update" },
                };

                await dataServices.CallSP("sp_TenantCRUD", null, sqlParameters);
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

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
                var TenantList = GetAllQuerable().Where(x => x.IsDelete == false && x.ParentTenantId == AppConstant.LoggedInTenantId ).ToList();
                foreach (var item in TenantList)
                {
                    item.BrandingInfoNavigation = tenantBrandingService.GetAllQuerable().Where(x => x.Id == item.BrandingInfo).FirstOrDefault();
                }
                var FilterRecords = TenantList
                                    .Where(x =>
                                    (filter.SearchValue != "" ?
                                    ((x.BrandingInfoNavigation?.Email ?? "").Contains(filter.SearchValue)
                                    || (x.BrandingInfoNavigation?.PhoneNmber ?? "").Contains(filter.SearchValue)
                                    || (x.FirstName ?? "").Contains(filter.SearchValue) || (x.LastName ?? "").Contains(filter.SearchValue)
                                    || (x.MiddleName ?? "").Contains(filter.SearchValue)
                                    ) : true))
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (FilterRecords != null && FilterRecords.Count > 0)
                {
                    responseVm.Data = FilterRecords;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.TotalRecords = TenantList.Count;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    responseVm.TotalRecords = TenantList.Count;
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
