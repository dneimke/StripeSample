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
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly StripeService _stripeService;
        private readonly UserContext _userContext;
        private readonly ILogger _logger;
        private readonly StripeSettings _stripeSettings;
        private readonly TestData _testData;

        public AccountController(ApplicationDbContext dbContext, StripeService stripeService, UserContext userContext, IOptions<TestData> testData, IOptions<StripeSettings> stripeSettings, ILogger<AccountController> logger)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
            _userContext = userContext;
            _logger = logger;
            _stripeSettings = stripeSettings.Value;
            _testData = testData.Value;
        }

        public async Task<IActionResult> Subscriptions()
        {
            var subscriptions = await _stripeService.ListSubscriptions(_userContext.CustomerId);

            if (subscriptions != null)
            {
                return View(subscriptions.FirstOrDefault());
            }

            return View();
        }

        public async Task<IActionResult> Invoices()
        {
            var subscriptions = await _stripeService.ListSubscriptions(_userContext.CustomerId);

            if (subscriptions != null && subscriptions.Count > 0)
            {
                var subscription = subscriptions.First();
                var invoices = await _stripeService.ListInvoices(subscription.Id);
                return View(invoices);
            }

            return View();
        }

        public async Task<IActionResult> Plans()
        {
            var subscriptions = await _stripeService.ListSubscriptions(_userContext.CustomerId);
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


        [HttpPost]
        public async Task<IActionResult> CancelSubscription()
        {
            var subscriptions = await _stripeService.ListSubscriptions(_userContext.CustomerId);
            await _stripeService.CancelSubscription(subscriptions[0].Id);
            return RedirectToAction(nameof(Plans));
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
            }
            else
            {
                _logger.LogWarning("{Entity} was {Action}.  Details: Unable to find a cart with {CartSession} {IsEcommerce}", "Cart", "Fulfilled", sessionId, true);
            }

            return RedirectToAction(nameof(Subscriptions));
        }

    }
}