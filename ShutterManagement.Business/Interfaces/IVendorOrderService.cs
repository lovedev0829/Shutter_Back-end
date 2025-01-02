using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Business.Interfaces
{
    public interface IVendorOrderService : IGenericRepository<TblVendorOrder>
    {
        Task<IResponseVm<string>> Create_VendorOrder(CreateVendorOrder model);
        Task<IResponseVm<string>> Update_VendorOrder(UpdateVendorOrder model);
        Task<IResponseVm<string>> Update_VendorOrderItem(UpdateVendorOrderItem model);
        Task<IResponseVm<string>> Delete_VendorOrderItem(Guid? VendorOrderIdItem);
        Task<IResponseVm<string>> Delete_VendorOrder(Guid? vendorOrderId);
        Task<IResponseVm<string>> ActiveInActive(Guid Id);
        Task<IResponseVm<List<VendorOrderListVm>>> GetAll_VendorOrder(Pagination filter);
    }
}
