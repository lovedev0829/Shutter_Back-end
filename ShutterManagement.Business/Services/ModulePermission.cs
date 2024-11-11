using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class ModuleService : IModulePermission
    {
        private readonly ShutterDbContext _context;

        public ModuleService(ShutterDbContext context)
        {
            _context = context;
        }

        public async Task<List<ModuleResponse>> GetModulesWithPermissionsAsync()
        {
            var modules = await _context.MstModules
                .Where(m => (m.IsActive == true) && (m.IsDelete == false))
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
                        })
                        .ToList()
                })
                .OrderBy(m => m.ModuleName)
                .ToListAsync();

            return modules;
        }


    }
}
