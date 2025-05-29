using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Boletos_Avion.Services;

namespace Boletos_Avion.Controllers
{
    public class AerolineaController : Controller
    {
        private readonly AerolineaService _aerolineaService;
        private readonly CiudadService _ciudadService;

        public AerolineaController()
        {
            _aerolineaService = new AerolineaService();
            _ciudadService = new CiudadService();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Aerolinea aerolinea)
        {
            System.Diagnostics.Debug.WriteLine("🚀 CREATE => Nombre recibido: " + aerolinea.Nombre);

            // 🔥 SIN validaciones innecesarias
            await _aerolineaService.CrearAsync(aerolinea);
            return RedirectToAction("AerolineasAdmin");
        }



        [HttpPost]
        public async Task<IActionResult> Edit(Aerolinea aerolinea)
        {
            Console.WriteLine($"✏️ EDIT => ID: {aerolinea.IdAerolinea}, Nombre: {aerolinea.Nombre}");

            // 🔥 SIN validaciones innecesarias
            await _aerolineaService.ActualizarAsync(aerolinea);
            return RedirectToAction("AerolineasAdmin");
        }

        public async Task<IActionResult> AerolineasAdmin()
        {
            if (HttpContext.Session.GetInt32("UserRole") != 1)
                return RedirectToAction("Index", "Home");

            var aerolineaService = new AerolineaService();
            var ciudadService = new CiudadService();
            var categoriaVueloService = new CategoriaVueloService();

            var aerolineas = await aerolineaService.ObtenerConCantidadVuelosAsync(); // este método ya lo usás
            var ciudades = ciudadService.ObtenerTodas();
            var categorias = categoriaVueloService.ObtenerTodas();
            var aerolineasTodas = await aerolineaService.ObtenerTodasAsync(); // para llenar el select del modal

            ViewData["Ciudades"] = ciudades;
            ViewData["CategoriasVuelo"] = categorias;
            ViewData["Aerolineas"] = aerolineasTodas;

            return View("~/Views/Admin/AerolineasAdmin.cshtml", aerolineas);
        }


        [HttpGet]
        public async Task<JsonResult> ExisteNombre(string nombre)
        {
            var lista = await _aerolineaService.ObtenerTodasAsync();
            bool existe = lista.Any(a => a.Nombre.ToLower() == nombre.Trim().ToLower());
            return Json(new { existe });
        }



    }
}
