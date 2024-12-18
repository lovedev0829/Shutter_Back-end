using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
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
        Task<IResponseVm<string>> CreateDiscountCoupon(CreateDiscount createDiscount);
        Task<IResponseVm<string>> UpdateDiscountCoupon(UpdateDiscount updateDiscount);
        Task<IResponseVm<string>> DeleteDiscountCoupon(Guid discountId); 
        Task<IResponseVm<string>> ActiveInActive(Guid discountId);
        Task<IResponseVm<List<DiscoountVm>>> GetAll_Coupons(Pagination filter);
    }
}
