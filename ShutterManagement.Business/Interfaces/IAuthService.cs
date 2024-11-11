using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Services
{
    public interface IAuthService
    {
        Task<IResponseVm<UserLoginDetails>> LoginAsync(LoginDetailVM user);
        Task<IResponseVm<dynamic>> HomePageContent(string CompanyCode);
    }
}
