using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core;
using Microsoft.AspNetCore.Http;
using ShutterManagement.Core.ViewModels;

namespace ShutterManagement.Business.Services
{
    public class ModuleService : IModulePermission
    {
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly ClaimsProperty claimsProperty;

        public ModuleService(BtprojecShuttermanagementDevContext context,ClaimsProperty claimsProperty)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<List<ModuleResponse>>> GetModulesWithPermissionsAsync()
        {
            IResponseVm<List<ModuleResponse>> responseVm = new ResponseVm<List<ModuleResponse>>();
            try
            {

                var LoggedUserRoleName = (from u in _context.TblUsers
                                          join t in _context.TblTenants on u.TenantId equals t.TenantId
                                          join ur in _context.TblUserRoles on u.Id equals ur.UserId
                                          join r in _context.TblRoles on ur.RoleId equals r.Id
                                          join rp in _context.TblRolesPermissions on r.Id equals rp.RoleId
                                          where u.Id == AppConstant.LoggedInUserID
                                          && u.IsActive == true
                                          && u.IsDelete == false
                                          && r.IsActive == true
                                          && r.IsDelete == false
                                          && t.IsDelete == false
                                          && t.IsActive == true
                                          select rp.ModulePermissionId).ToList();
                if (LoggedUserRoleName != null && LoggedUserRoleName.Count > 0)
                {
                    var permissionList = await _context.MstModulePermissions.Where(x =>
                    LoggedUserRoleName.Contains(x.Id)).ToListAsync();



                    var modules = await _context.MstModules
                        .Where(m => (m.IsActive == true) && (m.IsDelete == false) && (AppConstant.IsAdminRolesShow ? true : permissionList.Select(x => x.ModuleId).Distinct().Contains(m.Id)))
                        .Select(m => new ModuleResponse
                        {
                            ModuleName = m.ModuleName,
                            ModuleDisplayName = m.ModuleDisplayName,
                            Permissions = m.MstModulePermissions
                                .Where(mp => (mp.IsActive == true) && (mp.IsDelete == false))
                                .Select(mp => new PermissionResponse
                                {
                                    PermissionId = mp.Id,
                                    ModulePermission = mp.ModulePermission,
                                    ModulePermissionDisplay = mp.PermissionDisplayName
                                })
                                .ToList()
                        })
                        .OrderBy(m => m.ModuleName)
                        .ToListAsync();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.Data = modules;
                }
                else
                {
                    responseVm.Message = "No Roles found";
                }
            }
            catch(Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }


    }
}
