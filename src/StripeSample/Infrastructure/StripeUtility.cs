using Stripe;
using System;

namespace StripeSample.Infrastructure
{
    public static class StripeUtility
    {
        public static T ParseStripePayload<T>(Event stripeEvent)
        {
            if (!(stripeEvent.Data.Object is T data))
            {
                throw new InvalidOperationException("Unable to parse request data.");
            }

            return data;
        }
    }
}
