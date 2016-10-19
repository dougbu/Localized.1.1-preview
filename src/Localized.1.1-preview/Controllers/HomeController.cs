using Localized._1._1_preview.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Localized._1._1_preview.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(IStringLocalizer<HomeController> localizer)
        {
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = _localizer["Your application description page."];

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = _localizer["Your contact page."];

            return View();
        }

        [HttpPost]
        public IActionResult Contact(Model model)
        {
            ViewData["Message"] = _localizer["Your contact page."];
            if (ModelState.IsValid)
            {
                return RedirectToAction(actionName: "Index");
            }

            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
