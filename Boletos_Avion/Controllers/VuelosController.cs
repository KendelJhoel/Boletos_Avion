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
            return View();
        }

        [HttpPost]
        public IActionResult BuscarVuelos(string origen, string destino, DateTime? fechaIda)
        {
            // Verificar si el destino está vacío
            if (string.IsNullOrWhiteSpace(destino))
            {
                ViewBag.Error = "Debe ingresar un destino para realizar la búsqueda.";
                return RedirectToAction("Index", "Home");

            }

            var vuelos = ObtenerVuelos(origen, destino, fechaIda ?? DateTime.Now); 
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




        //---------------------------------------------------------------------------------------
        public IActionResult FiltrarVuelos(decimal? precioMin, decimal? precioMax, int? aerolineaId, int? duracionMax, int? categoriaAsientoId)
        {
            var vuelos = ObtenerVuelosFiltrados(precioMin, precioMax, aerolineaId, duracionMax, categoriaAsientoId);

            if (!vuelos.Any())
            {
                ViewBag.Mensaje = "No hay resultados disponibles.";
            }

            return View("~/Views/Home/Index.cshtml", vuelos);
        }

        private List<VueloViewModel> ObtenerVuelosFiltrados(decimal? precioMin, decimal? precioMax, int? aerolineaId, int? duracionMax, int? categoriaAsientoId)
        {
            List<VueloViewModel> vuelos = new List<VueloViewModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT v.idVuelo, v.codigo_vuelo, ao.codigo AS AeropuertoOrigen, co.nombre AS CiudadOrigen, 
                          ad.codigo AS AeropuertoDestino, cd.nombre AS CiudadDestino, v.fecha_salida, v.fecha_llegada, v.precio_base, 
                          v.idAerolinea, DATEDIFF(MINUTE, v.fecha_salida, v.fecha_llegada) AS Duracion
                          FROM VUELOS v
                          INNER JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
                          INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.codigo
                          INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
                          INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.codigo
                          INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
                          WHERE (@PrecioMin IS NULL OR v.precio_base >= @PrecioMin)
                          AND (@PrecioMax IS NULL OR v.precio_base <= @PrecioMax)
                          AND (@AerolineaId IS NULL OR v.idAerolinea = @AerolineaId)
                          AND (@DuracionMax IS NULL OR DATEDIFF(MINUTE, v.fecha_salida, v.fecha_llegada) <= @DuracionMax)
                          AND (@CategoriaAsientoId IS NULL OR EXISTS (SELECT 1 FROM ASIENTOS WHERE idVuelo = v.idVuelo AND idCategoria = @CategoriaAsientoId))";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@PrecioMin", SqlDbType.Decimal).Value = (object)precioMin ?? DBNull.Value;
                    cmd.Parameters.Add("@PrecioMax", SqlDbType.Decimal).Value = (object)precioMax ?? DBNull.Value;
                    cmd.Parameters.Add("@AerolineaId", SqlDbType.Int).Value = (object)aerolineaId ?? DBNull.Value;
                    cmd.Parameters.Add("@DuracionMax", SqlDbType.Int).Value = (object)duracionMax ?? DBNull.Value;
                    cmd.Parameters.Add("@CategoriaAsientoId", SqlDbType.Int).Value = (object)categoriaAsientoId ?? DBNull.Value;

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

        public IActionResult Detalle(int id)
        {
            // Utiliza tu DbController (o el método que uses para acceder a la base de datos)
            DbController dbController = new DbController();
            // Aquí podrías tener un método GetVueloById en tu DbController
            Vuelo vuelo = dbController.GetVueloById(id);

            if (vuelo == null)
            {
                return NotFound();
            }

            return View(vuelo);
        }



    }



}
