using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class RoleService : BaseRepository<ShutterDbContext,TblRole>,IRoleService
    {
        private readonly DataServices dataServices;
        private readonly IUserService userService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IRolePermissionService rolePermissionService;

        public RoleService(DataServices dataServices, IUserService userService
            , ClaimsProperty claimsProperty, IRolePermissionService rolePermissionService
            )
        {
            this.dataServices = dataServices;
            this.userService = userService;
            this.claimsProperty = claimsProperty;
            this.rolePermissionService = rolePermissionService;
        }
        public async Task<IResponseVm<string>> Create(AddRole model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                bool IsRoleExists = await AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId
                && x.IsDelete == false && x.IsActive == true && x.RoleName == model.RoleName);
                if (IsRoleExists)
                {
                    responseVm.Message = AppConstant.AlreadyExists;
                    return responseVm;
                }
                TblRole role = new TblRole
                {
                    RoleName = model.RoleName,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false,
                    TenantId = AppConstant.LoggedInTenantId

                };
                var res = await Add(role);
                if (res.Id != Guid.Empty)
                {
                    foreach (var item in model.RolePermissionId)
                    {
                        TblRolesPermission tblRolesPermission = new TblRolesPermission
                        {
                            RoleId = res.Id,
                            ModulePermissionId = item
                        };
                        await rolePermissionService.Add(tblRolesPermission);

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

        public async Task<IResponseVm<string>> Update(UpdateRole model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                bool IsRoleExists = await AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId
                && x.IsDelete == false && x.IsActive == true && x.Id == model.RoleId);
                if (!IsRoleExists)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                var RoleDetails = GetAllQuerable().Where(x => x.Id == model.RoleId && x.IsDelete == false
                && x.IsActive == true && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();

                if (RoleDetails != null)
                {
                    RoleDetails.RoleName = model.RoleName;
                    var res = await Update(RoleDetails);
                    if (res.Id != Guid.Empty)
                    {
                        var RolePermissionList = rolePermissionService.GetAllQuerable().
                            Where(x => x.RoleId == res.Id).ToList();
                        foreach (var rolePermission in RolePermissionList)
                        {
                            await rolePermissionService.Delete(rolePermission);
                        }
                        foreach (var item in model.RolePermissionId)
                        {
                            TblRolesPermission tblRolesPermission = new TblRolesPermission
                            {
                                RoleId = res.Id,
                                ModulePermissionId = item
                            };
                            await rolePermissionService.Add(tblRolesPermission);

                        }
                    }
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Delete(string RoleId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var RoleDetails = GetAllQuerable().Where(x => x.Id.ToString() == RoleId && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (RoleDetails != null)
                {
                    RoleDetails.IsActive = false;
                    RoleDetails.IsDelete = true;
                    RoleDetails.ModifiedBy = AppConstant.LoggedInUserID;
                    RoleDetails.ModifiedAt = DateTime.Now;


                    await Update(RoleDetails);
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

        public async Task<IResponseVm<string>> ActiveInactive(string RoleId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var RoleDetails = GetAllQuerable().Where(x => x.Id.ToString() == RoleId && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (RoleDetails != null)
                {
                    RoleDetails.IsActive = !RoleDetails.IsActive;
                    RoleDetails.ModifiedBy = AppConstant.LoggedInUserID;
                    RoleDetails.ModifiedAt = DateTime.Now;

                    
                    await Update(RoleDetails);
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

        public async Task<IResponseVm<dynamic>> GetAll(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var RoleList = GetAllQuerable().Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).ToList();
                
                var FilterRecords = RoleList
                                    .Where(x =>
                                    (filter.SearchValue != "" ?
                                    ((x.RoleName ?? "").Contains(filter.SearchValue)
                                    ) : true))
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (FilterRecords != null && FilterRecords.Count > 0)
                {
                    responseVm.Data = FilterRecords;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = RoleList.Count;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    responseVm.TotalRecords = RoleList.Count;
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
