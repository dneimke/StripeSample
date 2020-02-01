using Stripe;
using Stripe.Checkout;
using System;
using System.Threading.Tasks;

namespace StripeSample.Services
{
    public interface IStripeService
    {
        Task<Customer> CreateCustomerAsync(string emailAddress);
        Task<Session> CreateCheckoutSessionForCustomerAsync(string planId, string customerId, string cancelUrl = "");
       
        Task<Subscription> GetSubscriptionAsync(string subscriptionId);
        Task UpdateSubscriptionAsync(string subscriptionId, Action<SubscriptionUpdateOptions> action);
        Task CancelSubscription(string subscriptionId);

        Task<Invoice> GetInvoiceAsync(string invoiceId);
    }
}
