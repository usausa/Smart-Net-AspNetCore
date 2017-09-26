namespace WebApplication.Mvc.Controllers
{
    using System.Diagnostics;

    using Microsoft.AspNetCore.Mvc;

    using WebApplication.Models;

    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Forbidden()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
