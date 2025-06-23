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
    public interface IEmailConfigurationService : IGenericRepository<TblEmailConfiguration>
    {
        Task<IResponseVm<EmailConfigurationVm>> GetAll_EmailConfigurations();

        Task<IResponseVm<string>> AddOrUpdateEmailConfiguration(CreateEmailConfiguration emailConfigurationVm);

        Task<IResponseVm<List<GetEmailTypesVm>>> GetAll_EmailTypeConfigurations();
    }
}
