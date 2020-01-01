using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StripeSample.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }


    // https://stripe.com/docs/api/subscriptions
    public class Subscription : BaseEntity
    {
        [Required]
        public User User { get; set; }
        [Required]
        public string PlanId { get; set; }
        [Required]
        public string SubscriptionId { get; set; }
        [Required]
        public SubscriptionState State { get; set; }
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }

    public class PaymentMethod : BaseEntity
    {
        public string Type { get; set; }
        public Card Card { get; set; }
    }

    public class Card
    {
        public string Brand { get; set; }
        public int Exp_Month { get; set; }
        public int Exp_Year { get; set; }
        public string Last4 { get; set; }
        public string Fingerprint { get; set; }
    }

    public enum SubscriptionState
    {
        None = 0,
        Active = 1,
        Past_Due = 2,
        Incomplete_Expired = 3,
        Trialing = 4
    }

}
