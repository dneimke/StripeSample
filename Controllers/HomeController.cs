using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
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
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly StripePaymentService _paymentService;
        private readonly UserContext _userContext;
        private readonly TestData _testData;
        private readonly StripeSettings _stripeSettings;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, StripePaymentService paymentService, UserContext userContext, IOptions<TestData> testData, IOptions<StripeSettings> stripeSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _paymentService = paymentService;
            _userContext = userContext;
            _testData = testData.Value;
            _stripeSettings = stripeSettings.Value;
        }

        public async Task<IActionResult> Index()
        {
            var subscriptions = await _paymentService.ListSubscriptions(_userContext.CustomerId);
            ViewBag.HasSubscription = subscriptions.Any();
            return View();
        }

        public async Task<IActionResult> Customers()
        {
            var customers = await _paymentService.ListCustomers();
            var subscriptions = await _paymentService.ListSubscriptions(_userContext.CustomerId);
            ViewBag.Customers = customers;
            ViewBag.HasSubscription = subscriptions.Any();

            return View();
        }

        public async Task<IActionResult> Products()
        {
            var products = await _paymentService.ListProducts();
            var customers = await _paymentService.ListCustomers();
            var subscriptions = await _paymentService.ListSubscriptions(_userContext.CustomerId);
            ViewBag.Products = products;
            ViewBag.IsCustomerCreated = customers.Any();
            ViewBag.HasSubscription = subscriptions.Any();

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
        public async Task<IActionResult> CreatePlan()
        {
            var product = await _paymentService.CreateProduct("Product 1");
            var plan = await _paymentService.CreatePlan(product.Id, "Plan 1", 800);
            return Json(new { ProductId = product.Id, PlanId = plan.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer()
        {
            var customerId = Guid.NewGuid().ToString();
            var customer = await _paymentService.CreateCustomer(_userContext.EmailAddress, customerId);
            return Json(new { CustomerId = customer.Id, InternalCustomerId = customerId });
        }

        [HttpPost]
        public async Task<IActionResult> CancelSubscription()
        {
            var subscriptions = await _paymentService.ListSubscriptions(_userContext.CustomerId);
            await _paymentService.CancelSubscription(subscriptions[0].Id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Success()
        {
            var subscriptions = await _paymentService.ListSubscriptions(_userContext.CustomerId);
            ViewBag.HasSubscription = subscriptions.Any();
            return RedirectToAction(nameof(Index));
        }

        public async Task Webhook()
        {
            const string secret = "whsec_1q2QIM59vyFmD9DydHqnCafAGjMtVT04";
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret, throwOnApiVersionMismatch: false);

                if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
                {
                    var data = stripeEvent.Data.Object as Stripe.Subscription;
                    Console.WriteLine($"{data.CustomerId} created a subscription.");
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    var data = stripeEvent.Data.Object as Stripe.Subscription;
                    Console.WriteLine($"{data.CustomerId} deleted a subscription.");
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
            }
            catch (StripeException e)
            {
                Console.WriteLine(e);
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
