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
        Task Create_Address(CreateAddress model);
        Task Update_Address(CreateAddress model);
        Task<List<AddressResponse>> GetAll_Address();
        Task<AddressResponse> GetById_Address(Guid id);

        Task<IResponseVm<string>> ActiveInActiveAddress(Guid id);
        Task<IResponseVm<string>> DeleteAddress(Guid id);
    }
}
