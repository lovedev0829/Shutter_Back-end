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

    public interface IPaymentsService : IGenericRepository<TblPayment>
    {
        Task<IResponseVm<string>> CreatePayment(CreatePayments model);
        Task<IResponseVm<string>> UpdatePayment(UpdatePayments updatePayment);
        Task<IResponseVm<string>> DeletePayment(Guid PaymentId);
        Task<IResponseVm<string>> ActiveInActive(Guid PaymentId);
        Task<IResponseVm<List<PaymentsVM>>> GetAllPayment(Pagination filter);

    }

}
