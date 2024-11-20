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
    public  interface IVendorService : IGenericRepository<TblVendor>
    {
        Task<IResponseVm<string>> Create_Vendor(CreateVendor model);
        Task<IResponseVm<string>> Update_Vendor(UpdateVendor model);
        Task<IResponseVm<List<VendorListVm>>> GetAll_Vendor(Pagination filter);
        Task<IResponseVm<string>> ActiveInActive(Guid Id);
        Task<IResponseVm<string>> Delete(Guid Id);
    }
}
