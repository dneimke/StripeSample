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

        public HomeController(ApplicationDbContext dbContext, StripePaymentService paymentService, UserContext userContext, IOptions<TestData> testData, IOptions<StripeSettings> stripeSettings, ILogger logger)
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

        public IActionResult Success()
        {
            return RedirectToAction(nameof(Purchase));
        }

        public async Task<IAsyncResult> Webhook()
        {
            var secret = _stripeSettings.WebhookSecret;
            using (var stream = new StreamReader(HttpContext.Request.Body))
            {
                var json = await stream.ReadToEndAsync();

                try
                {
                    var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret, throwOnApiVersionMismatch: false);

                    _logger.LogInformation("Stripe event received {StripeEvent}", stripeEvent);

                    if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
                    {
                        var data = stripeEvent.Data.Object as Stripe.Subscription;

                        if (data == null)
                        {
                            _logger.LogWarning("DataObject {Object} for Type {Type}", stripeEvent.Data.Object, stripeEvent.Type);
                            throw new InvalidOperationException("Unable to read request data for CustomerSubscriptionCreated event");
                        }

                        var subscription = new Entities.Subscription
                        {
                            Id = Guid.NewGuid(),
                            PlanId = _testData.PlanId,
                            State = SubscriptionState.Active,
                            SubscriptionId = data.Id,
                            CreatedDateTime = DateTime.Now,
                            ModifiedDateTime = DateTime.Now,
                            User = _userContext.GetUser()
                        };

                        _dbContext.Subscription.Add(subscription);
                    }
                    else if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
                    {
                        var data = stripeEvent.Data.Object as Stripe.Subscription;

                        if (data == null)
                        {
                            _logger.LogWarning("DataObject {Object} for Type {Type}", stripeEvent.Data.Object, stripeEvent.Type);
                            throw new InvalidOperationException("Unable to read request data for CustomerSubscriptionUpdated event");
                        }

                        var subscription = await _dbContext.Subscription.FirstOrDefaultAsync(e => e.SubscriptionId == data.Id);

                        if (subscription == null)
                        {
                            _logger.LogWarning("Subscription not found {Id}", data.Id);
                            throw new InvalidOperationException("Subscription not found");
                        }
                            

                        var state = SubscriptionState.None;
                        Enum.TryParse(data.Status, true, out state);
                        subscription.State = state;
                        subscription.ModifiedDateTime = DateTime.Now;
                    }
                    else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                    {
                        var data = stripeEvent.Data.Object as Stripe.Subscription;

                        if (data == null)
                        {
                            _logger.LogWarning("DataObject {Object} for Type {Type}", stripeEvent.Data.Object, stripeEvent.Type);
                            throw new InvalidOperationException("Unable to read request data for CustomerSubscriptionDeleted event");
                        }

                        var subscription = await _dbContext.Subscription.FirstOrDefaultAsync(e => e.SubscriptionId == data.Id);

                        if (subscription == null)
                        {
                            _logger.LogWarning("Subscription not found {Id}", data.Id);
                            throw new InvalidOperationException("Subscription not found");
                        }

                        var state = SubscriptionState.None;
                        Enum.TryParse(data.Status, true, out state);
                        subscription.State = state;
                        subscription.ModifiedDateTime = DateTime.Now;
                    }
                    else if (stripeEvent.Type == Events.InvoiceCreated)
                    {
                        var data = stripeEvent.Data.Object as Stripe.Invoice;

                        if (data == null)
                        {
                            _logger.LogWarning("DataObject {Object} for Type {Type}", stripeEvent.Data.Object, stripeEvent.Type);
                            throw new InvalidOperationException("Unable to read request data for InvoiceCreated event");
                        }

                        var subscription = await _dbContext.Subscription.FirstOrDefaultAsync(e => e.SubscriptionId == data.SubscriptionId);

                        if (subscription == null)
                        {
                            _logger.LogWarning("Subscription not found {Id}", data.Id);
                            throw new InvalidOperationException("Subscription not found");
                        }

                        var status = InvoiceStatus.None;
                        Enum.TryParse(data.Status, true, out status);

                        var invoice = new Entities.Invoice
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
                    }
                    else if (stripeEvent.Type == Events.InvoiceUpdated)
                    {
                        var data = stripeEvent.Data.Object as Stripe.Invoice;

                        if (data == null)
                        {
                            _logger.LogWarning("DataObject {Object} for Type {Type}", stripeEvent.Data.Object, stripeEvent.Type);
                            throw new InvalidOperationException("Unable to read request data for InvoiceUpdated event");
                        }
                            

                        var invoice = await _dbContext.Invoice.FirstOrDefaultAsync(e => e.InvoiceId == data.Id);

                        if (invoice == null)
                        {
                            _logger.LogWarning("Invoice not found {Id}", data.Id);
                            throw new InvalidOperationException("Invoice not found");
                        }

                        var status = InvoiceStatus.None;
                        Enum.TryParse(data.Status, true, out status);

                        invoice.AmountDue = data.AmountDue;
                        invoice.AmountPaid = data.AmountPaid;
                        invoice.AmountRemaining = data.AmountRemaining;
                        invoice.Status = status;
                        invoice.ModifiedDateTime = DateTime.Now;
                    }
                    else if (stripeEvent.Type == Events.InvoicePaymentSucceeded || stripeEvent.Type == Events.InvoicePaymentFailed)
                    {
                        var data = stripeEvent.Data.Object as Stripe.Invoice;

                        if (data == null)
                        {
                            _logger.LogWarning("DataObject {Object} for Type {Type}", stripeEvent.Data.Object, stripeEvent.Type);
                            throw new InvalidOperationException("Unable to read request data for PaymentSucceeded/Failed event");
                        }

                        var invoice = await _dbContext.Invoice.FirstOrDefaultAsync(e => e.InvoiceId == data.Id);

                        if (invoice == null)
                            throw new InvalidOperationException("Invoice not found");

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
                    return StatusCode(500) as IAsyncResult;
                }
                catch (InvalidOperationException e)
                {
                    return StatusCode(400) as IAsyncResult;
                }
                catch (Exception e)
                {
                    return StatusCode(500) as IAsyncResult;
                }
            }
            
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
}
