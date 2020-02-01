using Microsoft.Extensions.DependencyInjection;
using Stripe;
using Stripe.Checkout;
using StripeSample.Handlers;
using StripeSample.Services;

namespace StripeSample.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStripe(this IServiceCollection services)
        {
            services.AddSingleton<ProductService>();
            services.AddSingleton<PlanService>();
            services.AddSingleton<CustomerService>();
            services.AddSingleton<SubscriptionService>();
            services.AddSingleton<SessionService>();
            services.AddSingleton<InvoiceService>();

            services.AddScoped<IStripeService, StripeService>();

            services.AddScoped<CheckoutSessionCompletedEventHandler>(); 
            services.AddScoped<SubscriptionChangedEventHandler>();
            services.AddScoped<InvoiceChangedEventHandler>();

            return services;
        }
    }
}
