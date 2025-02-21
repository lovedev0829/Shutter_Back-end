using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IJobTypeService : IGenericRepository<MstJobType>
    {
        Task<IResponseVm<List<JobTypeList>>> GetAllJobType(Pagination filter);

        Task<IResponseVm<string>> Create_JobType(CreateJobType model);
        Task<IResponseVm<string>> Update_JobType(UpdateJobType model);
        Task<IResponseVm<string>> ActiveInActive(Guid? JobTypeId);
        Task<IResponseVm<string>> DeleteJobType(Guid? JobTypeId);
    }
}
