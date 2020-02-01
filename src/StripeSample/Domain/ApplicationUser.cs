using System;

namespace StripeSample.Domain
{
    public class ApplicationUser
    {
        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public string CustomerId { get; set; } // Stripe Customer Id
    }
}
