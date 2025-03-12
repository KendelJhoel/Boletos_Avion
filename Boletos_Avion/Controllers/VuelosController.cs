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

        //private readonly string connectionString = "Data Source=DESKTOP-MP89LU5;Initial Catalog=GestionBoletos;User ID=jona;Password=4321;TrustServerCertificate=True;";
        private readonly string connectionString = "Data Source=DESKTOP-34DG23J\\SQLEXPRESS;Initial Catalog=GestionBoletos;User ID=sa;Password=Chiesafordel1+;TrustServerCertificate=True;";
        //private readonly string connectionString = "Data Source=DESKTOP-IT9FVD5\\SQLEXPRESS;Initial Catalog=GestionBoletos46;User ID=sa;Password=15012004;TrustServerCertificate=True;";

        private readonly DbController _dbController;

        public VuelosController( VuelosService vuelosService)
        {
            _dbController = new DbController();
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
            DbController dbController = new DbController();
            Vuelo vuelo = dbController.GetVueloDetallesById(id);

            if (vuelo == null)
            {
                return NotFound();
            }

            return View(vuelo);
        }

    }

}

