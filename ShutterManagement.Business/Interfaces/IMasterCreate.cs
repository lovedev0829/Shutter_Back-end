using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IMasterCreate
    {
        Task Create(CreateServiceVm model);
        Task Update(CreateServiceVm model);
        Task<GetContactResponse> GetAll();
        Task<Sp_Contact> GetById(Guid id);
        Task DeleteContact(Guid id);
    }
}
