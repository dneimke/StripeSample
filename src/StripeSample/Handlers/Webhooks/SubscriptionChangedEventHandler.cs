using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StripeSample.Handlers.Commands;
using StripeSample.Infrastructure.Configuration;
using StripeSample.Infrastructure.Data;
using StripeSample.Services;
using System.Threading.Tasks;

namespace StripeSample.Handlers
{
    public class SubscriptionChangedEventHandler
    {
        private readonly SubscriptionsContext _dbContext;
        private readonly IStripeService _stripeService;
        private readonly ILogger<SubscriptionChangedEventHandler> _logger;
        private readonly StripeSettings _stripeSettings;

        public SubscriptionChangedEventHandler(SubscriptionsContext dbContext, IStripeService stripeService, IOptions<StripeSettings> settings, ILogger<SubscriptionChangedEventHandler> logger)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
            _stripeSettings = settings.Value;
            _logger = logger;
        }


        public async Task HandleWebhookEvent(string subscriptionId)
        {
            await EnsureSubscriptionCommand.ExecAsync(
                    _dbContext,
                    _stripeService,
                    subscriptionId,
                    _stripeSettings,
                    _logger);
        }
    }
}
