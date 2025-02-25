using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace ShutterManagement.Stripe
{
    public class StripeService
    {
        private readonly StripeSettings _stripeSettings;

        public StripeService(IOptions<StripeSettings> stripeSettings)
        {
            _stripeSettings = stripeSettings.Value;

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
                PaymentMethodTypes = new List<string> { "amazon_pay" },
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
    }

}
