using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Boletos_Avion.Controllers;
using Microsoft.Data.SqlClient;

namespace Boletos_Avion.Controllers
{
    public class VuelosController : Controller
    {
        //Cambiar cadena
        private readonly string connectionString = "Data Source=DESKTOP-MP89LU5;Initial Catalog=GestionBoletos;User ID=jona;Password=4321;TrustServerCertificate=True;";

        public IActionResult Resultados()
        {
            return View();
        }

        public IActionResult Index()
        {
            // Retorna la vista con el formulario (o podrías reutilizar tu Vista "Inicio")
            return View();
        }


        [HttpPost]
        public IActionResult BuscarVuelos(string origen, string destino, DateTime? fechaIda, DateTime? fechaVuelta, string tipoViaje)
        {

            // Validación: no se permiten búsquedas sin origen, destino o fecha de ida.
            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino) || !fechaIda.HasValue)
            {
                TempData["Error"] = "Por favor ingresa origen, destino y fecha de ida.";
                return RedirectToAction("Index");
            }

            // Llamamos al método para obtener los vuelos desde la BD usando ADO.NET.
            var vuelos = ObtenerVuelosDeBD(origen, destino, fechaIda.Value);

            // Si no se encontraron vuelos, mostramos un mensaje.
            if (vuelos == null || vuelos.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron vuelos.";
                return View("Resultados", new List<VueloViewModel>());
            }

            // Aquí podrías agregar lógica para vuelos de regreso si es ida y vuelta.

            return View("Resultados", vuelos);
        }

        // Método para obtener vuelos usando ADO.NET
        private List<VueloViewModel> ObtenerVuelosDeBD(string origen, string destino, DateTime fechaIda)
        {
            var vuelos = new List<VueloViewModel>();

            // Consulta SQL para unir tablas y obtener datos necesarios.
            string query = @"
                SELECT 
                    v.idVuelo, 
                    v.codigo_vuelo, 
                    ao.codigo AS AeropuertoOrigen, 
                    co.nombre AS CiudadOrigen, 
                    ad.codigo AS AeropuertoDestino, 
                    cd.nombre AS CiudadDestino, 
                    v.fecha_salida, 
                    v.fecha_llegada, 
                    v.precio_base
                FROM VUELOS v
                INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.codigo
                INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
                INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.codigo
                INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
                WHERE co.nombre = @origen
                  AND cd.nombre = @destino
                  AND CONVERT(date, v.fecha_salida) = @fechaIda";

            // Usando SqlConnection y SqlCommand para ejecutar la consulta.
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Agregamos los parámetros para evitar inyección SQL.
                    cmd.Parameters.AddWithValue("@origen", origen);
                    cmd.Parameters.AddWithValue("@destino", destino);
                    cmd.Parameters.AddWithValue("@fechaIda", fechaIda.Date);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var vuelo = new VueloViewModel
                            {
                                IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                                CodigoVuelo = reader["codigo_vuelo"].ToString(),
                                AeropuertoOrigen = reader["AeropuertoOrigen"].ToString(),
                                CiudadOrigen = reader["CiudadOrigen"].ToString(),
                                AeropuertoDestino = reader["AeropuertoDestino"].ToString(),
                                CiudadDestino = reader["CiudadDestino"].ToString(),
                                FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                                FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                                PrecioBase = Convert.ToDecimal(reader["precio_base"])
                            };
                            vuelos.Add(vuelo);
                        }
                    }
                }
            }

            return vuelos;
        }
    }
}
