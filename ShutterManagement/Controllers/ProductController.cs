using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productservice;
        public ProductController(IProductService productService)
        {
            this.productservice = productService;
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
    }
}
