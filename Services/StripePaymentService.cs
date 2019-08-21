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
    public class StripePaymentService
    {
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;
        private readonly PlanService _planService;
        private readonly SubscriptionService _subscriptionService;
        private readonly SessionService _sessionService;

        public StripePaymentService(CustomerService customerService, ProductService productService, PlanService planService, SubscriptionService subscriptionService, SessionService sessionService)
        {
            _customerService = customerService;
            _productService = productService;
            _planService = planService;
            _subscriptionService = subscriptionService;
            _sessionService = sessionService;
        }

        public async Task<Product> CreateProduct(string name, string type = "service")
        {
            var options = new ProductCreateOptions
            {
                Name = name,
                Type = type,
            };

            var product = await _productService.CreateAsync(options);
            return product;
        }

        public async Task<Plan> CreatePlan(string productId, string name, long amount, string interval = "month", string currency = "aud")
        {
            var options = new PlanCreateOptions
            {
                Product = productId,
                Nickname = name,
                Currency = currency,
                Interval = interval,
                Amount = amount,
                UsageType = "licensed"
            };

            var plan = await _planService.CreateAsync(options);
            return plan;
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
                SuccessUrl = "http://localhost:55965/Home/Success",
                CancelUrl = "http://localhost:55965"
            };

            return await _sessionService.CreateAsync(options);
        }

        public async Task<List<Product>> ListProducts()
        {
            var products = await _productService.ListAsync();
            return products.Data;
        }

        public async Task<List<Customer>> ListCustomers()
        {
            var customers = await _customerService.ListAsync();
            return customers.Data;
        }

        public async Task<List<Subscription>> ListSubscriptions(string customerId)
        {
            var options = new SubscriptionListOptions
            {
                CustomerId = customerId
            };

            var subscriptions = await _subscriptionService.ListAsync(options);
            return subscriptions.Data;
        }

        public async Task CancelSubscription(string subscriptionId)
        {
            var options = new SubscriptionCancelOptions
            {
                
            };

            await _subscriptionService.CancelAsync(subscriptionId, options);
        }
    }
}
