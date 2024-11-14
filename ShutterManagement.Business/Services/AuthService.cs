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
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.Common;

namespace ShutterManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly DataServices dataServices;
        private readonly IMapper mapper;
        private readonly IConfiguration _config;
        private readonly ITenantService tenantService;
        private readonly ITenantBrandingService tenantBrandingService;

        public AuthService(DataServices dataServices, IMapper mapper, IConfiguration config,
            ITenantService tenantService, ITenantBrandingService tenantBrandingService)
        {
            this.dataServices = dataServices;
            this.mapper = mapper;
            _config = config;
            this.tenantService = tenantService;
            this.tenantBrandingService = tenantBrandingService;
        }
        public async Task<IResponseVm<UserLoginDetails>> LoginAsync(LoginDetailVM user)
        {
            IResponseVm<UserLoginDetails> responseVm = new ResponseVm<UserLoginDetails>();
            if (string.IsNullOrEmpty(user.Username))
            {
                responseVm.Message = "Username required.";
                return responseVm;
            }
            if (string.IsNullOrEmpty(user.Password))
            {
                responseVm.Message = "Password required.";
                return responseVm;
            }
            if (string.IsNullOrEmpty(user.CompanyCode))
            {
                responseVm.Message = "CompanyCode required.";
                return responseVm;
            }
            UserLoginDetails loginDetails = new UserLoginDetails();
            var HashPassword = AesEncryptionHelper.Encrypt(user.Password ?? "");
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
                    TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
                    ParentTenantId = reader.GetString(reader.GetOrdinal("ParentTenantId")),
                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Logo = reader.GetString(reader.GetOrdinal("Logo")),
                    PrimaryColor = reader.GetString(reader.GetOrdinal("PrimaryColor")),
                    SecondaryColor = reader.GetString(reader.GetOrdinal("SecondaryColor")),
                };
            },
             new SqlParameter("@Username", SqlDbType.NVarChar, 100) { SqlValue = user.Username },
             new SqlParameter("@Password", SqlDbType.NVarChar, 100) { SqlValue = HashPassword },
             new SqlParameter("@CompanyCode", SqlDbType.NVarChar, 100) { SqlValue = user.CompanyCode },
             new SqlParameter("@Result", SqlDbType.VarChar, 1000) { Direction = ParameterDirection.Output }
             );

            if (UserDetails.Count > 0)
            {
                var UserRole = await dataServices.CallSPtoRead<UserRoles>("Sp_GetLoggedinUserRoles", reader =>
                {
                    return new UserRoles
                    {
                        PermissionId = reader.GetGuid(reader.GetOrdinal("Id")),
                        PermissionName = reader.GetString(reader.GetOrdinal("Name")),
                        PermissionDisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                        Icon = reader.GetString(reader.GetOrdinal("Icon")),
                        Url = reader.GetString(reader.GetOrdinal("Url"))
                    };
                },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { SqlValue = UserDetails[0].UserId }
                );
                var claims = new ClaimsIdentity();
                foreach(var item in UserRole)
                {
                    claims.AddClaim(new Claim(ClaimTypes.Role, item.PermissionName));
                }

                claims.AddClaim(new Claim("UserId", UserDetails[0].UserId.ToString()));
                claims.AddClaim(new Claim("Username", UserDetails[0].Username ?? ""));
                claims.AddClaim(new Claim(ClaimTypes.Name, UserDetails[0].FirstName + " " + UserDetails[0].MiddleName + " " + UserDetails[0].LastName));
                claims.AddClaim(new Claim(ClaimTypes.Email, UserDetails[0].Email ?? ""));
                claims.AddClaim(new Claim("Phone", UserDetails[0].Phone ?? ""));
                claims.AddClaim(new Claim("TenantId", UserDetails[0].TenantId.ToString() ?? ""));
                claims.AddClaim(new Claim("ParentTenantId", loginDetails.ParentTenantId ?? ""));
                claims.AddClaim(new Claim("CompanyCode", user.CompanyCode));

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(AppConstant.JwtExpiryTime)),
                    Issuer = AppConstant.JwtIssuer,
                    Audience = AppConstant.JwtAudience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                string TokenValue = tokenHandler.WriteToken(token);

                responseVm.Data = mapper.Map(UserDetails[0], loginDetails);
                responseVm.Data.Roles = UserRole
                    .Select(x => new LoginRoles { RoleName = x.PermissionName, Url = x.Url, Icon = x.Icon }).ToArray();
                responseVm.Data.Token = TokenValue;
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.LoginSuccessMessgae;
                responseVm.TotalRecords = 1;
            }
            else
            {
                responseVm.Message = AppConstant.InvalidCred;
            }
            return responseVm;
        }

        public async Task<IResponseVm<HomePageContent>> HomePageContent(string CompanyCode)
        {
            IResponseVm<HomePageContent> responseVm = new ResponseVm<HomePageContent>();
            if (string.IsNullOrEmpty(CompanyCode))
            {
                responseVm.Message = "Company code required.";
                return responseVm;
            }

            var TenantHomePageData = tenantService.GetAllQuerable().Where(x => x.Code == CompanyCode
            && x.IsDelete == false && x.IsActive == true && x.ParentTenantId == null).FirstOrDefault();
            if (TenantHomePageData != null)
            {
                var brandInfo = tenantBrandingService.GetAllQuerable().Where(x => x.Id == TenantHomePageData.BrandingInfo).FirstOrDefault();
                if (brandInfo != null)
                {
                    HomePageContent homePageContent = new HomePageContent
                    {
                        Code = TenantHomePageData.Code,
                        URL = TenantHomePageData.Url,
                        FirstName = TenantHomePageData.FirstName,
                        LastName = TenantHomePageData.LastName,
                        MiddleName = TenantHomePageData.MiddleName,
                        Email = brandInfo.Email,
                        PhoneNumber = brandInfo.PhoneNmber,
                        Logo = brandInfo.Logo,
                        PrimaryColor = brandInfo.PrimaryColor,
                        SecondaryColor = brandInfo.SecondaryColor,
                    };
                    responseVm.IsSuccess = true;
                    responseVm.Data = homePageContent;
                }

            }
            else
            {
                responseVm.Message = AppConstant.NoDataFound;
            }
            return responseVm;
        }

        
    }
}
