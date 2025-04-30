using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Services;
using Boletos_Avion.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonitorModel = Boletos_Avion.Models.Monitor;
using System.Net.Mail;
using System.Net;
using Microsoft.Data.SqlClient;

namespace Boletos_Avion.Controllers
{
    public class MonitorController : Controller
    {
        private readonly MonitorService _monitorService;
        private readonly AerolineaService _aerolineaService;
        private readonly VuelosService _vuelosService;
        private readonly IConfiguration _configuration;
        private readonly ReservaService _reservaService;
        private readonly DbController _context;


        public MonitorController(IConfiguration configuration, ReservaService reservaService, DbController context)
        {
            _monitorService = new MonitorService();
            _aerolineaService = new AerolineaService();
            _vuelosService = new VuelosService();
            _configuration = configuration;
            _reservaService = reservaService;
            _context = context;

        }

        public async Task<IActionResult> GestionMonitor()
        {
            var monitores = await _monitorService.ObtenerTodosAsync();
            var aerolineas = await _aerolineaService.ObtenerTodasAsync();
            ViewData["Aerolineas"] = aerolineas;

            return View("~/Views/Admin/GestionMonitor.cshtml", monitores);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MonitorModel monitor)
        {
            if (monitor.Contrasena != "ADMIN2025")
            {
                TempData["Error"] = "Contraseña reservada incorrecta.";
                return RedirectToAction("GestionMonitor");
            }

            // Genera contraseña aleatoria única
            var authService = new AuthService();
            string nuevaPassword = authService.GenerateUniquePassword();
            monitor.Contrasena = nuevaPassword;

            // Guarda Montor
            await _monitorService.CrearAsync(monitor);

            // Enviar correo con credenciales
            bool enviado = SendMonitorCredentials(monitor.Correo, monitor.Nombre, nuevaPassword);

            if (!enviado)
            {
                TempData["Error"] = "Monitor creado, pero ocurrió un error al enviar las credenciales.";
                return RedirectToAction("GestionMonitor");
            }

            TempData["Success"] = "¡Monitor agregado exitosamente! Se han enviado las credenciales por correo.";
            return RedirectToAction("GestionMonitor");
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("MonitorEmail") == null)
            {
                return RedirectToAction("Authentication", "Auth");
            }

            int? idAerolinea = HttpContext.Session.GetInt32("IdAerolinea");
            string nombreAerolinea = "";
            List<Vuelo> vuelos = new List<Vuelo>();

            if (idAerolinea != null)
            {
                var monitorService = new MonitorService();
                var vuelosService = new VuelosService();
                var ciudadService = new CiudadService();
                var catService = new CategoriaVueloService();

                nombreAerolinea = monitorService.ObtenerNombreAerolinea(idAerolinea.Value);
                vuelos = vuelosService.GetVuelosPorAerolinea(idAerolinea.Value);

                ViewBag.Ciudades = ciudadService.ObtenerTodas();
                ViewBag.CategoriasVuelo = catService.ObtenerTodas();
            }

            ViewBag.NombreAerolinea = nombreAerolinea;
            ViewBag.CorreoMonitor = HttpContext.Session.GetString("MonitorEmail");

            return View(vuelos);
        }


        [HttpGet]
        public JsonResult GetAeropuertosPorCiudad(int idCiudad)
        {
            var aeropuertoService = new AeropuertoService();
            var aeropuertos = aeropuertoService.ObtenerPorCiudad(idCiudad);
            return Json(aeropuertos);
        }


        [HttpPost]
        public IActionResult CrearVuelo(
    string CodigoVuelo,
    int IdCiudadOrigen,
    int IdAeropuertoOrigen,
    int IdCiudadDestino,
    int IdAeropuertoDestino,
    DateTime FechaSalida,
    DateTime FechaLlegada,
    decimal PrecioBase,
    int CantidadAsientos,
    int IdCategoriaVuelo,
    int CantidadPrimera,
    int CantidadBusiness,
    int CantidadTurista// ← NUEVO
)
        {
            int? idAerolinea = HttpContext.Session.GetInt32("IdAerolinea");

            if (idAerolinea != null)
            {
                var vuelo = new Vuelo
                {
                    CodigoVuelo = CodigoVuelo,
                    IdAerolinea = idAerolinea.Value,
                    IdCiudadOrigen = IdCiudadOrigen,
                    IdCiudadDestino = IdCiudadDestino,
                    IdAeropuertoOrigen = IdAeropuertoOrigen,
                    IdAeropuertoDestino = IdAeropuertoDestino,
                    FechaSalida = FechaSalida,
                    FechaLlegada = FechaLlegada,
                    PrecioBase = PrecioBase,
                    CantidadAsientos = CantidadAsientos,
                    AsientosDisponibles = CantidadAsientos,
                    Estado = "Disponible",
                    IdCategoriaVuelo = IdCategoriaVuelo // ← NUEVO
                };

                var vuelosService = new VuelosService();
                vuelosService.CrearVuelo(vuelo, CantidadPrimera, CantidadBusiness, CantidadTurista);

                return Ok("Vuelo creado correctamente.");
            }

            return BadRequest("Sesión inválida.");
        }

        [HttpPost]
        public IActionResult ActualizarVuelo(int idVuelo, DateTime fechaSalida, DateTime fechaLlegada, int idAeropuertoOrigen, int idAeropuertoDestino, decimal precioBase)
        {
            try
            {
                var vueloService = new VuelosService();
                bool actualizado = vueloService.ActualizarVuelo(idVuelo, fechaSalida, fechaLlegada, idAeropuertoOrigen, idAeropuertoDestino, precioBase);

                if (actualizado)
                {
                    return Ok("✅ Vuelo actualizado correctamente.");
                }
                else
                {
                    return BadRequest("❌ No se pudo actualizar el vuelo.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al actualizar vuelo: " + ex.Message);
                return StatusCode(500, "❌ Error interno al actualizar el vuelo.");
            }
        }


        [HttpPost]
        public IActionResult EliminarVuelo(int idVuelo)
        {
            var vueloService = new VuelosService();
            bool ok = vueloService.EliminarVuelo(idVuelo);

            if (ok)
                return Content("Eliminado");

            return BadRequest("No se pudo eliminar el vuelo");
        }

        [HttpGet]
        public IActionResult ObtenerVuelo(int idVuelo)
        {
            try
            {
                var vueloService = new VuelosService();
                var aeropuertoService = new AeropuertoService();

                Vuelo vuelo = vueloService.GetVueloById(idVuelo);
                if (vuelo == null)
                    return NotFound("Vuelo no encontrado.");

                var origen = aeropuertoService.ObtenerPorId(vuelo.IdAeropuertoOrigen);
                var destino = aeropuertoService.ObtenerPorId(vuelo.IdAeropuertoDestino);

                return Json(new
                {
                    idVuelo = vuelo.IdVuelo,
                    codigoVuelo = vuelo.CodigoVuelo,
                    precioBase = vuelo.PrecioBase,
                    fechaSalida = vuelo.FechaSalida.ToString("yyyy-MM-ddTHH:mm"),
                    fechaLlegada = vuelo.FechaLlegada.ToString("yyyy-MM-ddTHH:mm"),
                    estado = vuelo.Estado,
                    categoria = vuelo.Categoria,
                    idCategoriaVuelo = vuelo.IdCategoriaVuelo,

                    idCiudadOrigen = origen?.IdCiudad,
                    idCiudadDestino = destino?.IdCiudad,

                    idAeropuertoOrigen = vuelo.IdAeropuertoOrigen,
                    idAeropuertoDestino = vuelo.IdAeropuertoDestino,

                    nombreAeropuertoOrigen = vuelo.NombreAeropuertoOrigen,
                    nombreAeropuertoDestino = vuelo.NombreAeropuertoDestino
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR EN ObtenerVuelo:");
                Console.WriteLine(ex.ToString()); 
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CancelarVuelo(int idVuelo)
        {
            Console.WriteLine("✈️ Solicitando cancelación de vuelo con ID: " + idVuelo);

            bool exito = _vuelosService.CancelarVuelo(idVuelo);

            if (exito)
            {
                Console.WriteLine("✅ Cancelación confirmada para vuelo ID: " + idVuelo + ". Correos enviados si aplica.");
                return Ok("Vuelo cancelado correctamente. Se notificará a los clientes si aplica.");
            }
            else
            {
                Console.WriteLine("❌ Error al cancelar el vuelo.");
                return BadRequest("Ocurrió un error al cancelar el vuelo.");
            }
        }

        [HttpPost]
        public IActionResult DisponibilizarVuelo(int idVuelo)
        {
            try
            {
                var vueloService = new VuelosService();
                bool actualizado = vueloService.CambiarEstadoVuelo(idVuelo, "Disponible");

                if (actualizado)
                {
                    return Ok("✈️ Vuelo ahora está disponible.");
                }
                else
                {
                    return BadRequest("❌ No se pudo cambiar el estado del vuelo.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al disponibilizar vuelo: " + ex.Message);
                return StatusCode(500, "❌ Error interno al cambiar el estado del vuelo.");
            }
        }

        public IActionResult ProfileMo()
        {
            if (HttpContext.Session.GetString("MonitorEmail") == null)
                return RedirectToAction("Authentication", "Auth");

            string correo = HttpContext.Session.GetString("MonitorEmail");
            string contrasena = HttpContext.Session.GetString("MonitorPassword");

            var monitor = _monitorService.ValidateMonitor(correo, contrasena);

            if (monitor != null)
            {
                var model = new EditProfileViewModel
                {
                    IdUsuario = monitor.IdMonitor,
                    Nombre = monitor.Nombre,
                    DocumentoIdentidad = monitor.DocumentoIdentidad,
                    Contrasena = monitor.Contrasena
                };

                return View("ProfileMo", model); 
            }

            return RedirectToAction("Authentication", "Auth");
        }




        [HttpPost]
        public IActionResult EditMonitorProfile(EditProfileViewModel model)
        {
            if (HttpContext.Session.GetString("MonitorEmail") == null)
                return RedirectToAction("Authentication", "Auth");

            try
            {
                _monitorService.EditarMonitor(model.IdUsuario, model.Nombre, model.DocumentoIdentidad, model.Contrasena);

                TempData["UpdateSuccess"] = "✅ Perfil actualizado correctamente.";
                return RedirectToAction("ProfileMo", "Monitor");
            }
            catch (Exception ex)
            {
                TempData["UpdateSuccess"] = $"❌ Error al actualizar: {ex.Message}";
                return RedirectToAction("ProfileMo", "Monitor");
            }
        }

        [HttpGet]
        public JsonResult CheckCorreoMonitor(string correo)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT COUNT(*) FROM MONITOR WHERE correo = @correo";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return Json(new { exists = count > 0 });
                }
            }
        }



        [HttpGet]
        public JsonResult CheckPassword(string password)
        {
            string correo = HttpContext.Session.GetString("MonitorEmail");
            if (string.IsNullOrEmpty(correo)) return Json(new { valid = false });

            var monitor = _monitorService.ValidateMonitor(correo, password);
            bool isValid = monitor != null;

            return Json(new { valid = isValid });
        }

        [HttpPost]
        public IActionResult SendEmailVerification()
        {
            string monitorEmail = HttpContext.Session.GetString("MonitorEmail");

            if (string.IsNullOrEmpty(monitorEmail))
            {
                return Json(new { success = false, message = "Monitor no autenticado." });
            }

            // Generar nuevo código
            string verificationCode = GenerateVerificationCode();
            Console.WriteLine($"📢 Código generado (monitor): {verificationCode}");

            HttpContext.Session.SetString("MonitorVerificationCode", verificationCode);
            HttpContext.Session.SetString("MonitorVerificationCodeExpiration", DateTime.UtcNow.AddMinutes(10).ToString());

            bool emailSent = SendVerificationCodeEmail(monitorEmail, verificationCode);
            if (emailSent)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Error al enviar el código." });
        }


        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private bool SendVerificationCodeEmail(string email, string code)
        {
            try
            {
                string senderEmail = _configuration["EmailSettings:SenderEmail"];
                string senderPassword = _configuration["EmailSettings:SenderPassword"];

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Código de Confirmación - Cambio de Correo",
                    Body = $@"
                <p>Hola, has solicitado cambiar tu correo en Boletos Avión.</p>
                <p>Tu código de verificación es: <strong>{code}</strong></p>
                <p>Ingresa este código en la pantalla de cambio de correo para confirmar tu identidad.</p>
                <p>Si no solicitaste este cambio, ignora este mensaje.</p>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar el correo de verificación: {ex.Message}");
                return false;
            }
        }

        [HttpGet]
        public async Task<JsonResult> CheckCorreoExists(string correo)
        {
            var existeEnMonitor = await _monitorService.ExisteCorreoAsync(correo);
            var usuarioService = new UsuarioService();
            var existeEnUsuarios = await usuarioService.ExisteCorreoAsync(correo);

            bool exists = existeEnMonitor || existeEnUsuarios;

            return Json(new { exists });
        }

        [HttpGet]
        public async Task<JsonResult> CheckDocumentoExists(string documento)
        {
            var existeEnMonitor = await _monitorService.ExisteDocumentoAsync(documento);
            var usuarioService = new UsuarioService(); 
            var existeEnUsuarios = await usuarioService.ExisteDocumentoAsync(documento);

            bool exists = existeEnMonitor || existeEnUsuarios;

            return Json(new { exists });
        }

        private bool SendMonitorCredentials(string email, string nombre, string password)
        {
            try
            {
                string senderEmail = _configuration["EmailSettings:SenderEmail"];
                string senderPassword = _configuration["EmailSettings:SenderPassword"];

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "🎫 Tus credenciales como Monitor en Boletos Avión",
                    Body = $@"
Hola {nombre},

Has sido registrado como Monitor en el sistema de Boletos Avión.

Estas son tus credenciales de acceso:

📧 Correo: {email}  
🔐 Contraseña: {password}

Por favor, inicia sesión con estas credenciales y cambia tu contraseña después de ingresar.

Saludos cordiales,  
Equipo de Boletos Avión",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(email);
                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar credenciales del monitor: {ex.Message}");
                return false;
            }
        }

        [HttpPost]
        public IActionResult EditarDesdeAdmin([FromBody] Models.Monitor monitor)
        {
            try
            {
                _monitorService.EditarMonitor(monitor.IdMonitor, monitor.Nombre, monitor.DocumentoIdentidad, null, monitor.IdAerolinea);
                return Ok("✅ Monitor actualizado correctamente desde admin.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "❌ Error al actualizar monitor: " + ex.Message);
            }
        }

    }
}
