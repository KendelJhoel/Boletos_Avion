using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Boletos_Avion.Services;

namespace Boletos_Avion.Controllers
{
    public class VuelosController : Controller
    {
        private readonly VuelosService _vuelosService;

        public VuelosController(VuelosService vuelosService)
        {
            _vuelosService = vuelosService;
        }

        public IActionResult Resultados()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BuscarVuelos(string origen, string destino, DateTime? fechaIda, decimal? precioMin, decimal? precioMax, string aerolinea, string categoria)
        {
            try
            {
                var vuelos = _vuelosService.GetVuelos(origen, destino, fechaIda, precioMin, precioMax, aerolinea, categoria);

                // Guardar valores en ViewBag para que se mantengan después de la búsqueda
                ViewBag.Origen = origen;
                ViewBag.Destino = destino;
                ViewBag.FechaIda = fechaIda?.ToString("yyyy-MM-dd"); // Formato compatible con input date
                ViewBag.PrecioMin = precioMin;
                ViewBag.PrecioMax = precioMax;
                ViewBag.Aerolinea = aerolinea;
                ViewBag.Categoria = categoria;

                return View("~/Views/Home/Index.cshtml", vuelos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al buscar vuelos. Intenta de nuevo.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Detalle(int id)
        {
            Vuelo vuelo = _vuelosService.GetVueloDetallesById(id);

            if (vuelo == null)
            {
                return NotFound();
            }

            return View(vuelo);
        }

        public IActionResult SeleccionarAsientos(int idVuelo)
        {
            var vuelo = _vuelosService.GetVueloById(idVuelo);
            if (vuelo == null)
            {
                TempData["Error"] = "El vuelo seleccionado no existe.";
                return RedirectToAction("Index");
            }

            ViewBag.IdVuelo = idVuelo;
            ViewBag.PrecioVuelo = vuelo.PrecioBase; // ✅ Pasamos el precio del vuelo a la vista

            return View();
        }
    }
}
