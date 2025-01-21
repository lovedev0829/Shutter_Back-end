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
        Task<IResponseVm<string>> Delete(string Id);
        Task<IResponseVm<string>> ActiveInactive(string Id);
        Task<IResponseVm<string>> UpdateJobStatus(UpdateJobStatus model);

        Task<IResponseVm<List<GetAllJobsdetails>>> GetAll(Pagination filter);

        Task<IResponseVm<List<GetAllJobsSubtenant_tenant>>> GetAllJobsTenant_SubTenant(Pagination filter);
    }
}
