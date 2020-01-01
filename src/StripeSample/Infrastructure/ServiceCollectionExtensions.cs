using Microsoft.Extensions.DependencyInjection;
using Stripe;
using Stripe.Checkout;
using StripeSample.Models;
using StripeSample.Services;
using System;

namespace StripeSample.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddStripe(this IServiceCollection services)
        {
            services.AddTransient<StripeService>();
            services.AddTransient<ProductService>();
            services.AddTransient<PlanService>();
            services.AddTransient<CustomerService>();
            services.AddTransient<SubscriptionService>();
            services.AddTransient<SessionService>();
            services.AddTransient<InvoiceService>();
        }
    }
}
