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
    public interface IMstProductType : IGenericRepository<MstProductType>
    {
        Task<IResponseVm<List<ProductTypeList>>> GetAll_ProductType();

        Task<IResponseVm<string>> Create_ProductType(CreateProductType model);
        Task<IResponseVm<string>> Update_ProductType(UpdateProductType model);
        Task<IResponseVm<string>> ActiveInActive(Guid? ProductId);
        Task<IResponseVm<string>> DeleteProductType(Guid? ProductId);
    }
}
