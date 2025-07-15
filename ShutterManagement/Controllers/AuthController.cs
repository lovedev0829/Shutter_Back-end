using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace ShutterManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDetailVM loginVm)
        {
            var UserDetails = await _authService.LoginAsync(loginVm);
            return Ok(UserDetails);
        }

        [HttpGet("HomePageContent")]
        public async Task<IActionResult> HomePageContant(string CompanyCode = "")
        {
            var UserDetails = await _authService.HomePageContent(CompanyCode);
            return Ok(UserDetails);
        }
        [Authorize]
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(PasswordChangeVm passwordChange)
        {
            var passwordDetails = await _authService.ResetPassWord(passwordChange);
            return Ok(passwordDetails);
        }
        [Authorize]
        [HttpPost]
        [Route("ProfileUpdate")]
        public async Task<IActionResult> ProfileUpdate(ProfileUpdateVm vm)
        {
            var profileUpdateDetails = await _authService.ProfileUpdate(vm);
            return Ok(profileUpdateDetails);
        }
        [Authorize]
        [HttpPost]
        [Route("ProfileDetails")]
        public async Task<IActionResult> ProfileGetById()
        {
            var profileGetById = await _authService.ProfileGetById();
            return Ok(profileGetById);
        }
        [HttpGet]
        [Route("GetCompanyCodesByEmail")]
        public async Task<IActionResult> GetCompanyCodesByEmail(string email = "")
        {
            var companyCodes = await _authService.GetCompanyCodesByEmail(email);
            return Ok(companyCodes);
        }

        [HttpPost]
        [Route("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPassword forgetPassword)
        {
            var ForgetPassword = await _authService.ForgetPassword(forgetPassword);
            return Ok(ForgetPassword);
        }

        [HttpPost]
        [Route("VerifyForgetPassword")]
        public async Task<IActionResult> VerifyForgetPassword(VerifyPassword model)
        {
            var ForgetPassword = await _authService.VerifyForgetPassword(model);
            return Ok(ForgetPassword);
        }
    }
}
