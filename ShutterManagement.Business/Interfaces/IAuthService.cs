using System.Threading.Tasks;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(TblUser user);
    }
}
