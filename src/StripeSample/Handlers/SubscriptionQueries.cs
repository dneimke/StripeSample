using Microsoft.EntityFrameworkCore;
using StripeSample.Domain;
using StripeSample.Infrastructure.Data;
using System.Threading.Tasks;

namespace StripeSample.Handlers.Subscriptions
{
    public static class SubscriptionQueries
    {
        public static async Task<bool> UserHasPlanAsync(SubscriptionsContext subscriptionsContext, string userId)
        {
            if (userId is null)
            {
                throw new System.ArgumentNullException(nameof(userId));
            }

            var hasActiveSubscription = await subscriptionsContext.Subscription
                .AnyAsync(x => x.Customer.IdentityKey == userId && x.Status.Id == SubscriptionStatus.Active.Id);

            return hasActiveSubscription;
        }
    }
}
