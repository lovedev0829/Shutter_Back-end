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
    public interface IStripeConfiguration :IGenericRepository<TblStripeConfiguration>
    {
        Task<IResponseVm<List<StripeConfigurationVm>>> GetAll_StripeConfiguration(Pagination fillter);

        Task<IResponseVm<string>> Create_StripeConfiguration(CreateStripeConfiguration model);
        Task<IResponseVm<string>> Update_StripeConfiguration(UpdateStripeConfiguration model);
        //Task<IResponseVm<string>> ActiveInActive(Guid? ProductId);
        Task<IResponseVm<string>> DeleteStripeConfiguration(Guid? id);
    }
}
