using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
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

        [HttpPost("Create")]
        [Authorize(Roles = "UsersAdd")]
        public async Task<IActionResult> Create([FromBody] CreateUser UserDetails)
        {
            var result = await _userService.Create(UserDetails);
            return Ok(result);
        }

        [HttpPost("Update")]
        [Authorize(Roles = "UsersEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateUser UserDetails)
        {
            var result = await _userService.Update(UserDetails);
            return Ok(result);
        }

        [HttpPost("Delete")]
        [Authorize(Roles = "UsersDelete")]
        public async Task<IActionResult> Delete(string UserId)
        {
            var result = await _userService.Delete(UserId);
            return Ok(result);
        }

        [HttpPost("ActiveInActive")]
        [Authorize(Roles = "UsersEdit")]
        public async Task<IActionResult> ActiveInactive(string UserId)
        {
            var result = await _userService.ActiveInactive(UserId);
            return Ok(result);
        }

        
        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "UsersView")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            filter.TenantId = AppConstant.LoggedInTenantId.ToString();
            var res = await _userService.GetAll(filter);
            return Ok(res);
        }
    }
}
