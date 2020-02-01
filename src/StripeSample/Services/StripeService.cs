using StripeSample.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StripeSample.Services
{
    public class StripeService : IStripeService
    {
        private readonly InvoiceService _invoiceService;
        private readonly CustomerService _customerService;

        private readonly SubscriptionService _subscriptionService;
        private readonly SessionService _sessionService;

        private readonly ApplicationSettings _appSettings;
        private readonly StripeSettings _settings;

        public StripeService(InvoiceService invoiceService, CustomerService customerService, SubscriptionService subscriptionService, SessionService sessionService, IOptions<ApplicationSettings> appSettings, IOptions<StripeSettings> options)
        {
            _invoiceService = invoiceService;
            _customerService = customerService;
            _subscriptionService = subscriptionService;
            _sessionService = sessionService;
            _appSettings = appSettings.Value;
            _settings = options.Value;
        }

        public async Task<Customer> CreateCustomerAsync(string emailAddress)
        {
            var options = new CustomerCreateOptions
            {
                Email = emailAddress
            };

            var customer = await _customerService.CreateAsync(options);
            return customer;
        }

        public async Task<Invoice> GetInvoiceAsync(string invoiceId)
        {
            var options = new InvoiceGetOptions
            {

            };

            var invoice = await _invoiceService.GetAsync(invoiceId, options);
            return invoice;
        }


        public async Task<Session> CreateCheckoutSessionForCustomerAsync(string planId, string customerId, string cancelUrl = "")
        {
            var baseUrl = string.IsNullOrEmpty(cancelUrl) ? _appSettings.BaseUrl : cancelUrl;

            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Items = new List<SessionSubscriptionDataItemOptions> {
                        new SessionSubscriptionDataItemOptions {
                            Plan = planId,
                        }
                    }
                },
                SuccessUrl = _settings.CheckoutSuccessRedirectUrl,
                CancelUrl = baseUrl
            };

            return await _sessionService.CreateAsync(options);
        }


        public async Task<Subscription> GetSubscriptionAsync(string subscriptionId)
        {
            var options = new SubscriptionGetOptions
            {

            };

            var subscription = await _subscriptionService.GetAsync(subscriptionId, options);
            return subscription;
        }

        public async Task CancelSubscription(string subscriptionId)
        {
            var options = new SubscriptionCancelOptions
            {

            };

            await _subscriptionService.CancelAsync(subscriptionId, options);
        }

        // refer: https://stripe.com/docs/api/subscriptions/update
        public async Task UpdateSubscriptionAsync(string subscriptionId, Action<SubscriptionUpdateOptions> action)
        {
            var options = new SubscriptionUpdateOptions();
            action(options);

            await _subscriptionService.UpdateAsync(subscriptionId, options);
        }
    }
}