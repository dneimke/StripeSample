using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StripeSample.Domain;
using StripeSample.Infrastructure.Configuration;
using StripeSample.Infrastructure.Data;
using StripeSample.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StripeSample.Handlers
{
    public static class UpgradeSubscription
    {
        public class Request : IRequest<ViewModel>
        {

        }

        public class ViewModel
        {
            const string _standardDescription = "Includes access to unlimited matches and groups.";

            public ViewModel(Customer customer, string emailAddress, string sessionId)
            {
                IsCustomer = customer != null;
                HasSubscription = customer != null && customer.Subscriptions.Any(x => x.Status.Id == SubscriptionStatus.Active.Id);
                EmailAddress = emailAddress;
                CheckoutSessionId = sessionId;
            }

            public string PlanTitle { get; set; } = "Standard Plan";
            public string PlanDescription { get; set; } = _standardDescription;
            public bool IsCustomer { get; set; } = false;
            public bool HasSubscription { get; set; } = false;
            public string EmailAddress { get; set; }
            public string CheckoutSessionId { get; set; } = "";
        }


        public class RequestHandler : IRequestHandler<Request, ViewModel>
        {
            private readonly SubscriptionsContext _dbContext;
            private readonly UserContext _userContext;
            private readonly IStripeService _stripeService;
            private readonly ILogger<RequestHandler> _logger;
            private readonly StripeSettings _settings;

            public RequestHandler(SubscriptionsContext dbContext, UserContext userContext, IStripeService stripeService, IOptions<StripeSettings> settings, ILogger<RequestHandler> logger)
            {
                _dbContext = dbContext;
                _userContext = userContext;
                _stripeService = stripeService;
                _logger = logger;
                _settings = settings.Value;
            }


            public async Task<ViewModel> Handle(Request request, CancellationToken cancellationToken)
            {
                var customer = await _dbContext.Customer.FirstOrDefaultAsync(x => x.IdentityKey == _userContext.Id.ToString());

                if(customer == null)
                {
                    var stripeCustomer = await _stripeService.CreateCustomerAsync(_userContext.EmailAddress);
                    customer = new Customer(_userContext.Id.ToString(), stripeCustomer.Id);
                    _dbContext.Customer.Add(customer);
                    await _dbContext.SaveChangesAsync();
                }

                var session = await _stripeService.CreateCheckoutSessionForCustomerAsync(_settings.DefaultPlanKey, customer.ExternalKey);

                var cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = DateTime.Now,
                    LastModifiedDateTime = DateTime.Now,
                    CartState = CartState.Created,
                    SessionId = session.Id,
                    Email = _userContext.EmailAddress
                };

                _dbContext.Cart.Add(cart);

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("{Entity} was {Action}.  Details: {CartId} {CartState} {CartSession}", "Cart", "Created", cart.Id, cart.CartState, session.Id);

                return new ViewModel(customer, _userContext.EmailAddress, session.Id);
            }
        }
    }
}
