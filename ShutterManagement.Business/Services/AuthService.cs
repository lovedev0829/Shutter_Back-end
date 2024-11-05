using Microsoft.IdentityModel.Tokens;
using ShutterManagement.DAL.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.SPModel;
using System.Data;
using ShutterManagement.Core.SPModel.SP_Responses;
using Microsoft.Data.SqlClient;
using ShutterManagement.Core.ViewModels.Response;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace ShutterManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly DataServices dataServices;
        private readonly IMapper mapper;
        private readonly IConfiguration _config;

        public AuthService(DataServices dataServices,IMapper mapper, IConfiguration config)
        {
            this.dataServices = dataServices;
            this.mapper = mapper;
            _config = config;
        }
        public async Task<IResponseVm<UserLoginDetails>> LoginAsync(LoginDetailVM user)
        {
            IResponseVm<UserLoginDetails> responseVm = new ResponseVm<UserLoginDetails>();
            UserLoginDetails loginDetails = new UserLoginDetails();
            var UserDetails = await dataServices.CallSPtoRead<LoginDetails>("Sp_GetUserDetailsOnLogin", reader =>
            {
                return new LoginDetails
                {
                    UserId = reader.GetGuid(reader.GetOrdinal("id")),
                    Username = reader.GetString(reader.GetOrdinal("username")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    MiddleName = reader.GetString(reader.GetOrdinal("MiddleName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.GetString(reader.GetOrdinal("Phone"))
                };
            },
             new SqlParameter("@Username", SqlDbType.NVarChar) { SqlValue = user.Username },
             new SqlParameter("@Password", SqlDbType.NVarChar) { SqlValue = user.Password },
             new SqlParameter("@CompanyCode", SqlDbType.NVarChar) { SqlValue = user.CompanyCode }
             );


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", UserDetails[0].UserId.ToString()),
                    new Claim("Username", UserDetails[0].Username),
                    new Claim(ClaimTypes.Name, UserDetails[0].FirstName + " " + UserDetails[0].MiddleName + " " + UserDetails[0].LastName ),
                    new Claim(ClaimTypes.Email, UserDetails[0].Email),
                    new Claim("Phone",UserDetails[0].Phone),
                }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(AppConstant.JwtExpiryTime)),
                Issuer = AppConstant.JwtIssuer,
                Audience = AppConstant.JwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string TokenValue = tokenHandler.WriteToken(token);

            responseVm.Data = mapper.Map(UserDetails[0], loginDetails);
            responseVm.Data.Token = TokenValue;
            responseVm.IsSuccess = true;
            responseVm.Message = AppConstant.LoginSuccessMessgae;
            return responseVm;
        }

        //public async Task<IResponseVm<UserLoginDetails>> HomePageContent(string CompanyCode)
        //{
        //    IResponseVm<UserLoginDetails> responseVm = new ResponseVm<UserLoginDetails>();
        //    UserLoginDetails loginDetails = new UserLoginDetails();
        //    var UserDetails = await dataServices.CallSPtoRead<LoginDetails>("Sp_GetUserDetailsOnLogin", reader =>
        //    {
        //        return new LoginDetails
        //        {
        //            UserId = reader.GetGuid(reader.GetOrdinal("id")),
        //            Username = reader.GetString(reader.GetOrdinal("username")),
        //            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //            MiddleName = reader.GetString(reader.GetOrdinal("MiddleName")),
        //            LastName = reader.GetString(reader.GetOrdinal("LastName")),
        //            Email = reader.GetString(reader.GetOrdinal("Email")),
        //            Phone = reader.GetString(reader.GetOrdinal("Phone"))
        //        };
        //    },
        //     new SqlParameter("@Username", SqlDbType.NVarChar) { SqlValue = user.Username },
        //     new SqlParameter("@Password", SqlDbType.NVarChar) { SqlValue = user.Password },
        //     new SqlParameter("@CompanyCode", SqlDbType.NVarChar) { SqlValue = user.CompanyCode }
        //     );


        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey);
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new[]
        //        {
        //            new Claim("UserId", UserDetails[0].UserId.ToString()),
        //            new Claim("Username", UserDetails[0].Username),
        //            new Claim(ClaimTypes.Name, UserDetails[0].FirstName + " " + UserDetails[0].MiddleName + " " + UserDetails[0].LastName ),
        //            new Claim(ClaimTypes.Email, UserDetails[0].Email),
        //            new Claim("Phone",UserDetails[0].Phone),
        //        }),
        //        Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(AppConstant.JwtExpiryTime)),
        //        Issuer = AppConstant.JwtIssuer,
        //        Audience = AppConstant.JwtAudience,
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };

        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    string TokenValue = tokenHandler.WriteToken(token);

        //    responseVm.Data = mapper.Map(UserDetails[0], loginDetails);
        //    responseVm.Data.Token = TokenValue;
        //    responseVm.IsSuccess = true;
        //    responseVm.Message = AppConstant.LoginSuccessMessgae;
        //    return responseVm;
        //}
    }
}
