using Microsoft.AspNetCore.Mvc;
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
    public interface IUserService : IGenericRepository<TblUser>
    {
        Task<IResponseVm<string>> Create(CreateUser userDetails);
        Task<IResponseVm<string>> Update(UpdateUser UserDetails);
        Task<IResponseVm<string>> Delete(string UserId);
        Task<IResponseVm<string>> ActiveInactive(string UserId);

        Task<IResponseVm<List<UsersList>>> GetAll(Pagination filter);
        Task PasswordEnc();
        Task<IResponseVm<List<UsersList>>> GetAllUserList();
        Task<string> GetTenantIdByUserEmailAndPwd(string Username, string Password);
    }
}
