using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using Boletos_Avion.Services;
using System.Net.Mail;
using System.Net;


public class VuelosService : DbController
{
    public VuelosService() : base() { } 

    private readonly IConfiguration _configuration;

    public VuelosService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private static readonly Dictionary<string, string> CodigoPaises = new Dictionary<string, string>{
        // América
        { "Argentina", "AR" },
        { "Belice", "BZ" },
        { "Brasil", "BR" },
        { "Canadá", "CA" },
        { "Chile", "CL" },
        { "Colombia", "CO" },
        { "Costa Rica", "CR" },
        { "El Salvador", "SV" },
        { "Estados Unidos", "US" },
        { "Guatemala", "GT" },
        { "Honduras", "HN" },
        { "México", "MX" },
        { "Panamá", "PA" },
        { "Perú", "PE" },
        { "Puerto Rico", "PR" },
        { "República Dominicana", "DO" },

        // Asia
        { "Japón", "JP" },

        // Europa
        { "Alemania", "DE" },
        { "España", "ES" },
        { "Francia", "FR" },
        { "Portugal", "PT" },
        { "Reino Unido", "GB" },
        { "Suecia", "SE" },

        // Oceanía
        { "Australia", "AU" },
        { "Nueva Zelanda", "NZ" }
    };


    // Obtener vuelos por continente (América, Asia, Europa, Oceanía)
    public List<Vuelo> GetVuelosByContinente(string continente)
    {
        List<Vuelo> vuelos = new List<Vuelo>();

        string query = @"
    SELECT 
        v.idVuelo, v.codigo_vuelo, v.idAeropuertoOrigen, 
        origen.nombre AS nombreAeropuertoOrigen, origenCiudad.nombre AS ciudadOrigen,
        v.idAeropuertoDestino, destino.nombre AS nombreAeropuertoDestino, destinoCiudad.nombre AS ciudadDestino,
        v.fecha_salida, v.fecha_llegada, v.idAerolinea, a.nombre AS nombreAerolinea,
        v.precio_base, v.cantidad_asientos, v.asientos_disponibles, v.estado
    FROM VUELOS v
    JOIN AEROPUERTOS origen ON v.idAeropuertoOrigen = origen.idAeropuerto
    JOIN CIUDADES origenCiudad ON origen.idCiudad = origenCiudad.idCiudad
    JOIN AEROPUERTOS destino ON v.idAeropuertoDestino = destino.idAeropuerto
    JOIN CIUDADES destinoCiudad ON destino.idCiudad = destinoCiudad.idCiudad
    JOIN PAISES p ON destinoCiudad.idPais = p.idPais
    JOIN CONTINENTES c ON p.idContinente = c.idContinente
    JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
    WHERE c.nombre = @Continente
      AND CONVERT(DATE, v.fecha_salida) >= CONVERT(DATE, GETDATE())
      AND v.estado <> 'Cancelado'";

        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Continente", continente);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vuelos.Add(MapVuelo(reader));
                    }
                }
            }
        }
        return vuelos;
    }


    public List<Vuelo> GetVuelosByPriceRange(decimal minPrice, decimal maxPrice)
    {
        List<Vuelo> vuelos = new List<Vuelo>();

        string query = @"
    SELECT 
        v.idVuelo, v.codigo_vuelo, v.idAeropuertoOrigen, 
        origen.nombre AS nombreAeropuertoOrigen, origenCiudad.nombre AS ciudadOrigen,
        v.idAeropuertoDestino, destino.nombre AS nombreAeropuertoDestino, destinoCiudad.nombre AS ciudadDestino,
        v.fecha_salida, v.fecha_llegada, v.idAerolinea, a.nombre AS nombreAerolinea,
        v.precio_base, v.cantidad_asientos, v.asientos_disponibles, v.estado
    FROM VUELOS v
    JOIN AEROPUERTOS origen ON v.idAeropuertoOrigen = origen.idAeropuerto
    JOIN CIUDADES origenCiudad ON origen.idCiudad = origenCiudad.idCiudad
    JOIN AEROPUERTOS destino ON v.idAeropuertoDestino = destino.idAeropuerto
    JOIN CIUDADES destinoCiudad ON destino.idCiudad = destinoCiudad.idCiudad
    JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
    WHERE v.precio_base BETWEEN @MinPrice AND @MaxPrice
      AND CONVERT(DATE, v.fecha_salida) >= CONVERT(DATE, GETDATE())
      AND v.estado <> 'Cancelado'";


        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MinPrice", minPrice);
                command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vuelos.Add(MapVuelo(reader));
                    }
                }
            }
        }
        return vuelos;
    }

    public List<Vuelo> GetVuelosByDuration(int maxMinutes)
    {
        List<Vuelo> vuelos = new List<Vuelo>();

        string query = @"
    SELECT 
        v.idVuelo, v.codigo_vuelo, v.idAeropuertoOrigen, 
        origen.nombre AS nombreAeropuertoOrigen, origenCiudad.nombre AS ciudadOrigen,
        v.idAeropuertoDestino, destino.nombre AS nombreAeropuertoDestino, destinoCiudad.nombre AS ciudadDestino,
        v.fecha_salida, v.fecha_llegada, v.idAerolinea, a.nombre AS nombreAerolinea,
        v.precio_base, v.cantidad_asientos, v.asientos_disponibles, v.estado,
        DATEDIFF(MINUTE, v.fecha_salida, v.fecha_llegada) AS DuracionMinutos
    FROM VUELOS v
    JOIN AEROPUERTOS origen ON v.idAeropuertoOrigen = origen.idAeropuerto
    JOIN CIUDADES origenCiudad ON origen.idCiudad = origenCiudad.idCiudad
    JOIN AEROPUERTOS destino ON v.idAeropuertoDestino = destino.idAeropuerto
    JOIN CIUDADES destinoCiudad ON destino.idCiudad = destinoCiudad.idCiudad
    JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
    WHERE DATEDIFF(MINUTE, v.fecha_salida, v.fecha_llegada) <= @MaxMinutes
      AND CONVERT(DATE, v.fecha_salida) >= CONVERT(DATE, GETDATE())
      AND v.estado <> 'Cancelado'";


        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MaxMinutes", maxMinutes);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vuelos.Add(MapVuelo(reader));
                    }
                }
            }
        }
        return vuelos;
    }

    private Vuelo MapVuelo(SqlDataReader reader)
    {
        return new Vuelo
        {
            IdVuelo = Convert.ToInt32(reader["idVuelo"]),
            CodigoVuelo = reader["codigo_vuelo"].ToString(),
            IdAeropuertoOrigen = Convert.ToInt32(reader["idAeropuertoOrigen"]),
            NombreAeropuertoOrigen = reader["nombreAeropuertoOrigen"].ToString(),
            CiudadOrigen = reader["ciudadOrigen"].ToString(),
            IdAeropuertoDestino = Convert.ToInt32(reader["idAeropuertoDestino"]),
            NombreAeropuertoDestino = reader["nombreAeropuertoDestino"].ToString(),
            CiudadDestino = reader["ciudadDestino"].ToString(),
            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
            IdAerolinea = Convert.ToInt32(reader["idAerolinea"]),
            NombreAerolinea = reader["nombreAerolinea"].ToString(),
            PrecioBase = Convert.ToDecimal(reader["precio_base"]),
            CantidadAsientos = Convert.ToInt32(reader["cantidad_asientos"]),
            AsientosDisponibles = Convert.ToInt32(reader["asientos_disponibles"]),
            Estado = reader["estado"].ToString()
        };
    }

    public Vuelo GetVueloById(int id)
    {
        Vuelo vuelo = null;
        try
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(@"
            SELECT v.idVuelo, v.codigo_vuelo, 
                   ao.idAeropuerto AS IdAeropuertoOrigen, co.nombre AS CiudadOrigen, 
                   ad.idAeropuerto AS IdAeropuertoDestino, cd.nombre AS CiudadDestino, 
                   v.fecha_salida, v.fecha_llegada, v.precio_base, 
                   a.nombre AS NombreAerolinea, 
                   cv.nombre AS Categoria,
                   v.idCategoriaVuelo,  -- ✅ añadido aquí
                   v.estado
            FROM VUELOS v
            INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.idAeropuerto
            INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
            INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
            INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
            INNER JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
            LEFT JOIN CATEGORIAS_VUELOS cv ON v.idCategoriaVuelo = cv.idCategoriaVuelo
            WHERE v.idVuelo = @id", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        vuelo = new Vuelo
                        {
                            IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                            CodigoVuelo = reader["codigo_vuelo"].ToString(),
                            IdAeropuertoOrigen = Convert.ToInt32(reader["IdAeropuertoOrigen"]),
                            CiudadOrigen = reader["CiudadOrigen"].ToString(),
                            IdAeropuertoDestino = Convert.ToInt32(reader["IdAeropuertoDestino"]),
                            CiudadDestino = reader["CiudadDestino"].ToString(),
                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            PrecioBase = Convert.ToDecimal(reader["precio_base"]),
                            Categoria = reader["Categoria"].ToString(),
                            IdCategoriaVuelo = Convert.ToInt32(reader["idCategoriaVuelo"]), // ✅ añadido aquí
                            Estado = reader["estado"].ToString(),
                            Aerolinea = new Aerolinea
                            {
                                Nombre = reader["NombreAerolinea"].ToString()
                            }
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al obtener detalles del vuelo: " + ex.Message);
        }
        return vuelo;
    }

    public List<Vuelo> GetVuelos(string origen, string destino, DateTime? fechaIda, decimal? precioMin, decimal? precioMax, string aerolinea, string categoria)
    {
        List<Vuelo> vuelos = new List<Vuelo>();

        try
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand())
            {
                List<string> condiciones = new List<string>();

                Console.WriteLine("🔍 [GET VUELOS] Parámetros recibidos:");
                Console.WriteLine($"   Origen: {origen}");
                Console.WriteLine($"   Destino: {destino}");
                Console.WriteLine($"   FechaIda: {fechaIda}");
                Console.WriteLine($"   PrecioMin: {precioMin}");
                Console.WriteLine($"   PrecioMax: {precioMax}");
                Console.WriteLine($"   Aerolínea: {aerolinea}");
                Console.WriteLine($"   Categoría: {categoria}");

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
                    condiciones.Add("CONVERT(DATE, v.fecha_salida) > @FechaIda");
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

                condiciones.Add("CONVERT(DATE, v.fecha_salida) >= CONVERT(DATE, GETDATE())");
                condiciones.Add("v.estado <> 'Cancelado'");

                string whereClause = condiciones.Count > 0 ? "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"
        SELECT v.idVuelo, v.codigo_vuelo, 
            ao.idAeropuerto AS IdAeropuertoOrigen, co.nombre AS CiudadOrigen, 
            ad.idAeropuerto AS IdAeropuertoDestino, cd.nombre AS CiudadDestino, 
            v.fecha_salida, v.fecha_llegada, v.precio_base, 
            a.nombre AS NombreAerolinea, 
            cv.nombre AS Categoria, v.estado,
            (SELECT COUNT(*) FROM VUELOS_ASIENTOS va WHERE va.idVuelo = v.idVuelo AND va.estado = 'Disponible') AS AsientosDisponibles
        FROM VUELOS v
        INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.idAeropuerto
        INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
        INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
        INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
        INNER JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
        LEFT JOIN CATEGORIAS_VUELOS cv ON v.idCategoriaVuelo = cv.idCategoriaVuelo
        {whereClause};";

                Console.WriteLine("🧠 [GET VUELOS] Consulta generada:");
                Console.WriteLine(query);

                cmd.CommandText = query;
                cmd.Connection = conn;
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vuelos.Add(new Vuelo
                        {
                            IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                            CodigoVuelo = reader["codigo_vuelo"].ToString(),
                            IdAeropuertoOrigen = Convert.ToInt32(reader["IdAeropuertoOrigen"]),
                            CiudadOrigen = reader["CiudadOrigen"].ToString(),
                            IdAeropuertoDestino = Convert.ToInt32(reader["IdAeropuertoDestino"]),
                            CiudadDestino = reader["CiudadDestino"].ToString(),
                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            PrecioBase = Convert.ToDecimal(reader["precio_base"]),
                            Categoria = reader["Categoria"].ToString(),
                            Estado = reader["estado"].ToString(),
                            AsientosDisponibles = Convert.ToInt32(reader["AsientosDisponibles"]),
                            Aerolinea = new Aerolinea
                            {
                                Nombre = reader["NombreAerolinea"].ToString()
                            }
                        });
                    }
                }

                Console.WriteLine($"✅ [GET VUELOS] Total de vuelos encontrados: {vuelos.Count}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al obtener vuelos: " + ex.Message);
        }

        return vuelos;
    }


    public List<Vuelo> GetVuelosPorPais(string paisOrigen, string paisDestino, DateTime? fechaDesde)
    {
        List<Vuelo> vuelos = new List<Vuelo>();

        try
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand())
            {
                List<string> condiciones = new List<string>();

                if (!string.IsNullOrWhiteSpace(paisOrigen))
                {
                    condiciones.Add("po.nombre = @PaisOrigen");
                    cmd.Parameters.AddWithValue("@PaisOrigen", paisOrigen);
                }
                if (!string.IsNullOrWhiteSpace(paisDestino))
                {
                    condiciones.Add("pd.nombre = @PaisDestino");
                    cmd.Parameters.AddWithValue("@PaisDestino", paisDestino);
                }
                if (fechaDesde.HasValue)
                {
                    condiciones.Add("CONVERT(DATE, v.fecha_salida) > @FechaDesde");
                    cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde.Value.Date);
                }

                string whereClause = condiciones.Count > 0 ? "WHERE " + string.Join(" AND ", condiciones) + " AND " : "WHERE ";
                string query = $@"
    SELECT v.idVuelo, v.codigo_vuelo,
           co.nombre AS CiudadOrigen, cd.nombre AS CiudadDestino,
           po.nombre AS PaisOrigen, pd.nombre AS PaisDestino,
           v.fecha_salida, v.fecha_llegada, v.precio_base,
           a.nombre AS NombreAerolinea, cv.nombre AS Categoria, v.estado,
           (SELECT COUNT(*) FROM VUELOS_ASIENTOS va WHERE va.idVuelo = v.idVuelo AND va.estado = 'Disponible') AS AsientosDisponibles
    FROM VUELOS v
    INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.idAeropuerto
    INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
    INNER JOIN PAISES po ON co.idPais = po.idPais
    INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
    INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
    INNER JOIN PAISES pd ON cd.idPais = pd.idPais
    INNER JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
    LEFT JOIN CATEGORIAS_VUELOS cv ON v.idCategoriaVuelo = cv.idCategoriaVuelo
    {whereClause}CONVERT(DATE, v.fecha_salida) >= CONVERT(DATE, GETDATE())
      AND v.estado <> 'Cancelado';";


                cmd.CommandText = query;
                cmd.Connection = conn;
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vuelos.Add(new Vuelo
                        {
                            IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                            CodigoVuelo = reader["codigo_vuelo"].ToString(),
                            CiudadOrigen = reader["CiudadOrigen"].ToString(),
                            CiudadDestino = reader["CiudadDestino"].ToString(),
                            PaisOrigen = reader["PaisOrigen"].ToString(),
                            PaisDestino = reader["PaisDestino"].ToString(),
                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            PrecioBase = Convert.ToDecimal(reader["precio_base"]),
                            Estado = reader["estado"].ToString(),
                            AsientosDisponibles = Convert.ToInt32(reader["AsientosDisponibles"]),
                            NombreAerolinea = reader["NombreAerolinea"].ToString(),
                            Categoria = reader["Categoria"].ToString()
                        });

                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al buscar vuelos por país: " + ex.Message);
        }

        return vuelos;
    }

    public List<Vuelo> GetVuelosPorAerolinea(int idAerolinea)
    {
        List<Vuelo> vuelos = new List<Vuelo>();

        string query = @"
    SELECT v.idVuelo, v.codigo_vuelo, 
           co.nombre AS CiudadOrigen, cd.nombre AS CiudadDestino,
           v.fecha_salida, v.fecha_llegada, v.precio_base, v.estado
    FROM VUELOS v
    INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.idAeropuerto
    INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
    INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
    INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
    WHERE v.idAerolinea = @idAerolinea
      AND CONVERT(DATE, v.fecha_salida) >= CONVERT(DATE, GETDATE())
      AND v.estado <> 'Cancelado'";


        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idAerolinea", idAerolinea);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vuelos.Add(new Vuelo
                        {
                            IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                            CodigoVuelo = reader["codigo_vuelo"].ToString(),
                            CiudadOrigen = reader["CiudadOrigen"].ToString(),
                            CiudadDestino = reader["CiudadDestino"].ToString(),
                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            PrecioBase = Convert.ToDecimal(reader["precio_base"]),
                            Estado = reader["estado"].ToString()
                        });
                    }
                }
            }
        }

        return vuelos;
    }

    public bool ActualizarVuelo(Vuelo vuelo)
    {
        using (SqlConnection conn = GetConnection())
        {
            conn.Open();

            string query = @"UPDATE VUELOS 
                         SET idCategoriaVuelo = @idCategoriaVuelo,
                             precio_base = @precioBase,
                             estado = @estado
                         WHERE idVuelo = @idVuelo";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idCategoriaVuelo", vuelo.IdCategoriaVuelo);
                cmd.Parameters.AddWithValue("@precioBase", vuelo.PrecioBase);
                cmd.Parameters.AddWithValue("@estado", vuelo.Estado);
                cmd.Parameters.AddWithValue("@idVuelo", vuelo.IdVuelo);

                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
        }
    }

    public bool ActualizarVuelo(int idVuelo, DateTime fechaSalida, DateTime fechaLlegada, int idAeropuertoOrigen, int idAeropuertoDestino, decimal precioBase, int idCategoriaVuelo, int idAerolinea)
    {
        using (SqlConnection conn = GetConnection())
        {
            conn.Open();

            string query = @"UPDATE VUELOS 
                         SET idCategoriaVuelo = @idCategoriaVuelo,
                             precio_base = @precioBase,
                             fecha_salida = @fechaSalida,
                             fecha_llegada = @fechaLlegada,
                             idAeropuertoOrigen = @idAeropuertoOrigen,
                             idAeropuertoDestino = @idAeropuertoDestino,
                             idAerolinea = @idAerolinea
                         WHERE idVuelo = @idVuelo";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idCategoriaVuelo", idCategoriaVuelo);
                cmd.Parameters.AddWithValue("@precioBase", precioBase);
                cmd.Parameters.AddWithValue("@fechaSalida", fechaSalida);
                cmd.Parameters.AddWithValue("@fechaLlegada", fechaLlegada);
                cmd.Parameters.AddWithValue("@idAeropuertoOrigen", idAeropuertoOrigen);
                cmd.Parameters.AddWithValue("@idAeropuertoDestino", idAeropuertoDestino);
                cmd.Parameters.AddWithValue("@idAerolinea", idAerolinea);
                cmd.Parameters.AddWithValue("@idVuelo", idVuelo);

                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
        }
    }


    public bool EliminarVuelo(int idVuelo)
    {
        using (SqlConnection conn = GetConnection())
        {
            conn.Open();

            string query = "DELETE FROM VUELOS WHERE idVuelo = @idVuelo";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idVuelo", idVuelo);
                int affected = cmd.ExecuteNonQuery();
                return affected > 0;
            }
        }
    }

    public bool CancelarVuelo(int idVuelo)
    {
        try
        {
            Console.WriteLine("🟠 Iniciando cancelación de vuelo con ID: " + idVuelo);

            var reservaService = new ReservaService();
            var accountService = new AccountService();
            var reservas = reservaService.ObtenerReservasPorVuelo(idVuelo);

            Console.WriteLine($"📩 Notificando cancelaciones a {reservas.Count} cliente(s)...");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfiguration config = builder.Build();
            string senderEmail = config["EmailSettings:SenderEmail"];
            string senderPassword = config["EmailSettings:SenderPassword"];

            foreach (var r in reservas)
            {
                var usuario = accountService.GetUserById(r.IdUsuario);
                if (usuario != null)
                {
                    try
                    {
                        var smtpClient = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            Credentials = new NetworkCredential(senderEmail, senderPassword),
                            EnableSsl = true,
                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(senderEmail),
                            Subject = "Cancelación de vuelo y reembolso del 100%",
                            Body = $@"
                            <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                            <p>Tu vuelo con número de reserva <strong>{r.NumeroReserva}</strong> ha sido <span style='color:red;'>cancelado</span>.</p>
                            <p>Se ha reembolsado el <strong>100%</strong> del total pagado: <strong>${r.Total:F2}</strong>.</p>
                            <p>Gracias por tu comprensión.</p>
                            <p><strong>Soporte de Boleto Avión</strong></p>",
                                                        IsBodyHtml = true,
                        };

                        mailMessage.To.Add(usuario.Correo);
                        smtpClient.Send(mailMessage);

                        Console.WriteLine($"✅ Correo enviado a: {usuario.Correo}");
                    }
                    catch (Exception exCorreo)
                    {
                        Console.WriteLine($"⚠ Error al enviar el correo a {usuario.Correo}: {exCorreo.Message}");
                    }
                }
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string queryVuelo = "UPDATE VUELOS SET estado = 'Cancelado' WHERE idVuelo = @idVuelo";
                        using (SqlCommand cmd = new SqlCommand(queryVuelo, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idVuelo", idVuelo);
                            cmd.ExecuteNonQuery();
                            Console.WriteLine("✅ Estado del vuelo actualizado.");
                        }

                        string queryAsientos = "UPDATE VUELOS_ASIENTOS SET estado = 'Disponible' WHERE idVuelo = @idVuelo";
                        using (SqlCommand cmd = new SqlCommand(queryAsientos, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idVuelo", idVuelo);
                            cmd.ExecuteNonQuery();
                            Console.WriteLine("✅ Asientos actualizados a 'Disponible'.");
                        }

                        string queryReservas = "UPDATE RESERVAS SET estado = 'Inactivo' WHERE idVuelo = @idVuelo";
                        using (SqlCommand cmd = new SqlCommand(queryReservas, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idVuelo", idVuelo);
                            cmd.ExecuteNonQuery();
                            Console.WriteLine("✅ Reservas cambiadas a 'Inactivo'.");
                        }

                        transaction.Commit();
                        Console.WriteLine("🎉 Vuelo cancelado correctamente.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("❌ Error durante la cancelación: " + ex.Message);
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error general al cancelar vuelo: " + ex.Message);
            return false;
        }
    }

    public bool ActualizarVuelo(int idVuelo, DateTime fechaSalida, DateTime fechaLlegada, int idAeropuertoOrigen, int idAeropuertoDestino, decimal precioBase)
    {
        try
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(@"
            UPDATE VUELOS
            SET 
                fecha_salida = @fechaSalida,
                fecha_llegada = @fechaLlegada,
                idAeropuertoOrigen = @idAeropuertoOrigen,
                idAeropuertoDestino = @idAeropuertoDestino,
                precio_base = @precioBase
            WHERE idVuelo = @idVuelo", connection))
            {
                command.Parameters.AddWithValue("@fechaSalida", fechaSalida);
                command.Parameters.AddWithValue("@fechaLlegada", fechaLlegada);
                command.Parameters.AddWithValue("@idAeropuertoOrigen", idAeropuertoOrigen);
                command.Parameters.AddWithValue("@idAeropuertoDestino", idAeropuertoDestino);
                command.Parameters.AddWithValue("@precioBase", precioBase);
                command.Parameters.AddWithValue("@idVuelo", idVuelo);

                connection.Open();
                int filasAfectadas = command.ExecuteNonQuery();

                return filasAfectadas > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error en ActualizarVuelo: " + ex.Message);
            return false;
        }
    }

    public bool CambiarEstadoVuelo(int idVuelo, string nuevoEstado)
    {
        try
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(@"
            UPDATE VUELOS
            SET estado = @estado
            WHERE idVuelo = @idVuelo", connection))
            {
                command.Parameters.AddWithValue("@estado", nuevoEstado);
                command.Parameters.AddWithValue("@idVuelo", idVuelo);

                connection.Open();
                int filas = command.ExecuteNonQuery();
                return filas > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al cambiar estado del vuelo: " + ex.Message);
            return false;
        }
    }


    public bool CancelarVueloPorAdmin(int idVuelo)
    {
        try
        {
            Console.WriteLine("🟠 [ADMIN] Iniciando cancelación de vuelo con ID: " + idVuelo);

            var reservaService = new ReservaService();
            var accountService = new AccountService();
            var reservas = reservaService.ObtenerReservasPorVuelo(idVuelo);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfiguration config = builder.Build();
            string senderEmail = config["EmailSettings:SenderEmail"];
            string senderPassword = config["EmailSettings:SenderPassword"];

            int correosClientesOK = 0;
            int correosClientesError = 0;

            foreach (var r in reservas)
            {
                var usuario = accountService.GetUserById(r.IdUsuario);
                if (usuario != null)
                {
                    try
                    {
                        var smtpClient = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            Credentials = new NetworkCredential(senderEmail, senderPassword),
                            EnableSsl = true,
                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(senderEmail),
                            Subject = "Cancelación de vuelo y reembolso del 100%",
                            Body = $@"
                            <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                            <p>Tu vuelo con número de reserva <strong>{r.NumeroReserva}</strong> ha sido <span style='color:red;'>cancelado</span> por administración.</p>
                            <p>Se ha reembolsado el <strong>100%</strong> del total pagado: <strong>${r.Total:F2}</strong>.</p>
                            <p>Gracias por tu comprensión.</p>
                            <p><strong>Soporte de Boleto Avión</strong></p>",
                            IsBodyHtml = true,
                        };

                        mailMessage.To.Add(usuario.Correo);
                        smtpClient.Send(mailMessage);

                        correosClientesOK++;
                        Console.WriteLine($"✅ [Cliente] Correo enviado a {usuario.Correo}");
                    }
                    catch (Exception exCorreo)
                    {
                        correosClientesError++;
                        Console.WriteLine($"❌ [Cliente] Error al enviar correo a {usuario.Correo}: {exCorreo.Message}");
                    }
                }
            }

            Console.WriteLine($"📊 Total correos enviados a clientes: {correosClientesOK}, fallidos: {correosClientesError}");

            //Notificar a monitores
            var vuelo = GetVueloDetallesById(idVuelo);
            var monitores = GetCorreosMonitoresPorAerolinea(vuelo.IdAerolinea);
            int correosMonitoresOK = 0;
            int correosMonitoresError = 0;

            foreach (var correoMonitor in monitores)
            {
                try
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(senderEmail, senderPassword),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail),
                        Subject = "✈ Vuelo cancelado por administración",
                        Body = $@"
                        <p>Estimado monitor,</p>
                        <p>El vuelo <strong>{vuelo.CodigoVuelo}</strong> ha sido <span style='color:red;'>cancelado</span> por el administrador del sistema.</p>
                        <p>Los pasajeros han sido notificados automáticamente.</p>
                        <p>Gracias por su colaboración.</p>",
                        IsBodyHtml = true,
                    };

                    mailMessage.To.Add(correoMonitor);
                    smtpClient.Send(mailMessage);

                    correosMonitoresOK++;
                    Console.WriteLine($"✅ [Monitor] Correo enviado a {correoMonitor}");
                }
                catch (Exception ex)
                {
                    correosMonitoresError++;
                    Console.WriteLine($"❌ [Monitor] Error al enviar correo a {correoMonitor}: {ex.Message}");
                }
            }

            Console.WriteLine($"📊 Total correos a monitores: {correosMonitoresOK}, fallidos: {correosMonitoresError}");

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string queryVuelo = "UPDATE VUELOS SET estado = 'Cancelado' WHERE idVuelo = @idVuelo";
                        using (SqlCommand cmd = new SqlCommand(queryVuelo, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idVuelo", idVuelo);
                            cmd.ExecuteNonQuery();
                        }

                        string queryAsientos = "UPDATE VUELOS_ASIENTOS SET estado = 'Disponible' WHERE idVuelo = @idVuelo";
                        using (SqlCommand cmd = new SqlCommand(queryAsientos, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idVuelo", idVuelo);
                            cmd.ExecuteNonQuery();
                        }

                        string queryReservas = "UPDATE RESERVAS SET estado = 'Inactivo' WHERE idVuelo = @idVuelo";
                        using (SqlCommand cmd = new SqlCommand(queryReservas, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idVuelo", idVuelo);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.WriteLine("✅ [ADMIN] Cancelación del vuelo completada y guardada en base de datos.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("❌ Error en transacción de cancelación admin: " + ex.Message);
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error general al cancelar vuelo por admin: " + ex.Message);
            return false;
        }
    }

    private List<string> GetCorreosMonitoresPorAerolinea(int idAerolinea)
    {
        var correos = new List<string>();

        Console.WriteLine($"🔍 Buscando monitores para aerolínea con ID: {idAerolinea}");

        using (SqlConnection conn = GetConnection())
        {
            try
            {
                string query = "SELECT correo FROM MONITOR WHERE idAerolinea = @id AND correo IS NOT NULL";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idAerolinea);
                    conn.Open();
                    Console.WriteLine("✅ Conexión abierta correctamente para buscar monitores.");

                    using (var reader = cmd.ExecuteReader())
                    {
                        int contador = 0;

                        while (reader.Read())
                        {
                            string correo = reader["correo"].ToString();
                            correos.Add(correo);
                            contador++;
                            Console.WriteLine($"📧 Monitor #{contador} → Correo: {correo}");
                        }

                        if (contador == 0)
                        {
                            Console.WriteLine("⚠ No se encontraron monitores registrados para esta aerolínea.");
                        }
                        else
                        {
                            Console.WriteLine($"✅ Total monitores encontrados: {contador}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener correos de monitores: {ex.Message}");
            }
        }

        return correos;
    }

    public Vuelo GetVueloDetallesById(int id)
    {
        Vuelo vuelo = null;
        string query = @"
    SELECT 
        v.idVuelo, 
        v.codigo_vuelo, 
        v.idAeropuertoOrigen, 
        ao.nombre AS nombreAeropuertoOrigen, 
        co.nombre AS ciudadOrigen, 
        po.nombre AS paisOrigen,
        v.idAeropuertoDestino, 
        ad.nombre AS nombreAeropuertoDestino, 
        cd.nombre AS ciudadDestino, 
        pd.nombre AS paisDestino,
        v.fecha_salida, 
        v.fecha_llegada, 
        v.idAerolinea, 
        a.nombre AS nombreAerolinea,
        v.precio_base, 
        v.cantidad_asientos, 
        v.asientos_disponibles, 
        v.estado,
        v.idCategoriaVuelo,  -- Se agrega para saber la categoría
        -- Cantidad total de asientos por tipo en el vuelo
        COALESCE(SUM(CASE WHEN va.idCategoria = 1 THEN 1 ELSE 0 END), 0) AS totalBusiness,
        COALESCE(SUM(CASE WHEN va.idCategoria = 2 THEN 1 ELSE 0 END), 0) AS totalTurista,
        COALESCE(SUM(CASE WHEN va.idCategoria = 3 THEN 1 ELSE 0 END), 0) AS totalPrimeraClase,
        -- Cantidad de asientos disponibles por tipo
        COALESCE(SUM(CASE WHEN va.idCategoria = 1 AND va.estado = 'Disponible' THEN 1 ELSE 0 END), 0) AS disponiblesBusiness,
        COALESCE(SUM(CASE WHEN va.idCategoria = 2 AND va.estado = 'Disponible' THEN 1 ELSE 0 END), 0) AS disponiblesTurista,
        COALESCE(SUM(CASE WHEN va.idCategoria = 3 AND va.estado = 'Disponible' THEN 1 ELSE 0 END), 0) AS disponiblesPrimeraClase
    FROM VUELOS v
    JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.idAeropuerto
    JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
    JOIN PAISES po ON co.idPais = po.idPais
    JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
    JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
    JOIN PAISES pd ON cd.idPais = pd.idPais
    JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
    LEFT JOIN VUELOS_ASIENTOS va ON v.idVuelo = va.idVuelo
    WHERE v.idVuelo = @id
    GROUP BY 
        v.idVuelo, 
        v.codigo_vuelo, 
        v.idAeropuertoOrigen, 
        ao.nombre, 
        co.nombre, 
        po.nombre,
        v.idAeropuertoDestino, 
        ad.nombre, 
        cd.nombre, 
        pd.nombre, 
        v.fecha_salida, 
        v.fecha_llegada, 
        v.idAerolinea, 
        a.nombre, 
        v.precio_base, 
        v.cantidad_asientos, 
        v.asientos_disponibles, 
        v.estado,
        v.idCategoriaVuelo";


        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        vuelo = new Vuelo
                        {
                            IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                            CodigoVuelo = reader["codigo_vuelo"].ToString(),
                            IdAeropuertoOrigen = Convert.ToInt32(reader["idAeropuertoOrigen"]),
                            NombreAeropuertoOrigen = reader["nombreAeropuertoOrigen"].ToString(),
                            CiudadOrigen = reader["ciudadOrigen"].ToString(),
                            PaisOrigen = reader["paisOrigen"].ToString(),
                            IdAeropuertoDestino = Convert.ToInt32(reader["idAeropuertoDestino"]),
                            NombreAeropuertoDestino = reader["nombreAeropuertoDestino"].ToString(),
                            CiudadDestino = reader["ciudadDestino"].ToString(),
                            PaisDestino = reader["paisDestino"].ToString(),
                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            IdAerolinea = Convert.ToInt32(reader["idAerolinea"]),
                            NombreAerolinea = reader["nombreAerolinea"].ToString(),
                            PrecioBase = Convert.ToDecimal(reader["precio_base"]),
                            CantidadAsientos = Convert.ToInt32(reader["cantidad_asientos"]),
                            AsientosDisponibles = Convert.ToInt32(reader["asientos_disponibles"]),
                            Estado = reader["estado"].ToString(),
                            // ¡AGREGAR EL CAMPO AQUI!
                            IdCategoriaVuelo = Convert.ToInt32(reader["idCategoriaVuelo"]),

                            // Mapeo de asientos:
                            AsientosBusiness = Convert.ToInt32(reader["totalBusiness"]),
                            AsientosTurista = Convert.ToInt32(reader["totalTurista"]),
                            AsientosPrimeraClase = Convert.ToInt32(reader["totalPrimeraClase"]),
                            AsientosBusinessDisponibles = Convert.ToInt32(reader["disponiblesBusiness"]),
                            AsientosTuristaDisponibles = Convert.ToInt32(reader["disponiblesTurista"]),
                            AsientosPrimeraClaseDisponibles = Convert.ToInt32(reader["disponiblesPrimeraClase"])
                        };

                        vuelo.CodigoPaisOrigen = CodigoPaises.ContainsKey(vuelo.PaisOrigen) ? CodigoPaises[vuelo.PaisOrigen] : "UN";
                        vuelo.CodigoPaisDestino = CodigoPaises.ContainsKey(vuelo.PaisDestino) ? CodigoPaises[vuelo.PaisDestino] : "UN";
                    }
                }
            }
        }

        return vuelo;
    }

    public bool CrearVuelo(Vuelo vuelo)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            string query = @"
            INSERT INTO VUELOS
            (idAerolinea, idAeropuertoOrigen, idAeropuertoDestino, fecha_salida, fecha_llegada,
             idCategoriaVuelo, precio_base, estado, asientos_disponibles)
            VALUES
            (@idAerolinea, @idAeropuertoOrigen, @idAeropuertoDestino, @fechaSalida, @fechaLlegada,
             @idCategoriaVuelo, @precioBase, @estado, 0)";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idAerolinea", vuelo.IdAerolinea);
                cmd.Parameters.AddWithValue("@idAeropuertoOrigen", vuelo.IdAeropuertoOrigen);
                cmd.Parameters.AddWithValue("@idAeropuertoDestino", vuelo.IdAeropuertoDestino);
                cmd.Parameters.AddWithValue("@fechaSalida", vuelo.FechaSalida);
                cmd.Parameters.AddWithValue("@fechaLlegada", vuelo.FechaLlegada);
                cmd.Parameters.AddWithValue("@idCategoriaVuelo", vuelo.IdCategoriaVuelo);
                cmd.Parameters.AddWithValue("@precioBase", vuelo.PrecioBase);
                cmd.Parameters.AddWithValue("@estado", "Disponible");

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    public string GenerarCodigoVueloUnico(int idAerolinea, SqlConnection conn, SqlTransaction tx)
    {
        string nombreAerolinea = "";
        string codigoGenerado = "";
        Random random = new Random();

        // 1. Obtener el nombre de la aerolínea
        using (SqlCommand cmd = new SqlCommand("SELECT nombre FROM AEROLINEAS WHERE idAerolinea = @id", conn, tx))
        {
            cmd.Parameters.AddWithValue("@id", idAerolinea);
            var result = cmd.ExecuteScalar();
            if (result == null) throw new Exception("❌ No se encontró la aerolínea.");
            nombreAerolinea = result.ToString();
        }

        // 2. Intentar generar un código único
        bool existe = true;
        int intentos = 0;
        do
        {
            int numero = random.Next(0, 1000); // 000-999
            string numeroStr = numero.ToString("D3");
            codigoGenerado = $"{numeroStr}-{nombreAerolinea}";

            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM VUELOS WHERE codigo_vuelo = @codigo", conn, tx))
            {
                checkCmd.Parameters.AddWithValue("@codigo", codigoGenerado);
                int count = (int)checkCmd.ExecuteScalar();
                existe = count > 0;
            }

            intentos++;
            if (intentos > 1000) throw new Exception("❌ No se pudo generar un código único para el vuelo.");
        }
        while (existe);

        return codigoGenerado;
    }

    public string CrearVuelo(Vuelo vuelo, int cantPrimera, int cantBusiness, int cantTurista)
    {
        using (SqlConnection conn = GetConnection())
        {
            conn.Open();
            SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                // ✅ 1. Generar código de vuelo único
                string codigoGenerado = GenerarCodigoVueloUnico(vuelo.IdAerolinea, conn, transaction);
                vuelo.CodigoVuelo = codigoGenerado;

                // ✅ 2. Insertar vuelo
                string insertVuelo = @"
                INSERT INTO VUELOS (
                    codigo_vuelo, idAerolinea, idCategoriaVuelo,
                    fecha_salida, fecha_llegada,
                    precio_base, cantidad_asientos, asientos_disponibles,
                    estado, idAeropuertoOrigen, idAeropuertoDestino
                )
                VALUES (
                    @codigo, @idAerolinea, @idCategoriaVuelo,
                    @salida, @llegada,
                    @precio, @cantidad, @disponibles,
                    @estado, @idAeropuertoOrigen, @idAeropuertoDestino
                );
                SELECT SCOPE_IDENTITY();";

                int idVuelo;
                using (SqlCommand cmd = new SqlCommand(insertVuelo, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@codigo", vuelo.CodigoVuelo);
                    cmd.Parameters.AddWithValue("@idAerolinea", vuelo.IdAerolinea);
                    cmd.Parameters.AddWithValue("@idCategoriaVuelo", vuelo.IdCategoriaVuelo);
                    cmd.Parameters.AddWithValue("@salida", vuelo.FechaSalida);
                    cmd.Parameters.AddWithValue("@llegada", vuelo.FechaLlegada);
                    cmd.Parameters.AddWithValue("@precio", vuelo.PrecioBase);
                    cmd.Parameters.AddWithValue("@cantidad", vuelo.CantidadAsientos);
                    cmd.Parameters.AddWithValue("@disponibles", vuelo.AsientosDisponibles);
                    cmd.Parameters.AddWithValue("@estado", vuelo.Estado);
                    cmd.Parameters.AddWithValue("@idAeropuertoOrigen", vuelo.IdAeropuertoOrigen);
                    cmd.Parameters.AddWithValue("@idAeropuertoDestino", vuelo.IdAeropuertoDestino);

                    idVuelo = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // ✅ 3. Insertar asientos
                var asientoService = new AsientoService();
                asientoService.InsertarAsientosPorTipo(idVuelo, cantPrimera, cantBusiness, cantTurista, conn, transaction);

                transaction.Commit();

                // ✅ 4. Devolver código generado
                return codigoGenerado;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("❌ Error al crear vuelo: " + ex.Message);
                throw;
            }
        }
    }

    public List<Vuelo> ObtenerTodosOrdenadosRecientes()
    {
        var lista = new List<Vuelo>();

        using (var conn = GetConnection())
        {
            conn.Open();
            var query = @"SELECT * FROM VUELOS ORDER BY fecha_salida DESC";

            using (var cmd = new SqlCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Vuelo
                    {
                        IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                        CodigoVuelo = reader["codigo_vuelo"].ToString(),
                        FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                        FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                        PrecioBase = Convert.ToDecimal(reader["precio_base"]),
                        Estado = reader["estado"].ToString()
                        // Puedes agregar más propiedades si quieres
                    });
                }
            }
        }

        return lista;
    }

    public List<Vuelo> ObtenerVuelosConDetallesOrdenados()
    {
        var vuelos = new List<Vuelo>();

        using (var conn = GetConnection())
        {
            conn.Open();
            var query = @"
            SELECT 
                V.idVuelo,
                V.codigo_vuelo,
                V.fecha_salida,
                V.fecha_llegada,
                V.precio_base,
                V.estado,

                V.idCategoriaVuelo,
                C.nombre AS Categoria,

                V.idAerolinea,
                A.nombre AS NombreAerolinea,

                AO.nombre AS NombreAeropuertoOrigen,
                CO.nombre AS CiudadOrigen,
                PO.nombre AS PaisOrigen,

                AD.nombre AS NombreAeropuertoDestino,
                CD.nombre AS CiudadDestino,
                PD.nombre AS PaisDestino

            FROM VUELOS V
            INNER JOIN CATEGORIAS_VUELOS C ON V.idCategoriaVuelo = C.idCategoriaVuelo
            INNER JOIN AEROLINEAS A ON V.idAerolinea = A.idAerolinea

            INNER JOIN AEROPUERTOS AO ON V.idAeropuertoOrigen = AO.idAeropuerto
            INNER JOIN CIUDADES CO ON AO.idCiudad = CO.idCiudad
            INNER JOIN PAISES PO ON CO.idPais = PO.idPais

            INNER JOIN AEROPUERTOS AD ON V.idAeropuertoDestino = AD.idAeropuerto
            INNER JOIN CIUDADES CD ON AD.idCiudad = CD.idCiudad
            INNER JOIN PAISES PD ON CD.idPais = PD.idPais

            ORDER BY V.fecha_salida DESC";

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
                        IdCategoriaVuelo = Convert.ToInt32(reader["idCategoriaVuelo"]),
                        Categoria = reader["Categoria"].ToString(),
                        IdAerolinea = Convert.ToInt32(reader["idAerolinea"]),
                        NombreAerolinea = reader["NombreAerolinea"].ToString(),

                        NombreAeropuertoOrigen = reader["NombreAeropuertoOrigen"].ToString(),
                        CiudadOrigen = reader["CiudadOrigen"].ToString(),
                        PaisOrigen = reader["PaisOrigen"].ToString(),

                        NombreAeropuertoDestino = reader["NombreAeropuertoDestino"].ToString(),
                        CiudadDestino = reader["CiudadDestino"].ToString(),
                        PaisDestino = reader["PaisDestino"].ToString()
                    });
                }
            }
        }

        return vuelos;
    }

    public Vuelo ObtenerVueloById(int id)
    {
        Vuelo vuelo = null;
        try
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(@"
        SELECT 
            v.idVuelo, 
            v.codigo_vuelo, 
            ao.idAeropuerto AS IdAeropuertoOrigen, 
            co.nombre AS CiudadOrigen, 
            co.idCiudad AS idCiudadOrigen,            -- ✅ añadido
            ad.idAeropuerto AS IdAeropuertoDestino, 
            cd.nombre AS CiudadDestino, 
            cd.idCiudad AS idCiudadDestino,           -- ✅ añadido
            v.fecha_salida, 
            v.fecha_llegada, 
            v.precio_base, 
            a.nombre AS NombreAerolinea, 
            v.idAerolinea,                            -- ✅ añadido
            cv.nombre AS Categoria,
            v.idCategoriaVuelo, 
            v.estado
        FROM VUELOS v
        INNER JOIN AEROPUERTOS ao ON v.idAeropuertoOrigen = ao.idAeropuerto
        INNER JOIN CIUDADES co ON ao.idCiudad = co.idCiudad
        INNER JOIN AEROPUERTOS ad ON v.idAeropuertoDestino = ad.idAeropuerto
        INNER JOIN CIUDADES cd ON ad.idCiudad = cd.idCiudad
        INNER JOIN AEROLINEAS a ON v.idAerolinea = a.idAerolinea
        LEFT JOIN CATEGORIAS_VUELOS cv ON v.idCategoriaVuelo = cv.idCategoriaVuelo
        WHERE v.idVuelo = @id", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        vuelo = new Vuelo
                        {
                            IdVuelo = Convert.ToInt32(reader["idVuelo"]),
                            CodigoVuelo = reader["codigo_vuelo"].ToString(),

                            IdCiudadOrigen = Convert.ToInt32(reader["idCiudadOrigen"]),     // ✅ asignado
                            IdCiudadDestino = Convert.ToInt32(reader["idCiudadDestino"]),   // ✅ asignado
                            IdAerolinea = Convert.ToInt32(reader["idAerolinea"]),           // ✅ asignado

                            IdAeropuertoOrigen = Convert.ToInt32(reader["IdAeropuertoOrigen"]),
                            CiudadOrigen = reader["CiudadOrigen"].ToString(),
                            IdAeropuertoDestino = Convert.ToInt32(reader["IdAeropuertoDestino"]),
                            CiudadDestino = reader["CiudadDestino"].ToString(),

                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            PrecioBase = Convert.ToDecimal(reader["precio_base"]),

                            Categoria = reader["Categoria"].ToString(),
                            IdCategoriaVuelo = Convert.ToInt32(reader["idCategoriaVuelo"]),
                            Estado = reader["estado"].ToString(),

                            Aerolinea = new Aerolinea
                            {
                                Nombre = reader["NombreAerolinea"].ToString()
                            }
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al obtener detalles del vuelo: " + ex.Message);
        }
        return vuelo;
    }

    public List<dynamic> ObtenerOcupacionPorCategoria()
    {
        var datos = new List<dynamic>();

        using (var conn = new DbController().GetConnection())
        {
            conn.Open();

            string query = @"
            SELECT 
                v.codigo_vuelo,
                ca.nombre AS categoria,
                COUNT(*) AS reservados
            FROM VUELOS_ASIENTOS va
            INNER JOIN VUELOS v ON va.idVuelo = v.idVuelo
            INNER JOIN CATEGORIAS_ASIENTOS ca ON va.idCategoria = ca.idCategoria
            WHERE va.estado = 'Reservado'
            GROUP BY v.codigo_vuelo, ca.nombre
            ORDER BY v.codigo_vuelo, ca.nombre;
        ";

            using (var cmd = new SqlCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    datos.Add(new
                    {
                        codigoVuelo = reader["codigo_vuelo"].ToString(),
                        categoria = reader["categoria"].ToString(),
                        reservados = Convert.ToInt32(reader["reservados"])
                    });
                }
            }
        }

        return datos;
    }


}