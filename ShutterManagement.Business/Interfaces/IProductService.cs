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
    public interface IProductService  : IGenericRepository<TblProduct>
    {
        Task<IResponseVm<string>> Create_Product(CreateProduct model);
        Task<IResponseVm<string>> Update_Product(UpdateProduct model);
        Task<IResponseVm<List<ProductListVm>>> GetAll_Product(Pagination filter);
        // Task<IResponseVm<AddressResponse>> GetById_Address(Guid AddressId);

        Task<IResponseVm<string>> ActiveInActiveProduct(Guid ProductId);
        Task<IResponseVm<string>> DeleteProduct(Guid ProductId);
    }
}
