using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StripeSample.Domain;
using StripeSample.Infrastructure.Configuration;
using StripeSample.Infrastructure.Data;
using StripeSample.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StripeSample.Handlers.Commands
{
    public static class EnsureSubscriptionCommand
    {
        static bool hasSubscription;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);


        public static async Task<Subscription> ExecAsync(SubscriptionsContext dbContext, IStripeService stripeService, string subscriptionId, StripeSettings settings, ILogger logger)
        {
            logger.LogInformation("Processing EnsureSubscriptionCommand for {StripeSubscriptionId}", subscriptionId);

            var subscription = await dbContext.Subscription.FirstOrDefaultAsync(e => e.ExternalKey == subscriptionId);
            var stripeSubscription = await stripeService.GetSubscriptionAsync(subscriptionId);

            await semaphoreSlim.WaitAsync();
            try
            {
                hasSubscription = subscription != null;

                if (!hasSubscription)
                {
                    subscription = await dbContext.Subscription.FirstOrDefaultAsync(e => e.ExternalKey == subscriptionId);
                    hasSubscription = subscription != null;

                    if (!hasSubscription)
                    {
                        logger.LogWarning("Creating subscription while processing EnsureSubscriptionCommand for {StripeSubscriptionId}", subscriptionId);

                        var customer = await dbContext.Customer.FirstOrDefaultAsync(e => e.ExternalKey == stripeSubscription.CustomerId);
                        var plan = await dbContext.Plan.FirstOrDefaultAsync(x => x.ExternalKey == settings.DefaultPlanKey);

                        subscription = new Subscription(subscriptionId, plan);

                        customer.AddSubscription(subscription);

                        dbContext.Subscription.Add(subscription);

                        hasSubscription = true;
                    }
                }

                var status = SubscriptionStatus.FindByName(stripeSubscription.Status);
                dbContext.Entry(status).State = EntityState.Unchanged;

                subscription.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
                subscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
                subscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;
                subscription.Status = status;
                subscription.LastModifiedDateTime = DateTime.Now;

                await dbContext.SaveChangesAsync();

                logger.LogInformation("Finished processing EnsureSubscriptionCommand for {StripeSubscriptionId} with {ApplicationSubscriptionId}", subscriptionId, subscription.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing EnsureSubscriptionCommand for {StripeSubscriptionId}", subscriptionId);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return subscription;
        }
    }
}
