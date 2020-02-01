
using System;

namespace StripeSample.Domain
{
    public abstract class StripeEntity : Entity
    {
        public StripeEntity(string externalKey)
        {
            ExternalKey = !string.IsNullOrWhiteSpace(externalKey) ? externalKey : throw new ArgumentNullException(nameof(externalKey));
        }

        public string ExternalKey { get; private set; }
    }
}
