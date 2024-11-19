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
    public interface IOption : IGenericRepository<TblOption>
    {
        Task<IResponseVm<string>> Create_Option(CreateOption model);
        //Task<IResponseVm<string>> Update_Option(UpdateOption model);
        //Task<IResponseVm<dynamic>> GetAll_Option(Pagination filter);
        Task<IResponseVm<string>> ActiveInActive(Guid Id);
        Task<IResponseVm<string>> Delete(Guid Id);
    }
}
