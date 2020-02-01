using System;

namespace StripeSample.Domain
{
    // https://stripe.com/docs/api/invoices
    public class Invoice : StripeEntity
    {
        protected Invoice(string externalKey) : base(externalKey)
        {
        }

        public Invoice(string externalKey, Subscription subscription, Guid customerId, string invoiceNumber) : base(externalKey)
        {
            Subscription = subscription;
            CustomerId = customerId;
            InvoiceNumber = invoiceNumber;
        }

        public Subscription Subscription { get; private set; }
        public Guid CustomerId { get; private set; }

        public string InvoiceNumber { get; private set; }
        public string CurrencyCode { get; set; }
        public int AmountDueInCents { get; set; }
        public int AmountPaidInCents { get; set; }
        public int AmountRemainingInCents { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public string HostedInvoiceUrl { get; set; }
        public string InvoicePdfUrl { get; set; }
        public bool IsPaid { get; set; }
        public string ReceiptNumber { get; set; }
        public int Total { get; set; }
        public InvoiceStatus Status { get; set; }
    }
}
