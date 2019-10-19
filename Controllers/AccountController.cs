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
            var subscription = await _dbContext.Subscription.AsNoTracking()
                .FirstOrDefaultAsync(x => x.User.Id == _userContext.Id);

            if(subscription != null)
            {
                var stripeSubscription = await _stripeService.ListSubscriptions(_userContext.CustomerId);
                return View(stripeSubscription.FirstOrDefault());
            }

            return View();
        }

        public async Task<IActionResult> Invoices()
        {
            var subscription = await _dbContext.Subscription.AsNoTracking()
                .FirstOrDefaultAsync(x => x.User.Id == _userContext.Id);

            if (subscription != null)
            {
                var invoices = await _stripeService.ListInvoices(subscription.SubscriptionId);
                return View(invoices);
            }

            return View();
        }

    }
}