using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Services
{
    public interface IAuthService
    {
        Task<IResponseVm<UserLoginDetails>> LoginAsync(LoginDetailVM user, bool? IsImpersonate = false);
        Task<IResponseVm<HomePageContent>> HomePageContent(string CompanyCode);
        Task<IResponseVm<PasswordChangeVm>> ResetPassWord(PasswordChangeVm passwordChangeVm);
        Task<IResponseVm<string>> ProfileUpdate(ProfileUpdateVm vm);
        Task<IResponseVm<UserResponse>> ProfileGetById();
        Task<IResponseVm<string>> ForgetPassword(ForgetPassword forgetPassword);
        Task<IResponseVm<string>> VerifyForgetPassword(VerifyPassword verifyPassword);
        Task<IResponseVm<List<CompanyCodeResponse>>> GetCompanyCodesByEmail(string email);
        Task<IResponseVm<string>> GenerateToken();
        IResponseVm<string> ReadTokenDetails(string token);

        Task<IResponseVm<Dictionary<string, string>>> GenerateLetterOnlyUniqueCodeAsync(string code);

        Task<IResponseVm<UserLoginDetails>> ImpersonateTenant(string SubTenantId);
    }
}
