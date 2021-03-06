﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StripeSample.Infrastructure.Data;
using StripeSample.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace StripeSample.Handlers
{
    public static class RemainActive
    {
        public class Request : IRequest
        {
            public Guid SubscriptionId { get; set; }
        }

        
        public class RequestHandler : IRequestHandler<Request, Unit>
        {
            private readonly SubscriptionsContext _dbContext;
            private readonly UserContext _userContext;
            private readonly IStripeService _stripeService;
            private readonly ILogger<RequestHandler> _logger;

            public RequestHandler(SubscriptionsContext dbContext, UserContext userContext, IStripeService stripeService, ILogger<RequestHandler> logger)
            {
                _dbContext = dbContext;
                _userContext = userContext;
                _stripeService = stripeService;
                _logger = logger;
            }


            public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var subscription = await _dbContext.Subscription
                    .Include(x => x.Customer)
                    .FirstOrDefaultAsync(x => x.Id == request.SubscriptionId && x.Customer.IdentityKey == _userContext.Id.ToString());

                if(subscription == null)
                {
                    return Unit.Value;
                }

                await _stripeService.UpdateSubscriptionAsync(subscription.ExternalKey, options => options.CancelAtPeriodEnd = false);

                _logger.LogInformation("Subscription {SubscriptionId} cancel at period end set to false.  Stripe subscription identifier is {StripeSubscriptionId}", subscription.Id, subscription.ExternalKey);

                return Unit.Value;
            }
        }
    }
}
