using Boletos_Avion.Models;
using Boletos_Avion.Services;
using GestionBoletos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        private readonly AgentService _agentService;
        public PagosController(AccountService accountService, VuelosService vuelosService,
                               ILogger<PagosController> logger, AsientoService asientoService,
                               ReservaService reservaService, IConfiguration configuration, AgentService agentService)
        {
            _accountService = accountService;
            _vuelosService = vuelosService;
            _logger = logger;
            _asientoService = asientoService;
            _reservaService = reservaService;
            _configuration = configuration;
            _agentService = agentService;
        }
        public IActionResult Pago(int idVuelo, string asientos, int? idCliente = null)
        {
            if (idCliente.HasValue)
            {
                ViewBag.IdCliente = idCliente.Value;
            }

            try
            {
                Console.WriteLine($"[LOG] Iniciando proceso de pago para el vuelo {idVuelo} con asientos: {asientos} para el cliente: {idCliente}");

                //Verifica si el usuario está autenticado
                int? userId = HttpContext.Session.GetInt32("UserId");
                var userRole = HttpContext.Session.GetInt32("UserRole");
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

                if (user.IdRol == 2 && idCliente.HasValue)
                {
                    // Caso AGENTE: Usar el cliente seleccionado
                    var cliente = _accountService.GetUserById(idCliente.Value);
                    ViewBag.Cliente = cliente;
                }
                else
                {
                    // Caso USUARIO NORMAL: Usar sus propios datos
                    ViewBag.Cliente = user;
                }

                if (userRole == 3)
                {
                    idCliente = userId;
                    ViewBag.IdCliente = userId.Value;
                }

                if (user.IdRol == 2) // Agente
                {
                    if (!idCliente.HasValue)
                    {
                        idCliente = HttpContext.Session.GetInt32("IdClienteRegreso");
                    }

                    if (!idCliente.HasValue)
                    {
                        TempData["Error"] = "Debes seleccionar un cliente.";
                        return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo, esRegreso = true });
                    }

                    var cliente = _accountService.GetUserById(idCliente.Value);
                    if (cliente == null || cliente.IdRol != 3)
                    {
                        TempData["Error"] = "El cliente seleccionado no es válido.";
                        return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo, esRegreso = true });
                    }

                    ViewBag.Cliente = cliente;
                    ViewBag.IdCliente = idCliente.Value;
                }

                //Obtiene el vuelo seleccionado con su precio
                var vuelo = _vuelosService.GetVueloDetallesById(idVuelo);
                if (vuelo == null)
                {
                    Console.WriteLine($"[ERROR] Vuelo {idVuelo} no encontrado.");
                    TempData["Error"] = "El vuelo seleccionado no existe.";
                    return RedirectToAction("SeleccionarAsientos", "Vuelos", new { idVuelo });
                }

                // Verifica si el vuelo tiene un precio válido
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

                string numeroReserva = GenerarNumeroReserva(idVuelo);
                Console.WriteLine($"[LOG] Número de reserva generado: {numeroReserva}");

                DateTime fechaReserva = DateTime.Now;

                var asientosSimplificados = detallesAsientos.Select(a => new { Id = a.IdVueloAsiento }).ToList();
                HttpContext.Session.SetString("AsientosSeleccionados", JsonConvert.SerializeObject(asientosSimplificados));
                HttpContext.Session.SetInt32("IdVuelo", idVuelo);
                HttpContext.Session.SetString("NumeroReserva", numeroReserva);
                HttpContext.Session.SetInt32("IdUsuario", userId.Value);
                HttpContext.Session.SetString("FechaReserva", fechaReserva.ToString("o")); // Formato ISO 8601

                bool esRegreso = HttpContext.Session.GetString("EsRegreso") == "True";
                ViewBag.EsRegreso = esRegreso;

                Console.WriteLine($"[DEBUG] EsRegreso en Pago: {esRegreso}");


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

                HttpContext.Session.SetString("TotalReserva", totalFinal.ToString("F2"));

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

        public IActionResult Confirmacion(
              string numeroReserva,
              string CodigoVuelo,
              string ciudadOrigen,
              string ciudadDestino,
              string fechaIda,
              string paisOrigen,
              string paisDestino,
              string NombreAeropuertoOrigen,
              string NombreAeropuertoDestino,
              string FechaLlegada,
              int idCliente,
              string Nombre,
              int idVuelo,
              string asientos
          )
        {
            ViewBag.NumeroReserva = numeroReserva;
            ViewBag.CodigoVuelo = CodigoVuelo;
            ViewBag.CiudadOrigen = ciudadOrigen;
            ViewBag.CiudadDestino = ciudadDestino;
            ViewBag.FechaIda = fechaIda;
            ViewBag.FechaLlegada = FechaLlegada;
            ViewBag.PaisOrigen = paisOrigen;
            ViewBag.PaisDestino = paisDestino;
            ViewBag.NombreAeropuertoOrigen = NombreAeropuertoOrigen;
            ViewBag.NombreAeropuertoDestino = NombreAeropuertoDestino;
            ViewBag.IdCliente = idCliente;
            ViewBag.Nombre = Nombre;

            // lo que faltaba
            ViewBag.IdVuelo = idVuelo;
            ViewBag.Asientos = asientos;

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

                int idVuelo = data.TryGetProperty("idVuelo", out JsonElement idVueloElement) ? idVueloElement.GetInt32() : 0;
                List<int> listaAsientos = data.TryGetProperty("asientos", out JsonElement asientosElement)
                    ? asientosElement.EnumerateArray().Select(a => a.GetInt32()).ToList()
                    : new List<int>();
                int? idCliente = data.TryGetProperty("idCliente", out var idClienteElement) ? idClienteElement.GetInt32() : null;/////////////
                bool esAgente = HttpContext.Session.GetInt32("UserRole") == 2;//////////////////

                Console.WriteLine($"[DEBUG] ID del vuelo recibido: {idVuelo}");
                Console.WriteLine($"[DEBUG] Asientos recibidos desde la UI: {string.Join(", ", listaAsientos)}");
                Console.WriteLine($"[DEBUG] ID del cliente seleccionado: {idCliente}");////////////////////


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

                //UserModel cliente = null;

                int idUsuarioReserva = userId.Value;
                if (esAgente)
                {
                    if (!idCliente.HasValue)
                        return Json(new { success = false, message = "Selecciona un cliente." });

                    var cliente = _accountService.GetUserById(idCliente.Value);
                    if (cliente == null || cliente.IdRol != 3)
                        return Json(new { success = false, message = "Cliente no válido." });

                    idUsuarioReserva = cliente.IdUsuario; // Asignar reserva al cliente
                    Console.WriteLine($"[AGENTE] Reserva asignada a cliente ID: {cliente.IdUsuario}");
                }

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

                var asientosGuardados = JsonConvert.DeserializeObject<List<dynamic>>(asientosJson);
                var asientosIds = asientosGuardados.Select(a => (int)a.Id).ToList();

                Console.WriteLine($"[DEBUG] Asientos almacenados en sesión después de la corrección: {string.Join(", ", asientosIds)}");

                if (!listaAsientos.All(a => asientosIds.Contains(a)))
                {
                    Console.WriteLine($"[ERROR] Diferencia entre UI ({string.Join(", ", listaAsientos)}) y sesión ({string.Join(", ", asientosIds)})");
                    return Json(new
                    {
                        success = false,
                        message = $"Error: Diferencia entre los asientos seleccionados en la UI ({string.Join(", ", listaAsientos)}) y los almacenados en sesión ({string.Join(", ", asientosIds)})."
                    });
                }

                bool disponibles = _asientoService.VerificarDisponibilidad(listaAsientos);
                if (!disponibles)
                {
                    Console.WriteLine("[ERROR] Uno o más asientos ya están reservados.");
                    return Json(new { success = false, message = "Uno o más asientos ya fueron reservados." });
                }

                string totalReservaStr = HttpContext.Session.GetString("TotalReserva");
                decimal totalFinal = !string.IsNullOrEmpty(totalReservaStr) ? decimal.Parse(totalReservaStr) : 0;

                string numeroReserva = HttpContext.Session.GetString("NumeroReserva");
                int idReserva = _reservaService.CrearReserva(idUsuarioReserva, idVuelo, numeroReserva, DateTime.Now, totalFinal);

                if (idReserva <= 0)
                {
                    Console.WriteLine("[ERROR] No se pudo crear la reserva en la base de datos.");
                    return Json(new { success = false, message = "Error al procesar la reserva." });
                }

                bool asientosGuardadosDB = _reservaService.RegistrarAsientos(idReserva, asientosIds);
                if (!asientosGuardadosDB)
                {
                    Console.WriteLine("[ERROR] Falló la inserción de los asientos.");
                    return Json(new { success = false, message = "Error al asignar los asientos a la reserva." });
                }

                bool actualizados = _asientoService.CambiarEstadoAsientos(asientosIds, "Reservado");
                if (!actualizados)
                {
                    Console.WriteLine("[ERROR] No se pudo actualizar el estado de los asientos.");
                    return Json(new { success = false, message = "Error al actualizar el estado de los asientos." });
                }

                var usuarioReserva = _accountService.GetUserById(idUsuarioReserva);
                if (usuarioReserva != null)
                {

                    var asientosCompletos = _asientoService.ObtenerDetallesAsientos(asientosIds);
                    var numerosAsientos = asientosCompletos.Select(a => new AsientoSeleccionado
                    {
                        Id = a.IdVueloAsiento,
                        Numero = !string.IsNullOrEmpty(a.Numero) ? a.Numero : "Desconocido",
                        Precio = a.Precio
                    }).ToList();

                    Console.WriteLine($"[DEBUG] Asientos que se enviarán al correo: {string.Join(", ", numerosAsientos.Select(a => a.Numero))}");

                    bool correoEnviado;

                    if (esAgente)
                    {
                        var agente = _accountService.GetUserById(userId.Value);
                        correoEnviado = EnviarCorreoConfirmacionAgente(
                            usuarioReserva.Correo,
                            usuarioReserva.Nombre,
                            numeroReserva,
                            DateTime.Now,
                            totalFinal,
                            numerosAsientos,
                            agente?.Nombre ?? "Agente no especificado"
                        );
                    }
                    else
                    {
                        correoEnviado = EnviarCorreoConfirmacion(
                            usuarioReserva.Correo,
                            usuarioReserva.Nombre,
                            numeroReserva,
                            DateTime.Now,
                            totalFinal,
                            numerosAsientos
                        );
                    }

                    if (!correoEnviado)
                    {
                        Console.WriteLine("⚠ Advertencia: No se pudo enviar el correo de confirmación.");
                    }
                }

                Console.WriteLine($"[LOG] Reserva confirmada: {numeroReserva} - Total: ${totalFinal:F2}");

                // Obtener detalles para extraer los números de asiento
                var detalles = _asientoService.ObtenerDetallesAsientos(asientosIds);
                string asientosTexto = string.Join(",", detalles.Select(a => a.Numero));

                string esRegreso = HttpContext.Session.GetString("EsRegreso") == "True" ? "true" : "false";
                HttpContext.Session.Remove("EsRegreso");
                Console.WriteLine($"[DEBUG] Valor final de esRegreso enviado: {esRegreso}");

                var clienteParaMostrar = esAgente
                    ? _accountService.GetUserById(idCliente.Value)
                    : _accountService.GetUserById(userId.Value);


                return Json(new
                {
                    success = true,

                    redirigir = Url.Action("Confirmacion", "Pagos", new
                    {
                        numeroReserva = numeroReserva,
                        ciudadOrigen = vuelo.CiudadOrigen,
                        ciudadDestino = vuelo.CiudadDestino,
                        paisOrigen = vuelo.PaisOrigen,
                        paisDestino = vuelo.PaisDestino,
                        NombreAeropuertoOrigen = vuelo.NombreAeropuertoOrigen,
                        NombreAeropuertoDestino = vuelo.NombreAeropuertoDestino,
                        CodigoVuelo = vuelo.CodigoVuelo,
                        fechaIda = vuelo.FechaSalida.ToString("yyyy-MM-dd HH:mm"),
                        esRegreso = esRegreso,
                        idCliente = idCliente,
                        Nombre = clienteParaMostrar?.Nombre,
                        idVuelo = idVuelo,
                        asientos = asientosTexto,


                        fechaLlegada = vuelo.FechaLlegada.ToString("yyyy-MM-dd HH:mm"),

                    })
                });

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
        private bool EnviarCorreoConfirmacionAgente(string emailCliente, string nombreCliente, string numeroReserva,
                                          DateTime fechaReserva, decimal totalReserva,
                                          List<AsientoSeleccionado> asientos, string nombreAgente)
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

                string listaAsientos = string.Join(", ", asientos.Select(a => a.Numero ?? "Número desconocido"));

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Confirmación de Reserva de Vuelo (Gestionada por Agente)",
                    Body = $@"
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Tu reserva ha sido confirmada con éxito.</p>
                <p><strong>Agente asignado:</strong> {nombreAgente}</p>
                <p><strong>Número de Reserva:</strong> {numeroReserva}</p>
                <p><strong>Fecha de Reserva:</strong> {fechaReserva:dddd, dd MMMM yyyy}</p>
                <p><strong>Asientos:</strong> {listaAsientos}</p>
                <p><strong>Total Pagado:</strong> ${totalReserva:F2}</p>
                <p>Para cualquier consulta, puedes contactar a tu agente.</p>
                <p>Atentamente,<br><strong>Soporte de Boleto Avión</strong></p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(emailCliente);
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo de agente: {ex.Message}");
                return false;
            }
        }

        public IActionResult GenerateBoleto(
    string numeroReserva,
    string asientos,
    string Nombre,
    string CodigoVuelo,
    string paisOrigen,
    string origen,
    string NombreAeropuertoOrigen,
    string paisDestino,
    string destino,
    string NombreAeropuertoDestino,
    DateTime FechaIda,
    DateTime FechaLlegada)
        {
            var listaAsientos = asientos.Split(',').Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

            var pdfBytes = Document.Create(container =>
            {
                foreach (var asiento in listaAsientos)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header().AlignCenter().Text("🎫 Detalles del Boleto")
                                      .SemiBold()
                                      .FontSize(20)
                                      .FontColor(Colors.Blue.Medium);

                        page.Content().Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text($"📅 Fecha de emisión: {DateTime.Now:dd/MM/yyyy}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            column.Item().Text($"👤 Pasajero: {Nombre}")
                                         .Bold()
                                         .FontSize(14);

                            column.Item().Text($"🔢 Número de Reserva: {numeroReserva}")
                                         .FontColor(Colors.Grey.Darken1);

                            column.Item().Text($"✈️ Código de Vuelo: {CodigoVuelo}")
                                         .FontColor(Colors.Grey.Darken1);

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            column.Item().Text($"📍 Origen: {origen}, {paisOrigen}").FontSize(13);
                            column.Item().Text($"🛫 Aeropuerto de salida: {NombreAeropuertoOrigen}");

                            column.Item().Text($"📍 Destino: {destino}, {paisDestino}").FontSize(13);
                            column.Item().Text($"🛬 Aeropuerto de llegada: {NombreAeropuertoDestino}");

                            column.Item().Text($"🕐 Fecha de salida: {FechaIda:dddd, dd MMMM yyyy} - {FechaIda:HH:mm}")
                                         .FontColor(Colors.Blue.Darken2);

                            column.Item().Text($"🕐 Fecha de llegada: {FechaLlegada:dddd, dd MMMM yyyy} - {FechaLlegada:HH:mm}")
                                         .FontColor(Colors.Green.Darken2);

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Aquí generamos el boleto individual
                            column.Item().PaddingVertical(5).Border(1).BorderColor(Colors.Grey.Lighten2).Column(inner =>
                            {
                                inner.Item().Text($"💺 Asiento: {asiento}").Bold();
                                inner.Item().Text("Estado: Confirmado ✅");
                            });

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            column.Item().AlignCenter().Text("¡Gracias por viajar con nosotros! ✈️")
                                         .Italic()
                                         .FontColor(Colors.Green.Darken2);
                        });

                        page.Footer().AlignCenter().Text("Generado con QuestPDF");
                    });
                }
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Boleto_Reserva_{numeroReserva}.pdf");
        }
    }
}
