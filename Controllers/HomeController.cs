using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
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
        private readonly TestData _testData;

        public HomeController(ApplicationDbContext dbContext, StripePaymentService paymentService, UserContext userContext, IOptions<TestData> testData)
        {
            _dbContext = dbContext;
            _paymentService = paymentService;
            _userContext = userContext;
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
            var customers = await _paymentService.ListCustomers();
            ViewBag.Customers = customers;

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
            const string secret = "whsec_1q2QIM59vyFmD9DydHqnCafAGjMtVT04";
            using var stream = new StreamReader(HttpContext.Request.Body);
            var json = await stream.ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret, throwOnApiVersionMismatch: false);

                if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
                {
                    var data = stripeEvent.Data.Object as Stripe.Subscription;

                    var subscription = new Entities.Subscription
                    {
                        Id = Guid.NewGuid(),
                        PlanId = _testData.PlanId,
                        State = Entities.SubscriptionState.Active,
                        SubscriptionId = data.Id,
                        User = _userContext.GetUser()
                    };

                    _dbContext.Subscription.Add(subscription);
                    await _dbContext.SaveChangesAsync();

                    Console.WriteLine($"{data.CustomerId} created a subscription.");
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    var data = stripeEvent.Data.Object as Stripe.Subscription;
                    Console.WriteLine($"{data.CustomerId} deleted a subscription.");

                    var subscription = _dbContext.Subscription
                        .FirstOrDefault(e => e.SubscriptionId == data.Id);

                    _dbContext.Subscription.Remove(subscription);
                    await _dbContext.SaveChangesAsync();
                }
                else if (stripeEvent.Type == Events.ChargeSucceeded)
                {
                    var data = stripeEvent.Data.Object as Stripe.Charge;
                    Console.WriteLine($"{data.CustomerId} was successfully charged {data.Amount}.");
                }
                else if (stripeEvent.Type == Events.ChargeFailed)
                {
                    var data = stripeEvent.Data.Object as Stripe.Charge;
                    Console.WriteLine($"{data.CustomerId} was not successfully charged {data.Amount}.");
                }

                return Ok() as IAsyncResult;
            }
            catch (StripeException e)
            {
                Console.WriteLine(e);
                return StatusCode(500) as IAsyncResult; 
                
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
