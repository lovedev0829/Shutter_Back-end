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
    public interface IAppointmentScheduleService : IGenericRepository<TblAppointmentSchedule>
    {
       
            Task<IResponseVm<string>> CreateAppointmentSchedule(CreateAppointment model);

             Task<IResponseVm<List<AppointmentScheduleVm>>> GetAppointmentSchedule(Guid? JobId);
          
    }
}
