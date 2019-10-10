using Hangfire;
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
        private readonly StripeService _stripeService;
        private readonly UserContext _userContext;
        private readonly ILogger _logger;
        private readonly IBackgroundJobClient _backgroundServer;
        private readonly StripeSettings _stripeSettings;
        private readonly TestData _testData;

        public HomeController(ApplicationDbContext dbContext, StripeService stripeService, UserContext userContext, IOptions<TestData> testData, IOptions<StripeSettings> stripeSettings, ILogger<HomeController> logger, IBackgroundJobClient backgroundServer)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
            _userContext = userContext;
            _logger = logger;
            _backgroundServer = backgroundServer;
            _stripeSettings = stripeSettings.Value;
            _testData = testData.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Purchase()
        {
            var subscriptions = await _stripeService.ListSubscriptions(_userContext.CustomerId);
            ViewBag.Subscriptions = subscriptions;
            ViewBag.HasSubscription = subscriptions.Any();

            if (!subscriptions.Any())
            {
                var session = await _stripeService.CreateCheckoutSession(_testData.PlanId, _userContext.CustomerId);

                var cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    CartState = CartState.Created,
                    SessionId = session.Id,
                    User = _userContext.GetUser()
                };

                _dbContext.Cart.Add(cart);

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("{Entity} was {Action}.  Details: {CartId} {CartState} {CartSession}", "Cart", "Created", cart.Id, cart.CartState, session.Id);

                ViewBag.CheckoutSessionId = session.Id;
            }
            return View();
        }

        public async Task<IActionResult> Subscription()
        {
            var subscriptions = await _stripeService.ListSubscriptions(_userContext.CustomerId);
            ViewBag.HasSubscription = subscriptions.Any();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CancelSubscription()
        {
            var subscriptions = await _stripeService.ListSubscriptions(_userContext.CustomerId);
            await _stripeService.CancelSubscription(subscriptions[0].Id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Success(string sessionId)
        {
            var cart = await _dbContext.Cart.FirstOrDefaultAsync(e => e.SessionId == sessionId);

            if (cart != null)
            {
                cart.CartState = CartState.Fulfilled;
                cart.ModifiedDateTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("{Entity} was {Action}.  Details: {CartId} {CartState} {CartSession} {IsEcommerce}", "Cart", "Fulfilled", cart.Id, cart.CartState, cart.SessionId, true);
            } else
            {
                _logger.LogWarning("{Entity} was {Action}.  Details: Unable to find a cart with {CartSession} {IsEcommerce}", "Cart", "Fulfilled", sessionId, true);
            }

            return RedirectToAction(nameof(Purchase));
        }

        public async Task<IAsyncResult> Webhook()
        {
            Event stripeEvent;

            var secret = _stripeSettings.WebhookSecret;
            string json = "";

            using (var stream = new StreamReader(HttpContext.Request.Body))
            {
                json = await stream.ReadToEndAsync();
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret, throwOnApiVersionMismatch: false);

                if (stripeEvent == null)
                {
                    throw new InvalidOperationException("Unable to extract event.");
                }
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
                else if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
                {

                    var job = new StripeJob { Payload = json };

                    _dbContext.StripeJob.Add(job);
                    await _dbContext.SaveChangesAsync();

                    _backgroundServer.Enqueue(() => ProcessSubscription(job.Id, "Created", Request.Headers["Stripe-Signature"]));

                    _logger.LogInformation("Webhook: Job queued for Customer Subscription Created.  {JobId}", job.Id);
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionUpdated /*|| stripeEvent.Type == Events.CustomerSubscriptionCreated || stripeEvent.Type == Events.CustomerSubscriptionDeleted */)
                {

                    var job = new StripeJob { Payload = json };

                    _dbContext.StripeJob.Add(job);
                    await _dbContext.SaveChangesAsync();

                    _backgroundServer.Enqueue(() => ProcessSubscription(job.Id, "Updated", Request.Headers["Stripe-Signature"]));

                    _logger.LogInformation("Webhook: Job queued for Customer Subscription Update.  {JobId}", job.Id);
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {

                    var job = new StripeJob { Payload = json };

                    _dbContext.StripeJob.Add(job);
                    await _dbContext.SaveChangesAsync();

                    _backgroundServer.Enqueue(() => ProcessSubscriptionDeleted(job.Id, Request.Headers["Stripe-Signature"]));

                    _logger.LogInformation("Webhook: Job queued for Customer Subscription Deleted.  {JobId}", job.Id);
                }
                else if (stripeEvent.Type == Events.InvoiceFinalized || stripeEvent.Type == Events.InvoicePaymentSucceeded)
                {

                    var job = new StripeJob { Payload = json };

                    _dbContext.StripeJob.Add(job);
                    await _dbContext.SaveChangesAsync();

                    _backgroundServer.Schedule(() => ProcessInvoice(job.Id, stripeEvent.Type.ToString(), Request.Headers["Stripe-Signature"]),
                        TimeSpan.FromMinutes(1));

                    _logger.LogInformation($"Webhook: Job queued for Invoice {stripeEvent.Type}.  {{JobId}}", job.Id);
                }
                else if (stripeEvent.Type == Events.InvoicePaymentFailed)
                {

                    var job = new StripeJob { Payload = json };

                    _dbContext.StripeJob.Add(job);
                    await _dbContext.SaveChangesAsync();

                    _backgroundServer.Schedule(() => ProcessInvoiceFailed(job.Id, Request.Headers["Stripe-Signature"]),
                        TimeSpan.FromMinutes(1));

                    _logger.LogInformation($"Webhook: Job queued for Invoice {stripeEvent.Type}.  {{JobId}}", job.Id);
                }


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
                _logger.LogWarning("Error parsing {StripeEventId} : DataObject {Object} for Type {Type}", stripeEvent.Id, stripeEvent.Data.Object, stripeEvent.Type);
                throw new InvalidOperationException("Unable to parse request data.");
            }

            return data;
        }

        public async Task ProcessSubscription(Guid jobId, string action, string stripeSignature)
        {
            var secret = _stripeSettings.WebhookSecret;
            var job = await _dbContext.StripeJob.FirstOrDefaultAsync(x => x.Id == jobId);

            Event stripeEvent = null;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(job.Payload, stripeSignature, secret, throwOnApiVersionMismatch: false);

                if (stripeEvent == null)
                {
                    throw new InvalidOperationException("Unable to extract event.");
                }

                _logger.LogDebug("Stripe event {StripeEvent} received {StripeEventId} data {StripeEventPayload} {IsEcommerce}", stripeEvent, stripeEvent.Id, stripeEvent.Data, true);
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing {Entity} with {Action} {IsEcommerce}", "Subscription", action, true);
                throw;
            }


            var stripeSubscription = ParseStripePayload<Stripe.Subscription>(stripeEvent);
            var subscription = await EnsureSubscriptionAsync(stripeSubscription.Id);

            var state = SubscriptionState.None;
            Enum.TryParse(stripeSubscription.Status, true, out state);
            subscription.State = state;
            subscription.ModifiedDateTime = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("{Entity} was {Action}.  Details: {StripeSubscriptionId} {SubscriptionId} {SubscriptionState} {LatestInvoiceId} {IsEcommerce}", "Subscription", action, stripeSubscription.Id, subscription.Id, state, stripeSubscription.LatestInvoiceId, true);

            await EnsureInvoiceAsync(stripeSubscription.LatestInvoiceId, subscription);
        }

        public async Task ProcessInvoice(Guid jobId, string action, string stripeSignature)
        {
            var secret = _stripeSettings.WebhookSecret;
            var job = await _dbContext.StripeJob.FirstOrDefaultAsync(x => x.Id == jobId);

            Event stripeEvent = null;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(job.Payload, stripeSignature, secret, throwOnApiVersionMismatch: false);

                if (stripeEvent == null)
                {
                    throw new InvalidOperationException("Unable to extract event.");
                }

                _logger.LogDebug("Stripe event {StripeEvent} received {StripeEventId} data {StripeEventPayload} {IsEcommerce}", stripeEvent, stripeEvent.Id, stripeEvent.Data, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing {Entity} {IsEcommerce}", "Invoice", true);
                throw;
            }

            var stripeInvoice = ParseStripePayload<Stripe.Invoice>(stripeEvent);
            var subscription = await EnsureSubscriptionAsync(stripeInvoice.SubscriptionId);
            var invoice = await EnsureInvoiceAsync(stripeInvoice.Id, subscription);

            _logger.LogInformation("{Entity} was {Action}.  Details: {SubscriptionId} {InvoiceId} {InvoiceStatus} {IsEcommerce}", "Invoice", action, subscription.Id, invoice.Id, invoice.Status, true);
        }


        public async Task ProcessInvoiceFailed(Guid jobId, string stripeSignature)
        {
            var secret = _stripeSettings.WebhookSecret;
            var job = await _dbContext.StripeJob.FirstOrDefaultAsync(x => x.Id == jobId);

            Event stripeEvent = null;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(job.Payload, stripeSignature, secret, throwOnApiVersionMismatch: false);

                if (stripeEvent == null)
                {
                    throw new InvalidOperationException("Unable to extract event.");
                }

                _logger.LogDebug("Stripe event {StripeEvent} received {StripeEventId} data {StripeEventPayload} {IsEcommerce}", stripeEvent, stripeEvent.Id, stripeEvent.Data, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing {Entity} {IsEcommerce}", "Invoice", true);
                throw;
            }

            var stripeInvoice = ParseStripePayload<Stripe.Invoice>(stripeEvent);
            var subscription = await EnsureSubscriptionAsync(stripeInvoice.SubscriptionId);
            var invoice = await EnsureInvoiceAsync(stripeInvoice.Id, subscription);

            _logger.LogInformation("{Entity} was {Action}.  Details: {SubscriptionId} {InvoiceId} {InvoiceStatus} {IsEcommerce}", "Invoice", "Failed", subscription.Id, invoice.Id, invoice.Status, true);
        }


        public async Task ProcessSubscriptionDeleted(Guid jobId, string stripeSignature)
        {
            var secret = _stripeSettings.WebhookSecret;
            var job = await _dbContext.StripeJob.FirstOrDefaultAsync(x => x.Id == jobId);

            Event stripeEvent = null;

            try
            {

                stripeEvent = EventUtility.ConstructEvent(job.Payload, stripeSignature, secret, throwOnApiVersionMismatch: false);

                if (stripeEvent == null)
                {
                    throw new InvalidOperationException("Unable to extract event.");
                }

                _logger.LogDebug("Stripe event {StripeEvent} received {StripeEventId} data {StripeEventPayload}", stripeEvent, stripeEvent.Id, stripeEvent.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing {Entity} with {Action} {IsEcommerce}", "Subscription", "Deleted", true);
                throw;
            }

            var stripeSubscription = ParseStripePayload<Stripe.Subscription>(stripeEvent);
            var subscription = await _dbContext.Subscription.FirstOrDefaultAsync(x => x.SubscriptionId == stripeSubscription.Id);

            if(subscription != null)
            {
                var state = SubscriptionState.None;
                Enum.TryParse(stripeSubscription.Status, true, out state);
                subscription.State = state;
                subscription.ModifiedDateTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("{Entity} was {Action}.  Details: {StripeSubscriptionId} {SubscriptionId} {SubscriptionState} {LatestInvoiceId} {IsEcommerce}", "Subscription", "Deleted", stripeSubscription.Id, subscription.Id, state, stripeSubscription.LatestInvoiceId, true);
            }
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

                _logger.LogInformation("Created subscription for {StripeSubscriptionId} with {ApplicationSubscriptionId} {IsEcommerce}", subscriptionId, subscription.Id, true);
            }

            return subscription;
        }


        private async Task<Entities.Invoice> EnsureInvoiceAsync(string invoiceId, Entities.Subscription subscription)
        {
            var invoice = await _dbContext.Invoice.FirstOrDefaultAsync(e => e.InvoiceId == invoiceId);

            var stripeInvoice = await _stripeService.GetInvoice(invoiceId);
            var status = InvoiceStatus.None;
            Enum.TryParse(stripeInvoice.Status, true, out status);

            if (invoice == null)
            {
                _logger.LogInformation("Creating invoice for {StripeInvoiceId}", invoiceId);

                invoice = new Entities.Invoice
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = stripeInvoice.Id,
                    InvoiceNumber = stripeInvoice.Number,
                    AmountDue = stripeInvoice.AmountDue,
                    AmountPaid = stripeInvoice.AmountPaid,
                    AmountRemaining = stripeInvoice.AmountRemaining,
                    BillingReason = stripeInvoice.BillingReason,
                    InvoicePdfUrl = stripeInvoice.HostedInvoiceUrl,
                    PeriodEnd = stripeInvoice.PeriodEnd,
                    PeriodStart = stripeInvoice.PeriodStart,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    Status = status,
                    Subscription = subscription
                };

                _dbContext.Invoice.Add(invoice);

                _logger.LogInformation("Created invoice for {StripeInvoiceId} with {ApplicationInvoiceId} {IsEcommerce}", invoiceId, invoice.Id, true);

            } else
            {
                invoice.Status = status;

                _logger.LogInformation("Updated invoice for {StripeInvoiceId} with {ApplicationInvoiceId} status is {StripeStatus} {IsEcommerce}", invoiceId, invoice.Id, status, true);
            }

            await _dbContext.SaveChangesAsync();
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
