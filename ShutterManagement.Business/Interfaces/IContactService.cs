using ShutterManagement.Core.SPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
     public interface IContactService
    {
        Task Create_Contact(Sp_Contact model);
        Task Update_Contact(Sp_Contact model);
        Task<List<Sp_Contact>> GetAll_Contact();
        Task<Sp_Contact> GetById_Contact(Guid id);

        Task Delete_Contact(Guid id);
    }
}
