﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StripeSample.Entities
{
    public class BaseEntity
    {
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }

    public class Subscription : BaseEntity
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
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }

    public class Invoice : BaseEntity
    {
        public Guid Id { get; set; }
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

    public enum SubscriptionState
    {
        None = 0,
        Active = 1,
        Past_Due = 2,
        Incomplete_Expired = 3,
        Trialing = 4
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
