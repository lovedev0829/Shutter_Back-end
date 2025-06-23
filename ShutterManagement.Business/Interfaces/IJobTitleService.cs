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
    public interface IJobTitleService :IGenericRepository<MstJobTitle>
    {
        Task<IResponseVm<List<JobTitlesList>>> GetAll(Pagination filter);

        Task<IResponseVm<string>> Create_Title(CreateJobTitle model);
        Task<IResponseVm<string>> Update_Title(UpdateJobTitle model);
        Task<IResponseVm<string>> ActiveInActive(Guid? JobTitleId);
        Task<IResponseVm<string>> DeleteTitle(Guid? JobTitleId);
    }
}
