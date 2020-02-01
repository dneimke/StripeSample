using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StripeSample.Domain;
using StripeSample.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StripeSample.Handlers
{
    public static class InvoicesForCurrentUser
    {
        public class Request : IRequest<ViewModel>
        {
        }

        public class ViewModel
        {
            public ViewModel(List<Invoice> invoices)
            {
                Invoices = invoices;
            }

            public List<Invoice> Invoices { get; } = new List<Invoice>();

            public bool HasInvoices => Invoices.Any();
        }

        
        public class RequestHandler : IRequestHandler<Request, ViewModel>
        {
            private readonly SubscriptionsContext _dbContext;
            private readonly UserContext _userContext;
            private readonly ILogger<RequestHandler> _logger;

            public RequestHandler(SubscriptionsContext dbContext, UserContext userContext, ILogger<RequestHandler> logger)
            {
                _dbContext = dbContext;
                _userContext = userContext;
                _logger = logger;
            }


            public async Task<ViewModel> Handle(Request request, CancellationToken cancellationToken)
            {
                var customer = await _dbContext.Customer.FirstOrDefaultAsync(x => x.IdentityKey == _userContext.Id.ToString());

                if(customer == null)
                {
                    _logger.LogWarning("No customer found for user {UserEmail}", _userContext.EmailAddress);
                    return new ViewModel(new List<Invoice>());
                }

                var invoices = await _dbContext.Invoice
                    .Where(x => x.CustomerId == customer.Id)
                    .OrderByDescending(x => x.PeriodEnd)
                    .ToListAsync();

                _logger.LogInformation("Returning {InvoiceCount} invoices for user {UserEmail}", invoices.Count, _userContext.EmailAddress);

                return new ViewModel(invoices); ;
            }
        }
    }
}
