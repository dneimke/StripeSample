using System;
using System.Collections.Generic;
using System.Linq;

namespace StripeSample.Domain
{
    // https://stripe.com/docs/api/subscriptions
    public class Subscription : StripeEntity
    {
        protected Subscription(string externalKey) : base(externalKey)
        {
        }

        public Subscription(string externalKey, Plan plan) : base(externalKey)
        {
            Plan = plan;
        }

        public Plan Plan { get; private set; }

        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public SubscriptionStatus Status { get; set; }
        public bool CancelAtPeriodEnd { get; set; }

        public Customer Customer { get; private set; }

        public void SetPlan(Plan plan)
        {
            Plan = plan;
        }


        private readonly List<Invoice> _invoices = new List<Invoice>();
        public IReadOnlyCollection<Invoice> Invoices => _invoices;

        public Invoice AddInvoice(string externalInvoiceKey, Guid customerId, string invoiceNumber)
        {
            var invoice = _invoices.FirstOrDefault(x => x.ExternalKey == externalInvoiceKey);

            if (invoice == null)
            {
                invoice = new Invoice(externalInvoiceKey, this, customerId, invoiceNumber);
                _invoices.Add(invoice);
            }

            return invoice;
        }

        public bool IsPendingCancellation => Status.Id != SubscriptionStatus.Canceled.Id && CancelAtPeriodEnd == true;
    }
}
