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
    public interface IRoleService : IGenericRepository<TblRole>
    {
        Task<IResponseVm<string>> Create(AddRole model);
        Task<IResponseVm<string>> Update(UpdateRole model);
        Task<IResponseVm<string>> Delete(string RoleId);
        Task<IResponseVm<string>> ActiveInactive(string RoleId);
        Task<IResponseVm<List<RoleList>>> GetAll(Pagination filter);
    }
}
