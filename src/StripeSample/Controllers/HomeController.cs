using MediatR;
using Microsoft.AspNetCore.Mvc;
using StripeSample.Handlers;
using System.Threading.Tasks;

namespace StripeSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new HomePage.Request());
            return View(result);
        }
    }
}
