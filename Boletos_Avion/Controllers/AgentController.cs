using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mail;
using Boletos_Avion.Models;
using System.Linq;
using Boletos_Avion.Services;
using System.Data;

namespace Boletos_Avion.Controllers
{
    public class AgentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AgentService _agentService;
        private readonly ILogger<AgentController> _logger;
        private readonly AccountService _accountService;

        public AgentController(IConfiguration configuration, AgentService agentService, AccountService accountService, ILogger<AgentController> logger)
        {
            _configuration = configuration;
            _agentService = agentService;
            _accountService = accountService;
            _logger = logger;
        }
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserRole") != 2)
            {
                return RedirectToAction("Index", "Home"); // Si no es agente, lo sacamos
            }
            return View();
        }

        

        public IActionResult RegisterClient()
        {

            return View();
        }
        public IActionResult Client()
        {
            var clients = _agentService.GetAllClients();
            return View(clients);
        }

        [HttpPost]
        public IActionResult RegisterClient(IFormCollection form)
        {
            try
            {
                // Validación básica de campos requeridos
                if (string.IsNullOrEmpty(form["Nombre"]) || string.IsNullOrEmpty(form["Apellido"]) ||
                    string.IsNullOrEmpty(form["Correo"]) || string.IsNullOrEmpty(form["Telefono"]) ||
                    string.IsNullOrEmpty(form["Direccion"]) || string.IsNullOrEmpty(form["DocumentoIdentidad"]))
                {
                    ViewBag.RegisterError = "Todos los campos son obligatorios.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }

                // Generar contraseña automática segura
                string generatedPassword = GenerateSecurePassword();

                UserModel newUser = new UserModel
                {
                    Nombre = $"{form["Nombre"]} {form["Apellido"]}",
                    Correo = form["Correo"],
                    Telefono = form["Telefono"],
                    Direccion = form["Direccion"],
                    DocumentoIdentidad = form["DocumentoIdentidad"],
                    Contrasena = generatedPassword, // Usamos la contraseña generada
                    IdRol = 3 // Rol de cliente
                };

                // Validaciones de duplicados
                if (_agentService.CheckUserExists(newUser.Correo))
                {
                    ViewBag.RegisterError = "El correo ya está registrado.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }

                if (_agentService.CheckPhoneExists(newUser.Telefono))
                {
                    ViewBag.RegisterError = "El teléfono ya está registrado.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }

                if (_agentService.CheckDocumentExists(newUser.DocumentoIdentidad))
                {
                    ViewBag.RegisterError = "El documento ya está registrado.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }

                // Registrar usuario
                bool success = _agentService.RegisterUser(newUser);

                if (success)
                {
                    // Enviar correo con credenciales
                    SendWelcomeEmailWithCredentials(newUser.Correo, newUser.Nombre, generatedPassword);

                    TempData["SuccessMessage"] = $"Cliente registrado exitosamente. Las credenciales se enviaron a {newUser.Correo}";
                    return RedirectToAction("Client"); // Redirige a la lista de clientes
                }

                ViewBag.RegisterError = "Error al registrar el cliente.";
                SetRegistrationValues(form);
                return View("RegisterClient");
            }
            catch (Exception ex)
            {
                ViewBag.RegisterError = $"Error: {ex.Message}";
                SetRegistrationValues(form);
                return View("RegisterClient");
            }
        }

        private string GenerateSecurePassword()
        {
            // Genera una contraseña aleatoria segura
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            var random = new Random();
            var chars = new char[12];

            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }

            return new string(chars);
        }

        // Función para validar los requerimientos de la contraseña
        private bool ValidatePassword(string password)
        {
            if (password.Length < 8)
                return false;
            if (!password.Any(char.IsUpper))
                return false;
            if (!password.Any(char.IsLower))
                return false;
            if (!password.Any(char.IsDigit))
                return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return false;
            return true;
        }

        private void SetRegistrationValues(IFormCollection form)
        {
            ViewBag.Nombre = form["Nombre"];
            ViewBag.Apellido = form["Apellido"];
            ViewBag.Correo = form["Correo"];
            ViewBag.Telefono = form["Telefono"];
            ViewBag.DocumentoIdentidad = form["DocumentoIdentidad"];
            ViewBag.Direccion = form["Direccion"];
        }

        private void SendWelcomeEmailWithCredentials(string email, string name, string password)
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

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "¡Bienvenido a Nuestro Sistema!",
                    Body = $@"
Hola {name},

Tu cuenta ha sido creada exitosamente por nuestro equipo.

Tus credenciales de acceso son:
Correo: {email}
Contraseña: {password}

Por seguridad, te recomendamos cambiar tu contraseña después de iniciar sesión por primera vez.

¡Bienvenido!
",
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(email);
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Loggear el error
                Console.WriteLine($"Error enviando correo: {ex.Message}");
                // Puedes decidir si quieres mostrar este error al agente o no
            }
        }
        [HttpGet]
        public IActionResult CheckEmail(string email)
        {
            try
            {
                bool exists = _agentService.CheckUserExists(email);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando email");
                return Json(new { exists = false });
            }
        }

        [HttpGet]
        public IActionResult CheckPhone(string phone)
        {
            try
            {
                // Limpiar formato para la búsqueda
                string cleanPhone = phone.Replace("-", "");
                bool exists = _agentService.CheckPhoneExists(cleanPhone);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando teléfono");
                return Json(new { exists = false });
            }
        }

        [HttpGet]
        public IActionResult CheckDocument(string document)
        {
            try
            {
                // Limpiar formato para la búsqueda
                string cleanDocument = document.Replace("-", "");
                bool exists = _agentService.CheckDocumentExists(cleanDocument);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando documento");
                return Json(new { exists = false });
            }
        }
        [HttpGet]
        public IActionResult EditClient(int id)
        {
            var client = _accountService.GetUserById(id);
            if (client == null)
            {
                return NotFound();
            }

            var reservas = _accountService.GetReservasActivas(id);
            var model = new EditarCliente
            {
                Client = client,
                Reservas = reservas ?? new List<Reservas>() // Asegúrate de inicializar la lista de reservas
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditClient(EditarCliente model)
        {
            if (!ModelState.IsValid)
            {
                model.Reservas = _accountService.GetReservasActivas(model.Client.IdUsuario); // Asegúrate de volver a cargar las reservas en caso de error
                return View(model);
            }

            var updateSuccess = _accountService.UpdateUser(model.Client);
            if (updateSuccess)
            {
                TempData["UpdateSuccess"] = "Datos actualizados correctamente.";
                return RedirectToAction("Client");
            }
            else
            {
                ModelState.AddModelError("", "Error al actualizar los datos. Intente nuevamente.");
                model.Reservas = _accountService.GetReservasActivas(model.Client.IdUsuario); // Asegúrate de volver a cargar las reservas en caso de error
                return View(model);
            }
        }





        public IActionResult gestionReserva()
        {
            if (HttpContext.Session.GetInt32("UserRole") != 2)
            {
                return RedirectToAction("Index", "Home"); // Si no es agente, lo sacamos
            }
            return View();
        }

        // POST: Cliente/Buscar
        [HttpPost]
        public ActionResult gestionReserva(string documentoIdentidad)
        {
            var cliente = _agentService.GetClientByDUI(documentoIdentidad);

            if (cliente != null)
            {
                ViewBag.Cliente = cliente;
            }
            else
            {
                ViewBag.Mensaje = "Cliente no encontrado.";
            }

            return View();
        }


    }
}
