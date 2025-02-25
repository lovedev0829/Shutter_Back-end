using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Stripe;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly StripeService stripeService;

        public StripeController(StripeService stripeService)
        {
            this.stripeService = stripeService;
        }

        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession([FromBody] PaymentRequest request)
        {
            var checkoutUrl = stripeService.CreateCheckoutSession(request);
            return Ok(new { checkoutUrl });
        }
    }
}

