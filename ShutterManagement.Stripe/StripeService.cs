using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using Microsoft.AspNetCore.Http;

namespace ShutterManagement.Stripe
{
    public class StripeService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripeService> _logger;
        public StripeService(IOptions<StripeSettings> stripeSettings, ILogger<StripeService> logger)
        {
            _stripeSettings = stripeSettings.Value;
            _logger = logger;


            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        //// Method to create a Payment Intent
        //public PaymentIntent CreatePaymentIntent(long amount, string currency = "usd")
        //{
        //    var paymentIntentService = new PaymentIntentService();
        //    var options = new PaymentIntentCreateOptions
        //    {
        //        Amount = amount,
        //        Currency = currency,
        //        PaymentMethodTypes = new List<string> { "card" },
        //    };

        //    return paymentIntentService.Create(options);
        //}

        //// Method to create a Checkout Session
        //public Session CreateCheckoutSession()
        //{
        //    var sessionService = new SessionService();
        //    var sessionOptions = new SessionCreateOptions
        //    {
        //        PaymentMethodTypes = new List<string> { "card" },
        //        LineItems = new List<SessionLineItemOptions>
        //        {
        //            new SessionLineItemOptions
        //            {
        //                PriceData = new SessionLineItemPriceDataOptions
        //                {
        //                    Currency = "usd",
        //                    ProductData = new SessionLineItemPriceDataProductDataOptions
        //                    {
        //                        Name = "T-shirt",
        //                    },
        //                    UnitAmount = 2000, // Price in cents ($20.00)
        //                },
        //                Quantity = 1,
        //            }
        //        },
        //        Mode = "payment",
        //        SuccessUrl = "https://yourdomain.com/success?session_id={CHECKOUT_SESSION_ID}",
        //        CancelUrl = "https://yourdomain.com/cancel",
        //    };

        //    return sessionService.Create(sessionOptions);
        //}

        public string CreateCheckoutSession(PaymentRequest request, string currency = "usd")
        {
            var sessionService = new SessionService();
            var sessionOptions = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card", "amazon_pay" },

                CustomerEmail = request.CustomerEmail, 

                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = currency,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = request.ProductName,
                    },
                    UnitAmount = request.Amount,
                },
                Quantity = request.Quantity,

            }
        },
                Mode = "payment",
                SuccessUrl = _stripeSettings.successUrl,
                CancelUrl = _stripeSettings.cancelUrl,
            };

            var session = sessionService.Create(sessionOptions);
            return session.Url;
        }

        public async Task HandleStripeWebhook(HttpRequest request)
        {
            foreach (var header in request.Headers)
            {
                _logger.LogInformation($"Header: {header.Key}, Value: {string.Join(", ", header.Value)}");
            }
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning("Webhook body is empty");
                return; 
            }

            Event stripeEvent;
            var stripeSignatureHeader = request.Headers["Stripe-Signature"];
            if (string.IsNullOrEmpty(stripeSignatureHeader))
            {
                _logger.LogError("Stripe-Signature header is missing");
                return; 
            }
            _logger.LogInformation($"Received Stripe-Signature: {stripeSignatureHeader}");
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignatureHeader,
                    _stripeSettings.WebhookSecret
                );
            }
            catch (StripeException e)
            {
                _logger.LogError($"Webhook signature verification failed: {e.Message}");
                throw new InvalidOperationException("Webhook signature verification failed", e);
            }
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    var session = stripeEvent.Data.Object as Session;
                    if (session != null)
                    {
                        _logger.LogInformation($"Checkout session completed: {session.Id}");
                    }
                    break;

                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        _logger.LogInformation($"Payment intent succeeded: {paymentIntent.Id}");
                    }
                    break;
                case "payment_intent.failed":
                    var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (failedPaymentIntent != null)
                    {
                        _logger.LogWarning($"Payment intent failed. ID: {failedPaymentIntent.Id}");
                        _logger.LogWarning($"Error message: {failedPaymentIntent.LastPaymentError?.Message}");
                    }
                    break;
                default:
                    _logger.LogWarning($"Unhandled event type: {stripeEvent.Type}");
                    break;
            }
        }
    }
}

           
 
    
