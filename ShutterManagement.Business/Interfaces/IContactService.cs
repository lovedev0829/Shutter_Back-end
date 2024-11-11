using ShutterManagement.Core.SPModel;
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
     public interface IContactService : IGenericRepository<TblContact>
    {
        Task Create_Contact(Sp_Contact model);
        Task Update_Contact(Sp_Contact model);
        Task<List<Sp_Contact>> GetAll_Contact();
        Task<Sp_Contact> GetById_Contact(Guid id);

        Task<IResponseVm<string>> ActiveInActiveContact(Guid id);
        Task<IResponseVm<string>> DeleteContact(Guid id);
    }
}
