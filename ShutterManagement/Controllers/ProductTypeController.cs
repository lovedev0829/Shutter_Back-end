using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly IMstProductType mstProductType;

        public ProductTypeController(IMstProductType mstProductType)
        {
            this.mstProductType = mstProductType;
        }
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create_ProductType([FromBody] CreateProductType model)
        {
            var res = await mstProductType.Create_ProductType(model);
            return Ok(res);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update_ProductType(UpdateProductType model)
        {
            var res = await mstProductType.Update_ProductType(model);
            return Ok(res);
        }
        [HttpPost]
        [Route("ActiveInActive")]
        public async Task<IActionResult> ActiveInActive(Guid? ProductTypeId)
        {
            var res = await mstProductType.ActiveInActive(ProductTypeId);
            return Ok(res);
        }
        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> DeleteProductType(Guid? ProductTypeId)
        {
            var res = await mstProductType.DeleteProductType(ProductTypeId);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        public async Task<IActionResult> GetAllProductType(Pagination filter)
        {
            var res = await mstProductType.GetAll_ProductType(filter);
            return Ok(res);
        }

    }
}
