using StripeSample.Domain;
using StripeSample.Infrastructure.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StripeSample.Infrastructure.Data
{
    public class SubscriptionsContextSeed
    {
        public async Task SeedAsync(SubscriptionsContext context, StripeSettings config)
        {
            // Seed Enumeration types
            if (!context.Currency.Any())
            {
                await context.Set<Currency>().AddRangeAsync(Currency.ListAll());
                await context.SaveChangesAsync();
            }

            if (!context.CardType.Any())
            {
                await context.Set<CardType>().AddRangeAsync(CardType.ListAll());
                await context.SaveChangesAsync();
            }

            if (!context.BillingInterval.Any())
            {
                await context.Set<BillingInterval>().AddRangeAsync(BillingInterval.ListAll());
                await context.SaveChangesAsync();
            }

            if (!context.InvoiceStatus.Any())
            {
                await context.Set<InvoiceStatus>().AddRangeAsync(InvoiceStatus.ListAll());
                await context.SaveChangesAsync();
            }

            if (!context.SubscriptionStatus.Any())
            {
                await context.Set<SubscriptionStatus>().AddRangeAsync(SubscriptionStatus.ListAll());
                await context.SaveChangesAsync();
            }

            // Seed our Default Plan
            if (!context.Product.Any(x => x.ExternalKey == config.DefaultProductKey))
            {
                var product = new Product(config.DefaultProductName, config.DefaultProductKey);
                await context.Set<Product>().AddAsync(product);
                await context.SaveChangesAsync();
            }

            if (!context.Plan.Any(x => x.ExternalKey == config.DefaultPlanKey))
            {
                var currency = Currency.USD;
                var interval = BillingInterval.Month;
                var product = context.Product.First(x => x.ExternalKey == config.DefaultProductKey);

                context.Entry(currency).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                context.Entry(interval).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;

                product.AddPlan(config.DefaultPlanName, config.DefaultPlanKey, config.DefaultPlanAmountInCents, currency, interval);
                await context.SaveChangesAsync();
            }

            // Seed a Test User
            var userId = Guid.Parse("07b742cc-8c82-43b6-8615-de54635db929");
            if(!context.ApplicationUser.Any(x => x.Id == userId))
            {
                var user = new ApplicationUser
                {
                    Id = userId,
                    EmailAddress = "test.user@stripesample.com"
                };

                context.ApplicationUser.Add(user);
                await context.SaveChangesAsync();
            }
        }
    }
}
