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

    }
}
