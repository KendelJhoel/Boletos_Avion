using Boletos_Avion.Models;
using Boletos_Avion.Services;
using Microsoft.AspNetCore.Mvc;

namespace Boletos_Avion.Controllers
{
    public class AeropuertoController : Controller
    {
        public IActionResult AeropuertosAdmin(int? idCiudad)
        {
            var servicio = new AeropuertoService();
            var ciudadService = new CiudadService();

            var aeropuertos = servicio.ObtenerTodos();

            if (idCiudad.HasValue)
            {
                aeropuertos = aeropuertos.Where(a => a.IdCiudad == idCiudad.Value).ToList();
            }

            ViewBag.Ciudades = ciudadService.ObtenerTodas();
            ViewBag.IdCiudadSeleccionada = idCiudad;

            return View("~/Views/Admin/AeropuertosAdmin.cshtml", aeropuertos);
        }



        [HttpPost]
        public IActionResult AgregarAeropuerto(Aeropuerto aeropuerto)
        {
            var servicio = new AeropuertoService();
            var exito = servicio.Agregar(aeropuerto);
            return RedirectToAction("AeropuertosAdmin");
        }

        [HttpPost]
        public IActionResult EliminarAeropuerto(int id)
        {
            var servicio = new AeropuertoService();
            var exito = servicio.Eliminar(id);
            return RedirectToAction("AeropuertosAdmin");
        }

        [HttpPost]
        public IActionResult EditarDesdeAdmin([FromBody] Aeropuerto aeropuerto)
        {
            try
            {
                var servicio = new AeropuertoService();
                var exito = servicio.EditarAeropuerto(aeropuerto.IdAeropuerto, aeropuerto.Nombre);

                if (exito)
                    return Ok("✅ Aeropuerto actualizado correctamente.");
                else
                    return StatusCode(500, "❌ No se pudo actualizar el aeropuerto.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al actualizar aeropuerto desde admin:");
                Console.WriteLine(ex.Message);
                return StatusCode(500, "❌ Error interno.");
            }
        }


    }

}
