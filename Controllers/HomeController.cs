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

            _logger.LogWarning("{Cart} successfully created using {CartSession}", cart, sessionId);

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
                else if (stripeEvent.Type == Events.CustomerSubscriptionUpdated /*|| stripeEvent.Type == Events.CustomerSubscriptionCreated || stripeEvent.Type == Events.CustomerSubscriptionDeleted */)
                {

                    var job = new StripeJob { Payload = json };

                    _dbContext.StripeJob.Add(job);
                    await _dbContext.SaveChangesAsync();

                    _backgroundServer.Enqueue(() => ProcessSubscriptionUpdate(job.Id));
                }
                //else if (stripeEvent.Type == Events.InvoiceUpdated || stripeEvent.Type == Events.InvoicePaymentSucceeded || stripeEvent.Type == Events.InvoicePaymentFailed)
                //{
                //    var data = ParseStripePayload<Stripe.Invoice>(stripeEvent);
                //    _logger.LogInformation("Webhook: Processing {StripeEventType} ({StripeEventId}) for {StripeSubscriptionId}", stripeEvent.Type, stripeEvent.Id, data.Id);

                //    var invoice = await EnsureInvoiceAsync(data);
                //    _logger.LogInformation("Webhook: Processing {StripeEventType} ({StripeEventId}), have invoice for {StripeInvoiceId} with {ApplicationInvoiceId}", stripeEvent.Type, stripeEvent.Id, data.Id, invoice.Id);

                //    var status = InvoiceStatus.None;
                //    Enum.TryParse(data.Status, true, out status);

                //    invoice.AmountDue = data.AmountDue;
                //    invoice.AmountPaid = data.AmountPaid;
                //    invoice.AmountRemaining = data.AmountRemaining;
                //    invoice.Status = status;
                //    invoice.ModifiedDateTime = DateTime.Now;
                //}

                
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



        public async Task ProcessSubscriptionUpdate(Guid jobId)
        {
            var secret = _stripeSettings.WebhookSecret;
            var job = await _dbContext.StripeJob.FirstOrDefaultAsync(x => x.Id == jobId);

            var stripeEvent = EventUtility.ConstructEvent(job.Payload, Request.Headers["Stripe-Signature"], secret, throwOnApiVersionMismatch: false);

            if (stripeEvent == null)
            {
                throw new InvalidOperationException("Unable to extract event.");
            }

            _logger.LogDebug("Stripe event {StripeEvent} received {StripeEventId} data {StripeEventPayload}", stripeEvent, stripeEvent.Id, stripeEvent.Data);

            var stripeSubscription = ParseStripePayload<Stripe.Subscription>(stripeEvent);
            _logger.LogInformation("Webhook: Processing {StripeEventType} ({StripeEventId}) for {StripeSubscriptionId}", job.MessageType, job.Id, stripeSubscription.Id);

            var subscription = await EnsureSubscriptionAsync(stripeSubscription.Id);
            await EnsureInvoiceAsync(stripeSubscription.LatestInvoiceId, subscription);

            _logger.LogInformation("Webhook: Processing {StripeEventType} ({StripeEventId}), have subscription for {StripeSubscriptionId} with {ApplicationSubscriptionId}", stripeEvent.Type, stripeEvent.Id, stripeSubscription.Id, subscription.Id);

            var state = SubscriptionState.None;
            Enum.TryParse(stripeSubscription.Status, true, out state);
            subscription.State = state;
            subscription.ModifiedDateTime = DateTime.Now;
            await _dbContext.SaveChangesAsync();
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

                _logger.LogInformation("Created invoice for {StripeInvoiceId} with {ApplicationInvoiceId}", invoiceId, invoice.Id);

            } else
            {
                invoice.Status = status;

                _logger.LogInformation("Updated invoice for {StripeInvoiceId} with {ApplicationInvoiceId} status is {StripeStatus}", invoiceId, invoice.Id, status);
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
