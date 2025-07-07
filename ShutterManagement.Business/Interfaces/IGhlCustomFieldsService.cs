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
    public interface IGhlCustomFieldsService : IGenericRepository<TblGhlCustomField>
    {
        Task<IResponseVm<string>> SaveGhlCustomFieldsAsync(GetGHLConfiguration getGHLConfiguration,Guid? tenantId = null);
    }
}
