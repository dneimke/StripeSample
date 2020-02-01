using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StripeSample.Domain;
using StripeSample.Infrastructure.Data;
using StripeSample.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StripeSample.Handlers.Commands
{
    public static class EnsureInvoiceCommand
    {
        static bool hasInvoice;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public static async Task<Invoice> ExecAsync(SubscriptionsContext dbContext, IStripeService stripeService, string invoiceId, ILogger logger)
        {
            logger.LogInformation("Processing EnsureInvoiceCommand for {StripeInvoiceId}", invoiceId);

            var invoice = await dbContext.Invoice.FirstOrDefaultAsync(e => e.ExternalKey == invoiceId);
            var stripeInvoice = await stripeService.GetInvoiceAsync(invoiceId);

            await semaphoreSlim.WaitAsync();
            try
            {
                hasInvoice = invoice != null;

                if (!hasInvoice)
                {
                    logger.LogWarning("Creating invoice while processing EnsureInvoiceCommand for {StripeInvoiceId}", invoiceId);

                    invoice = await dbContext.Invoice.FirstOrDefaultAsync(e => e.ExternalKey == invoiceId);
                    hasInvoice = invoice != null;

                    if (!hasInvoice)
                    {
                        logger.LogInformation("Creating invoice for {StripeInvoiceId}", invoiceId);

                        var subscription = await dbContext.Subscription
                            .Include(x => x.Customer)
                            .FirstOrDefaultAsync(e => e.ExternalKey == stripeInvoice.SubscriptionId);

                        invoice = subscription.AddInvoice(invoiceId, subscription.Customer.Id, stripeInvoice.Number);

                        dbContext.Invoice.Add(invoice);

                        hasInvoice = true;
                    }
                }

                var status = InvoiceStatus.FindByName(stripeInvoice.Status);
                dbContext.Entry(status).State = EntityState.Unchanged;

                invoice.AmountRemainingInCents = (int)stripeInvoice.AmountRemaining;
                invoice.AmountDueInCents = (int)stripeInvoice.AmountDue;
                invoice.AmountPaidInCents = (int)stripeInvoice.AmountPaid;
                invoice.CurrencyCode = stripeInvoice.Currency;
                invoice.HostedInvoiceUrl = stripeInvoice.HostedInvoiceUrl;
                invoice.InvoicePdfUrl = stripeInvoice.InvoicePdf;
                invoice.IsPaid = stripeInvoice.Paid;
                invoice.ReceiptNumber = stripeInvoice.ReceiptNumber;
                invoice.PeriodStart = stripeInvoice.PeriodStart;
                invoice.PeriodEnd = stripeInvoice.PeriodEnd;
                invoice.Total = (int)stripeInvoice.Total;

                invoice.Status = status;
                invoice.LastModifiedDateTime = DateTime.Now;

                await dbContext.SaveChangesAsync();

                logger.LogInformation("Finished processing EnsureInvoiceCommand for {StripeInvoiceId} with {ApplicationInvoiceId} and {StripeSubscriptionId}", invoiceId, invoice.Id, stripeInvoice.SubscriptionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing EnsureInvoiceCommand for {StripeInvoiceId}", invoiceId);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return invoice;
        }
    }
}
