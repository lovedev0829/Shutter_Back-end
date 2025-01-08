using ShutterManagement.Business.Services;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface ITenantService : IGenericRepository<TblTenant> 
    {
        Task<IResponseVm<string>> TenantCreate(CreateTenant model);
        Task<IResponseVm<string>> TenantUpdate(UpdateTenant model);
        Task<IResponseVm<string>> TenantDelete(string TenantId);
        Task<IResponseVm<string>> TenantActiveInactive(string TenantId);
        Task<IResponseVm<List<GetAllTenant>>> TenantGetAll(Pagination filter);
    }
}
