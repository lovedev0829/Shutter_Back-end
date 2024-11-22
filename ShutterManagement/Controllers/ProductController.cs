using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productservice;
        private readonly IProductComponentService productcomponentservice;
        public ProductController(IProductService productService,IProductComponentService productComponentService)
        {
            this.productservice = productService;
            this.productcomponentservice=productComponentService;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "ProductsAdd")]
        public async Task<IActionResult> Create([FromBody] CreateProduct product)
        {
            var result = await productservice.Create_Product(product);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize(Roles = "ProductsEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateProduct updateVm)
        {
            var result = await productservice.Update_Product(updateVm);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "ProductsDelete")]
        public async Task<IActionResult> Delete(Guid ProductId)
        {
            var result = await productservice.DeleteProduct(ProductId);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "ProductsEdit")]
        public async Task<IActionResult> ActiveInactive(Guid ProductId)
        {
            var result = await productservice.ActiveInActiveProduct(ProductId);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "ProductsView")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var result = await productservice.GetAll_Product(filter);
            return Ok(result);
        }


        [HttpPost]
        [Route("AddProductComponent")]
        //[Authorize(Roles = "ComponentView")]
        public async Task<IActionResult> AddComponentMaterial(CreateProductComponentVm model)
        {
            var result = await productcomponentservice.AddProductComponent(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateProductComponent")]
        //[Authorize(Roles = "ComponentView")]
        public async Task<IActionResult> UpdateComponentMaterial(UpdateProductComponentVm model)
        {
            var result = await productcomponentservice.UpdateProductComponent(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteProductComponent")]
        //[Authorize(Roles = "ComponentView")]
        public async Task<IActionResult> DeleteComponentMaterial(Guid id)
        {
            var result = await productcomponentservice.Delete(id);
            return Ok(result);
        }

    }
}
