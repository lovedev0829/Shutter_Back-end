using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Stripe;
using Stripe;

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

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
           await stripeService.HandleStripeWebhook(HttpContext.Request);
           return Ok(); 
        }


            [HttpPost("Webhook")]
            public async Task<IActionResult> IndexWebhook()
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                try
                {
                    var stripeEvent = EventUtility.ParseEvent(json);

                    // Handle the event
                    // If on SDK version < 46, use class Events instead of EventTypes
                    if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
                    {
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        // Then define and call a method to handle the successful payment intent.
                        // handlePaymentIntentSucceeded(paymentIntent);
                    }
                    else if (stripeEvent.Type == EventTypes.PaymentMethodAttached)
                    {
                        var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                        // Then define and call a method to handle the successful attachment of a PaymentMethod.
                        // handlePaymentMethodAttached(paymentMethod);
                    }
                    // ... handle other event types
                    else
                    {
                        // Unexpected event type
                        Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                    }
                    return Ok();
                }
                catch (StripeException e)
                {
                    return BadRequest();
                }
            }
        }
    }


