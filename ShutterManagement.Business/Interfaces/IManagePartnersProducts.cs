using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;

namespace ShutterManagement.Business.Interfaces
{
    public interface IManagePartnersProducts : IGenericRepository<TblManagePartnersProduct>
    {
        Task<IResponseVm<string>> CreateManagePartnerProduct(CreateManagePartnerProduct model);
    }
}
