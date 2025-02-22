using Microsoft.AspNetCore.Mvc;

namespace Boletos_Avion.Controllers
{
    public class AgentController : Controller
    {
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserRole") != 2)
            {
                return RedirectToAction("Index", "Home"); // Si no es agente, lo sacamos
            }
            return View();
        }
    }
}
