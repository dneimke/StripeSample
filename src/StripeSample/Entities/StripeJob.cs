using System;

namespace StripeSample.Entities
{
    public class StripeJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Payload { get; set; }
        public string MessageType { get; set; } = GetTypeString(typeof(Stripe.Event));
        public bool IsProcessed { get; set; } = false;

        public static string GetTypeString(Type type)
        {
            return type.FullName + "," + type.Assembly.GetName().Name;
        }
    }
}
