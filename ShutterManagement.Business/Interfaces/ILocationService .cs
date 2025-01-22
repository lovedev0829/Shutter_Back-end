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
    public interface ILocationService : IGenericRepository<TblLocation>
    {
        Task<IResponseVm<string>> Create_Location(CreateLocation model);
        Task<IResponseVm<List<LocationVm>>> GetAll_Location();
    }
}
