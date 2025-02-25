using System.Diagnostics;
using Boletos_Avion.Models;
using Microsoft.AspNetCore.Mvc;

namespace Boletos_Avion.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Verificar que el usuario esté autenticado
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                ViewBag.UserRole = HttpContext.Session.GetInt32("UserRole");
            }
            return View();
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
