using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Boletos_Avion.Models;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Text.Json;



namespace Boletos_Avion.Controllers
{
    public class PagosController : Controller
    {
        private readonly AccountService _accountService;
        private readonly VuelosService _vuelosService;
        private readonly ILogger<PagosController> _logger;
        private readonly AsientoService _asientoService;
        private readonly ReservaService _reservaService;
        private readonly IConfiguration _configuration;

        public PagosController(AccountService accountService, VuelosService vuelosService,
                               ILogger<PagosController> logger, AsientoService asientoService,
                               ReservaService reservaService, IConfiguration configuration)
        {
            _accountService = accountService;
            _vuelosService = vuelosService;
            _logger = logger;
            _asientoService = asientoService;
            _reservaService = reservaService;
            _configuration = configuration;
        }
        public IActionResult Pago(int idVuelo, string asientos)
        {
            try
            {
                Console.WriteLine($"[LOG] Iniciando proceso de pago para el vuelo {idVuelo} con asientos: {asientos}");

                // ✅ Verificar si el usuario está autenticado
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    Console.WriteLine("[ERROR] Usuario no autenticado.");
                    TempData["Error"] = "Debes iniciar sesión antes de continuar.";
                    return RedirectToAction("Authentication", "Auth");
                }

                var user = _accountService.GetUserById(userId.Value);
                if (user == null)
                {
                    Console.WriteLine("[ERROR] No se pudo recuperar la información del usuario.");
                    TempData["Error"] = "No se pudo recuperar la información del usuario.";
                    return RedirectToAction("Authentication", "Auth");
                }

                // ✅ Obtener el vuelo seleccionado con su precio
                var vuelo = _vuelosService.GetVueloDetallesById(idVuelo);
                if (vuelo == null)
                {
                    Console.WriteLine($"[ERROR] Vuelo {idVuelo} no encontrado.");
                    TempData["Error"] = "El vuelo seleccionado no existe.";
                    return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo });
                }

                // ✅ Verificar si el vuelo tiene un precio válido
                if (vuelo.PrecioBase == null || vuelo.PrecioBase <= 0)
                {
                    Console.WriteLine("[ERROR] El precio del vuelo no es válido.");
                    TempData["Error"] = "No se pudo obtener el precio del vuelo.";
                    return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo });
                }

                // ✅ Pasar el precio del vuelo a la vista
                ViewBag.PrecioVuelo = vuelo.PrecioBase;

                // ✅ Verificar si se pasaron asientos válidos
                if (string.IsNullOrEmpty(asientos))
                {
                    Console.WriteLine("[ERROR] No se han seleccionado asientos.");
                    TempData["Error"] = "No has seleccionado asientos.";
                    return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo });
                }

                var asientosIds = asientos.Split(',').Select(int.Parse).ToList();
                Console.WriteLine($"[LOG] Asientos seleccionados: {string.Join(", ", asientosIds)}");

                bool disponibles = _asientoService.VerificarDisponibilidad(asientosIds);
                if (!disponibles)
                {
                    Console.WriteLine("[ERROR] Uno o más asientos ya están reservados.");
                    TempData["Error"] = "Uno o más asientos seleccionados ya están reservados.";
                    return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo });
                }

                var detallesAsientos = _asientoService.ObtenerDetallesAsientos(asientosIds);
                if (detallesAsientos == null || !detallesAsientos.Any())
                {
                    Console.WriteLine("[ERROR] Los asientos seleccionados no son válidos.");
                    TempData["Error"] = "Los asientos seleccionados no son válidos.";
                    return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo });
                }

                // ✅ Generar número de reserva único
                string numeroReserva = GenerarNumeroReserva(idVuelo);
                Console.WriteLine($"[LOG] Número de reserva generado: {numeroReserva}");

                // ✅ Obtener la fecha de la reserva (momento actual)
                DateTime fechaReserva = DateTime.Now;

                // ✅ Guardar en TempData para persistir los datos temporalmente
                TempData["AsientosSeleccionados"] = JsonConvert.SerializeObject(detallesAsientos);
                TempData["IdVuelo"] = idVuelo;
                TempData["NumeroReserva"] = numeroReserva;
                TempData["IdUsuario"] = userId.Value;
                TempData["FechaReserva"] = fechaReserva.ToString("o"); // Formato ISO 8601

                // ✅ Asegurar que los datos no se pierdan después de la redirección
                TempData.Keep("AsientosSeleccionados");
                TempData.Keep("IdVuelo");
                TempData.Keep("NumeroReserva");
                TempData.Keep("IdUsuario");
                TempData.Keep("FechaReserva");

                // ✅ Crear el modelo de Pago con solo la información necesaria
                var modeloPago = new Pago
                {
                    IdVuelo = idVuelo,
                    Asientos = detallesAsientos.Select(a => new AsientoSeleccionado
                    {
                        Id = a.IdVueloAsiento,
                        Numero = a.Numero,
                        Precio = a.Precio
                    }).ToList(),
                    NumeroReserva = numeroReserva
                };

                // ✅ Crear el ViewModel que combina Pago y Vuelo
                var viewModel = new PagoViewModel
                {
                    Pago = modeloPago,
                    Vuelo = vuelo
                };

                Console.WriteLine("[LOG] Proceso de pago completado correctamente.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excepción en el proceso de pago: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");

                TempData["Error"] = "Ocurrió un error al procesar el pago.";
                return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo });
            }
        }

        [HttpGet]
        public JsonResult ObtenerDetallesAsientos(string asientos)
        {
            if (string.IsNullOrEmpty(asientos))
            {
                return Json(new { success = false, message = "No hay asientos seleccionados." });
            }

            var ids = asientos.Split(',').Select(int.Parse).ToList();
            var detalles = _asientoService.ObtenerDetallesAsientos(ids);

            return Json(detalles);
        }

        public IActionResult Confirmacion(string numeroReserva)
        {
            ViewBag.NumeroReserva = numeroReserva;
            return View();
        }

        private string GenerarNumeroReserva(int idVuelo)
        {
            return $"RSV-{idVuelo}-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";
        }

        [HttpPost]
        public IActionResult ConfirmarPago([FromBody] JsonElement data)
        {
            try
            {
                Console.WriteLine($"[DEBUG] JSON Recibido: {data}");

                // 🔹 Extraer valores del JSON
                int idVuelo = data.TryGetProperty("idVuelo", out JsonElement idVueloElement) ? idVueloElement.GetInt32() : 0;
                string asientosRaw = data.TryGetProperty("asientos", out JsonElement asientosElement) ? asientosElement.ToString() : "";

                Console.WriteLine($"[DEBUG] ID del vuelo recibido: {idVuelo}");
                Console.WriteLine($"[DEBUG] Asientos recibidos: {asientosRaw}");

                // ✅ Verificar si el usuario está autenticado
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    Console.WriteLine("[ERROR] Usuario no autenticado.");
                    return Json(new { success = false, message = "Debes iniciar sesión antes de continuar." });
                }

                var user = _accountService.GetUserById(userId.Value);
                if (user == null)
                {
                    Console.WriteLine("[ERROR] Usuario no encontrado en la BD.");
                    return Json(new { success = false, message = "Error al recuperar la información del usuario." });
                }

                // ✅ Verificar si el vuelo existe
                var vuelo = _vuelosService.GetVueloDetallesById(idVuelo);
                if (vuelo == null)
                {
                    Console.WriteLine("[ERROR] Vuelo no encontrado.");
                    return Json(new { success = false, message = "El vuelo seleccionado no existe." });
                }

                // ✅ Procesar los asientos recibidos y convertirlos en IDs de asientos
                var listaAsientos = asientosRaw.ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(a => int.TryParse(a.Trim(), out _)) // Filtra valores numéricos
                    .Select(a => int.Parse(a.Trim())) // Convierte a int
                    .ToList();

                // ✅ Llamar a ObtenerDetallesAsientos con la lista corregida
                var asientosSeleccionados = _asientoService.ObtenerDetallesAsientos(listaAsientos);

                if (asientosSeleccionados == null || asientosSeleccionados.Count == 0)
                {
                    Console.WriteLine("[ERROR] No se encontraron detalles de los asientos seleccionados.");
                    return Json(new { success = false, message = "Los asientos seleccionados no existen o ya fueron reservados." });
                }

                // ✅ Obtener solo los IDs de los asientos válidos
                List<int> asientosIds = asientosSeleccionados.Select(a => a.IdVueloAsiento).ToList();

                if (asientosIds.Count == 0)
                {
                    Console.WriteLine("[ERROR] No se han seleccionado asientos válidos.");
                    return Json(new { success = false, message = "Debes seleccionar al menos un asiento." });
                }

                Console.WriteLine($"[DEBUG] Asientos procesados: {string.Join(", ", asientosIds)}");

                // ✅ Calcular costos
                decimal subtotal = vuelo.PrecioBase + asientosSeleccionados.Sum(a => a.Precio);
                decimal iva = subtotal * 0.13M;
                decimal totalFinal = subtotal + iva;

                // ✅ Crear la reserva
                string numeroReserva = GenerarNumeroReserva(idVuelo);
                int idReserva = _reservaService.CrearReserva(userId.Value, idVuelo, numeroReserva, DateTime.Now, totalFinal);
                if (idReserva <= 0)
                {
                    Console.WriteLine("[ERROR] No se pudo crear la reserva en la base de datos.");
                    return Json(new { success = false, message = "Error al procesar la reserva." });
                }

                // ✅ Registrar los asientos en la reserva (ahora con IDs correctos)
                bool asientosGuardados = _reservaService.RegistrarAsientos(idReserva, asientosIds);

                if (!asientosGuardados)
                {
                    Console.WriteLine("[ERROR] Falló la inserción de los asientos.");
                    return Json(new { success = false, message = "Error al asignar los asientos a la reserva." });
                }

                // ✅ Confirmación exitosa
                Console.WriteLine($"[LOG] Reserva confirmada: {numeroReserva} - Total: ${totalFinal:F2}");
                return Json(new { success = true, numeroReserva, totalFinal });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excepción en ConfirmarPago: {ex.Message}");
                return Json(new { success = false, message = "Ocurrió un error al procesar el pago." });
            }
        }

        private bool EnviarCorreoConfirmacion(string emailDestino, string nombreUsuario, string numeroReserva, DateTime fechaReserva, decimal totalReserva, List<AsientoSeleccionado> asientos)
        {
            try
            {
                string senderEmail = _configuration["EmailSettings:SenderEmail"];
                string senderPassword = _configuration["EmailSettings:SenderPassword"];

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                };

                string listaAsientos = string.Join(", ", asientos.Select(a => $"{a.Numero} (${a.Precio})"));

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Confirmación de Reserva de Vuelo",
                    Body = $@"
            <p>Hola <strong>{nombreUsuario}</strong>,</p>
            <p>Tu reserva ha sido confirmada con éxito.</p>
            <p><strong>Número de Reserva:</strong> {numeroReserva}</p>
            <p><strong>Fecha de Reserva:</strong> {fechaReserva.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("es-ES"))}</p>
            <p><strong>Asientos:</strong> {listaAsientos}</p>
            <p><strong>Total Pagado:</strong> ${totalReserva.ToString("F2")}</p>
            <p>Gracias por confiar en nosotros.</p>
            <br>
            <p>Atentamente,</p>
            <p><strong>Soporte de Boleto Avión</strong></p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(emailDestino);
                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al enviar el correo de confirmación: {ex.Message}");
                return false;
            }
        }

    }
}
