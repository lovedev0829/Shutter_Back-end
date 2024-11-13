using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
     public interface IAddress
    {
        Task<IResponseVm<string>> Create_Address(CreateAddress model);
        Task<IResponseVm<AddressResponse>> Update_Address(UpdateAddress model);
        Task<IResponseVm<dynamic>> GetAll_Address(Pagination filter);
        Task<IResponseVm<AddressResponse>> GetById_Address(Guid AddressId);

        Task<IResponseVm<string>> ActiveInActiveAddress(Guid AddressId);
        Task<IResponseVm<string>> DeleteAddress(Guid AddressId);
    }
}
