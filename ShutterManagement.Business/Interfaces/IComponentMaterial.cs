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
        //Task<IResponseVm<string>> AddComponentMaterial(ComponentMaterialModel model);
        //Task<IResponseVm<dynamic>> UpdateComponentMaterial(UpdateComponent model);
        ////Task<IResponseVm<List<ComponentListVm>>> GetAll_Component(Pagination filter);
        ////Task<IResponseVm<string>> ActiveInActive(Guid Id);
        //Task<IResponseVm<string>> Delete(Guid Id);
    }
}
