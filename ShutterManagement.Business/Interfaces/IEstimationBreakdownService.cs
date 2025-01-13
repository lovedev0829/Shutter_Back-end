using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;

namespace ShutterManagement.Business.Interfaces
{
    public interface IEstimationBreakdownService
    {
        Task<IResponseVm<List<ContactsList>>> GetDistinctActiveContacts(Pagination filter);
        Task<IResponseVm<List<ContactBreakdown>>> GetContactUniqueId(Pagination filter);
        Task<IResponseVm<List<GetAllJobsSubtenant_tenant>>> GetContactJobsBreakdown(Pagination filter);

        Task<IResponseVm<List<EstimateList>>> GetContactEstimationsBreakdown(Pagination filter);
        Task<IResponseVm<List<object>>> GetProductListForEstimation(Guid EstimationId);
        Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> GetProductBreakdown(Guid EstimationId, string WindowProductId = "");

    }
}
