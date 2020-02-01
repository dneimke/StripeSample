using System;
using System.Collections.Generic;
using System.Linq;

namespace StripeSample.Domain
{
    // https://stripe.com/docs/api/products
    public class Product : StripeEntity
    {
        public Product(string name, string externalKey)
            : base(externalKey)
        {
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; private set; }

        private readonly List<Plan> _plans = new List<Plan>();
        public IReadOnlyCollection<Plan> Plans => _plans;


        public void AddPlan(string planName, string externalKey, int amountInCents, Currency currency, BillingInterval interval)
        {
            var existingPlanForProduct = _plans.Where(o => o.ExternalKey == externalKey)
                .SingleOrDefault();

            if (existingPlanForProduct == null)
            {
                var plan = new Plan(planName, externalKey);
                plan.SetPricing(amountInCents, currency, interval);
                _plans.Add(plan);
            }
        }
    }
}
