using System;
using System.ComponentModel.DataAnnotations;

namespace StripeSample.Entities
{
    public class Invoice : BaseEntity
    {
        [Required]
        public string InvoiceId { get; set; }
        [Required]
        public string InvoiceNumber { get; set; }
        [Required]
        public Subscription Subscription { get; set; }
        [Required]
        public DateTime PeriodStart { get; set; }
        [Required]
        public DateTime PeriodEnd { get; set; }
        [Required]
        public long AmountDue { get; set; }
        [Required]
        public long AmountPaid { get; set; }
        [Required]
        public long AmountRemaining { get; set; }
        public string InvoicePdfUrl { get; set; }
        public string BillingReason { get; set; }
        [Required]
        public InvoiceStatus Status { get; set; }
    }

    public enum InvoiceStatus
    {
        None = 0,
        Draft = 1,
        Open = 2,
        Paid = 3,
        Uncollectible = 3,
        Void = 3,
    }
}
