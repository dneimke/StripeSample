using System;
using System.Collections.Generic;

namespace StripeSample.Domain
{
    // https://stripe.com/docs/api/customers
    public class Customer : StripeEntity
    {
        public Customer(string identityKey, string externalKey)
            : base(externalKey)
        {
            IdentityKey = !string.IsNullOrEmpty(identityKey) ? identityKey : throw new ArgumentNullException(nameof(identityKey));
        }
    
        /// <summary>
        /// Id of the user in the source system
        /// </summary>
        public string IdentityKey { get; private set; }

        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions;

        public void AddSubscription(Subscription subscription)
        {
            if (!_subscriptions.Exists(o => o.ExternalKey == subscription.ExternalKey))
            {
                _subscriptions.Add(subscription);
            }
        }
    }
}
