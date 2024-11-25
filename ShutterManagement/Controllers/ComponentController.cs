using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentController : ControllerBase
    {
        private readonly IComponentService _componentService;
        private readonly IComponentMaterial componentMaterial;

        public ComponentController(IComponentService componentService, IComponentMaterial componentMaterial)
        {
            _componentService = componentService;
            this.componentMaterial = componentMaterial;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "ComponentAdd")]
        public async Task<IActionResult> Create([FromBody] CreateComponent componentVm)
        {
            var result = await _componentService.Create_Component(componentVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize(Roles = "ComponentEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateComponent updateVm)
        {
            var result = await _componentService.Update_Component(updateVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "ComponentDelete")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var result = await _componentService.Delete(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "ComponentEdit")]
        public async Task<IActionResult> ActiveInactive(Guid Id)
        {
            var result = await _componentService.ActiveInActive(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "ComponentView")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var result = await _componentService.GetAll_Component(filter);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddComponentMaterial")]
        //[Authorize(Roles = "ComponentView")]
        public async Task<IActionResult> AddComponentMaterial(CreateComponentMaterialVm model)
        {
            var result = await componentMaterial.AddComponentMaterial(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateComponentMaterial")]
        //[Authorize(Roles = "ComponentView")]
        public async Task<IActionResult> UpdateComponentMaterial(UpdateComponentMaterialVm model)
        {
            var result = await componentMaterial.UpdateComponentMaterial(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteComponentMaterial")]
        //[Authorize(Roles = "ComponentView")]
        public async Task<IActionResult> DeleteComponentMaterial(Guid id)
        {
            var result = await componentMaterial.Delete(id);
            return Ok(result);
        }

    }
}
