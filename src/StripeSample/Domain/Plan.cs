using System;
using System.Globalization;

namespace StripeSample.Domain
{
    // https://stripe.com/docs/api/plans
    public class Plan : StripeEntity
    {
        public Plan(string name, string externalKey)
            : base(externalKey)
        {
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; private set; }
        public int AmountInCents { get; set; }

        public Currency Currency { get; set; }
        public BillingInterval Interval { get; set; }
        
        public string GetFormattedCurrency() {
            var tmp = AmountInCents / 100;
            return $"{tmp.ToString("C2", CultureInfo.CreateSpecificCulture(Currency.Language))}";
        }

        internal void SetPricing(int amountInCents, Currency currency, BillingInterval interval)
        {
            AmountInCents = (amountInCents >= 0) ? amountInCents : throw new ArgumentException("Invalid amount in cents");
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
            Interval = interval ?? throw new ArgumentNullException(nameof(interval));
        }
    }
}
