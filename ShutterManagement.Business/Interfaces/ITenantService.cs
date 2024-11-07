using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface ITenantService
    {
        Task TenantCreate(CreateTenantRequest model);
        Task TenantUpdate(UpdateTenant model);
        //Task<List<Sp_Tenant>> GetAll();
        //Task<Sp_Tenant> GetById(Guid id);

        //Task DeleteTenant(Guid id);

    }
}
