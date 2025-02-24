using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Boletos_Avion.Controllers;
using Microsoft.Data.SqlClient;
using System.Data;

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

        //[HttpPost]
        //public IActionResult BuscarVuelos(string origen, string destino, DateTime fechaIda)
        //{
        //    var vuelos = ObtenerVuelos(origen, destino, fechaIda);
        //    return View("Resultados", vuelos);
        //}

        [HttpPost]
        public IActionResult BuscarVuelos(string origen, string destino, DateTime? fechaIda)
        {
            // Verificar si el destino está vacío
            if (string.IsNullOrWhiteSpace(destino))
            {
                ViewBag.Error = "Debe ingresar un destino para realizar la búsqueda.";
                return RedirectToAction("Index", "Home");

            }

            var vuelos = ObtenerVuelos(origen, destino, fechaIda ?? DateTime.Now); // Usamos la fecha actual si está vacía
            return View("~/Views/Home/Index.cshtml", vuelos);
        }


        private List<VueloViewModel> ObtenerVuelos(string origen, string destino, DateTime fechaIda)
        {
            List<VueloViewModel> vuelos = new List<VueloViewModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT v.idVuelo, v.codigo_vuelo, 
                       ao.codigo AS AeropuertoOrigen, co.nombre AS CiudadOrigen, 
                       ad.codigo AS AeropuertoDestino, cd.nombre AS CiudadDestino, 
                       v.fecha_salida, v.fecha_llegada, v.precio_base
                FROM VUELOS v
                INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.codigo
                INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
                INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.codigo
                INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
                WHERE co.nombre = @Origen AND cd.nombre = @Destino AND CONVERT(DATE, v.fecha_salida) = @FechaIda";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Origen", SqlDbType.VarChar).Value = origen;
                    cmd.Parameters.Add("@Destino", SqlDbType.VarChar).Value = destino;
                    cmd.Parameters.Add("@FechaIda", SqlDbType.Date).Value = fechaIda;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vuelos.Add(new VueloViewModel
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
                            });
                        }
                    }
                }
            }

            return vuelos;
        }

    }
}
