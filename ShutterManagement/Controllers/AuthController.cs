using Microsoft.AspNetCore.Mvc;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new TblUser
            {
                Username = loginVm.Username,
                Pwd = loginVm.Pwd
            };

            try
            {
                var token = await _authService.LoginAsync(user);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
