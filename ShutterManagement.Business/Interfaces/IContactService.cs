using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using ShutterManagment.GHL.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IContactService : IGenericRepository<TblContact>
    {
        Task<IResponseVm<string>> Create_Contact(Sp_Contact model, Guid? LoggedInTenantId = null, bool? IsGHLContact = false);
        Task<IResponseVm<string>> GetContactByEmailOrPhone(string Email, string Phone);
        Task<IResponseVm<string>> Update_Contact(UpdateContact model, Guid? LoggedInTenantId = null,bool? IsGHLContact = false);
        Task<IResponseVm<List<ContactsList>>> GetAll_Contact(Pagination filter, bool Role ,bool? IsArchive);
        Task<IResponseVm<string>> ActiveInActiveContact(Guid ContactId);
        Task<IResponseVm<string>> DeleteContact(Guid ContactId);
        //Task<IResponseVm<string>> GetAllArchiveContacts();
        Task<IResponseVm<string>> ArchiveContacts(ArchiveUser archiveModel);
        Task<IResponseVm<string>> ContactFromGHL(GHLContactTrigger contactData);
        Task<IResponseVm<string>> UpdateContactFromGHL(GHLContactTrigger contactData);
        Task<IResponseVm<string>> ContactSync(Guid? LoggedInTenantId = null);
        //  Task<IResponseVm<string>> SyncContactandJobforPartner(string TenantId);

        string FormatPhoneNumber(string phoneNumber);
    }
}
