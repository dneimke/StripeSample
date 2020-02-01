using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using StripeSample.Handlers;
using StripeSample.Infrastructure;
using StripeSample.Infrastructure.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StripeSample.Controllers
{
    public class WebhookController : Controller
    {
        private readonly CheckoutSessionCompletedEventHandler _checkoutSessionCompleted;
        private readonly SubscriptionChangedEventHandler _subscriptionChanged;
        private readonly InvoiceChangedEventHandler _invoiceChanged;
        private readonly IBackgroundJobClient _backgroundJobQueue;
        private readonly StripeSettings _settings;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            CheckoutSessionCompletedEventHandler checkoutSessionCompletedEventHandler,
            SubscriptionChangedEventHandler subscriptionChangedHandler,
            InvoiceChangedEventHandler invoiceChanged,
            IOptions<StripeSettings> settings, 
            IBackgroundJobClient backgroundJobQueue,
            ILogger<WebhookController> logger
            )
        {
            _settings = settings.Value;
            _backgroundJobQueue = backgroundJobQueue;
            _checkoutSessionCompleted = checkoutSessionCompletedEventHandler;
            _subscriptionChanged = subscriptionChangedHandler;
            _invoiceChanged = invoiceChanged;
            _logger = logger;
        }

        public async Task<IAsyncResult> Webhook()
        {
            Event stripeEvent;

            var secret = _settings.WebhookSecret;

            using (var stream = new StreamReader(HttpContext.Request.Body))
            {
                var json = await stream.ReadToEndAsync();
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret, throwOnApiVersionMismatch: false);

                if (stripeEvent == null)
                {
                    throw new InvalidOperationException("Unable to extract event.");
                }
            }

            try
            {
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var data = StripeUtility.ParseStripePayload<Session>(stripeEvent);
                    _logger.LogInformation("Webhook: Checkout Session completed for Session {CartSession}", data.Id);
                    _backgroundJobQueue.Enqueue(() => _checkoutSessionCompleted.HandleWebhookEvent(data.Id));
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionCreated || stripeEvent.Type == Events.CustomerSubscriptionUpdated || stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    var data = StripeUtility.ParseStripePayload<Subscription>(stripeEvent);
                    _logger.LogInformation("Webhook: Subscription {SubscriptionId} changed for customer {CustomerId}", data.Id, data.CustomerId);
                    _backgroundJobQueue.Enqueue(() => _subscriptionChanged.HandleWebhookEvent(data.Id));
                }
                else if (stripeEvent.Type == Events.InvoiceFinalized || stripeEvent.Type == Events.InvoicePaymentSucceeded)
                {
                    var data = StripeUtility.ParseStripePayload<Invoice>(stripeEvent);
                    _logger.LogInformation("Webhook: Invoice {InvoiceId} changed for customer {CustomerId} with subscription {SubscriptionId}", data.Id, data.CustomerId, data.SubscriptionId);
                    _backgroundJobQueue.Enqueue(() => _invoiceChanged.HandleWebhookEvent(data.SubscriptionId, data.Id));
                }

                return Ok() as IAsyncResult;
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "StripeException occurred");
                // return StatusCode(500) as IAsyncResult;
                throw;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "InvalidOperationException occurred");
                // return StatusCode(500) as IAsyncResult;
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred");
                // return StatusCode(500) as IAsyncResult;
                throw;
            }
        }
    }
}
