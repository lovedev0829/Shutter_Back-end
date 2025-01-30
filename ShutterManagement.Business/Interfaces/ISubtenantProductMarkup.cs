using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Business.Interfaces
{
    public interface ISubtenantProductMarkup : IGenericRepository<TblSubtenantProductMarkup>
    {
        Task<IResponseVm<string>> Create_SubtenatProductMarkup(CreateSubtenantProductmarkup model);
        Task<IResponseVm<string>> Update_SubtenantProductMarkup(UpdateSubtenantProductMarkup model);
        Task<IResponseVm<string>> ActiveInActive(Guid Id);
        Task<IResponseVm<string>> Delete(Guid Id);
        Task<IResponseVm<List<GetAllSubtenantProductMarkup>>> GetAllSubtenantProductMarkup(Pagination filter);
    }
}
