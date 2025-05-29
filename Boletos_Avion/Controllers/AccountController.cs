using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Boletos_Avion.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Boletos_Avion.Services;

namespace Boletos_Avion.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AccountService _accountService;
        private readonly AuthService _authService;


        public AccountController(IConfiguration configuration, AuthService authService)
        {
            _configuration = configuration;
            _accountService = new AccountService();
            _authService = authService;
        }

        // GET: Account/EditProfile
        [HttpGet]
        public IActionResult Profile()
        {
            // Obtener el ID del usuario desde la sesión
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Si no hay usuario autenticado, redirigir al login
                return RedirectToAction("Authentication", "Auth");
            }

            // Recuperar el usuario desde la base de datos por su ID
            UserModel user = _accountService.GetUserById(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Authentication", "Auth");
            }

            // Mapear los datos del usuario a nuestro ViewModel
            EditProfileViewModel model = new EditProfileViewModel
            {
                IdUsuario = user.IdUsuario,
                Nombre = user.Nombre,
                Telefono = user.Telefono,
                Direccion = user.Direccion,
                DocumentoIdentidad = user.DocumentoIdentidad,
                Contrasena = user.Contrasena
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Authentication", "Auth");
            }

            // Obtener el usuario actual de la BD
            UserModel user = _accountService.GetUserById(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Authentication", "Auth");
            }

            // Actualizar los datos con lo que viene del formulario
            user.Nombre = model.Nombre;
            user.Telefono = model.Telefono;
            user.Direccion = model.Direccion;
            user.DocumentoIdentidad = model.DocumentoIdentidad;
            user.Contrasena = model.Contrasena; // Se sobreescribe la contraseña

            // Guardar cambios en la BD
            bool updateSuccess = _accountService.UpdateUser(user);
            if (updateSuccess)
            {
                // Actualizar la sesión para reflejar el nuevo nombre en el navbar
                HttpContext.Session.SetString("UserName", user.Nombre);

                TempData["UpdateSuccess"] = "Datos actualizados correctamente.";
                return RedirectToAction("Profile");
            }
            else
            {
                ModelState.AddModelError("", "Error al actualizar los datos. Intente nuevamente.");
                return View(model);
            }
        }

        // nuevas funciones --------------------------------------------------

        public IActionResult ChangeEmail()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? monitorId = HttpContext.Session.GetInt32("MonitorId");

            if (userId == null && monitorId == null)
            {
                return RedirectToAction("Authentication", "Auth");
            }

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ChangeEmail([FromBody] EditProfileViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? monitorId = HttpContext.Session.GetInt32("MonitorId");

            if (userId == null && monitorId == null)
            {
                return Json(new { success = false, message = "Sesión expirada. Inicia sesión nuevamente." });
            }

            string storedPassword = "";
            string currentEmail = "";
            bool updated = false;

            if (userId != null)
            {
                // 🔍 USUARIO NORMAL
                storedPassword = _authService.GetUserPasswordById(userId.Value);
                currentEmail = HttpContext.Session.GetString("UserEmail");

                if (storedPassword != model.Contrasena)
                {
                    return Json(new { success = false, message = "Contraseña incorrecta." });
                }

                if (string.IsNullOrEmpty(model.Nombre))
                {
                    return Json(new { success = false, message = "El correo no puede estar vacío." });
                }

                updated = _accountService.UpdateUserEmail(userId.Value, model.Nombre);

                if (updated)
                {
                    HttpContext.Session.SetString("UserEmail", model.Nombre);
                }
            }
            else if (monitorId != null)
            {
                // 🔍 MONITOR
                storedPassword = HttpContext.Session.GetString("MonitorPassword");
                currentEmail = HttpContext.Session.GetString("MonitorEmail");

                if (storedPassword != model.Contrasena)
                {
                    return Json(new { success = false, message = "Contraseña incorrecta." });
                }

                if (string.IsNullOrEmpty(model.Nombre))
                {
                    return Json(new { success = false, message = "El correo no puede estar vacío." });
                }

                var monitorService = new MonitorService();
                updated = await monitorService.ActualizarCorreoMonitorAsync(monitorId.Value, model.Nombre);

                if (updated)
                {
                    HttpContext.Session.SetString("MonitorEmail", model.Nombre);
                }
            }

            if (updated)
            {
                bool emailSent = SendEmailChangeConfirmation(model.Nombre);
                if (!emailSent)
                {
                    return Json(new { success = false, message = "Correo actualizado, pero no se pudo enviar la confirmación." });
                }

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Error al actualizar el correo." });
        }


        // 🔹 Función que envía el correo de confirmación usando la misma lógica existente
        private bool SendEmailChangeConfirmation(string newEmail)
        {
            try
            {
                string senderEmail = _configuration["EmailSettings:SenderEmail"];
                string senderPassword = _configuration["EmailSettings:SenderPassword"];

                var smtpClient = new SmtpClient("smtp.gmail.com") // 🔥 Usa la misma configuración de envío
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Confirmación de Cambio de Correo",
                    Body = @"
                <p>Hola,</p>
                <p>Te informamos que el correo asociado a tu cuenta ha sido actualizado exitosamente.</p>
                <p>Si tú realizaste este cambio, no es necesario que hagas nada más.</p>
                <p>Si no reconoces esta acción, por favor, contacta de inmediato con nuestro equipo de soporte.</p>
                <br>
                <p>Atentamente,</p>
                <p><strong>Soporte de Boleto Avión</strong></p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(newEmail);
                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al enviar el correo de confirmación: {ex.Message}");
                return false;
            }
        }


        [HttpGet]
        public JsonResult CheckPassword(string password)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? monitorId = HttpContext.Session.GetInt32("MonitorId");

            if (userId == null && monitorId == null)
            {
                return Json(new { valid = false });
            }

            string storedPassword = "";

            if (userId != null)
            {
                storedPassword = _authService.GetUserPasswordById(userId.Value);
            }
            else if (monitorId != null)
            {
                storedPassword = HttpContext.Session.GetString("MonitorPassword");
            }

            bool isValid = storedPassword == password;

            return Json(new { valid = isValid });
        }


        [HttpGet] // [HttpGet] para que coincida con la solicitud fetch
        public IActionResult ValidateVerificationCode(string code)
        {
            string storedCode = HttpContext.Session.GetString("VerificationCode");
            string expirationTimeStr = HttpContext.Session.GetString("VerificationCodeExpiration");

            Console.WriteLine($"?? Código en sesión al validar: {storedCode}");
            Console.WriteLine($"?? Código ingresado: {code}");

            if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(expirationTimeStr))
            {
                return Json(new { success = false, storedCode = storedCode });
            }

            if (storedCode.Trim() != code.Trim()) // Asegurar que no haya espacios ocultos
            {
                return Json(new { success = false, storedCode = storedCode });
            }

            return Json(new { success = true, storedCode = storedCode });
        }


        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Código de 6 dígitos
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
                Console.WriteLine($"Error al enviar el correo de verificación: {ex.Message}");
                return false;
            }
        }

        [HttpPost]
        public IActionResult SendEmailVerification()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? monitorId = HttpContext.Session.GetInt32("MonitorId");

            string userEmail = userId != null
                ? HttpContext.Session.GetString("UserEmail")
                : HttpContext.Session.GetString("MonitorEmail");

            if ((userId == null && monitorId == null) || string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "Usuario no autenticado." });
            }

            // Generar nuevo código
            string verificationCode = GenerateVerificationCode();
            Console.WriteLine($"📢 Código generado: {verificationCode}");
            HttpContext.Session.SetString("VerificationCode", verificationCode);
            HttpContext.Session.SetString("VerificationCodeExpiration", DateTime.UtcNow.AddMinutes(10).ToString());

            bool emailSent = SendVerificationCodeEmail(userEmail, verificationCode);
            if (emailSent)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Error al enviar el código." });
        }


        [HttpPost]
        public IActionResult ClearVerificationCode()
        {
            HttpContext.Session.Remove("VerificationCode");
            HttpContext.Session.Remove("VerificationCodeExpiration");
            Console.WriteLine("✅ Código de verificación eliminado de la sesión.");
            return Json(new { success = true });
        }

        private void EnviarCorreoConfirmacion(string nuevoCorreo, string nombreUsuario)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.tu-servidor.com") // 🔥 Reemplaza con tu servidor SMTP
                {
                    Port = 587, // Usa el puerto correcto (465/587)
                    Credentials = new NetworkCredential("tu-email@dominio.com", "tu-contraseña"), // 🔥 Reemplaza con tus credenciales
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("tu-email@dominio.com"), // 🔥 Reemplaza con tu email
                    Subject = "Confirmación de Cambio de Correo",
                    Body = $"<h2>Hola, {nombreUsuario}.</h2><p>Tu correo ha sido actualizado correctamente a {nuevoCorreo}.</p><p>Si no hiciste este cambio, por favor contacta con soporte.</p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(nuevoCorreo);
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
            }
        }


    }
}
