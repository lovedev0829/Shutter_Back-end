using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using System.Net;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterial _material;
        public MaterialController(IMaterial material)
        {
            _material = material;
        }

        [HttpPost]
        [Route("Create")]
        //[Authorize(Roles = "AddressAdd")]
        public async Task<IActionResult> Create([FromBody] CreateMaterial materialVm)
        {
            var result = await _material.Create_Material(materialVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        //[Authorize(Roles = "AddressEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateMaterial materialVm)
        {
            var result = await _material.Update_Material(materialVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        //[Authorize(Roles = "AddressDelete")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var result = await _material.Delete(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        //[Authorize(Roles = "AddressEdit")]
        public async Task<IActionResult> ActiveInactive(Guid Id)
        {
            var result = await _material.ActiveInActive(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        //[Authorize(Roles = "AddressView")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var result = await _material.GetAll_Material(filter);
            return Ok(result);
        }

    }
}
