using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class VendorService : BaseRepository<ShutterDbContext, TblVendor>, IVendorService
    {
        public Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IResponseVm<string>> Create_Vendor(CreateComponent model)
        {
            throw new NotImplementedException();
        }

        public Task<IResponseVm<string>> Delete(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IResponseVm<dynamic>> GetAll_Vendor(Pagination filter)
        {
            throw new NotImplementedException();
        }

        public Task<IResponseVm<dynamic>> Update_Vendor(UpdateComponent model)
        {
            throw new NotImplementedException();
        }
    }
}
