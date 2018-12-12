namespace Example.WebApplication.Mvc.Controllers
{
    using System.Diagnostics;

    using Example.WebApplication.Mvc.Models;

    using Microsoft.AspNetCore.Mvc;

    public class ErrorController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Forbidden()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
