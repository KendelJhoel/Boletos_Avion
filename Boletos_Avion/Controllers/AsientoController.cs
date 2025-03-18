using Boletos_Avion.Models;
using Boletos_Avion.Services;
using GestionBoletos.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Boletos_Avion.Controllers
{
    [Route("Asientos")]
    public class AsientoController : Controller
    {
        private readonly AsientoService _asientoService;

        public AsientoController(AsientoService asientoService)
        {
            _asientoService = asientoService;
        }

        [HttpGet("Obtener")]
        public JsonResult ObtenerAsientosPorVuelo(int idVuelo)
        {
            var asientos = _asientoService.ObtenerAsientosPorVuelo(idVuelo)
                .Select(a => new {
                    idVueloAsiento = a.IdVueloAsiento,
                    numero = a.Numero,
                    idCategoria = a.IdCategoria,
                    nombreCategoria = a.NombreCategoria,  // 🔹 Agregar el nombre de la categoría
                    precio = a.Precio,  // 🔹 Agregar el precio del asiento
                    estado = a.Estado
                }).ToList();

            return Json(asientos);
        }


        [HttpPost("Reservar")]
        public JsonResult ReservarAsientos([FromBody] List<int> asientosIds)
        {
            bool exito = _asientoService.ReservarAsientos(asientosIds);
            return Json(new { success = exito });
        }

        [HttpGet("Asientos/VerificarDisponibilidad")]
        public JsonResult VerificarDisponibilidad(string asientos)
        {
            if (string.IsNullOrEmpty(asientos))
            {
                return Json(new { success = false, message = "No se proporcionaron asientos." });
            }

            try
            {
                var asientosIds = asientos.Split(',').Select(int.Parse).ToList();
                bool disponibles = _asientoService.VerificarDisponibilidad(asientosIds);
                return Json(new { success = true, disponibles });
            }
            catch
            {
                return Json(new { success = false, message = "Formato incorrecto de asientos." });
            }
        }

        [HttpPost("Asientos/ConfirmarReserva")]
        public JsonResult ConfirmarReserva([FromBody] dynamic data)
        {
            try
            {
                int userId = data.userId;
                int idVuelo = data.idVuelo;
                string numeroReserva = data.numeroReserva;
                List<int> asientosIds = JsonConvert.DeserializeObject<List<int>>(data.asientos.ToString());

                if (asientosIds == null || asientosIds.Count == 0)
                {
                    return Json(new { success = false, message = "No se han seleccionado asientos para la reserva." });
                }

                bool reservaExitosa = _asientoService.GuardarReserva(userId, idVuelo, asientosIds, numeroReserva);

                if (reservaExitosa)
                {
                    return Json(new { success = true, message = "Reserva confirmada exitosamente.", numeroReserva });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo completar la reserva. Inténtalo nuevamente." });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Error al procesar la reserva." });
            }
        }


        [HttpPost("Asientos/GuardarSeleccion")]
        public JsonResult GuardarAsientosSeleccionados([FromBody] List<int> asientosIds)
        {
            if (asientosIds == null || asientosIds.Count == 0)
            {
                return Json(new { success = false, message = "No has seleccionado asientos." });
            }

            try
            {
                string jsonAsientos = JsonConvert.SerializeObject(asientosIds);
                HttpContext.Session.SetString("AsientosSeleccionados", jsonAsientos);
                return Json(new { success = true, message = "Asientos guardados correctamente." });
            }
            catch
            {
                return Json(new { success = false, message = "Error al guardar los asientos en sesión." });
            }
        }
    }
}
