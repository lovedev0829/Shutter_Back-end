using ShutterManagement.Core.SPModel;
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
    public interface IJobContactService :IGenericRepository<TblJobContact>
    {
        Task<IResponseVm<string>> Create_JobContact(CreateJobContact model);
        Task<IResponseVm<string>> Update_JobContact(UpdateJobContact model);
        Task<IResponseVm<List<JobContactVm>>> GetAll_JobContact(Pagination filter);
        Task<IResponseVm<string>> ActiveInActiveJobContact(Guid JobContactId);
        Task<IResponseVm<string>> DeleteJobContact(Guid? JobContactId);
    }
}
