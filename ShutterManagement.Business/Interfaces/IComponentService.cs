using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IComponentService
    {
        Task<IResponseVm<string>> Create_Component(CreateComponent model);
        Task<IResponseVm<dynamic>> Update_Component(UpdateComponent model);
        Task<IResponseVm<dynamic>> GetAll_Component(Pagination filter);
        Task<IResponseVm<string>> ActiveInActive(Guid Id);
        Task<IResponseVm<string>> Delete(Guid Id);
    }
}
