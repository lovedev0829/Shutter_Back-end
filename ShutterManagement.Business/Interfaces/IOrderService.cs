using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;

namespace ShutterManagement.Business.Interfaces
{
    public interface IOrderService : IGenericRepository<TblOrder>
    {
        Task<IResponseVm<string>> Create_Order_From_Estimate(Guid EstimateId, bool fromEstimate = false, UpdateOrder updateOrder = null);
        Task<IResponseVm<string>> Update_Order(UpdateOrder model);
        Task<IResponseVm<string>> ActiveInActiveOrder(Guid OrderId);
        Task<IResponseVm<string>> DeleteOrder(Guid OrderId);
        Task<IResponseVm<List<OrderList>>> GetLatestVersionOrders(Pagination filter);
        Task<IResponseVm<string>> SendForApproval(Guid OrderId);
        Task<IResponseVm<dynamic>> ViewHistory(Guid EstimateId);
        Task<IResponseVm<string>> ChangeStatus(Guid OrderId, Estimation NewEstimationStatus);
        Task<IResponseVm<dynamic>> ViewOrder(Guid OrderId);
        Task<IResponseVm<List<OrderList>>> GetAll_Order(Pagination filter, Guid? TenantId = null);

        Task<IResponseVm<string>> GenerateOrderSheet(Guid? orderId);

    }
}
