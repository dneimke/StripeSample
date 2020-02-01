using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StripeSample.Domain;
using StripeSample.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace StripeSample.Handlers
{
    public class CheckoutSessionCompletedEventHandler
    {
        private readonly SubscriptionsContext _dbContext;
        private readonly ILogger<CheckoutSessionCompletedEventHandler> _logger;

        public CheckoutSessionCompletedEventHandler(SubscriptionsContext dbContext, ILogger<CheckoutSessionCompletedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task HandleWebhookEvent(string sessionId)
        {
            var cart = await _dbContext.Cart.FirstOrDefaultAsync(e => e.SessionId == sessionId);

            if (cart != null)
            {
                cart.CartState = CartState.Fulfilled;
                cart.LastModifiedDateTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("{Entity} was {Action}.  Details: {CartId} {CartState} {CartSession} {IsEcommerce}", "Cart", "Fulfilled", cart.Id, cart.CartState, sessionId, true);
            }
            else
            {
                _logger.LogWarning("{Entity} was {Action}.  Details: Unable to find a cart with {CartSession} {IsEcommerce}", "Cart", "Fulfilled", sessionId, true);
            }
        }
    }
}
