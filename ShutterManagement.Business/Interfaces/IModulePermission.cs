using ShutterManagement.Core.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IModulePermission
    {

        Task<List<ModuleResponse>> GetModulesWithPermissionsAsync();


    }
}
