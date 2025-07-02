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

    public interface IJobSchedule : IGenericRepository<TblJobSchedule>
    {
        Task<IResponseVm<string>> CreateJobSchedule(CreateJobSchedule model);
        Task<IResponseVm<string>> UpdateJobSchedule(UpdateJobSchedule model);
        Task<IResponseVm<string>> DeleteJobSchedule(Guid JobScheduleId);
        Task<IResponseVm<string>> ActiveInActive(Guid JobScheduleId);
        Task<IResponseVm<List<JobScheduleVm>>> GetAll_JobSchedule(Pagination filter);
        Task<IResponseVm<string>> AppointmentSync(Guid? LoggedInTenantId = null);
    }
    
}
