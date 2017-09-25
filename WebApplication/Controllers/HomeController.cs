namespace WebApplication.Controllers
{
    using System.Diagnostics;

    using Microsoft.AspNetCore.Mvc;

    using WebApplication.Models;

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
