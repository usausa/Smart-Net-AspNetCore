namespace Example.WebApplication.Controllers
{
    using System.Diagnostics;

    using Example.WebApplication.Models;

    using Microsoft.AspNetCore.Mvc;

    public class ErrorController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
