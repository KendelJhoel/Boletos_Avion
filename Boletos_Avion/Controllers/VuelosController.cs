using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Boletos_Avion.Services;

using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net.Mail;
using System.Net;

namespace Boletos_Avion.Controllers
{
    public class VuelosController : Controller
    {
        private readonly VuelosService _vuelosService;
        private readonly IConfiguration _configuration;
        private readonly AgentService _agentService;

        public VuelosController(VuelosService vuelosService, IConfiguration configuration, AgentService agentService)
        {
            _vuelosService = vuelosService;
            _configuration = configuration;
            _agentService = agentService;
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

                ViewBag.Origen = origen;
                ViewBag.Destino = destino;
                ViewBag.FechaIda = fechaIda?.ToString("yyyy-MM-dd"); 
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

        public IActionResult SeleccionarAsientos(int idVuelo, bool esRegreso = false)
        {
            var vuelo = _vuelosService.GetVueloById(idVuelo);
            if (vuelo == null)
            {
                TempData["Error"] = "El vuelo seleccionado no existe.";
                return RedirectToAction("Index");
            }

            ViewBag.IdVuelo = idVuelo;
            ViewBag.PrecioVuelo = vuelo.PrecioBase;
            var clients = _agentService.GetAllClients();
            ViewBag.Clientes = clients;

            HttpContext.Session.SetString("EsRegreso", esRegreso.ToString());

            return View();
        }

        public IActionResult VuelosDeVuelta(string origen, string destino, DateTime? fechaIda, int? idCliente, bool esRegreso = false)
        {
            if (esRegreso && idCliente.HasValue)
            {
                HttpContext.Session.SetInt32("IdClienteRegreso", idCliente.Value); 
                ViewBag.IdCliente = idCliente.Value;
            }
            try
            {

                Console.WriteLine($"[VUELOS DE VUELTA] Buscando de {origen} a {destino} después de {fechaIda}");

                var vuelosVuelta = _vuelosService.GetVuelos(origen, destino, fechaIda, null, null, null, null)
                    .Where(v => v.FechaSalida.Date > fechaIda.Value.Date)
                    .ToList();

                Console.WriteLine($"[VUELOS DE VUELTA] Encontrados: {vuelosVuelta.Count}");

                return View(vuelosVuelta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        public IActionResult GeneratePdf(int id)
        {
            var vuelo = _vuelosService.GetVueloDetallesById(id);
            if (vuelo == null)
                return NotFound();

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // Encabezado del PDF
                    page.Header().Text($"Detalle del Vuelo: {vuelo.CodigoVuelo}")
                                  .SemiBold()
                                  .FontSize(20)
                                  .FontColor(Colors.Blue.Medium);

                    // Contenido del PDF
                    page.Content().Column(column =>
                    {
                        column.Item().Text($"Aerolínea: {vuelo.NombreAerolinea}");
                        column.Item().Text($"Precio Base: {vuelo.PrecioBase.ToString("N2")}");
                        column.Item().Text($"Duración: {(vuelo.FechaLlegada - vuelo.FechaSalida).TotalHours.ToString("0.0")} hrs");

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Origen:").SemiBold();
                        column.Item().Text($"País: {vuelo.PaisOrigen}");
                        column.Item().Text($"Ciudad: {vuelo.CiudadOrigen}");
                        column.Item().Text($"Aeropuerto: {vuelo.NombreAeropuertoOrigen}");
                        column.Item().Text($"Fecha Salida: {vuelo.FechaSalida.ToString("dddd d 'de' MMMM yyyy")} a las {vuelo.FechaSalida.ToString("hh:mm tt")}");

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Destino:").SemiBold();
                        column.Item().Text($"País: {vuelo.PaisDestino}");
                        column.Item().Text($"Ciudad: {vuelo.CiudadDestino}");
                        column.Item().Text($"Aeropuerto: {vuelo.NombreAeropuertoDestino}");
                        column.Item().Text($"Fecha Llegada: {vuelo.FechaLlegada.ToString("dddd d 'de' MMMM yyyy")} a las {vuelo.FechaLlegada.ToString("hh:mm tt")}");

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Asientos Totales:").SemiBold();
                        column.Item().Text($"Business: {vuelo.AsientosBusiness}");
                        column.Item().Text($"Primera Clase: {vuelo.AsientosPrimeraClase}");
                        column.Item().Text($"Turista: {vuelo.AsientosTurista}");

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Asientos Disponibles:").SemiBold();
                        column.Item().Text($"Business: {vuelo.AsientosBusinessDisponibles}");
                        column.Item().Text($"Primera Clase: {vuelo.AsientosPrimeraClaseDisponibles}");
                        column.Item().Text($"Turista: {vuelo.AsientosTuristaDisponibles}");
                    });

                    // Pie de página
                    page.Footer().AlignCenter().Text("Generado con QuestPDF");
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "DetalleVuelo.pdf");
        }

        [HttpPost]
        public IActionResult CancelarVuelo([FromBody] dynamic data)
        {
            int idVuelo = data.idVuelo;
            Console.WriteLine($"✈ ID recibido desde frontend: {idVuelo}");

            var vuelosService = new VuelosService();
            bool exito = vuelosService.CancelarVuelo(idVuelo);

            if (exito)
                return Ok("Vuelo cancelado correctamente.");
            else
                return BadRequest("Error al cancelar el vuelo.");
        }
        [HttpGet]
        public IActionResult GuardarClienteRegreso(int idCliente)
        {
            HttpContext.Session.SetInt32("IdClienteRegreso", idCliente);
            return Ok(); // Puedes devolver algo si querés, pero no es obligatorio
        }


        [HttpPost]
        public IActionResult CrearVuelo(Vuelo vuelo, int CantidadPrimera, int CantidadBusiness, int CantidadTurista)
        {
            try
            {
                vuelo.Estado = "Disponible";
                vuelo.CantidadAsientos = CantidadPrimera + CantidadBusiness + CantidadTurista;
                vuelo.AsientosDisponibles = vuelo.CantidadAsientos;

                // ✅ Llama al método actualizado y recibe el código generado
                string codigoGenerado = _vuelosService.CrearVuelo(vuelo, CantidadPrimera, CantidadBusiness, CantidadTurista);

                // ✅ Enviar como JSON para usar SweetAlert2 en el frontend
                return Json(new { success = true, codigo = codigoGenerado });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al crear vuelo: " + ex.Message);
                return Json(new { success = false, message = "Ocurrió un error al crear el vuelo." });
            }
        }
    }
}
