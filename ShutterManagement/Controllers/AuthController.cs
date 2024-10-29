using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels; 
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
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
        public async Task<IActionResult> Login([FromBody] LoginVm loginVm)
        {
            IResponseVm<LoginVm> res = new ResponseVm<LoginVm>();
            if (!ModelState.IsValid)
            {
                res.Message = $"{AppConstant.ModelError}";
            }

            var user = new TblUser
            {
                Username = loginVm.Username,
                Pwd = loginVm.Pwd
            };

            try
            {
                var token = await _authService.LoginAsync(user);
                res.IsSuccess = true;
                res.Message = "User is authenticated";
                res.Data = new LoginVm { Token = token };
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                res.Message = ex.Message;   
            }
            catch (ArgumentException ex)
            {
                res.Message = $"Failed to login {ex.Message}";
            }
            return Ok(res);
        }
      
    }
}
