using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;

namespace ShutterManagement.Business.Interfaces
{
    public interface IExpressionService : IGenericRepository<TblExpression>
    {
        Task<IResponseVm<string>> Create_Expression(CreateExpression model);
        Task<IResponseVm<string>> Update_Expression(UpdateExpression model);
        Task<IResponseVm<List<GetExpression>>> Get_AllExpression(Pagination filter);
        Task<IResponseVm<string>> ActiveInActive(Guid ExpressionId);
        Task<IResponseVm<string>> Delete(Guid ExpressionId);

    }
}
