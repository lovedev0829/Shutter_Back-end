using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;

namespace ShutterManagement.Business.Interfaces
{
    public interface IReportServices
    {
        Task<IResponseVm<List<OrderReportList>>> GetOrderConfirmed();
    }
}
