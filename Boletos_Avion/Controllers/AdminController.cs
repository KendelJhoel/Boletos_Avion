using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Services;
using System.Threading.Tasks;
using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;


namespace Boletos_Avion.Controllers
{
    public class AdminController : Controller
    {
        private readonly AerolineaService _aerolineaService;
        private readonly MonitorService _monitorService;
        private readonly ReservaService _reservaService;
        private readonly VuelosService _vuelosService;

        public AdminController()
        {
            _aerolineaService = new AerolineaService();
            _monitorService = new MonitorService();
            _reservaService = new ReservaService();
            _vuelosService = new VuelosService();
        }

        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserRole") != 1)
                return RedirectToAction("Index", "Home");

            var vuelos = new List<Vuelo>();

            using (var conn = new DbController().GetConnection())
            {
                conn.Open();
                string query = @"
        SELECT TOP 15 
            v.idVuelo,
            v.codigo_vuelo,
            v.fecha_salida,
            v.fecha_llegada,
            v.precio_base,
            v.estado,
            v.asientos_disponibles,
            a.nombre AS nombre_aerolinea,
            cv.nombre AS categoria,
            c1.nombre AS ciudad_origen,
            c2.nombre AS ciudad_destino
        FROM VUELOS v
        INNER JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
        INNER JOIN CATEGORIAS_VUELOS cv ON v.idCategoriaVuelo = cv.idCategoriaVuelo
        INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.idAeropuerto
        INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
        INNER JOIN CIUDADES c1 ON ao.idCiudad = c1.idCiudad
        INNER JOIN CIUDADES c2 ON ad.idCiudad = c2.idCiudad
        ORDER BY v.idVuelo DESC";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vuelos.Add(new Vuelo
                        {
                            IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                            CodigoVuelo = reader["codigo_vuelo"].ToString(),
                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            PrecioBase = Convert.ToDecimal(reader["precio_base"]),
                            Estado = reader["estado"].ToString(),
                            AsientosDisponibles = Convert.ToInt32(reader["asientos_disponibles"]),
                            Categoria = reader["categoria"].ToString(),
                            CiudadOrigen = reader["ciudad_origen"].ToString(),
                            CiudadDestino = reader["ciudad_destino"].ToString(),
                            NombreAerolinea = reader["nombre_aerolinea"].ToString()
                        });
                    }
                }
            }

            var aerolineas = await _aerolineaService.ObtenerTodasAsync();
            var ciudadService = new CiudadService();
            var ciudades = ciudadService.ObtenerTodas();

            var categoriaVueloService = new CategoriaVueloService();
            var categoriasVuelo = categoriaVueloService.ObtenerTodas();

            ViewData["Aerolineas"] = aerolineas;
            ViewData["Ciudades"] = ciudades;
            ViewBag.VuelosRecientes = vuelos;
            ViewBag.CategoriasVuelo = categoriasVuelo;

            return View("~/Views/Admin/Dashboard.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GestionarMonitor()
        {
            if (HttpContext.Session.GetInt32("UserRole") != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            var monitores = await _monitorService.ObtenerTodosAsync();
            ViewData["Aerolineas"] = await _aerolineaService.ObtenerTodasAsync();

            return View("~/Views/Admin/GestioNMonitor.cshtml", monitores);
        }

        public IActionResult GraficosAdmin()
        {
            int? userRole = HttpContext.Session.GetInt32("UserRole");

            if (userRole != 1) // Solo admin
            {
                return RedirectToAction("AccessDenied", "Auth"); // o redirige a donde prefieras
            }

            return View();
        }

        [HttpGet]
        public JsonResult GraficoReservasPorPaisDestino()
        {
            try
            {
                var datos = new List<dynamic>();

                using (var conn = new DbController().GetConnection())
                {
                    conn.Open();

                    string query = @"
                SELECT pd.nombre AS PaisDestino, COUNT(*) AS TotalReservas
                FROM RESERVAS r
                INNER JOIN VUELOS v ON r.idVuelo = v.idVuelo
                INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
                INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
                INNER JOIN PAISES pd ON cd.idPais = pd.idPais
                GROUP BY pd.nombre
                ORDER BY TotalReservas DESC;";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            datos.Add(new
                            {
                                pais = reader["PaisDestino"].ToString(),
                                total = Convert.ToInt32(reader["TotalReservas"])
                            });
                        }
                    }
                }

                return Json(datos);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR en GraficoReservasPorPaisDestino: " + ex.Message);
                return Json(new { error = true, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GraficoTopClientes()
        {
            var datos = new List<dynamic>();

            using (var conn = new DbController().GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT u.nombre, COUNT(*) AS TotalReservas
            FROM RESERVAS r
            INNER JOIN USUARIOS u ON r.idUsuario = u.idUsuario
            GROUP BY u.nombre
            ORDER BY TotalReservas DESC
            OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datos.Add(new
                        {
                            nombre = reader["nombre"].ToString(),
                            total = Convert.ToInt32(reader["TotalReservas"])
                        });
                    }
                }
            }

            return Json(datos);
        }

        [HttpGet]
        public JsonResult GraficoVentasPorAerolinea()
        {
            var datos = new List<dynamic>();

            using (var conn = new DbController().GetConnection())
            {
                conn.Open();

                string query = @"
            SELECT 
                a.nombre AS Aerolinea,
                FORMAT(r.FechaReserva, 'MMMM', 'es-ES') AS Mes,
                SUM(r.total) AS TotalVentas
            FROM RESERVAS r
            INNER JOIN VUELOS v ON r.idVuelo = v.idVuelo
            INNER JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
            GROUP BY a.nombre, FORMAT(r.FechaReserva, 'MMMM', 'es-ES')
            ORDER BY a.nombre, MIN(r.FechaReserva);";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    var resultadoTemp = new Dictionary<string, List<dynamic>>();

                    while (reader.Read())
                    {
                        string aerolinea = reader["Aerolinea"].ToString();
                        string mes = reader["Mes"].ToString();
                        decimal total = Convert.ToDecimal(reader["TotalVentas"]);

                        if (!resultadoTemp.ContainsKey(aerolinea))
                            resultadoTemp[aerolinea] = new List<dynamic>();

                        resultadoTemp[aerolinea].Add(new { mes, total });
                    }

                    foreach (var aerolinea in resultadoTemp)
                    {
                        datos.Add(new
                        {
                            aerolinea = aerolinea.Key,
                            ventas = aerolinea.Value
                        });
                    }
                }
            }

            return Json(datos);
        }



        [HttpGet]
        public JsonResult ReporteReservasPorCiudad()
        {
            try
            {
                var datos = new List<dynamic>();

                using (var conn = new DbController().GetConnection())
                {
                    conn.Open();
                    string query = @"
                SELECT r.origen AS CiudadOrigen, COUNT(*) AS Total
                FROM RESERVAS r
                GROUP BY r.origen
                ORDER BY Total DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            datos.Add(new
                            {
                                ciudad = reader["CiudadOrigen"].ToString(),
                                total = Convert.ToInt32(reader["Total"])
                            });
                        }
                    }
                }

                return Json(datos);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR en ReporteReservasPorCiudad: " + ex.Message);
                return Json(new { error = true, message = ex.Message });
            }
        }



        [HttpGet]
        public JsonResult ReporteAsientosPorVuelo()
        {
            var datos = new List<dynamic>();

            using (var conn = new DbController().GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT v.codigo_vuelo, COUNT(ra.idVueloAsiento) AS TotalAsientos
            FROM RESERVA_ASIENTOS ra
            INNER JOIN RESERVAS r ON ra.idReserva = r.idReserva
            INNER JOIN VUELOS v ON r.idVuelo = v.idVuelo
            GROUP BY v.codigo_vuelo";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datos.Add(new
                        {
                            vuelo = reader["codigo_vuelo"].ToString(),
                            total = Convert.ToInt32(reader["TotalAsientos"])
                        });
                    }
                }
            }

            return Json(datos);
        }


        [HttpGet]
        public JsonResult ReporteFrecuenciaClientes()
        {
            var datos = new List<dynamic>();

            using (var conn = new DbController().GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT u.nombre, COUNT(*) AS Viajes
            FROM RESERVAS r
            INNER JOIN USUARIOS u ON r.idUsuario = u.idUsuario
            GROUP BY u.nombre
            ORDER BY Viajes DESC";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        datos.Add(new
                        {
                            cliente = reader["nombre"].ToString(),
                            total = Convert.ToInt32(reader["Viajes"])
                        });
                    }
                }
            }

            return Json(datos);
        }

        [HttpPost]
        public IActionResult CancelarVuelo(int idVuelo)
        {
            var vuelosService = new VuelosService();
            bool exito = vuelosService.CancelarVueloPorAdmin(idVuelo);

            if (exito)
            {
                return Ok("✈ Vuelo cancelado correctamente. Se notificó a clientes y monitores.");
            }

            return BadRequest("❌ Ocurrió un error al cancelar el vuelo.");
        }

        public IActionResult HistorialTransacciones()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userEmail == null || userId == null)
            {
                return RedirectToAction("Authentication", "Auth");
            }

            var historial = _reservaService.ObtenerHistorialTransacciones();
            ViewBag.TotalGeneral = historial.Sum(t => t.TotalPagado);

            return View("~/Views/Admin/HistorialTransacciones.cshtml", historial);
        }

        [HttpPost]
        public IActionResult CrearVuelo(Vuelo vuelo)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Verifica los datos del formulario.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var exito = _vuelosService.CrearVuelo(vuelo);

            if (exito)
            {
                TempData["Mensaje"] = "✅ Vuelo creado exitosamente.";
            }
            else
            {
                TempData["Error"] = "❌ Ocurrió un error al crear el vuelo.";
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        public async Task<IActionResult> VuelosAdmin()
        {
            var vuelosService = new VuelosService();
            var vuelos = vuelosService.ObtenerVuelosConDetallesOrdenados();

            var ciudadService = new CiudadService();
            var categoriaVueloService = new CategoriaVueloService();
            var aerolineaService = new AerolineaService();

            ViewData["Ciudades"] = ciudadService.ObtenerTodas();
            ViewData["CategoriasVuelo"] = categoriaVueloService.ObtenerTodas();
            ViewData["Aerolineas"] = await aerolineaService.ObtenerTodasAsync();

            return View(vuelos);
        }

        [HttpGet]
        public IActionResult ObtenerVuelo(int idVuelo)
        {
            var vuelo = _vuelosService.ObtenerVueloById(idVuelo);
            if (vuelo == null)
                return NotFound();

            return Json(new
            {
                vuelo.IdVuelo,
                vuelo.CodigoVuelo,
                vuelo.PrecioBase,
                vuelo.FechaSalida,
                vuelo.FechaLlegada,
                vuelo.IdCiudadOrigen,
                vuelo.IdCiudadDestino,
                vuelo.IdAeropuertoOrigen,
                vuelo.IdAeropuertoDestino,
                vuelo.IdCategoriaVuelo,
                vuelo.IdAerolinea,
                vuelo.Estado
            });
        }

        [HttpGet]
        public JsonResult ObtenerAeropuertosPorCiudad(int idCiudad)
        {
            var servicio = new AeropuertoService();
            var aeropuertos = servicio.ObtenerPorCiudad(idCiudad);
            return Json(aeropuertos);
        }

        [HttpGet]
        public JsonResult ObtenerCiudades()
        {
            var servicio = new CiudadService(); // si no tienes uno, te hago uno express
            var ciudades = servicio.ObtenerTodas();
            return Json(ciudades);
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerAerolineas()
        {
            var servicio = new AerolineaService();
            var aerolineas = await servicio.ObtenerTodasAsync(); // ✅ esperamos el resultado
            return Json(aerolineas); // ahora sí se puede serializar correctamente
        }

        [HttpPost]
        public IActionResult ActualizarVuelo([FromBody] Vuelo vuelo)
        {
            try
            {
                var vueloService = new VuelosService();
                var vueloActual = vueloService.GetVueloById(vuelo.IdVuelo);

                if (vueloActual == null)
                    return NotFound("❌ Vuelo no encontrado.");

                if (vueloActual.Estado != "Cancelado")
                    return BadRequest("⚠️ Solo se pueden editar vuelos en estado 'Cancelado'.");

                bool actualizado = vueloService.ActualizarVuelo(
                    vuelo.IdVuelo,
                    vuelo.FechaSalida,
                    vuelo.FechaLlegada,
                    vuelo.IdAeropuertoOrigen,
                    vuelo.IdAeropuertoDestino,
                    vuelo.PrecioBase,
                    vuelo.IdCategoriaVuelo,
                    vuelo.IdAerolinea
                );

                if (actualizado)
                    return Ok("✅ Vuelo actualizado correctamente.");
                else
                    return BadRequest("❌ No se pudo actualizar el vuelo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al actualizar vuelo: " + ex.Message);
                return StatusCode(500, "❌ Error interno al actualizar el vuelo.");
            }
        }

        [HttpPost]
        public IActionResult DisponibilizarVuelo(int idVuelo)
        {
            try
            {
                var vueloService = new VuelosService();
                bool actualizado = vueloService.CambiarEstadoVuelo(idVuelo, "Disponible");

                if (actualizado)
                    return Ok("✈️ Vuelo ahora está disponible.");
                else
                    return BadRequest("❌ No se pudo cambiar el estado del vuelo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al disponibilizar vuelo: " + ex.Message);
                return StatusCode(500, "❌ Error interno al cambiar el estado del vuelo.");
            }
        }

        [HttpGet]
        public JsonResult GraficoOcupacionPorCategoria()
        {
            try
            {
                var datos = _vuelosService.ObtenerOcupacionPorCategoria();
                return Json(datos);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR en GraficoOcupacionPorCategoria: " + ex.Message);
                return Json(new { error = true, message = ex.Message });
            }
        }
    }
}
