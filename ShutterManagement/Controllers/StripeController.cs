using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Stripe;
using Stripe;
using Stripe.Checkout;


namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly StripeService stripeService;
        private readonly ShutterDBContext dBContext;
        private readonly IPaymentsService paymentsService;

        public StripeController(StripeService stripeService , ShutterDBContext dBContext , IPaymentsService paymentsService)
        {
            this.stripeService = stripeService;
            this.dBContext = dBContext;
            this.paymentsService = paymentsService;
        }

        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession([FromBody] PaymentRequest request)
        {
            var checkoutUrl = stripeService.CreateCheckoutSession(request);
            return Ok(new { checkoutUrl });
        }

        //[HttpPost]
        //public async Task<IActionResult> HandleWebhook()
        //{
        //   await stripeService.HandleStripeWebhook(HttpContext.Request);
        //   return Ok(); 
        //}

        [HttpPost]
        public async Task<IActionResult> Index()
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
                    Console.WriteLine("PaymentIntent was successful!");
                }
                else if (stripeEvent.Type == EventTypes.PaymentMethodAttached)
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    Console.WriteLine("PaymentMethod was attached to a Customer!");
                }
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }


        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ParseEvent(json);

            // Log the webhook event
            FileLogger.Log($"Stripe Webhook Response: {json}");

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session != null && session.Metadata.ContainsKey("tblInvoiceId") && session.Metadata.ContainsKey("tblPaymentId"))
                {
                    string invoiceId = session.Metadata["tblInvoiceId"];
                    string paymentId = session.Metadata["tblPaymentId"];

                    FileLogger.Log($"Invoice {invoiceId} and Payment {paymentId} received from Stripe Webhook.");

                    // Fetch Payment record
                    var payment = dBContext.TblPayments
                        .FirstOrDefault(p => p.PaymentId.ToString() == paymentId && p.IsActive == true && p.IsDelete == false);

                    FileLogger.Log($"DB Payment Details: {payment}");

                    if (payment != null)
                    {
                        // Prepare UpdatePayment model
                        var updatePayment = new UpdatePayments
                        {
                            PaymentId = payment.PaymentId,
                            TransactionId = session.PaymentIntentId, // Stripe transaction ID
                            PaymentDate = DateTime.Now,
                            PaymentMethod = "Stripe",
                            Amount = payment.Amount,
                            Notes = "Payment completed via Stripe.",
                            PaymentStatus = PaymentStatus.Paid
                        };

                        // Update the payment and invoice
                        var updateResponse = await paymentsService.UpdatePayment(updatePayment);

                        if (!updateResponse.IsSuccess)
                        {
                            FileLogger.Log($"Error updating payment: {updateResponse.Message}");
                            return BadRequest(updateResponse.Message);
                        }

                        // ✅ Save Payment Info to TblStripePayment
                        var stripePayment = new
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = payment.PaymentId.ToString(),
                            PaymentAmount = payment.Amount,
                            PaymentMethod = "Stripe",
                            PaymentDate = DateTime.Now,
                            PaymentStatus = "Paid",
                            TenantId = payment.TenantId,
                            CreatedDate = DateTime.Now,
                            ModeifiedAt = DateTime.Now
                        };

                        //dBContext.TblStripePayment.Add(stripePayment);
                        await dBContext.SaveChangesAsync();

                        FileLogger.Log($"Invoice {invoiceId} marked as Paid and Stripe payment details saved.");
                    }
                    else
                    {
                        FileLogger.Log($"Payment ID {paymentId} not found in database.");
                    }
                }
            }

            return Ok();
        }





    }
}


