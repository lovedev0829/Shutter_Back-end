using ShutterManagement.Core.SPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface ITenantService
    {
        Task TenantCreate(Sp_Tenant model);
        Task TenantUpdate(Sp_Tenant model);
        Task<GetTenantResponse> GetAll();
        Task<Sp_Tenant> GetById(Guid id);

        Task DeleteTenant(Guid id);

    }
}
