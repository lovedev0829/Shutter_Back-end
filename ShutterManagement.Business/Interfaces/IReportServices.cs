using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Business.Interfaces
{
    public interface IReportServices
    {
        Task<IResponseVm<List<OrderList>>> GetOrderConfirmed(Pagination filter);
    }
}
