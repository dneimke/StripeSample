using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StripeSample.Handlers.Commands;
using StripeSample.Infrastructure.Configuration;
using StripeSample.Infrastructure.Data;
using StripeSample.Services;
using System.Threading.Tasks;

namespace StripeSample.Handlers
{
    public class InvoiceChangedEventHandler
    {
        private readonly SubscriptionsContext _dbContext;
        private readonly IStripeService _stripeService;
        private readonly ILogger<InvoiceChangedEventHandler> _logger;
        private readonly StripeSettings _settings;

        public InvoiceChangedEventHandler(SubscriptionsContext dbContext, IStripeService stripeService, IOptions<StripeSettings> settings, ILogger<InvoiceChangedEventHandler> logger)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task HandleWebhookEvent(string subscriptionId, string invoiceId)
        {
            await EnsureSubscriptionCommand.ExecAsync(
                    _dbContext,
                    _stripeService,
                    subscriptionId,
                    _settings,
                    _logger);

            await EnsureInvoiceCommand.ExecAsync(
                _dbContext,
                _stripeService,
                invoiceId,
                _logger);
        }
    }
}
