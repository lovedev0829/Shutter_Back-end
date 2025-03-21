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

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ParseEvent(json);

                // Log the received webhook
                FileLogger.Log($"Received Stripe Webhook Event: {stripeEvent.Type}");

                if (stripeEvent.Data.Object is Session session)
                {
                    // Extract metadata safely
                    string paymentId = session.Metadata?.ContainsKey("db_PaymentId") == true ? session.Metadata["db_PaymentId"] : Guid.NewGuid().ToString();
                    string invoiceId = session.Metadata?.ContainsKey("db_InvoiceId") == true ? session.Metadata["db_InvoiceId"] : Guid.NewGuid().ToString();
                    string tenantId = session.Metadata?.ContainsKey("db_TenantId") == true ? session.Metadata["db_TenantId"] : Guid.NewGuid().ToString();

                    // Payment details
                    decimal? paymentAmount = session.AmountTotal / 100m;
                    string stripeTransactionId = session.PaymentIntentId ?? "N/A";
                    string paymentMethod = session.PaymentMethodTypes?.FirstOrDefault() ?? "Stripe";
                    DateTime paymentDate = session.Created;
                    string customerEmail = session.CustomerEmail ?? "N/A";
                    string customerName = session.CustomerDetails?.Name ?? "N/A";
                    string paymentMode = session.Mode ?? "N/A";
                    string paymentStatus = session.PaymentStatus ?? "unknown";
                    string status = session.Status ?? "N/A";

                    FileLogger.Log($"Processing Event: {stripeEvent.Type}, PaymentId: {paymentId}, InvoiceId: {invoiceId}, Status: {paymentStatus}");

                    // Store webhook data for all Stripe events
                    var stripePayment = new TblStripePayment
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = paymentId,
                        PaymentAmount = paymentAmount ?? 0m,
                        PaymentMethod = paymentMethod,
                        PaymentDate = paymentDate,
                        PaymentStatus = paymentStatus,
                        TenantId = Guid.Parse(tenantId),
                        CreatedDate = DateTime.Now,
                        WebhookResponse = json,
                        StripeEventType = stripeEvent.Type,
                        StripeTransactionId = stripeTransactionId,
                        InvoiceId = Guid.Parse(invoiceId),
                        CustomerEmail = customerEmail,
                        CustomerName = customerName,
                        PaymentMode = paymentMode,
                        Status = status
                    };

                    dBContext.TblStripePayments.Add(stripePayment);
                    await dBContext.SaveChangesAsync();

                    FileLogger.Log($"Webhook event stored successfully: {stripeEvent.Type}, PaymentId: {paymentId}");

                    if (stripeEvent.Type == "checkout.session.completed" && paymentStatus == "paid")
                    {
                        var existingPayment = dBContext.TblPayments
                            .FirstOrDefault(p => p.PaymentId == Guid.Parse(paymentId) && p.InvoiceId == Guid.Parse(invoiceId));

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
                                FileLogger.Log($"Payment Updated Successfully: {existingPayment.PaymentId}");
                            else
                                FileLogger.Log($"Error Updating Payment: {updateResponse.Message}");
                        }
                        else
                        {
                            FileLogger.Log($"No Matching Payment Found in DB for PaymentId: {paymentId}, InvoiceId: {invoiceId}");
                        }
                    }

                    return Ok(new { message = "Webhook processed successfully." });
                }

                FileLogger.Log("Error: Invalid session data received.");
                return BadRequest(new { message = "Invalid session data from Stripe." });
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Exception in Stripe Webhook: {ex.Message} | StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal Server Error. Check logs for details." });
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


