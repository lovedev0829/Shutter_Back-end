using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;

namespace ShutterManagement.Business.Interfaces
{
     public interface IMaterial : IGenericRepository<TblRawMaterial>
    {
        Task<IResponseVm<string>> Create_Material(CreateMaterial model);
        Task<IResponseVm<string>> Update_Material(UpdateMaterial model);
        Task<IResponseVm<List<GetAllMaterialVm>>> GetAll_Material(Pagination filter);
        Task<IResponseVm<string>> ActiveInActive(Guid Id);
        Task<IResponseVm<string>> Delete(Guid Id);
    }
}
