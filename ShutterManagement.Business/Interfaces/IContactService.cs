using ShutterManagement.Core.SPModel;
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
    public interface IContactService : IGenericRepository<TblContact>
    {
        Task<IResponseVm<string>> Create_Contact(Sp_Contact model);
        Task<IResponseVm<string>> GetContactByEmailOrPhone(string Email, string Phone);
        Task<IResponseVm<string>> Update_Contact(UpdateContact model);
        Task<IResponseVm<List<ContactsList>>> GetAll_Contact(Pagination filter, bool Role);
        Task<IResponseVm<string>> ActiveInActiveContact(Guid ContactId);
        Task<IResponseVm<string>> DeleteContact(Guid ContactId);
        //Task<IResponseVm<string>> GetAllArchiveContacts();
        Task<IResponseVm<string>> ArchiveContacts(ArchiveUser archiveModel);
    }
}
