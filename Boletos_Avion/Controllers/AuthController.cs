using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mail;
using Boletos_Avion.Models;
using System.Linq;

namespace Boletos_Avion.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthController(IConfiguration configuration, AuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
        }

        public IActionResult Authentication()
        {
            return View();
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
                    return View("Authentication");
                }

                // Validación de coincidencia de contraseñas
                if (newUser.Contrasena != confirmPassword)
                {
                    ViewBag.RegisterError = "Las contraseñas no coinciden.";
                    SetRegistrationValues(form);
                    return View("Authentication");
                }

                // Asignación de roles según la contraseña ingresada
                bool isReservedPassword = false;
                if (newUser.Contrasena == "AGENT123")
                {
                    newUser.IdRol = 2; // Agente
                    newUser.Contrasena = GenerateRandomPassword();
                    isReservedPassword = true;
                }
                else if (newUser.Contrasena == "ADMIN2025")
                {
                    newUser.IdRol = 1; // Administrador
                    newUser.Contrasena = GenerateRandomPassword();
                    isReservedPassword = true;
                }
                else
                {
                    newUser.IdRol = 3; // Cliente
                }

                // Validación de la contraseña para clientes
                if (newUser.IdRol == 3 && !ValidatePassword(newUser.Contrasena))
                {
                    ViewBag.RegisterError = "La contraseña no cumple con los requerimientos de seguridad.";
                    SetRegistrationValues(form);
                    return View("Authentication");
                }

                // Validación de aceptación de términos (solo para clientes)
                if (newUser.IdRol == 3 && form["acceptTerms"] != "on")
                {
                    ViewBag.RegisterError = "Debes aceptar los términos y condiciones para continuar.";
                    SetRegistrationValues(form);
                    return View("Authentication");
                }

                // Validación de duplicados
                if (_authService.CheckUserExists(newUser.Correo))
                {
                    ViewBag.RegisterError = "El correo ya está registrado.";
                    SetRegistrationValues(form);
                    return View("Authentication");
                }
                if (_authService.CheckPhoneExists(newUser.Telefono))
                {
                    ViewBag.RegisterError = "El número de teléfono ya está en uso.";
                    SetRegistrationValues(form);
                    return View("Authentication");
                }
                if (_authService.CheckDocumentExists(newUser.DocumentoIdentidad))
                {
                    ViewBag.RegisterError = "El documento de identidad ya está registrado.";
                    SetRegistrationValues(form);
                    return View("Authentication");
                }

                // Registro para Agentes y Administradores
                if (isReservedPassword)
                {
                    bool success = _authService.RegisterUser(newUser);
                    if (success)
                    {
                        bool emailSent = SendEmailWithCredentials(newUser.Correo, newUser.Contrasena, newUser.IdRol == 1 ? "Administrador" : "Agente");
                        if (emailSent)
                        {
                            TempData["RegisterSuccess"] = $"✅ CUENTA TIPO '{(newUser.IdRol == 1 ? "Administrador" : "Agente")}' REGISTRADA CORRECTAMENTE. Las credenciales fueron enviadas a {newUser.Correo}.";
                            ModelState.Clear(); // Limpiar los campos del formulario
                            return RedirectToAction("Authentication", "Auth");
                        }
                        else
                        {
                            ViewBag.RegisterError = "❌ Usuario registrado pero ocurrió un error al enviar las credenciales por correo.";
                            SetRegistrationValues(form);
                            return View("Authentication");
                        }
                    }
                    else
                    {
                        ViewBag.RegisterError = "❌ Error al registrar el usuario en la base de datos. Verifica los datos e intenta nuevamente.";
                        SetRegistrationValues(form);
                        return View("Authentication");
                    }
                }
                else
                {
                    // Registro inmediato para Clientes
                    bool success = _authService.RegisterUser(newUser);
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
                        return View("Authentication");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.RegisterError = $"Ocurrió un error inesperado: {ex.Message}";
                SetRegistrationValues(form);
                return View("Authentication");
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

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _authService.ValidateUser(email, password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.IdUsuario);
                HttpContext.Session.SetString("UserEmail", user.Correo);
                HttpContext.Session.SetString("UserName", user.Nombre);
                HttpContext.Session.SetInt32("UserRole", user.IdRol);

                switch (user.IdRol)
                {
                    case 1:
                        return RedirectToAction("Dashboard", "Admin");
                    case 2:
                        return RedirectToAction("Dashboard", "Agent");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ViewBag.LoginError = "Correo o contraseña incorrectos.";
                return View("Authentication");
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            int? userRole = HttpContext.Session.GetInt32("UserRole");
            HttpContext.Session.Clear();
            return (userRole == 1 || userRole == 2) ? RedirectToAction("Authentication", "Auth") : RedirectToAction("Index", "Home");
        }

        private string GenerateRandomPassword()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@$!%*?&";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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

        private bool SendEmailWithCredentials(string email, string password, string role)
        {
            try
            {
                Console.WriteLine($"✉ Intentando enviar credenciales a: {email}");

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
                    Subject = $"Cuenta de {role} - Boletos Avión",
                    Body = $@"
                <p>Hola, tu cuenta de tipo '{role}' ha sido registrada correctamente.</p>
                <p>Aquí están tus credenciales de acceso:</p>
                <ul>
                    <li><strong>Correo:</strong> {email}</li>
                    <li><strong>Contraseña:</strong> {password}</li>
                </ul>
                <p>Por favor, inicia sesión en el sistema y cambia tu contraseña después de ingresar.</p>
                <a href='https://localhost:5279/Auth/Authentication'>Ir al Login</a>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);
                smtpClient.Send(mailMessage);

                Console.WriteLine($"✅ Correo de credenciales enviado correctamente a: {email}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar correo de credenciales: {ex.Message}");
                return false;
            }
        }

        [HttpGet]
        public JsonResult CheckCorreo(string correo)
        {
            bool exists = _authService.CheckUserExists(correo);
            return Json(new { exists });
        }

        [HttpGet]
        public JsonResult CheckTelefono(string telefono)
        {
            bool exists = _authService.CheckPhoneExists(telefono);
            return Json(new { exists });
        }

        [HttpGet]
        public JsonResult CheckDocumento(string documento)
        {
            bool exists = _authService.CheckDocumentExists(documento);
            return Json(new { exists });
        }

        [HttpGet]
        public IActionResult Password_reset()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendPasswordReset(string correo)
        {
            try
            {
                Console.WriteLine($"🔍 Verificando existencia del usuario con correo: {correo}");

                if (_authService.CheckUserExists(correo))
                {
                    Console.WriteLine($"✅ Usuario encontrado, obteniendo contraseña...");

                    string password = _authService.GetUserPasswordByEmail(correo);
                    Console.WriteLine($"🔐 Contraseña obtenida: {password}");

                    bool emailSent = SendPasswordEmail(correo, password);

                    if (emailSent)
                    {
                        Console.WriteLine($"✅ Correo de recuperación enviado correctamente a: {correo}");
                        TempData["ResetSuccess"] = "Tus credenciales han sido enviadas al correo proporcionado.";
                        return RedirectToAction("Password_reset", "Auth");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Error al enviar el correo a: {correo}");
                        ViewBag.ResetError = "Error al enviar el correo. Intenta nuevamente.";
                        return View("Password_reset");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ El correo ingresado no está registrado: {correo}");
                    ViewBag.ResetError = "El correo ingresado no está registrado.";
                    return View("Password_reset");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SendPasswordReset: {ex.Message}");
                ViewBag.ResetError = $"Error inesperado: {ex.Message}";
                return View("Password_reset");
            }
        }

        private bool SendPasswordEmail(string correo, string password)
        {
            try
            {
                Console.WriteLine($"✉ Intentando enviar correo de recuperación a: {correo}");

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
                    Subject = "Recuperación de Contraseña - Boletos Avión",
                    Body = $@"
                <p>Hola, has solicitado recuperar tu contraseña.</p>
                <p>Estas son tus credenciales actuales:</p>
                <ul>
                    <li><strong>Correo:</strong> {correo}</li>
                    <li><strong>Contraseña:</strong> {password}</li>
                </ul>
                <p>Te recomendamos cambiar tu contraseña después de iniciar sesión.</p>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(correo);
                smtpClient.Send(mailMessage);

                Console.WriteLine($"✅ Correo de recuperación enviado correctamente a: {correo}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar correo de recuperación: {ex.Message}");
                return false;
            }
        }

        // nuevas

        [HttpGet]
        public JsonResult CheckPassword(string password)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { valid = false });
            }

            // Obtener la contraseña de la base de datos
            string storedPassword = _authService.GetUserPasswordById(userId.Value);
            bool isValid = storedPassword == password;

            return Json(new { valid = isValid });
        }
    }
}
