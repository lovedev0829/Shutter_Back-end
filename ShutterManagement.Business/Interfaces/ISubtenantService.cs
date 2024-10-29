using ShutterManagement.Core.SPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
     public interface ISubtenantService
    {
        Task SubtenantCreate(Sp_Tenant model);
        Task SubtenantUpdate(Sp_Tenant model);
        Task<GetTenantResponse> GetAll();
        Task<Sp_Tenant> Subtenant_GetById(Guid id);
        Task Delete_Subtenant(Guid id);
    }
}
