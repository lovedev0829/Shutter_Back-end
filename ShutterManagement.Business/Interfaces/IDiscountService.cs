using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IDiscountService : IGenericRepository<TblDiscount>
    {
        Task<IResponseVm<string>> CreateDiscountAsync(CreateDiscount createDiscount);
        Task<IResponseVm<string>> UpdateAsync(UpdateDiscount updateDiscount);
        Task<IResponseVm<string>> DeleteAsync(Guid discountId); 
        Task<IResponseVm<string>> ActiveInActive(Guid discountId); 
    }
}
