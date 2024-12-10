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
    public interface IWindowService :IGenericRepository<TblWindow>
    {
        Task<IResponseVm<string>> Create_Window(CreateWindow model);
        Task<IResponseVm<string>> Update_Window(UpdateWindow  model);
        Task<IResponseVm<List<WindowsList>>> GetAll_Window(Pagination filter);
        // Task<IResponseVm<AddressResponse>> GetById_Address(Guid AddressId);
        Task<IResponseVm<string>> ActiveInActiveWindow(Guid WindowId);
        Task<IResponseVm<string>> DeleteWindow(Guid WindowId);
    }
}
