using Microsoft.Data.SqlClient;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ShutterManagement.Services
{
    public class UserService : IUserService
    {
        private readonly DataServices _dataServices;

        public UserService(DataServices dataServices)
        {
            _dataServices = dataServices;
        }

        public async Task<string> CreateUserAsync(LoginVm loginVm)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@username", SqlDbType.NVarChar) { SqlValue = loginVm.Username },
                new SqlParameter("@pwd", SqlDbType.NVarChar) { SqlValue = loginVm.Password }, 
                new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = loginVm.Code },
                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Insert" }
            };

            await _dataServices.CallSP("sp_UserCRUD",null, parameters);
            return "User created successfully.";
        }

        public async Task<List<LoginVm>> GetAllUsersAsync(string Code)
        {
           
            var users = await _dataServices.CallSPtoRead<LoginVm>("sp_UserCRUD", reader =>
            {
                return new LoginVm
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    Password = reader.GetString(reader.GetOrdinal("Pwd")),
                };
            },
            new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = Code },
            new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "SelectAll" });

            return users.ToList();
        }

        public async Task<LoginVm> GetUserByIdAsync(Guid id, string Code)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { SqlValue = id },
                new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = Code },
                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "SelectById" }
            };

            var user = await _dataServices.CallSPtoRead<LoginVm>("sp_UserCRUD", reader =>
            {
                return new LoginVm
                {
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    Password = reader.GetString(reader.GetOrdinal("Pwd")),
                };
            }, parameters);

            return user.FirstOrDefault();
        }

        public async Task<string> UpdateUserAsync(Guid id, LoginVm loginVm)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { SqlValue = id },
                new SqlParameter("@username", SqlDbType.NVarChar) { SqlValue = loginVm.Username },
                new SqlParameter("@pwd", SqlDbType.NVarChar) { SqlValue = loginVm.Password }, 
                new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = loginVm.Code },
                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Update" }
            };

            await _dataServices.CallSP("sp_UserCRUD", parameters);
            return "User updated successfully.";
        }

        public async Task<string> DeleteUserAsync(Guid id,string Code)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { SqlValue = id },
                new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = Code },
                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Delete" }
            };

            await _dataServices.CallSP("sp_UserCRUD", parameters);
            return "User deleted successfully.";
        }

        public async Task<string> SetUserInactiveAsync(Guid id,string Code)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { SqlValue = id },
                new SqlParameter("@Code", SqlDbType.NVarChar) { SqlValue = Code },
                new SqlParameter("@Action", SqlDbType.NVarChar) { SqlValue = "Inactive" }
            };

            await _dataServices.CallSP("sp_UserCRUD", parameters);
            return "User set to inactive.";
        }
    }
}
