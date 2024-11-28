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
    public class GenerateUniqueId
    {
        private readonly ShutterDbContext dbContext;

        public GenerateUniqueId(ShutterDbContext dbContext)
        {
            this.dbContext = dbContext;
        }



        public string GenerateUniqueIds(ModuleName moduleName)
        {
            var Tenantcode = dbContext.TblTenants
                .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false)
                .FirstOrDefault();
            var GenerateUniqueName = "";
            if (Tenantcode != null)
            {
               GenerateUniqueName = $"{Tenantcode.Code}-{moduleName}-{DateTime.Now:yyyyMMddHHmmss}";
            }
            return GenerateUniqueName;
        }
    }
}
