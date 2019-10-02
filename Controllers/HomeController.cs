using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using StripeSample.Entities;
using StripeSample.Infrastructure.Data;
using StripeSample.Models;
using StripeSample.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StripeSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly StripePaymentService _paymentService;
        private readonly UserContext _userContext;
        private readonly ILogger _logger;
        private readonly StripeSettings _stripeSettings;
        private readonly TestData _testData;

        public HomeController(ApplicationDbContext dbContext, StripePaymentService paymentService, UserContext userContext, IOptions<TestData> testData, IOptions<StripeSettings> stripeSettings, ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _paymentService = paymentService;
            _userContext = userContext;
            _logger = logger;
            _stripeSettings = stripeSettings.Value;
            _testData = testData.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Purchase()
        {
            var subscriptions = await _paymentService.ListSubscriptions(_userContext.CustomerId);
            ViewBag.Subscriptions = subscriptions;
            ViewBag.HasSubscription = subscriptions.Any();

            if (!subscriptions.Any())
            {
                var session = await _paymentService.CreateCheckoutSession(_testData.PlanId, _userContext.CustomerId);
                _dbContext.Cart.Add(new Cart
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    CartState = CartState.Created,
                    SessionId = session.Id,
                    User = _userContext.GetUser()
                });

                await _dbContext.SaveChangesAsync();

                ViewBag.CheckoutSessionId = session.Id;
            }
            return View();
        }

        public async Task<IActionResult> Subscription()
        {
            var subscriptions = await _paymentService.ListSubscriptions(_userContext.CustomerId);
            ViewBag.HasSubscription = subscriptions.Any();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CancelSubscription()
        {
            var subscriptions = await _paymentService.ListSubscriptions(_userContext.CustomerId);
            await _paymentService.CancelSubscription(subscriptions[0].Id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Success(string sessionId)
        {
            var cart = await _dbContext.Cart.FirstOrDefaultAsync(e => e.SessionId == sessionId);

            _logger.LogWarning("{Cart} successfully created using {CartSession}", cart, sessionId);

            return RedirectToAction(nameof(Purchase));
        }

        public async Task<IAsyncResult> Webhook()
        {
            Event stripeEvent;

            var secret = _stripeSettings.WebhookSecret;
            using (var stream = new StreamReader(HttpContext.Request.Body))
            {
                var json = await stream.ReadToEndAsync();
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret, throwOnApiVersionMismatch: false);

                if(stripeEvent == null)
                {
                    throw new InvalidOperationException("Unable to extract event.");
                }

                _logger.LogInformation("Stripe event {StripeEvent} received {StripeEventId} data {StripeEventPayload}", stripeEvent, stripeEvent.Id, stripeEvent.Data);
            }

            try
            {
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var data = ParseStripePayload<Stripe.Checkout.Session>(stripeEvent);
                    _logger.LogInformation("Webhook: Checkout Session completed for Session {CartSession}", data.Id);

                    var cart = await _dbContext.Cart.FirstOrDefaultAsync(e => e.SessionId == data.Id);

                    if(cart != null)
                    {
                        cart.CartState = CartState.Fulfilled;
                        cart.ModifiedDateTime = DateTime.Now;
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionCreated || stripeEvent.Type == Events.CustomerSubscriptionUpdated || stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    var data = ParseStripePayload<Stripe.Subscription>(stripeEvent);
                    _logger.LogInformation("Webhook: Processing {StripeEventType} ({StripeEventId}) for {StripeSubscriptionId}", stripeEvent.Type, stripeEvent.Id, data.Id);

                    var subscription = await EnsureSubscriptionAsync(data.Id);

                    _logger.LogInformation("Webhook: Processing {StripeEventType} ({StripeEventId}), have subscription for {StripeSubscriptionId} with {ApplicationSubscriptionId}", stripeEvent.Type, stripeEvent.Id, data.Id, subscription.Id);

                    var state = SubscriptionState.None;
                    Enum.TryParse(data.Status, true, out state);
                    subscription.State = state;
                    subscription.ModifiedDateTime = DateTime.Now;
                }
                else if (stripeEvent.Type == Events.InvoiceUpdated || stripeEvent.Type == Events.InvoicePaymentSucceeded || stripeEvent.Type == Events.InvoicePaymentFailed)
                {
                    var data = ParseStripePayload<Stripe.Invoice>(stripeEvent);
                    _logger.LogInformation("Webhook: Processing {StripeEventType} ({StripeEventId}) for {StripeSubscriptionId}", stripeEvent.Type, stripeEvent.Id, data.Id);

                    var invoice = await EnsureInvoiceAsync(data);
                    _logger.LogInformation("Webhook: Processing {StripeEventType} ({StripeEventId}), have invoice for {StripeInvoiceId} with {ApplicationInvoiceId}", stripeEvent.Type, stripeEvent.Id, data.Id, invoice.Id);

                    var status = InvoiceStatus.None;
                    Enum.TryParse(data.Status, true, out status);

                    invoice.AmountDue = data.AmountDue;
                    invoice.AmountPaid = data.AmountPaid;
                    invoice.AmountRemaining = data.AmountRemaining;
                    invoice.Status = status;
                    invoice.ModifiedDateTime = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();
                return Ok() as IAsyncResult;
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "StripeException occurred");
                // return StatusCode(500) as IAsyncResult;
                throw;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "InvalidOperationException occurred");
                // return StatusCode(500) as IAsyncResult;
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred");
                // return StatusCode(500) as IAsyncResult;
                throw;
            }


        }

        private T ParseStripePayload<T>(Event stripeEvent)
        {
            if (!(stripeEvent.Data.Object is T data))
            {
                _logger.LogWarning(LoggingEvents.CustomerSubscriptionCreated, "{StripeEventId} : DataObject {Object} for Type {Type}", stripeEvent.Id, stripeEvent.Data.Object, stripeEvent.Type);
                throw new InvalidOperationException("Unable to parse request data.");
            }

            return data;
        }

        private async Task<Entities.Subscription> EnsureSubscriptionAsync(string subscriptionId)
        {
            var subscription = await _dbContext.Subscription.FirstOrDefaultAsync(e => e.SubscriptionId == subscriptionId);

            if (subscription == null)
            {
                _logger.LogInformation("Creating subscription for {StripeSubscriptionId}", subscriptionId);

                subscription = new Entities.Subscription
                {
                    Id = Guid.NewGuid(),
                    PlanId = _testData.PlanId,
                    State = SubscriptionState.Active,
                    SubscriptionId = subscriptionId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    User = _userContext.GetUser()
                };

                _dbContext.Subscription.Add(subscription);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Created subscription for {StripeSubscriptionId} with {ApplicationSubscriptionId}", subscriptionId, subscription.Id);
            }

            return subscription;
        }


        private async Task<Entities.Invoice> EnsureInvoiceAsync(Stripe.Invoice data)
        {
            var invoice = await _dbContext.Invoice.FirstOrDefaultAsync(e => e.InvoiceId == data.Id);

            if (invoice == null)
            {
                _logger.LogInformation("Creating invoice for {StripeInvoiceId}", data.Id);

                var subscription = await EnsureSubscriptionAsync(data.SubscriptionId);

                var status = InvoiceStatus.None;
                Enum.TryParse(data.Status, true, out status);

                invoice = new Entities.Invoice
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = data.Id,
                    InvoiceNumber = data.Number,
                    AmountDue = data.AmountDue,
                    AmountPaid = data.AmountPaid,
                    AmountRemaining = data.AmountRemaining,
                    BillingReason = data.BillingReason,
                    InvoicePdfUrl = data.HostedInvoiceUrl,
                    PeriodEnd = data.PeriodEnd,
                    PeriodStart = data.PeriodStart,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    Status = status,
                    Subscription = subscription
                };

                _dbContext.Invoice.Add(invoice);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Created invoice for {StripeInvoiceId} with {ApplicationInvoiceId}", data.Id, invoice.Id);
            }

            return invoice;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


    public class LoggingEvents
    {
        public const int CustomerSubscriptionCreated = 1000;
        public const int CustomerSubscriptionUpdated = 1001;
        public const int CustomerSubscriptionDeleted = 1002;
        public const int InvoiceCreated = 1003;
        public const int InvoiceUpdated = 1004;
        public const int InvoiceResult = 1005;
    }
}
