using MediatR;
using Microsoft.EntityFrameworkCore;
using StripeSample.Domain;
using StripeSample.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StripeSample.Handlers
{
    public static class SubscriptionDetails
    {
        public class Request : IRequest<ViewModel>
        {

        }

        public class ViewModel
        {
            const string _freeDescription = "Subscribe to get unlimited matches and groups.";
            const string _paidDescription = "You have access to unlimited matches and groups.";

            public ViewModel(Customer customer = null)
            {
                IsCustomer = customer != null;
            }

            public ViewModel(Subscription subscription, Invoice latestInvoice)
            {
                IsCustomer = true;
                HasSubscription = subscription != null;
                SubscriptionId = subscription.Id;
                IsPendingCancellation = subscription.IsPendingCancellation;
                CurrentPeriodEnd = subscription.CurrentPeriodEnd;
                
                PlanTitle = subscription.Plan.Name;
                PlanDescription = _paidDescription;
                
                if(latestInvoice != null)
                {
                    LatestInvoiceNumber = latestInvoice.InvoiceNumber;
                    LatestInvoicePdfUrl = latestInvoice.InvoicePdfUrl;
                }
            }

            public string PlanTitle { get; } = "Free Plan";
            public string PlanDescription { get; } = _freeDescription;
            public bool IsCustomer { get; } = false;
            public bool HasSubscription { get; set; } = false;
            public bool IsPendingCancellation { get; set; } = false;
            public DateTime? CurrentPeriodEnd { get; set; } = null;
            public Guid? SubscriptionId { get; }
            public string LatestInvoiceNumber { get; }
            public string LatestInvoicePdfUrl { get;  }
            public bool HasLatestInvoice => !string.IsNullOrEmpty(LatestInvoiceNumber);
            public bool ShowCancelOption => HasSubscription && !IsPendingCancellation;
            public bool ShowUpgradeOption => !HasSubscription;
        }


        public class RequestHandler : IRequestHandler<Request, ViewModel>
        {
            private readonly SubscriptionsContext _dbContext;
            private readonly UserContext _userContext;

            public RequestHandler(SubscriptionsContext dbContext, UserContext userContext)
            {
                _dbContext = dbContext;
                _userContext = userContext;
            }


            public async Task<ViewModel> Handle(Request request, CancellationToken cancellationToken)
            {
                var customer = await _dbContext.Customer
                    .Include(x => x.Subscriptions).ThenInclude(x => x.Plan)
                    .Include(x => x.Subscriptions).ThenInclude(x => x.Status)
                    .FirstOrDefaultAsync(x => x.IdentityKey == _userContext.Id.ToString());

                if (customer == null || !customer.Subscriptions.Any(x => x.Status.Id == SubscriptionStatus.Active.Id))
                {
                    return new ViewModel(customer);
                }
               
                var subscription = customer.Subscriptions.First(x => x.Status.Id == SubscriptionStatus.Active.Id);
                var latestInvoice = await _dbContext.Invoice
                    .OrderByDescending(x => x.PeriodEnd)
                    .FirstOrDefaultAsync(x => x.CustomerId == customer.Id);

                return new ViewModel(subscription, latestInvoice);
            }
        }
    }
}
