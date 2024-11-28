using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;

namespace ShutterManagement.Business.Interfaces
{
    public interface IJobService : IGenericRepository<TblJob>
    {
        Task<IResponseVm<string>> Create(CreateJob model);
        Task<IResponseVm<string>> Update(UpdateJob model);
        Task<IResponseVm<string>> Delete(string UserId);
        Task<IResponseVm<string>> ActiveInactive(string UserId);

        Task<IResponseVm<List<UsersList>>> GetAll(Pagination filter);
    }
}
