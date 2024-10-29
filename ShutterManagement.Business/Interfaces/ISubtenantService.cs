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
        Task CreateSubtenant(Sp_Tenant model);
        Task UpdateSubtenant(Sp_Tenant model);
        Task<List<Sp_Tenant>> GetAllSubtenants();
        Task<Sp_Tenant> GetSubtenantById(Guid id);
        Task DeleteSubtenant(Guid id);
    }
}
