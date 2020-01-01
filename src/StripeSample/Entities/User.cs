using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StripeSample.Entities
{
    public class User 
    {
        public Guid Id { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        public string CustomerId { get; set; } // Stripe Customer Id
        public List<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public List<Cart> Carts { get; set; } = new List<Cart>();
    }
}
