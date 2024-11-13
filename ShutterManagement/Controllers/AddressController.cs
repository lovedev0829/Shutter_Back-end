using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Services;

namespace ShutterManagement.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    { 
        private readonly IAddress _address;

        public AddressController(IAddress address)
        {
            _address = address;
        }

        [HttpPost]
        [Route("Create")]
        //[Authorize(Roles = "AddressAdd")]
        public async Task<IActionResult> Create([FromBody] CreateAddress addressVm)
        {
            var result = await _address.Create_Address(addressVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        //[Authorize(Roles = "AddressUpdate")]
        public async Task<IActionResult> Update([FromBody] UpdateAddress address)
        {
            var result = await _address.Update_Address(address);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        //[Authorize(Roles = "AddressDelete")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var result = await _address.DeleteAddress(Id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAll")]
        //[Authorize(Roles = "AddressViewAll")]
        public async Task<IActionResult> GetAllAddresses()
        {
            var result = await _address.GetAll_Address();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetById")]
        //[Authorize(Roles = "AddressViewById")]

        public async Task<IActionResult> GetById(Guid Id)
        {
            var result = await _address.GetById_Address(Id);
            return Ok(result);
        }


        [HttpPost]
        [Route("ActiveInActive")]
        //[Authorize(Roles = "AddressActiveInActive")]
        public async Task<IActionResult> ActiveInactive(Guid Id)
        {
            var result = await _address.ActiveInActiveAddress(Id);
            return Ok(result);
        }
    }
}
