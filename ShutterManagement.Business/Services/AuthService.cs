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
using ShutterManagement.Business;
using MimeKit.Cryptography;

namespace ShutterManagement.Services
{
    public class AuthService : BaseRepository<ShutterDBContext, TblUser>, IAuthService
    {
        #region Private Constants
        private const int MIN_CODE_LENGTH = 3;
        private const int MAX_CODE_LENGTH = 10;
        private const int MAX_GENERATION_ATTEMPTS = 50;
        private const string LOGIN_COMPANY_CODE = "login";
        private readonly EmailSendHelperService emailSendHelperService;
        #endregion

        #region Private Fields
        private readonly DataServices _dataServices;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly ITenantService _tenantService;
        private readonly ITenantBrandingService _tenantBrandingService;
        private readonly INotificationService _notification;
        private readonly ClaimsProperty _claimsProperty;
        private readonly IGHLConfiguration _ghlConfiguration;
        private readonly IStripeConfiguration _stripeConfiguration;
        private readonly IUserService _userService;
        #endregion

        #region Constructor
        public AuthService(
            EmailSendHelperService emailSendHelperService,
            DataServices dataServices, 
            IMapper mapper, 
            IConfiguration config,
            ITenantService tenantService, 
            ITenantBrandingService tenantBrandingService,
            INotificationService notification,
            ClaimsProperty claimsProperty,
            IGHLConfiguration ghlConfiguration,
            IStripeConfiguration stripeConfiguration, 
            IUserService userService)
        {
            this.emailSendHelperService = emailSendHelperService;
            _dataServices = dataServices ?? throw new ArgumentNullException(nameof(dataServices));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _tenantBrandingService = tenantBrandingService ?? throw new ArgumentNullException(nameof(tenantBrandingService));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _claimsProperty = claimsProperty ?? throw new ArgumentNullException(nameof(claimsProperty));
            _ghlConfiguration = ghlConfiguration ?? throw new ArgumentNullException(nameof(ghlConfiguration));
            _stripeConfiguration = stripeConfiguration ?? throw new ArgumentNullException(nameof(stripeConfiguration));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        #endregion

        #region Public Methods
        public async Task<IResponseVm<UserLoginDetails>> LoginAsync(LoginDetailVM user, bool? IsImpersonate = false)
        {
            var responseVm = new ResponseVm<UserLoginDetails>();

            try
            {
                // Validate input
                var validationResult = ValidateLoginInput(user);
                if (!validationResult.IsValid)
                {
                    responseVm.Message = validationResult.ErrorMessage;
                    return responseVm;
                }

                // Handle special login case and get company code
                var companyCode = await GetValidCompanyCodeAsync(user);
                if (string.IsNullOrEmpty(companyCode))
                {
                    responseVm.Message = AppConstant.InvalidCred;
                    return responseVm;
                }

                // Get user details
                var userDetails = await GetUserLoginDetailsAsync(user.Username, user.Password, companyCode, IsImpersonate);
                if (!userDetails.Any())
                {
                    responseVm.Message = AppConstant.InvalidCred;
                    return responseVm;
                }

                var loginDetail = userDetails.First();

                // Get user roles and create token
                var userRoles = await GetUserRolesAsync(loginDetail.UserId);
                var token = CreateAuthenticationToken(loginDetail, userRoles);

                // Build response
                var loginResponse = BuildLoginResponse(loginDetail, userRoles, token);
                
                responseVm.Data = loginResponse;
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.LoginSuccessMessage;
                responseVm.TotalRecords = 1;

                // Initialize configurations
              //  await _ghlConfiguration.GetGHLConfiguration(loginDetail.TenantId.ToString());
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<HomePageContent>> HomePageContent(string companyCode)
        {
            var responseVm = new ResponseVm<HomePageContent>();

            try
            {
                if (string.IsNullOrWhiteSpace(companyCode))
                {
                    responseVm.Message = "Company code required.";
                    return responseVm;
                }

                var tenantData = await GetTenantByCodeAsync(companyCode);
                if (tenantData == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var brandInfo = await GetTenantBrandingInfoAsync(tenantData.BrandingInfo);
                if (brandInfo == null)
                {
                    responseVm.Message = "Branding information not found.";
                    return responseVm;
                }

                var homePageContent = CreateHomePageContent(tenantData, brandInfo);
                
                responseVm.IsSuccess = true;
                responseVm.Data = homePageContent;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

                return responseVm;
        }

        public async Task<IResponseVm<PasswordChangeVm>> ResetPassWord(PasswordChangeVm vm)
        {
            var result = new ResponseVm<PasswordChangeVm>();

            try
            {
                if (vm == null || string.IsNullOrWhiteSpace(vm.OldPassWord) || string.IsNullOrWhiteSpace(vm.PassWord))
                {
                    result.Message = "Invalid password change request.";
                    return result;
                }

                var user = await GetUserForPasswordChangeAsync(vm.OldPassWord);
                if (user == null)
                {
                    result.Message = AppConstant.NoUserFound;
                    return result;
                }

                await UpdateUserPasswordAsync(user, vm.PassWord);
                
                result.IsSuccess = true;
                result.Message = AppConstant.PasswordChangeMessage;
            }
            catch (Exception ex)
            {
                result.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return result;
        }

        public async Task<IResponseVm<string>> ProfileUpdate(ProfileUpdateVm profileUpdate)
        {
            var result = new ResponseVm<string>();

            try
            {
                if (profileUpdate == null)
                {
                    result.Message = "Profile data is required.";
                    return result;
                }

                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    result.Message = AppConstant.NoUserFound;
                    return result;
                }

                // Validate phone uniqueness
                if (await IsPhoneAlreadyExistsAsync(profileUpdate.Phone, user.Id))
                {
                    result.Message = "Phone " + AppConstant.AlreadyExists;
                    return result;
                }

                await UpdateUserProfileAsync(user, profileUpdate);
                await CreateNotificationAsync(NotificationModule.Tenant, $"Tenant profile {AppConstant.Update_Notification}");

                result.IsSuccess = true;
                result.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                result.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return result;
        }

        public async Task<IResponseVm<UserResponse>> ProfileGetById()
        {
            var responseVm = new ResponseVm<UserResponse>();

            try
            {
                var userProfile = await GetUserProfileAsync();
                if (userProfile == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                return responseVm;
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.Data = userProfile;
                responseVm.TotalRecords = 1;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

                return responseVm;
        }

        public async Task<IResponseVm<string>> ForgetPassword(ForgetPassword forgetPassword)
        {
            var result = new ResponseVm<string>();

            try
            {
                if (forgetPassword == null || string.IsNullOrWhiteSpace(forgetPassword.Email) || string.IsNullOrWhiteSpace(forgetPassword.TenantId))
                {
                    result.Message = "Email and code are required.";
                    return result;
                }

                var user = await GetUserByEmailAsync(forgetPassword.Email, forgetPassword.TenantId);
                if (user == null)
                {
                    result.Message = AppConstant.UserNotExist;
                    return result;
                }

                //var isValidCode = await ValidateCompanyCodeAsync(forgetPassword.Code, user.TenantId ?? Guid.Empty);
                //if (!isValidCode)
                //{
                //    result.Message = "Invalid company code.";
                //    return result;
                //}

                var token = GeneratePasswordResetToken(user.Email, user.Id);
                if (string.IsNullOrEmpty(token))
                {
                    result.Message = "Failed to generate reset token.";
                    return result;
                }

                await UpdateUserTokenAsync(user, token);
                var emailResult = await SendPasswordResetEmailAsync(user.Email, forgetPassword.TenantId, token, forgetPassword.Code);
                
                if (!emailResult.IsSuccess)
                {
                    result.Message = emailResult.Message;
                    return result;
                }
                
                await CreateNotificationAsync(NotificationModule.Tenant, $"Password {AppConstant.Update_Notification}",user.Id);

                result.IsSuccess = true;
                result.Message = AppConstant.ForgotPasswordSuccess;
            }
            catch (Exception ex)
            {
                result.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return result;
        }

        public async Task<IResponseVm<string>> VerifyForgetPassword(VerifyPassword verifyPassword)
        {
            var result = new ResponseVm<string>();

            try
            {
                if (verifyPassword == null || string.IsNullOrWhiteSpace(verifyPassword.Token) || string.IsNullOrWhiteSpace(verifyPassword.Password))
                {
                    result.Message = "Token and password are required.";
                    return result;
                }

                var user = await GetUserByTokenAsync(verifyPassword.Token);
                if (user == null)
                {
                    result.Message = "Invalid or expired link.";
                    return result;
                }

                var tokenValidation = ValidatePasswordResetToken(verifyPassword.Token, user.Id, user.Email);
                if (!string.IsNullOrEmpty(tokenValidation))
                {
                    result.Message = tokenValidation;
                    return result;
                }

                await ResetUserPasswordAsync(user, verifyPassword.Password);
                //await emailSendHelperService.GetEmailConfigAndSendEmail(user.Email, AppConstant.Con_ResetPassword, AppConstant.Sub_ForgotPassword);

                result.IsSuccess = true;
                result.Message = AppConstant.PasswordChangeMessage;
            }
            catch (Exception ex)
            {
                result.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return result;
        }

        public async Task<IResponseVm<string>> GenerateToken()
        {
            var result = new ResponseVm<string>();

            try
            {
                FileLogger.Log("Generate Token start");
                
                var token = CreateNonExpiringToken();
                await UpdateGHLConfigurationTokenAsync(token);

                result.IsSuccess = true;
                result.Data = token;
                
                FileLogger.Log("Token generated successfully", true);
            }
            catch (Exception ex)
            {
                result.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                FileLogger.Log("Token generation failed", ex.Message);
            }

            return result;
        }

        public IResponseVm<string> ReadTokenDetails(string token)
        {
            var result = new ResponseVm<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    result.Message = "Token is required.";
                    return result;
                }

                var tenantId = ExtractTenantIdFromToken(token);
                if (string.IsNullOrEmpty(tenantId))
                {
                    result.Message = "Invalid token format.";
                    return result;
                }

                result.IsSuccess = true;
                result.Data = tenantId;
            }
            catch (Exception ex)
            {
                result.Message = $"Error reading token: {ex.Message}";
            }

            return result;
        }

        public async Task<IResponseVm<Dictionary<string, string>>> GenerateLetterOnlyUniqueCodeAsync(string inputName)
        {
            var response = new ResponseVm<Dictionary<string, string>>();

            try
            {
                if (string.IsNullOrWhiteSpace(inputName))
                {
                    response.Message = "Input name cannot be empty.";
                    return response;
                }

                var cleanedInput = CleanInputForCodeGeneration(inputName);
                if (cleanedInput.Length < MIN_CODE_LENGTH)
                {
                    response.Message = $"Input must contain at least {MIN_CODE_LENGTH} valid letters.";
                    return response;
                }

                var generatedCode = await GenerateUniqueCodeAsync(cleanedInput);
                var generatedPrefix = await GenerateUniqueInvoicePrefixAsync(cleanedInput, generatedCode);

                if (string.IsNullOrEmpty(generatedCode) || string.IsNullOrEmpty(generatedPrefix))
                {
                    response.Message = "Failed to generate unique codes after maximum attempts.";
                    return response;
                }

                response.IsSuccess = true;
                response.Message = "Unique codes generated successfully.";
                response.Data = new Dictionary<string, string>
                {
                    { "Code", generatedCode },
                    { "InvoicePrefix", generatedPrefix }
                };
            }
            catch (Exception ex)
            {
                response.Message = $"Error generating codes: {ex.Message}";
            }

            return response;
        }
        #endregion

        #region Private Helper Methods
        private static (bool IsValid, string ErrorMessage) ValidateLoginInput(LoginDetailVM user)
        {
            if (user == null)
                return (false, "Login data is required.");
            
            if (string.IsNullOrWhiteSpace(user.Username))
                return (false, "Username required.");
            
            if (string.IsNullOrWhiteSpace(user.Password))
                return (false, "Password required.");
            
            if (string.IsNullOrWhiteSpace(user.CompanyCode))
                return (false, "CompanyCode required.");

            return (true, string.Empty);
        }

        private async Task<string> GetValidCompanyCodeAsync(LoginDetailVM user)
        {
            if (user.CompanyCode != LOGIN_COMPANY_CODE)
                return user.CompanyCode;

            var hashPassword = AesEncryptionHelper.Encrypt(user.Password);
            var tenantId = await _userService.GetTenantIdByUserEmailAndPwd(user.Username, hashPassword);
            
            if (string.IsNullOrEmpty(tenantId))
                return null;

            var tenant = await _tenantService.GetAllQuerable()
                .Where(x => x.TenantId.ToString() == tenantId && x.IsActive == true && x.IsDelete != true)
                .FirstOrDefaultAsync();

            return tenant?.Code;
        }

        private async Task<List<LoginDetails>> GetUserLoginDetailsAsync(string username, string password, string companyCode, bool? IsImpersonate = false)
        {
            var hashPassword = "";
            hashPassword = IsImpersonate == true ? password : AesEncryptionHelper.Encrypt(password);

            return await _dataServices.CallSPtoRead<LoginDetails>("Sp_GetUserDetailsOnLogin", reader =>
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
                    IsGHLConfigSameAsTenant = reader.GetBoolean(reader.GetOrdinal("IsGHLConfigSameAsTenant"))
                };
            },
            new SqlParameter("@Username", SqlDbType.NVarChar, 100) { SqlValue = username },
            new SqlParameter("@Password", SqlDbType.NVarChar, 100) { SqlValue = hashPassword },
            new SqlParameter("@CompanyCode", SqlDbType.NVarChar, 100) { SqlValue = companyCode },
            new SqlParameter("@Result", SqlDbType.VarChar, 1000) { Direction = ParameterDirection.Output });
        }

        private async Task<List<UserRoles>> GetUserRolesAsync(Guid userId)
        {
            return await _dataServices.CallSPtoRead<UserRoles>("Sp_GetLoggedinUserRoles", reader =>
            {
                return new UserRoles
                    {
                        PermissionId = reader.GetGuid(reader.GetOrdinal("Id")),
                        PermissionName = reader.GetString(reader.GetOrdinal("Name")),
                        PermissionDisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                        ModuleDisplayName = reader.GetString(reader.GetOrdinal("ModuleDisplayName")),
                        Icon = reader.GetString(reader.GetOrdinal("Icon")),
                        Url = reader.GetString(reader.GetOrdinal("Url"))
                    };
                },
            new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { SqlValue = userId });
        }
                
        private string CreateAuthenticationToken(LoginDetails userDetail, List<UserRoles> userRoles)
        {
                var claims = new ClaimsIdentity();
            
            // Add role claims
            foreach (var role in userRoles)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, role.PermissionName));
            }

            // Add user claims
            var isTenant = string.IsNullOrEmpty(userDetail.ParentTenantId);
            
            claims.AddClaim(new Claim("UserId", userDetail.UserId.ToString().ToUpper()));
            claims.AddClaim(new Claim("Username", userDetail.Username ?? string.Empty));
            claims.AddClaim(new Claim(ClaimTypes.Name, $"{userDetail.FirstName} {userDetail.MiddleName} {userDetail.LastName}".Trim()));
            claims.AddClaim(new Claim(ClaimTypes.Email, userDetail.Email ?? string.Empty));
            claims.AddClaim(new Claim("Phone", userDetail.Phone ?? string.Empty));
            claims.AddClaim(new Claim("TenantId", userDetail.TenantId.ToString().ToUpper()));
            claims.AddClaim(new Claim("ParentTenantId", userDetail.ParentTenantId?.ToUpper() ?? string.Empty));
            claims.AddClaim(new Claim("CompanyCode", userDetail.Code));
            claims.AddClaim(new Claim("CompanyName", userDetail.CompanyName));
            claims.AddClaim(new Claim("IsTenant", isTenant.ToString()));
               
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
            return tokenHandler.WriteToken(token);
        }

        private UserLoginDetails BuildLoginResponse(LoginDetails userDetail, List<UserRoles> userRoles, string token)
        {
            var loginDetails = _mapper.Map<UserLoginDetails>(userDetail);
            
            loginDetails.Roles = userRoles.Select(x => x.PermissionName).ToArray();
            loginDetails.Menu = userRoles
                    .Select(x => new LoginRoles
                    {
                        RoleName = RoleNameHelper.CleanRoleName(x.PermissionName),
                        RoleDisplayName = x.ModuleDisplayName,
                        Icon = x.Icon,
                        Url = x.Url
                    })
                    .DistinctBy(x => x.RoleName)
                    .Where(x => !string.IsNullOrWhiteSpace(x.RoleName))
                    .ToArray();
             
            loginDetails.IsGHLConfigSameAsTenant = userDetail.IsGHLConfigSameAsTenant;
            loginDetails.Token = token;
            loginDetails.IsTenant = string.IsNullOrEmpty(userDetail.ParentTenantId);

            return loginDetails;
        }

        private async Task<TblTenant> GetTenantByCodeAsync(string companyCode)
        {
            return await _tenantService.GetAllQuerable()
                .Where(x => x.Code == companyCode && x.IsDelete != true && x.IsActive == true)
                .FirstOrDefaultAsync();
        }

        private async Task<TblTenantBrandingInfo> GetTenantBrandingInfoAsync(Guid? brandingInfoId)
        {
            if (!brandingInfoId.HasValue)
                return null;

            return await _tenantBrandingService.GetAllQuerable()
                .Where(x => x.Id == brandingInfoId.Value)
                .FirstOrDefaultAsync();
        }

        private static HomePageContent CreateHomePageContent(TblTenant tenant, TblTenantBrandingInfo brandInfo)
        {
            return new HomePageContent
            {
                Code = tenant.Code,
                URL = tenant.Url,
                FirstName = tenant.FirstName,
                LastName = tenant.LastName,
                suffix = tenant.Suffix,
                MiddleName = tenant.MiddleName,
                        Email = brandInfo.Email,
                        PhoneNumber = brandInfo.PhoneNumber,
                        Logo = brandInfo.Logo,
                        PrimaryColor = brandInfo.PrimaryColor,
                        SecondaryColor = brandInfo.SecondaryColor,
                        CompanyName = brandInfo.CompanyName,
                    };
        }

        private async Task<TblUser> GetUserForPasswordChangeAsync(string oldPassword)
        {
            var hashPassword = AesEncryptionHelper.Encrypt(oldPassword);
            
            return await GetAllQuerable()
                .Where(x => x.IsDelete != true && x.IsActive == true && 
                           x.Id == _claimsProperty.GetUserId() && 
                           x.Pwd == hashPassword && 
                           x.TenantId == _claimsProperty.GetTenantId())
                .FirstOrDefaultAsync();
        }

        private async Task UpdateUserPasswordAsync(TblUser user, string newPassword)
        {
            user.Pwd = AesEncryptionHelper.Encrypt(newPassword);
            user.ModifiedAt = DateTime.Now;
            user.ModifiedBy = _claimsProperty.GetUserId();
            user.ExpireAt = null;
            user.Token = null;
            
            await Update(user);
        }

        private async Task<TblUser> GetCurrentUserAsync()
        {
            return await GetAllQuerable()
                .Where(x => x.IsDelete != true && x.IsActive == true && 
                           x.Id == _claimsProperty.GetUserId() && 
                           x.TenantId == _claimsProperty.GetTenantId())
                .FirstOrDefaultAsync();
        }

        private async Task<bool> IsPhoneAlreadyExistsAsync(string phone, Guid currentUserId)
        {
            return await AnyAsync(x => x.IsDelete != true && 
                                     x.Phone == phone && 
                                     x.Id != currentUserId && 
                                     x.TenantId == _claimsProperty.GetTenantId());
        }

        private async Task UpdateUserProfileAsync(TblUser user, ProfileUpdateVm profileUpdate)
        {
            user.FirstName = profileUpdate.FirstName;
            user.LastName = profileUpdate.LastName;
            user.Suffix = profileUpdate.suffix;
            user.Phone = profileUpdate.Phone;
            user.ModifiedAt = DateTime.Now;
            user.ModifiedBy = _claimsProperty.GetUserId();
            
            await Update(user);
        }

        private async Task CreateNotificationAsync(NotificationModule module, string body, Guid? userId = null)
        {
            var notification = new CreateNotification
            {
                UserId = userId == Guid.Empty || userId == null ? _claimsProperty.GetUserId() : userId,
                Subject = Enum.GetName(typeof(NotificationModule), module),
                Body = body,
                CreatedDate = DateTime.Now,
                IsReaded = true
            };

            await _notification.CreateNotification(notification);
        }
        private async Task<UserResponse> GetUserProfileAsync()
        {
            return await GetAllQuerable()
                .Where(c => c.Id == _claimsProperty.GetUserId() && c.IsActive == true && c.IsDelete != true && 
                           c.TenantId == _claimsProperty.GetTenantId())
                    .Select(c => new UserResponse
                    {
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                    suffix = c.Suffix ?? string.Empty,
                        Email = c.Email,
                        UserName = c.Username,
                        Phone = c.Phone
                    })
                    .FirstOrDefaultAsync();
        }

        private async Task<TblUser> GetUserByEmailAsync(string email, string TenantId)
        {
            return await GetAllQuerable()
                .Where(x => x.IsActive == true && x.IsDelete != true && x.Email == email && x.TenantId.ToString() == TenantId)
                .FirstOrDefaultAsync();
        }

        private string GeneratePasswordResetToken(string email, Guid userId)
        {
            var passwordExpiry = DateTime.Now.AddMinutes(Convert.ToInt32(AppConstant.JwtExpiryTime));
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
        {
                new Claim("UserId", userId.ToString()),
                new Claim("EmailId", email),
                new Claim("ExpiredOn", passwordExpiry.ToString())
            };
            
            var token = new JwtSecurityToken(
                AppConstant.JwtIssuer, 
                AppConstant.JwtAudience, 
                claims,
                expires: passwordExpiry, 
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task UpdateUserTokenAsync(TblUser user, string token)
        {
            user.Token = token;
            user.ModifiedAt = DateTime.Now;
            await Update(user);
        }

        private async Task<(bool IsSuccess, string Message)> SendPasswordResetEmailAsync(string email, string TenantId, string token, string code = "")
        {
            var resetLink = $"{AppConstant.UIBaseURL}reset-password/{code}/{token}";
            var content = $"Please reset your password using the following link: <a href='{resetLink}'>Reset Password</a>";
            var emailResult = await emailSendHelperService.GetEmailConfigAndSendEmail(email, content, AppConstant.Sub_ForgotPassword,string.Empty,null,TenantId);
            
            return (emailResult.IsSuccess, emailResult.Message ?? "Email sending failed");
        }

        private async Task<TblUser> GetUserByTokenAsync(string token)
        {
            return await GetAllQuerable()
                .Where(x => x.IsDelete != true && x.IsActive == true && x.Token == token)
                .FirstOrDefaultAsync();
        }

        private string ValidatePasswordResetToken(string token, Guid userId, string email)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var claims = jwtToken.Claims.Select(claim => (claim.Type, claim.Value)).ToList();

                if (claims.Any(x => x.Type == "ExpiredOn" && Convert.ToDateTime(x.Value) < DateTime.Now))
                    return "Token expired.";

                if (claims.Any(x => x.Type == "UserId" && Guid.Parse(x.Value) != userId))
                    return "Invalid user.";

                if (claims.Any(x => x.Type == "EmailId" && x.Value != email))
                    return "Invalid user email.";

                return string.Empty;
            }
            catch
            {
                return "Invalid token format.";
            }
        }

        private async Task ResetUserPasswordAsync(TblUser user, string newPassword)
        {
            user.Token = null;
            user.ModifiedAt = DateTime.Now;
            user.Pwd = AesEncryptionHelper.Encrypt(newPassword);
            
            await Update(user);
        }

        private string CreateNonExpiringToken()
        {
            var claims = new List<Claim>
    {
                new Claim("tenant_id", _claimsProperty.GetTenantId().ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = AppConstant.JwtIssuer,
                Audience = AppConstant.JwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                // Expires is intentionally omitted to make token never expire
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task UpdateGHLConfigurationTokenAsync(string token)
        {
            var ghlConfig = await _ghlConfiguration.GetAllQuerable()
                .Where(x => x.TenantId == _claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete != true)
                .FirstOrDefaultAsync();

            if (ghlConfig != null)
            {
                ghlConfig.GeneratedToken = token;
                await _ghlConfiguration.Update(ghlConfig);
            }
        }

        private static string ExtractTenantIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            }
            catch
            {
                return null;
            }
        }

        private static string CleanInputForCodeGeneration(string input)
        {
            return new string(input.Where(char.IsLetter).ToArray());
        }

        private async Task<string> GenerateUniqueCodeAsync(string cleanedInput)
        {
            var rng = new Random();
            
            for (int attempt = 0; attempt < MAX_GENERATION_ATTEMPTS; attempt++)
            {
                var shuffled = new string(cleanedInput.OrderBy(_ => rng.Next()).ToArray());
                var candidateCode = shuffled.Substring(0, Math.Min(MAX_CODE_LENGTH, shuffled.Length)).ToLowerInvariant();

                if (candidateCode.Length < MIN_CODE_LENGTH)
                        continue;

                var codeExists = await _tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete != true && x.Code == candidateCode);
                if (!codeExists)
                    return candidateCode;
            }

            return null;
        }

        private async Task<string> GenerateUniqueInvoicePrefixAsync(string cleanedInput, string excludeCode)
        {
            var rng = new Random();
            
            for (int attempt = 0; attempt < MAX_GENERATION_ATTEMPTS; attempt++)
            {
                var shuffled = new string(cleanedInput.OrderBy(_ => rng.Next()).ToArray());
                var candidatePrefix = shuffled.Substring(0, Math.Min(MAX_CODE_LENGTH, shuffled.Length)).ToUpperInvariant();

                if (candidatePrefix.Length < MIN_CODE_LENGTH)
                    continue;

                var prefixExists = await dbContext.TblTenantBrandingInfos
                    .AnyAsync(x => x.IsActive == true && x.IsDelete != true && x.InvoicePrefix == candidatePrefix);

                if (prefixExists || HasCodeConflict(candidatePrefix.ToLowerInvariant(), excludeCode))
                    continue;

                return candidatePrefix;
            }

            return null;
        }

        private static bool HasCodeConflict(string prefix, string code)
        {
            return prefix == code ||
                   code.StartsWith(prefix.Substring(0, MIN_CODE_LENGTH)) ||
                   code.EndsWith(prefix.Substring(prefix.Length - MIN_CODE_LENGTH, MIN_CODE_LENGTH));
        }
        #endregion

        public async Task<IResponseVm<List<CompanyCodeResponse>>> GetCompanyCodesByEmail(string email)
        {
            var result = new ResponseVm<List<CompanyCodeResponse>>();

            try
            {
                var companyCodes = new List<CompanyCodeResponse>();

                if (!string.IsNullOrEmpty(email))
                {
                    // Get all users with this email and their company codes
                    var users = await GetAllQuerable()
                        .Where(u => u.Email.ToLower() == email.ToLower() && u.TenantId.HasValue)
                        .Select(u => u.TenantId.Value)
                        .Distinct()
                        .ToListAsync();

                    if (users.Any())
                    {
                        // Get tenants for these users
                        var tenants = await _tenantService.GetAllQuerable()
                            .Include(t => t.BrandingInfoNavigation)
                            .Where(t => users.Contains(t.TenantId) && t.IsActive == true && t.IsDelete == false)
                            .ToListAsync();

                        companyCodes = tenants
                            .Where(t => !string.IsNullOrEmpty(t.Code))
                            .Select(t => new CompanyCodeResponse
                            {
                                Code = t.Code,
                                Name = t.BrandingInfoNavigation?.CompanyName ?? t.Code,
                                TenantId = t.TenantId.ToString()
                            })
                            .DistinctBy(c => c.Code)
                            .ToList();
                    }
                }

                result.IsSuccess = true;
                result.Data = companyCodes;
                result.Message = companyCodes.Any() 
                    ? "Company codes retrieved successfully." 
                    : "No company codes found for this email.";
            }
            catch (Exception ex)
            {
                result.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return result;
        }

        public async Task<IResponseVm<UserLoginDetails>> ImpersonateTenant(string SubTenantId)
        {
            var responseVm = new ResponseVm<UserLoginDetails>();

            try
            {
                if (string.IsNullOrWhiteSpace(SubTenantId))
                {
                    responseVm.Message = "Tenant ID and Parent Tenant ID are required.";
                    return responseVm;
                }

                var ValidPartner = _tenantService.GetAllQuerable().Where(u => u.ParentTenantId == _claimsProperty.GetTenantId() && u.TenantId.ToString().ToLower() == SubTenantId.ToLower() && u.IsActive == true && u.IsDelete == false).FirstOrDefault();

                if (ValidPartner == null)
                {
                    responseVm.Message = $"Invalid or {AppConstant.TenantNotFound}";
                    return responseVm;
                }

                var PartnerTenantAdmin = GetAllQuerable()
                    .Where(t => t.TenantId.ToString().ToLower() == SubTenantId.ToLower() && t.IsTenantAdmin == true && t.IsActive == true && t.IsDelete == false).FirstOrDefault();
                if (PartnerTenantAdmin == null)
                {
                    responseVm.Message = "No tenant admin found for the specified tenant.";
                    return responseVm;
                }

                var loginDetail = new LoginDetailVM
                {
                    Username = PartnerTenantAdmin.Username,
                    Password = PartnerTenantAdmin.Pwd,
                    CompanyCode = ValidPartner.Code
                };
                var userLoginResponse = await LoginAsync(loginDetail, true);
                if (!userLoginResponse.IsSuccess)
                {
                    responseVm.Message = userLoginResponse.Message;
                    return responseVm;
                }

                responseVm.IsSuccess = userLoginResponse.IsSuccess;
                responseVm.Data = userLoginResponse.Data;
                responseVm.Message = userLoginResponse.Message;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }
    }
}



