using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using StripeSample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StripeSample.Services
{
    public class StripeService
    {
        private readonly InvoiceService _invoiceService;
        private readonly CustomerService _customerService;
        
        private readonly SubscriptionService _subscriptionService;
        private readonly SessionService _sessionService;
        private readonly IConfiguration _configuration;

        public StripeService(InvoiceService invoiceService, CustomerService customerService, SubscriptionService subscriptionService, SessionService sessionService, IConfiguration configuration)
        {
            _invoiceService = invoiceService;
            _customerService = customerService;
            _subscriptionService = subscriptionService;
            _sessionService = sessionService;
            _configuration = configuration;
        }

        public async Task<Customer> CreateCustomer(string emailAddress, string customerId)
        {
            var options = new CustomerCreateOptions
            {
                Email = emailAddress,
                Metadata = new Dictionary<string, string>
                {
                    { "CustomerId", customerId }
                }
            };

            var customer = await _customerService.CreateAsync(options); 
            return customer;
        }

        public async Task<Session> CreateCheckoutSession(string planId, string customerId)
        {
            var baseUrl = _configuration["BaseUrl"];
            var options = new SessionCreateOptions
            {
                CustomerId = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Items = new List<SessionSubscriptionDataItemOptions> {
                        new SessionSubscriptionDataItemOptions {
                            PlanId = planId,
                        }
                    }
                },
                SuccessUrl = $"{baseUrl}/Home/Success?sessionId={{CHECKOUT_SESSION_ID}}",
                CancelUrl = baseUrl
            };

            return await _sessionService.CreateAsync(options);
        }

        public async Task<List<Subscription>> ListSubscriptions(string customerId)
        {
            var options = new SubscriptionListOptions
            {
                CustomerId = customerId
            };

            options.AddExpand("data.plan.product");

            var subscriptions = await _subscriptionService.ListAsync(options);
            return subscriptions.Data;
        }

        public async Task<Subscription> GetSubscription(string subscriptionId)
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

        public async Task<List<Invoice>> ListInvoices(string subscriptionId)
        {
            var options = new InvoiceListOptions
            {
                Limit = 20,
                SubscriptionId = subscriptionId
            };

            var invoices = await _invoiceService.ListAsync(options);
            return invoices.Data;
        }

        public async Task<Invoice> GetInvoice(string invoiceId)
        {
            var options = new InvoiceGetOptions
            {

            };

            var invoice = await _invoiceService.GetAsync(invoiceId, options);
            return invoice;
        }





        //public async Task<Product> CreateProduct(string name, string type = "service")
        //{
        //    var options = new ProductCreateOptions
        //    {
        //        Name = name,
        //        Type = type,
        //    };

        //    var product = await _productService.CreateAsync(options);
        //    return product;
        //}

        //public async Task<Plan> CreatePlan(string productId, string name, long amount, string interval = "month", string currency = "aud")
        //{
        //    var options = new PlanCreateOptions
        //    {
        //        Product = productId,
        //        Nickname = name,
        //        Currency = currency,
        //        Interval = interval,
        //        Amount = amount,
        //        UsageType = "licensed"
        //    };

        //    var plan = await _planService.CreateAsync(options);
        //    return plan;
        //}

        //public async Task<List<Product>> ListProducts()
        //{
        //    var products = await _productService.ListAsync();
        //    return products.Data;
        //}

        //public async Task<List<Customer>> ListCustomers()
        //{
        //    var customers = await _customerService.ListAsync();
        //    return customers.Data;
        //}
    }
}
