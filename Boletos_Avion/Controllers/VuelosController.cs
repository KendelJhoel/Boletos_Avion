using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Boletos_Avion.Controllers;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Boletos_Avion.Controllers
{
    public class VuelosController : Controller
    {


        //private readonly string connectionString = "Data Source=DESKTOP-MP89LU5;Initial Catalog=GestionBoletos;User ID=jona;Password=4321;TrustServerCertificate=True;";
        private readonly string connectionString = "Data Source=DESKTOP-34DG23J\\SQLEXPRESS;Initial Catalog=GestionBoletos;User ID=sa;Password=Chiesafordel1+;TrustServerCertificate=True;";
        //private readonly string connectionString = "Data Source=DESKTOP-IT9FVD5\\SQLEXPRESS;Initial Catalog=GestionBoletos46;User ID=sa;Password=15012004;TrustServerCertificate=True;";



        public IActionResult Resultados()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BuscarVuelos(string origen, string destino, DateTime? fechaIda, decimal? precioMin, decimal? precioMax, string aerolinea, string categoria)
        {


            var vuelos = ObtenerVuelos(origen, destino, fechaIda, precioMin, precioMax, aerolinea, categoria);

            // Guardar valores en ViewBag para que se mantengan después de la búsqueda
            ViewBag.Origen = origen;
            ViewBag.Destino = destino;
            ViewBag.FechaIda = fechaIda?.ToString("yyyy-MM-dd"); // Formato compatible con input date
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Aerolinea = aerolinea;
            ViewBag.Categoria = categoria;

            return View("~/Views/Home/Index.cshtml", vuelos);
        }

        private List<VueloViewModel> ObtenerVuelos(string origen, string destino, DateTime? fechaIda, decimal? precioMin, decimal? precioMax, string aerolinea, string categoria)
        {
            List<VueloViewModel> vuelos = new List<VueloViewModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                List<string> condiciones = new List<string>();
                SqlCommand cmd = new SqlCommand();

                if (!string.IsNullOrWhiteSpace(origen))
                {
                    condiciones.Add("co.nombre = @Origen");
                    cmd.Parameters.AddWithValue("@Origen", origen);
                }
                if (!string.IsNullOrWhiteSpace(destino))
                {
                    condiciones.Add("cd.nombre = @Destino");
                    cmd.Parameters.AddWithValue("@Destino", destino);
                }
                if (fechaIda.HasValue)
                {
                    condiciones.Add("CONVERT(DATE, v.fecha_salida) = @FechaIda");
                    cmd.Parameters.AddWithValue("@FechaIda", fechaIda.Value.Date);
                }
                if (precioMin.HasValue)
                {
                    condiciones.Add("v.precio_base >= @PrecioMin");
                    cmd.Parameters.AddWithValue("@PrecioMin", precioMin.Value);
                }
                if (precioMax.HasValue)
                {
                    condiciones.Add("v.precio_base <= @PrecioMax");
                    cmd.Parameters.AddWithValue("@PrecioMax", precioMax.Value);
                }
                if (!string.IsNullOrWhiteSpace(aerolinea))
                {
                    condiciones.Add("a.nombre = @Aerolinea");
                    cmd.Parameters.AddWithValue("@Aerolinea", aerolinea);
                }
                if (!string.IsNullOrWhiteSpace(categoria))
                {
                    condiciones.Add("cv.nombre = @Categoria");
                    cmd.Parameters.AddWithValue("@Categoria", categoria);
                }

                string whereClause = condiciones.Count > 0 ? "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"
                SELECT v.idVuelo, v.codigo_vuelo, 
                    ao.idAeropuerto AS AeropuertoOrigen, co.nombre AS CiudadOrigen, 
                    ad.idAeropuerto AS AeropuertoDestino, cd.nombre AS CiudadDestino, 
                    v.fecha_salida, v.fecha_llegada, v.precio_base, 
                    a.nombre AS Aerolinea, cv.nombre AS Categoria, v.estado
                FROM VUELOS v
                INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.idAeropuerto
                INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
                INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
                INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
                INNER JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
                LEFT JOIN CATEGORIAS_VUELOS cv ON v.idCategoriaVuelo = cv.idCategoriaVuelo
                {whereClause};";  

                cmd.CommandText = query;
                cmd.Connection = conn;
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
                            PrecioBase = Convert.ToDecimal(reader["precio_base"]),
                            Aerolinea = reader["Aerolinea"].ToString(),
                            Categoria = reader["Categoria"].ToString(),
                            Estado = reader["estado"].ToString()
                        });
                    }
                }
            }
            return vuelos;
        }



        //-----------
        public IActionResult Detalle(int id)
        {
            DbController dbController = new DbController();
            Vuelo vuelo = dbController.GetVueloDetallesById(id);

            if (vuelo == null)
            {
                return NotFound();
            }

            return View(vuelo);
        }

    }

}

