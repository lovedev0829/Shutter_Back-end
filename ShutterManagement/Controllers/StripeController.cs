using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
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
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ParseEvent(json);

                // Log the webhook event
                FileLogger.Log($"Stripe Webhook Response: {json}");

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session != null &&
                        session.Metadata.ContainsKey("db_PaymentId") &&
                        session.Metadata.ContainsKey("db_InvoiceId") &&
                        session.Metadata.ContainsKey("db_TenantId"))
                    {
                        string paymentId = session.Metadata["db_PaymentId"];
                        string invoiceId = session.Metadata["db_InvoiceId"];
                        string tenantId = session.Metadata["db_TenantId"];
                        decimal? paymentAmount = session.AmountTotal / 100m;
                        string stripeTransactionId = session.PaymentIntentId ?? "N/A";
                        string paymentMethod = session.PaymentMethodTypes?.FirstOrDefault() ?? "Stripe";
                        DateTime paymentDate = session.Created;
                        string customerEmail = session.CustomerEmail ?? "N/A";
                        string customerName = session.CustomerDetails?.Name ?? "N/A";
                        string paymentMode = session.Mode ?? "N/A"; 
                        FileLogger.Log($"Processing Stripe payment. PaymentId: {paymentId}, InvoiceId: {invoiceId}");
                        FileLogger.Log($"Stripe Amount: {paymentAmount}");

                        var existingPayment = dBContext.TblPayments
                            .FirstOrDefault(p => p.PaymentId == Guid.Parse(paymentId) && p.InvoiceId == Guid.Parse(invoiceId));

                        FileLogger.Log($"Existing Payment in DB: {existingPayment?.PaymentId}");

                        if (existingPayment != null)
                        {
                            var updatePaymentRequest = new UpdatePayments
                            {
                                PaymentId = existingPayment.PaymentId,
                                PaymentDate = paymentDate,
                                Amount = paymentAmount ?? 0m,
                                PaymentStatus = PaymentStatus.Paid,
                                PaymentMethod = paymentMethod,
                                TransactionId = stripeTransactionId,
                                Notes = "",
                            };

                            var updateResponse = await paymentsService.UpdatePayment(updatePaymentRequest);

                            if (updateResponse.IsSuccess)
                            {
                                FileLogger.Log($"Existing Payment Updated Successfully. PaymentId: {existingPayment.PaymentId}");
                            }
                            else
                            {
                                FileLogger.Log($"Error updating payment: {updateResponse.Message}");
                            }
                        }
                        else
                        {
                            FileLogger.Log($"No existing payment found for PaymentId: {paymentId}, InvoiceId: {invoiceId}");
                        }

                        var stripePayment = new TblStripePayment
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = paymentId,
                            PaymentAmount = paymentAmount ?? 0m,
                            PaymentMethod = paymentMethod,
                            PaymentDate = paymentDate,
                            PaymentStatus = "paid",
                            TenantId = Guid.Parse(tenantId),
                            CreatedDate = DateTime.Now,
                            WebhookResponse = json,
                            StripeEventType = stripeEvent.Type,
                            StripeTransactionId= stripeTransactionId,
                            InvoiceId = Guid.Parse(invoiceId),
                            CustomerEmail = customerEmail,
                            CustomerName = customerName,
                            PaymentMode = paymentMode
                        };

                        dBContext.TblStripePayments.Add(stripePayment);
                        await dBContext.SaveChangesAsync();

                        FileLogger.Log($"Stripe webhook details saved. PaymentId: {paymentId}");

                        return Ok(new { message = "Webhook processed successfully." });
                    }
                    else
                    {
                        FileLogger.Log("Error: Stripe metadata missing required keys.");
                        return BadRequest(new { message = "Invalid metadata received from Stripe." });
                    }
                }

                return Ok(new { message = "Unhandled Stripe event type." });
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in Stripe Webhook: {ex.Message} | StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal Server Error. Please check logs for details." });
            }
        }


        [HttpGet]
        [Route("GetPaymentDetails")]
        public async Task<IActionResult> GetPaymentDetails(string Tranjectionid, string PaymentId)
        {
            var result = await paymentsService.GetPaymentDetails(Tranjectionid,PaymentId);

            return Ok(result);
        }










    }
}


