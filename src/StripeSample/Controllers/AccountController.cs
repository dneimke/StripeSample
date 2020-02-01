using MediatR;
using Microsoft.AspNetCore.Mvc;
using StripeSample.Handlers;
using System.Threading.Tasks;

namespace StripeSample.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new SubscriptionDetails.Request());
            return View(result);
        }

        public async Task<IActionResult> Invoices()
        {
            var result = await _mediator.Send(new InvoicesForCurrentUser.Request());
            return View(result);
        }

        public async Task<IActionResult> Upgrade()
        {
            var result = await _mediator.Send(new UpgradeSubscription.Request());
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> RemainActive([FromBody] RemainActive.Request request)
        {
            await _mediator.Send(request);
            return Json(new { Status = "Completed" });
        }

        [HttpPost]
        public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscription.Request request)
        {
            await _mediator.Send(request);
            return Json(new { Status = "Completed" });
        }

        public IActionResult UpgradeCB(string sessionId)
        {
            // could complete the checkout session here... in this sample, we're only handling it via webhooks

            return RedirectToAction(nameof(Index));
        }

    }
}