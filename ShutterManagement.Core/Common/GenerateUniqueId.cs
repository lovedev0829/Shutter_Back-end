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
   
        public static string GenerateUniqueIds(ModuleName moduleName, string Code, int Version)
        {

            var GenerateUniqueName = "";
            Code = Code.Substring(0, 3);
            var res = Version.ToString("00000");
            if (Code != null)
            {
                GenerateUniqueName = $"{Code.ToUpper()}-{EnumValue.GetEnumValue(moduleName).ToUpper()}-{res}";
            }
            return GenerateUniqueName;
        }

        
    }
}
