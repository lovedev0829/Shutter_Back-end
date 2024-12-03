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
    public interface ITaxRate : IGenericRepository<TblTaxRate>
    {
        Task<IResponseVm<string>> CreateTax(CreateTax model);
        Task<IResponseVm<string>> UpdateTax(UpdateTax model);
        Task<IResponseVm<List<GetAllTax>>> GetAll_Tax(Pagination filter);

        Task<IResponseVm<string>> ActiveInActiveTax(Guid Id);
        Task<IResponseVm<string>> DeleteTax(Guid Id);

    }
}
