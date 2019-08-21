using System;

namespace StripeSample.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public string CustomerId { get; set; } // Stripe Customer Id
    }
}
