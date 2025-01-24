using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public static class GenerateUniqueId
    {
   
        public static string GenerateUniqueIds(ModuleName moduleName, string Code, int Version, bool forEstimationName = false)
        {
            var Tenantcode = Code;
            var GenerateUniqueName = "";
            if (Tenantcode != null)
            {
               if(forEstimationName)
                {
                 GenerateUniqueName = $"{EnumValue.GetEnumValue(moduleName)}-{Tenantcode.ToUpper()}{DateTime.Now:yyyyMMdd}-{Version}";
                }
                else
                {
                GenerateUniqueName = $"{EnumValue.GetEnumValue(moduleName)}-{Tenantcode.ToUpper()}{DateTime.Now:yyyyMMddhhmmssffffff}-{Version}";
                }
            }
            return GenerateUniqueName;
        }
    }
}
