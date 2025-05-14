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
using System.Security.AccessControl;

namespace ShutterManagement.Services
{
    public class AuthService : BaseRepository<ShutterDBContext,TblUser >, IAuthService
    {
        private readonly DataServices dataServices;
        private readonly IMapper mapper;
        private readonly IConfiguration _config;
        private readonly ITenantService tenantService;
        private readonly ITenantBrandingService tenantBrandingService;
        private readonly INotificationService notification;
        private readonly IGHLConfiguration gHLConfiguration;
        private readonly IStripeConfiguration stripeConfiguration;

        public AuthService(DataServices dataServices, IMapper mapper, IConfiguration config,
            ITenantService tenantService, ITenantBrandingService tenantBrandingService,INotificationService notification,
            IGHLConfiguration gHLConfiguration,IStripeConfiguration stripeConfiguration)
        {
            this.dataServices = dataServices;
            this.mapper = mapper;
            _config = config;
            this.tenantService = tenantService;
            this.tenantBrandingService = tenantBrandingService;
            this.notification = notification;
            this.gHLConfiguration = gHLConfiguration;
            this.stripeConfiguration = stripeConfiguration;
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
                    suffix = reader.GetString(reader.GetOrdinal("suffix")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    TenantId = reader.GetGuid(reader.GetOrdinal("TenantId")),
                    ParentTenantId = reader.GetString(reader.GetOrdinal("ParentTenantId")),
                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Logo = reader.GetString(reader.GetOrdinal("Logo")),
                    PrimaryColor = reader.GetString(reader.GetOrdinal("PrimaryColor")),
                    SecondaryColor = reader.GetString(reader.GetOrdinal("SecondaryColor")),
                    CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
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
                bool IsTenant = string.IsNullOrEmpty(UserDetails[0].ParentTenantId) ? true : false;
                claims.AddClaim(new Claim("UserId", UserDetails[0].UserId.ToString().ToUpper()));
                claims.AddClaim(new Claim("Username", UserDetails[0].Username ?? ""));
                claims.AddClaim(new Claim(ClaimTypes.Name, UserDetails[0].FirstName + " " + UserDetails[0].MiddleName + " " + UserDetails[0].LastName));
                claims.AddClaim(new Claim(ClaimTypes.Email, UserDetails[0].Email ?? ""));
                claims.AddClaim(new Claim("Phone", UserDetails[0].Phone ?? ""));
                claims.AddClaim(new Claim("TenantId", UserDetails[0].TenantId.ToString().ToUpper() ?? ""));
                claims.AddClaim(new Claim("ParentTenantId", UserDetails[0].ParentTenantId.ToString().ToUpper() ?? ""));
               // claims.AddClaim(new Claim("CompanyCode", user.CompanyCode));
                claims.AddClaim(new Claim("CompanyCode", UserDetails[0].Code));
                claims.AddClaim(new Claim("CompanyName", UserDetails[0].CompanyName));
                claims.AddClaim(new Claim("IsTenant", IsTenant.ToString()));
              //  AppConstant.IsTenant = IsTenant;
                //// Store GHL Credentials in Claims
                //claims.AddClaim(new Claim("GHL_APIKey", GHLres.APIkey ?? ""));
                //claims.AddClaim(new Claim("GHL_BaseAddress", GHLres.BaseAddress ?? ""));
                //claims.AddClaim(new Claim("GHL_PipelineId", GHLres.PipelineId ?? ""));
                //claims.AddClaim(new Claim("GHL_Locationd", GHLres.StageId ?? ""));
                // Fetch GHL Configuration (Ensure to await the method)
               
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(AppConstant.JwtExpiryTime)),
                    Issuer = AppConstant.JwtIssuer,
                    Audience = AppConstant.JwtAudience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                string TokenValue = tokenHandler.WriteToken(token);

                responseVm.Data = mapper.Map(UserDetails[0], loginDetails);
                responseVm.Data.Roles = UserRole
                    .Select(x => x.PermissionName).ToArray();

                responseVm.Data.Menu = UserRole
                    .Select(x => new LoginRoles
                    {
                        RoleName = x.PermissionName.Replace("Edit", "").Replace("View", "").Replace("Delete", "")
                    .Replace("Address", "Adress").Replace("Add", "").Replace("Adress", "Address").Replace("SubTenant", "Partners"),
                        Icon = x.Icon,
                        Url = x.Url
                    })
                    .DistinctBy(x=>x.RoleName).ToArray();
                responseVm.Data.Token = TokenValue;
                responseVm.Data.IsTenant = IsTenant;
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.LoginSuccessMessgae;
                responseVm.TotalRecords = 1;

                await gHLConfiguration.GetGHLConfiguration(UserDetails[0].TenantId.ToString());
               // await stripeConfiguration.GetStripeConfiguration(UserDetails[0].TenantId.ToString());
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
            && x.IsDelete == false && x.IsActive == true).FirstOrDefault();
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
                        suffix=TenantHomePageData.Suffix,
                        MiddleName = TenantHomePageData.MiddleName,
                        Email = brandInfo.Email,
                        PhoneNumber = brandInfo.PhoneNumber,
                        Logo = brandInfo.Logo,
                        PrimaryColor = brandInfo.PrimaryColor,
                        SecondaryColor = brandInfo.SecondaryColor,
                        CompanyName = brandInfo.CompanyName,
                    };

                    responseVm.IsSuccess = true;
                    responseVm.Data = homePageContent;
                }

            }
            else
            {
                responseVm.Message = AppConstant.TenantNotFound;
            }
            return responseVm;
        }

        public async Task<IResponseVm<PasswordChangeVm>> ResetPassWord(PasswordChangeVm vm)
        {
            IResponseVm<PasswordChangeVm> result = new ResponseVm<PasswordChangeVm>();
            try
            {
                var IsUserExists = (await GetAll()).Where(x => x.IsDelete == false 
                && x.IsActive == true && x.Id == AppConstant.LoggedInUserID && x.Pwd == AesEncryptionHelper.Encrypt(vm.OldPassWord ?? "") && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if(IsUserExists != null)
                {
                    IsUserExists.Pwd = AesEncryptionHelper.Encrypt(vm.PassWord ?? "");
                    IsUserExists.ModifiedAt = DateTime.Now;
                    IsUserExists.ModifiedBy= AppConstant.LoggedInUserID;
                    IsUserExists.ExpireAt = null;
                    IsUserExists.Token = null;
                    await Update(IsUserExists);
                    result.IsSuccess = true;
                    result.Message = AppConstant.PasswordChangeMessage;
                }
                else
                {
                    result.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public async Task<IResponseVm<string>> ProfileUpdate(ProfileUpdateVm profileUpdate)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                var UserExistsDetails = GetAllQuerable().Where(x => x.IsDelete == false && x.IsActive == true &&
                x.Id == (AppConstant.LoggedInUserID)
                && x.TenantId == AppConstant.LoggedInTenantId
                ).FirstOrDefault();
                if (UserExistsDetails != null)
                {

                    bool IsPhoneExists = await AnyAsync(x => x.IsDelete == false &&
                    (x.Phone == profileUpdate.Phone) && x.Id != AppConstant.LoggedInUserID && x.TenantId == AppConstant.LoggedInTenantId);
                    if (IsPhoneExists)
                    {
                        result.Message = "Phone " + AppConstant.AlreadyExists;
                        return result;
                    }

                    //bool IsEmailExists = await AnyAsync(x => x.IsDelete == false &&
                    //  (x.Email == profileUpdate.Email) && x.Id != AppConstant.LoggedInUserID && x.TenantId == AppConstant.LoggedInTenantId);
                    //if (IsEmailExists)
                    //{
                    //    result.Message = "Email " + AppConstant.AlreadyExists;
                    //    return result;
                    //}

                    var UpdateUserDetails = UserExistsDetails;
                    if (UpdateUserDetails != null)
                    {
                        UpdateUserDetails.FirstName = profileUpdate.FirstName;
                        UpdateUserDetails.LastName = profileUpdate.LastName;
                        UpdateUserDetails.Suffix = profileUpdate.suffix;
                        //UpdateUserDetails.Username = profileUpdate.Email;
                        //UpdateUserDetails.Email = profileUpdate.Email;
                        UpdateUserDetails.Phone = profileUpdate.Phone;
                        UpdateUserDetails.ModifiedAt = DateTime.Now;
                        UpdateUserDetails.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = await Update(UpdateUserDetails);
                        result.IsSuccess = true;
                        result.Message = AppConstant.UpdateSuccessMsg;

                        CreateNotification notification2 = new CreateNotification()
                        {
                            UserId = AppConstant.LoggedInUserID,
                            Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Tenant),
                            Body = $"Tenant profile {AppConstant.Update_Notification}",
                            CreatedDate = DateTime.Now,
                            IsReaded = true
                        };
                        var result1 = notification.CreateNotification(notification2);
                    }
                    else
                    {
                        result.Message = AppConstant.NoUserFound;
                    }
                }
                else
                {
                    result.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex)
            {
                result.Message = AppConstant.ExceptionError + ex.Message;
            }
            return result;
        }

        public async Task<IResponseVm<UserResponse>> ProfileGetById()
        {
            IResponseVm<UserResponse> responseVm = new ResponseVm<UserResponse>();
            try
            {
                var address = await GetAllQuerable()
                    .Where(c => c.Id == AppConstant.LoggedInUserID && c.IsActive == true && c.IsDelete == false && c.TenantId == AppConstant.LoggedInTenantId)
                    .Select(c => new UserResponse
                    {
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        suffix=c.Suffix??"",
                        Email = c.Email,
                        UserName = c.Username,
                        Phone = c.Phone
                    })
                    .FirstOrDefaultAsync();

                if (address != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                    responseVm.Data = address;
                    responseVm.TotalRecords = 1;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> ForgetPassword(ForgetPassword forgetPassword)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            try
            {
                var user = (await GetAll())
                    .FirstOrDefault(x => x.IsActive ==true && x.IsDelete==false && x.Email == forgetPassword.Email);

                if (user == null)
                {
                    result.IsSuccess = false;
                    result.Message = AppConstant.UserNotExist;
                    return result;
                }

                var verifyCode = tenantService.GetAllQuerable()
                    .FirstOrDefault(x => x.Code == forgetPassword.Code && x.IsActive==true && x.IsDelete==false);

                if (verifyCode == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Code does not Exist.";
                    return result;
                }

                var isRelateToTenant = await tenantService.AnyAsync(x =>
                    x.IsDelete==false && x.IsActive==true &&
                    (x.TenantId == verifyCode.TenantId || x.ParentTenantId == verifyCode.TenantId) &&
                    x.TenantId == user.TenantId);

                if (!isRelateToTenant)
                {
                    result.IsSuccess = false;
                    result.Message = AppConstant.UserNotExist;
                    return result;
                }

                var token = GenerateTokenForgotPassword(user.Email ?? "", user.Id);
                if (string.IsNullOrEmpty(token))
                {
                    result.IsSuccess = false;
                    result.Message = "TokenGeneration getting Failed.";
                    return result;
                }

                //string resetLink = $"{AppConstant.UIBaseURL}reset-password/{forgetPassword.Code}/{token}";

                user.Token = token;
                user.ModifiedAt = DateTime.Now;
                await Update(user);

                var content = $"Please reset your password using the following link: <a href='{AppConstant.UIBaseURL}reset-password/{forgetPassword.Code}/{token}'>Reset Password</a>";
                EmailHelper.ForgotPassword(user.Email, content);
                result.IsSuccess = true;
                result.Message = AppConstant.ForgotPasswordSuccess;

                CreateNotification notification2 = new CreateNotification()
                {
                    UserId = AppConstant.LoggedInUserID,
                    Subject = Enum.GetName(typeof(NotificationModule), NotificationModule.Tenant),
                    Body = $"Tenant password {AppConstant.Update_Notification}",
                    CreatedDate = DateTime.Now,
                    IsReaded = true
                };
                var result1 = notification.CreateNotification(notification2);
            }
            catch (Exception ex)
            {
                result.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return result;
        }

        private string GenerateTokenForgotPassword(string Email, Guid UserId)
        {
            DateTime PasswordExpiry = DateTime.Now.AddMinutes(Convert.ToInt32(AppConstant.JwtExpiryTime));
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
        {
          new Claim("UserId",UserId.ToString()),
          new Claim("EmailId",Email.ToString()),
          new Claim("ExpiredOn",PasswordExpiry.ToString())
         };
            var token = new JwtSecurityToken(AppConstant.JwtIssuer, AppConstant.JwtAudience, claims,
                         expires: PasswordExpiry, signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string DecodeJwt(string Token, Guid UserId, string Email)
        {
            string res = "";
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(Token);
            var claims = token.Claims.Select(claim => (claim.Type, claim.Value)).ToList();
            if (claims.Any(x => x.Type == "ExpiredOn" && Convert.ToDateTime(x.Value) < DateTime.Now))
            {
                res = "Token Expired.";
                return res;
            }
            if (claims.Any(x => x.Type == "UserId" && Guid.Parse(x.Value) != UserId))
            {
                res = "Invalid User.";
                return res;
            }
            if (claims.Any(x => x.Type == "EmailId" && x.Value != Email))
            {
                res = "Invalid User Email.";
                return res;
            }
            return res;
        }

        public async Task<IResponseVm<string>> VerifyForgetPassword(VerifyPassword verifyPassword)
        {
            IResponseVm<string> result = new ResponseVm<string>();

            try
            {
                var tokenValidationResult = verifyPassword.Token;
                if (!string.IsNullOrEmpty(tokenValidationResult))
                {
                    var IsUserTokenExists = (await GetAll())
                        .Where(x => x.IsDelete == false && x.IsActive == true && x.Token == verifyPassword.Token)
                        .FirstOrDefault();
                    if (IsUserTokenExists == null)
                    {
                        result.Message = "Invalid Link.";
                        return result;
                    }
                    var res = DecodeJwt(verifyPassword.Token ?? "", IsUserTokenExists.Id, IsUserTokenExists.Email ?? "");
                    if (string.IsNullOrEmpty(res))
                    {
                        IsUserTokenExists.Token = null;
                        IsUserTokenExists.ModifiedAt = DateTime.Now;
                        IsUserTokenExists.Pwd = AesEncryptionHelper.Encrypt(verifyPassword.Password);
                        await Update(IsUserTokenExists);

                        EmailHelper.ResetPassword(IsUserTokenExists.Email);
                        result.Message = AppConstant.PasswordChangeMessage;
                    }
                    else
                    {
                        result.Message = res;
                    }
                }
                else
                {
                    result.Message = "Invalid or malformed token.";
                }
            }
            catch (Exception ex)
            {
                result.Message = AppConstant.ExceptionError + ex.Message;
            }
            return result;
        }

        public async Task<IResponseVm<string>> GenerateToken()
        {
            IResponseVm<string> result = new ResponseVm<string>();
            FileLogger.Log("Generate Token start");
            var claims = new List<Claim>
    {
        new Claim("tenant_id", AppConstant.LoggedInTenantId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    };
            FileLogger.Log("Generate Token claims",claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey);
            FileLogger.Log("AppConstant.JWTSecretKey", AppConstant.JWTSecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = AppConstant.JwtIssuer,
                Audience = AppConstant.JwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                // Expires is removed to make token never expire
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            FileLogger.Log("token", token);

            string TokenValue = tokenHandler.WriteToken(token);

            result.IsSuccess = true;
            result.Data = TokenValue;
            FileLogger.Log("IsSuccess", true);

            return result;
        }

        public IResponseVm<string> ReadTokenDetails(string token)
        {
            IResponseVm<string> result = new ResponseVm<string>();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken == null)
            {
                Console.WriteLine("Invalid token.");
                return result;
            }

            var tenantId = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;

           result.IsSuccess = true;
            result.Data = tenantId;
            return result;
        }

        public async Task<IResponseVm<Dictionary<string, string>>> GenerateLetterOnlyUniqueCodeAsync(string inputName)
        {
            IResponseVm<Dictionary<string, string>> response = new ResponseVm<Dictionary<string, string>>();

            if (string.IsNullOrWhiteSpace(inputName))
            {
                response.IsSuccess = false;
                response.Message = "Input cannot be empty.";
                return response;
            }

            // Clean input: Only letters
            string cleaned = new string(inputName.Where(char.IsLetter).ToArray());
            if (string.IsNullOrEmpty(cleaned) || cleaned.Length < 3)
            {
                response.IsSuccess = false;
                response.Message = "Input must contain at least 3 valid letters.";
                return response;
            }

            string code = "";
            string invoicePrefix = "";
            int attempt = 0;
            const int maxAttempts = 50;

            try
            {
                Random rng = new Random();

                // Try generating unique lowercase code
                while (attempt < maxAttempts)
                {
                    string shuffled = new string(cleaned.OrderBy(_ => rng.Next()).ToArray());
                    string candidateCode = shuffled.Substring(0, Math.Min(10, shuffled.Length)).ToLowerInvariant();

                    if (candidateCode.Length < 3)
                    {
                        attempt++;
                        continue;
                    }

                    bool isCodeExists = await tenantService.AnyAsync(x =>
                        x.IsActive == true && x.IsDelete == false && x.Code == candidateCode);

                    if (!isCodeExists)
                    {
                        code = candidateCode;
                        break;
                    }

                    attempt++;
                }

                if (string.IsNullOrEmpty(code))
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to generate a unique code.";
                    return response;
                }

                // Reset attempt for prefix
                attempt = 0;

                while (attempt < maxAttempts)
                {
                    string shuffled = new string(cleaned.OrderBy(_ => rng.Next()).ToArray());
                    string candidatePrefix = shuffled.Substring(0, Math.Min(10, shuffled.Length)).ToUpperInvariant();

                    if (candidatePrefix.Length < 3)
                    {
                        attempt++;
                        continue;
                    }

                    bool isPrefixExists = await dbContext.TblTenantBrandingInfos.AnyAsync(x =>
                        x.IsActive == true && x.IsDelete == false && x.InvoicePrefix == candidatePrefix);

                    string lowerPrefix = candidatePrefix.ToLowerInvariant();

                    bool conflicts = lowerPrefix == code ||
                                     code.StartsWith(lowerPrefix.Substring(0, 3)) ||
                                     code.EndsWith(lowerPrefix.Substring(lowerPrefix.Length - 3, 3));

                    if (!isPrefixExists && !conflicts)
                    {
                        invoicePrefix = candidatePrefix;
                        break;
                    }

                    attempt++;
                }

                if (string.IsNullOrEmpty(invoicePrefix))
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to generate a valid unique invoice prefix.";
                    return response;
                }

                response.IsSuccess = true;
                response.Message = "Unique codes generated successfully.";
                response.Data = new Dictionary<string, string>
        {
            { "Code", code },
            { "InvoicePrefix", invoicePrefix }
        };
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error occurred while generating codes: {ex.Message}";
            }

            return response;
        }








    }



}



