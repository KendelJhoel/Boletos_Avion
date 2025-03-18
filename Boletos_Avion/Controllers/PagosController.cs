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

                DateTime fechaReserva = DateTime.Now;

                // ✅ **Solución: Guardar correctamente los IDs de los asientos en `Session`**
                var asientosSimplificados = detallesAsientos.Select(a => new { Id = a.IdVueloAsiento }).ToList();
                HttpContext.Session.SetString("AsientosSeleccionados", JsonConvert.SerializeObject(asientosSimplificados));
                HttpContext.Session.SetInt32("IdVuelo", idVuelo);
                HttpContext.Session.SetString("NumeroReserva", numeroReserva);
                HttpContext.Session.SetInt32("IdUsuario", userId.Value);
                HttpContext.Session.SetString("FechaReserva", fechaReserva.ToString("o")); // Formato ISO 8601

                // ✅ Crear el modelo de Pago con la información necesaria
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

                decimal subtotal = vuelo.PrecioBase + detallesAsientos.Sum(a => a.Precio);
                decimal iva = subtotal * 0.13M;
                decimal totalFinal = subtotal + iva;

                // ✅ Guardar total en la sesión para que se use en ConfirmarPago()
                HttpContext.Session.SetString("TotalReserva", totalFinal.ToString("F2"));

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

                // ✅ Extraer valores correctamente desde el JSON
                int idVuelo = data.TryGetProperty("idVuelo", out JsonElement idVueloElement) ? idVueloElement.GetInt32() : 0;
                List<int> listaAsientos = data.TryGetProperty("asientos", out JsonElement asientosElement)
                    ? asientosElement.EnumerateArray().Select(a => a.GetInt32()).ToList()
                    : new List<int>();

                Console.WriteLine($"[DEBUG] ID del vuelo recibido: {idVuelo}");
                Console.WriteLine($"[DEBUG] Asientos recibidos desde la UI: {string.Join(", ", listaAsientos)}");

                // ✅ Validar autenticación del usuario
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Debes iniciar sesión antes de continuar." });
                }

                var vuelo = _vuelosService.GetVueloDetallesById(idVuelo);
                if (vuelo == null)
                {
                    Console.WriteLine("[ERROR] El vuelo seleccionado no existe.");
                    return Json(new { success = false, message = "El vuelo seleccionado no existe." });
                }

                // ✅ Extraer los asientos desde la sesión
                string asientosJson = HttpContext.Session.GetString("AsientosSeleccionados");

                var asientosSeleccionados = JsonConvert.DeserializeObject<List<AsientoSeleccionado>>(asientosJson);
                if (asientosSeleccionados == null || !asientosSeleccionados.Any())
                {
                    Console.WriteLine("[ERROR] No se encontraron asientos válidos en la sesión.");
                    return Json(new { success = false, message = "Debes seleccionar al menos un asiento." });
                }

                if (string.IsNullOrEmpty(asientosJson))
                {
                    Console.WriteLine("[ERROR] No hay asientos en la sesión. Intentando recuperar...");

                    // ✅ Si los asientos están en la solicitud, los guardamos en la sesión
                    if (listaAsientos.Count > 0)
                    {
                        HttpContext.Session.SetString("AsientosSeleccionados", JsonConvert.SerializeObject(listaAsientos));
                        asientosJson = HttpContext.Session.GetString("AsientosSeleccionados");
                        Console.WriteLine("[LOG] Asientos recuperados desde la solicitud y guardados en sesión.");
                    }
                    else
                    {
                        return Json(new { success = false, message = "Los asientos seleccionados no existen o ya fueron reservados." });
                    }
                }

                // ✅ **Solución: Leer correctamente los asientos de la sesión**
                var asientosGuardados = JsonConvert.DeserializeObject<List<dynamic>>(asientosJson);
                var asientosIds = asientosGuardados.Select(a => (int)a.Id).ToList();

                Console.WriteLine($"[DEBUG] Asientos almacenados en sesión después de la corrección: {string.Join(", ", asientosIds)}");

                // ✅ Verificar que los asientos seleccionados en la UI coincidan con los de la sesión
                if (!listaAsientos.All(a => asientosIds.Contains(a)))
                {
                    Console.WriteLine($"[ERROR] Diferencia entre UI ({string.Join(", ", listaAsientos)}) y sesión ({string.Join(", ", asientosIds)})");
                    return Json(new
                    {
                        success = false,
                        message = $"Error: Diferencia entre los asientos seleccionados en la UI ({string.Join(", ", listaAsientos)}) y los almacenados en sesión ({string.Join(", ", asientosIds)})."
                    });
                }

                // ✅ Verificar disponibilidad antes de reservar
                bool disponibles = _asientoService.VerificarDisponibilidad(listaAsientos);
                if (!disponibles)
                {
                    Console.WriteLine("[ERROR] Uno o más asientos ya están reservados.");
                    return Json(new { success = false, message = "Uno o más asientos ya fueron reservados." });
                }

                string totalReservaStr = HttpContext.Session.GetString("TotalReserva");
                decimal totalFinal = !string.IsNullOrEmpty(totalReservaStr) ? decimal.Parse(totalReservaStr) : 0;

                string numeroReserva = HttpContext.Session.GetString("NumeroReserva");
                int idReserva = _reservaService.CrearReserva(userId.Value, idVuelo, numeroReserva, DateTime.Now, totalFinal);

                if (idReserva <= 0)
                {
                    Console.WriteLine("[ERROR] No se pudo crear la reserva en la base de datos.");
                    return Json(new { success = false, message = "Error al procesar la reserva." });
                }

                // ✅ Guardar asientos en la reserva
                bool asientosGuardadosDB = _reservaService.RegistrarAsientos(idReserva, asientosIds);
                if (!asientosGuardadosDB)
                {
                    Console.WriteLine("[ERROR] Falló la inserción de los asientos.");
                    return Json(new { success = false, message = "Error al asignar los asientos a la reserva." });
                }

                // ✅ Cambiar el estado de los asientos a "Reservado"
                bool actualizados = _asientoService.CambiarEstadoAsientos(asientosIds, "Reservado");
                if (!actualizados)
                {
                    Console.WriteLine("[ERROR] No se pudo actualizar el estado de los asientos.");
                    return Json(new { success = false, message = "Error al actualizar el estado de los asientos." });
                }

                var usuario = _accountService.GetUserById(userId.Value);
                if (usuario != null)
                {

                    var asientosCompletos = _asientoService.ObtenerDetallesAsientos(asientosIds); // Asegura obtener detalles reales
                    var numerosAsientos = asientosCompletos.Select(a => new AsientoSeleccionado
                    {
                        Id = a.IdVueloAsiento,
                        Numero = !string.IsNullOrEmpty(a.Numero) ? a.Numero : "Desconocido",
                        Precio = a.Precio
                    }).ToList();

                    Console.WriteLine($"[DEBUG] Asientos que se enviarán al correo: {string.Join(", ", numerosAsientos.Select(a => a.Numero))}");

                    bool correoEnviado = EnviarCorreoConfirmacion(
                        usuario.Correo,
                        usuario.Nombre,
                        numeroReserva,
                        DateTime.Now,
                        totalFinal,
                        numerosAsientos
                    );

                    if (!correoEnviado)
                    {
                        Console.WriteLine("⚠️ Advertencia: No se pudo enviar el correo de confirmación.");
                    }
                }


                Console.WriteLine($"[LOG] Reserva confirmada: {numeroReserva} - Total: ${totalFinal:F2}");
                return Json(new { success = true, numeroReserva, totalFinal });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excepción en ConfirmarPago: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");

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

                string listaAsientos = asientos.Any()
    ? string.Join(", ", asientos.Select(a => string.IsNullOrEmpty(a.Numero) ? "Número desconocido" : a.Numero))
    : "Sin asientos registrados";

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
