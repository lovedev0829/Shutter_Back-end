using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Interfaces;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Business.Interfaces
{
    public interface ITemplatesService : IGenericRepository<TblTemplate>
    {
        //Task<IResponseVm<string>> Create_Temaplate(CreateTemplate model);
        //Task<IResponseVm<dynamic>> Update_Template(UpdateTemplate model);
        //Task<IResponseVm<List<GetAllTemplate>>> GetAll_Template(Pagination filter);
        //Task<IResponseVm<string>> ActiveInActive(Guid Id);
        //Task<IResponseVm<string>> Delete(Guid Id);
    }
}
