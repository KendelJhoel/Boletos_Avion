using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Boletos_Avion.Models;

namespace Boletos_Avion.Controllers
{
    public class AccountController : Controller
    {
        private readonly DbController _dbController;

        public AccountController()
        {
            _dbController = new DbController();
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
            UserModel user = _dbController.GetUserById(userId.Value);
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
            UserModel user = _dbController.GetUserById(userId.Value);
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
            bool updateSuccess = _dbController.UpdateUser(user);
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
    }
}
