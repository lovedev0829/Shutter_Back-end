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

        public async Task<IActionResult> CreateManagePartnerProduct(CreateManagePartnerProduct createManagePartnerProduct)
        {
            var res = managePartnersProducts.CreateManagePartnerProduct(createManagePartnerProduct);    
            return Ok(res);
        }
    }
}
