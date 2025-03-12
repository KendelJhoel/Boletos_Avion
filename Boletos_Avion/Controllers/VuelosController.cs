using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Boletos_Avion.Controllers;
using Microsoft.Data.SqlClient;
using System.Data;
using Boletos_Avion.Services;

namespace Boletos_Avion.Controllers
{
    public class VuelosController : Controller
    {
     
        private readonly VuelosService _vuelosService;

        public VuelosController( VuelosService vuelosService)
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

        //-----------
        public IActionResult Detalle(int id)
        {
            
            Vuelo vuelo = _vuelosService.GetVueloDetallesById(id);

            if (vuelo == null)
            {
                return NotFound();
            }

            return View(vuelo);
        }

    }

}

