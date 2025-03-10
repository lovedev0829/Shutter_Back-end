using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IGHLConfiguration gHLConfiguration;
        private readonly IStripeConfiguration stripeConfiguration;

        public ConfigurationController(IGHLConfiguration gHLConfiguration,IStripeConfiguration stripeConfiguration) {
            this.gHLConfiguration = gHLConfiguration;
            this.stripeConfiguration = stripeConfiguration;
        }

        [HttpGet]
        [Route("GetGHLConfiguration")]
        [Authorize(Roles = "GHLView")]
        public async Task<IActionResult> GetGHLConfiguration()
        {
            Guid? tenantId = Guid.Empty; 
            var res = await gHLConfiguration.GetGHLConfiguration(tenantId);
            return Ok(res);
        }


        [HttpPost]
        [Route("AddGHLConfiguration")]
        [Authorize(Roles = "GHLEdit")]

        public async Task<IActionResult> AddGHLConfiguration(GHLConfiguration gHLConfiguration1)
        {
            var res = await gHLConfiguration.GHLConfiguration(gHLConfiguration1);
            return Ok(res);
        }

        [HttpPost]
        [Route("AddStripeConfiguration")]
        [Authorize(Roles = "StripeEdit")]

        public async Task<IActionResult> AddStripeConfiguration(CreateStripeConfiguration model)
        {
            var res = await stripeConfiguration.Create_OrUpdate_StripeConfiguration(model);
            return Ok(res);
        }


        [HttpGet]
        [Route("GetStripeConfiguration")]
        [Authorize(Roles = "StripeView")]

        public async Task<IActionResult> GetStripeConfiguration()
        {
            var res = await stripeConfiguration.GetAll_StripeConfiguration();
            return Ok(res);
        }

        
    }
}
