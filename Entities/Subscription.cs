using System;
using System.ComponentModel.DataAnnotations;

namespace StripeSample.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }
        [Required]
        public User User { get; set; }
        [Required]
        public string PlanId { get; set; }
        [Required]
        public string SubscriptionId { get; set; }
        [Required]
        public SubscriptionState State { get; set; }
    }

    public enum SubscriptionState
    {
        None = 0,
        Active = 1,
        PastDue = 2,
        Closed = 3
    }
}
