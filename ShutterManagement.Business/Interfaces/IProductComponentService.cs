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
    public interface IProductComponentService :IGenericRepository<TblProductComponent>
    {
        Task<IResponseVm<string>> Create_ProductComponent(CreateAddress model);
        Task<IResponseVm<AddressResponse>> Update_ProductComponent(UpdateAddress model);
        Task<IResponseVm<List<GetAllAddress>>> GetAll_ProductComponent(Pagination filter);
        // Task<IResponseVm<AddressResponse>> GetById_Address(Guid AddressId);

        Task<IResponseVm<string>> ActiveInActiveProductComponent(Guid AddressId);
        Task<IResponseVm<string>> DeleteProductComponent(Guid AddressId);
    }
}
