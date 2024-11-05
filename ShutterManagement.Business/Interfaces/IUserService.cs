using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IUserService
    {
        Task<string> CreateUserAsync(LoginVm loginVm);
        Task<List<LoginVm>> GetAllUsersAsync(string code);
        Task<LoginVm> GetUserByIdAsync(Guid id,string Code);
        Task<string> UpdateUserAsync(Guid id, LoginVm loginVm);
        Task<string> DeleteUserAsync(Guid id, string Code);
        Task<string> SetUserInactiveAsync(Guid id, string Code);
    }
}
