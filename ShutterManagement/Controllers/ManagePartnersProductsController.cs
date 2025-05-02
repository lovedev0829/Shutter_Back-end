using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagePartnersProductsController : ControllerBase
    {
        private readonly IManagePartnersProducts managePartnersProducts;

        public ManagePartnersProductsController(IManagePartnersProducts managePartnersProducts)
        {
            this.managePartnersProducts = managePartnersProducts;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "ManagePartnerProductEdit")]
        public async Task<IActionResult> CreateManagePartnerProduct(CreateManagePartnerProduct createManagePartnerProduct)
        {
            var res = await managePartnersProducts.CreateManagePartnerProduct(createManagePartnerProduct);    
            return Ok(res);
        }


        [HttpPost]
        [Route("GetAllManagePartnerProduct")]
        [Authorize(Roles = "ManagePartnerProductView")]
        public async Task<IActionResult> GetAllManagePartnerProduct(Pagination filter)
        {
            var res = await managePartnersProducts.GetAllProductOptions(filter);
            return Ok(res);
        }
    }
}
