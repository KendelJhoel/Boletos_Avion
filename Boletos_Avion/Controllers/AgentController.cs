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

        public AgentController(IConfiguration configuration, AgentService agentService)
        {
            _configuration = configuration;
            _agentService = agentService;
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
        public IActionResult Register(IFormCollection form)
        {
            try
            {
                UserModel newUser = new UserModel
                {
                    Nombre = $"{form["Nombre"]} {form["Apellido"]}",
                    Correo = form["Correo"],
                    Telefono = form["Telefono"],
                    Direccion = form["Direccion"],
                    DocumentoIdentidad = form["DocumentoIdentidad"],
                    Contrasena = form["Contrasena"]
                };

                string confirmPassword = form["ConfirmarContrasena"];

                // Validación de campos vacíos
                if (string.IsNullOrWhiteSpace(newUser.Correo) || string.IsNullOrWhiteSpace(newUser.Contrasena) ||
                    string.IsNullOrWhiteSpace(newUser.Nombre) || string.IsNullOrWhiteSpace(newUser.Telefono) ||
                    string.IsNullOrWhiteSpace(newUser.Direccion) || string.IsNullOrWhiteSpace(newUser.DocumentoIdentidad))
                {
                    ViewBag.RegisterError = "Todos los campos son obligatorios.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }

                // Validación de coincidencia de contraseñas
                if (newUser.Contrasena != confirmPassword)
                {
                    ViewBag.RegisterError = "Las contraseñas no coinciden.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }

                bool isReservedPassword = false;
                if (newUser.Contrasena == "AGENT123")
                {
                    newUser.IdRol = 2; // Agente
                    //newUser.Contrasena = GenerateRandomPassword();
                    isReservedPassword = true;
                }
                else if (newUser.Contrasena == "ADMIN2025")
                {
                    newUser.IdRol = 1; // Administrador
                    //newUser.Contrasena = GenerateRandomPassword();
                    isReservedPassword = true;
                }
                else
                {
                    newUser.IdRol = 3; // Cliente
                }

                // Validación de la contraseña para clientes
                if (!ValidatePassword(newUser.Contrasena))
                {
                    ViewBag.RegisterError = "La contraseña no cumple con los requerimientos de seguridad.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }

                // Validación de duplicados
                if (_agentService.CheckUserExists(newUser.Correo))
                {
                    ViewBag.RegisterError = "El correo ya está registrado.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }
                if (_agentService.CheckPhoneExists(newUser.Telefono))
                {
                    ViewBag.RegisterError = "El número de teléfono ya está en uso.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }
                if (_agentService.CheckDocumentExists(newUser.DocumentoIdentidad))
                {
                    ViewBag.RegisterError = "El documento de identidad ya está registrado.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }

                bool success = _agentService.RegisterUser(newUser);
                if (success)
                {
                    SendWelcomeEmail(newUser.Correo, newUser.Nombre);
                    TempData["RegisterSuccess"] = "Registro exitoso. Por favor, inicia sesión.";
                    return RedirectToAction("Authentication", "Auth");
                }
                else
                {
                    ViewBag.RegisterError = "❌ Error al registrar el cliente en la base de datos.";
                    SetRegistrationValues(form);
                    return View("RegisterClient");
                }
            }
            catch (Exception ex)
            {
                ViewBag.RegisterError = $"Ocurrió un error inesperado: {ex.Message}";
                SetRegistrationValues(form);
                return View("RegisterClient");
            }
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
            // Indicamos a la vista que se debe activar la pestaña de registro
            ViewBag.ActiveTab = "register";
            // Preservamos los valores ingresados
            ViewBag.Nombre = form["Nombre"];
            ViewBag.Apellido = form["Apellido"];
            ViewBag.Correo = form["Correo"];
            ViewBag.Telefono = form["Telefono"];
            ViewBag.DocumentoIdentidad = form["DocumentoIdentidad"];
            ViewBag.Direccion = form["Direccion"];
        }

        private bool SendWelcomeEmail(string email, string name)
        {
            try
            {
                Console.WriteLine($"✉ Intentando enviar correo de bienvenida a: {email}");

                string senderEmail = _configuration["EmailSettings:SenderEmail"];
                string senderPassword = _configuration["EmailSettings:SenderPassword"];

                Console.WriteLine($"📧 Usando credenciales: {senderEmail}");

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "¡Bienvenido a Boletos Avión!",
                    Body = $"Hola {name}, tu cuenta ha sido registrada exitosamente. ¡Disfruta de nuestros servicios!",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(email);
                smtpClient.Send(mailMessage);

                Console.WriteLine($"✅ Correo enviado correctamente a: {email}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar correo de bienvenida: {ex.Message}");
                return false;
            }
        }
        public IActionResult EditClient(int id)
        {
            var client = _agentService.GetUserById(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        [HttpPost]
        public IActionResult EditClient(UserModel client)
        {
            bool success = _agentService.UpdateClient(client);
            if (success)
            {
                return RedirectToAction("ClientList");
            }
            else
            {
                ViewBag.ErrorMessage = "Error al actualizar la información del cliente.";
                return View(client);
            }
        }

    }
}
