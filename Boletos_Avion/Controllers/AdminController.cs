using Microsoft.AspNetCore.Mvc;

namespace Boletos_Avion.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserRole") != 1)
            {
                return RedirectToAction("Index", "Home"); // Si no es admin, lo sacamos
            }
            return View();
        }
    }
}
