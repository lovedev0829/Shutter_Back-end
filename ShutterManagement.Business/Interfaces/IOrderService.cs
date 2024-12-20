using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;

namespace ShutterManagement.Business.Interfaces
{
    public interface IOrderService
    {
        Task<IResponseVm<string>> Create_Order(CreateOrder model);
        Task<IResponseVm<string>> Update_Order(UpdateOrder model);
        //Task<IResponseVm<string>> ActiveInActiveOrder(Guid EstimateId);
        //Task<IResponseVm<string>> DeleteOrder(Guid EstimateId);
        //Task<IResponseVm<List<EstimateList>>> GetAll_Order(Pagination filter);
        //Task<IResponseVm<string>> SendForApproval(Guid EstimateId);
        //Task<IResponseVm<List<EstimateList>>> ViewEachOrder(Guid EstimateId);
        //Task<IResponseVm<string>> ChangeStatus(Guid EstimationId, Estimation NewEstimationStatus);

    }
}
