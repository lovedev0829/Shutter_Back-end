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
using ShutterManagement.Core.Common;

namespace ShutterManagement.Business.Interfaces
{
    public interface IInvoiceService :IGenericRepository<TblInvoice>
    {
        Task<IResponseVm<string>> CreateInvoice(CreateInvoice createInvoice);
        Task<IResponseVm<string>> UpdateInvoice(UpdateInvoice updateInvoice);
        Task<IResponseVm<string>> DeleteInvoice(Guid InvoiceId);
        Task<IResponseVm<string>> ActiveInActive(Guid InvoiceId);
        Task<IResponseVm<string>> SendInvoiceAttachment(GenerateInvoiceModel model);
        Task<IResponseVm<List<InvoiceVM>>> GetAllInvoice(Pagination filter);
        Task<IResponseVm<List<InvoiceVM>>> InvoiceReport(Pagination filter, ReportFilterType Type);

        bool IsStripeApiKeyValid();

    }
}
