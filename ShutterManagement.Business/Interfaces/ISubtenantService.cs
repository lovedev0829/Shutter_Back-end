using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Business.Interfaces
{
     public interface ISubtenantService : IGenericRepository<TblTenant>
    {
        Task<IResponseVm<string>> SubTenantCreate(CreateTenant model);
        Task<IResponseVm<string>> SubTenantUpdate(UpdateSubTenant model);
        Task<IResponseVm<string>> SubTenantDelete(string TenantId);
        Task<IResponseVm<string>> SubTenantActiveInactive(string TenantId);
        Task<IResponseVm<List<GetAllTenant>>> SubTenantGetAll(Pagination filter);
    }
}
