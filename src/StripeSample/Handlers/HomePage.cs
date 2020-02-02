using MediatR;
using Microsoft.EntityFrameworkCore;
using StripeSample.Domain;
using StripeSample.Infrastructure.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StripeSample.Handlers
{
    public static class HomePage
    {
        public class Request : IRequest<ViewModel>
        {

        }

        public class ViewModel
        {
            public ViewModel(bool hasSubscription)
            {
                HasSubscription = hasSubscription;
            }

            public bool HasSubscription { get; set; } = false;
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
                    .Include(x => x.Subscriptions).ThenInclude(x => x.Status)
                    .FirstOrDefaultAsync(x => x.IdentityKey == _userContext.Id.ToString());

                if (customer == null)
                {
                    return new ViewModel(false);
                }

                var hasSubscription = customer.Subscriptions.Any(x => x.Status.Id == SubscriptionStatus.Active.Id);
                return new ViewModel(hasSubscription);
            }
        }
    }
}
