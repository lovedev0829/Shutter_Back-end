using ShutterManagement.Core.SPModel;
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
     public interface IContactService : IGenericRepository<TblContact>
    {
        Task<IResponseVm<string>> Create_Contact(Sp_Contact model);
        Task<IResponseVm<string>> Update_Contact(Sp_Contact model);
        Task<dynamic> GetAll_Contact(Pagination filter);

        Task<IResponseVm<string>> ActiveInActiveContact(Guid id);
        Task<IResponseVm<string>> DeleteContact(Guid id);
    }
}
