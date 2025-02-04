using ShutterManagement.Core.ViewModels;
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
    public interface IDashboard :IGenericRepository<TblJob>
    {
        Task<IResponseVm<List<StatusCountVm>>> GetallDashBoardData();

    }
}
