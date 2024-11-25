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
    public interface IComponentMaterial: IGenericRepository<TblComponentMaterial>
    {
        Task<IResponseVm<string>> AddComponentMaterial(CreateComponentMaterialVm model);
        Task<IResponseVm<string>> UpdateComponentMaterial(UpdateComponentMaterialVm model);
        Task<IResponseVm<string>> Delete(Guid Id);
        ////Task<IResponseVm<string>> ActiveInActive(Guid Id);
        //Task<IResponseVm<string>> Delete(Guid Id);
    }
}
