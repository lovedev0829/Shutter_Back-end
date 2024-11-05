using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using System;
using System.Threading.Tasks;

namespace ShutterManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] LoginVm loginVm)
        {
            IResponseVm<LoginVm> res = new ResponseVm<LoginVm>();

            if (loginVm == null)
            {
                res.Message = "Invalid user data.";
            }

            var result = await _userService.CreateUserAsync(loginVm);
            res.IsSuccess = true;
            res.Message = "Successfully Added.";
            return Ok(res);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers(string code)
        {
            IResponseVm<List<LoginVm>> res = new ResponseVm<List<LoginVm>>(); 

            try
            {
                var users = await _userService.GetAllUsersAsync(code);

                if (users == null || !users.Any())
                {
                    res.IsSuccess = false;
                    res.Message = "No users found for the provided code.";
                    res.Data = new List<LoginVm>();
                }
                else
                {
                    res.IsSuccess = true;
                    res.Message = "Fetched Successfully";
                    res.Data = users; 
                }
            }
            catch (Exception ex)
            {
                res.IsSuccess = false;
                res.Message = $"Error: {ex.Message}";
                res.Data = new List<LoginVm>(); 
            }

            return Ok(res); 
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id, string code)
        {
            IResponseVm<LoginVm> res = new ResponseVm<LoginVm>();
            var user = await _userService.GetUserByIdAsync(id,code);

            if (user == null)
            {
                res.Message = $"{AppConstant.ModelError}";
            }
            res.IsSuccess= true;
            res.Message = $"{AppConstant.FetchSuccessMsg}";
            res.Data = user;
            return Ok(res);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] LoginVm loginVm)
        {
            IResponseVm<LoginVm> res = new ResponseVm<LoginVm>();

            if (loginVm == null)
            {
                res.Message = "Invalid user data.";
            }

            var result = await _userService.UpdateUserAsync(id, loginVm);
            res.IsSuccess = true;
            res.Message = $"{AppConstant.UpdateSuccessMsg}";
            return Ok(new { message = result });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id, string code)
        {
            IResponseVm<LoginVm> res = new ResponseVm<LoginVm>();

            var result = await _userService.DeleteUserAsync(id, code);
            res.IsSuccess = true;
            res.Message = $"{AppConstant.DeletedSuccessMsg}";
            return Ok(new { message = result });
        }

        [HttpPut("inactive/{id}")]
        public async Task<IActionResult> SetUserInactive(Guid id, string code)
        {
            IResponseVm<LoginVm> res = new ResponseVm<LoginVm>();

            var result = await _userService.SetUserInactiveAsync(id, code);
            res.IsSuccess = true;
            res.Message = "Inactive user added";
            return Ok(new { message = result });
        }
    }
}
