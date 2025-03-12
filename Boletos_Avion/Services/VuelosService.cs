using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;


public class VuelosService : DbController
{
        public VuelosService() : base() { } // ✅ Llama al constructor de DbController



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


    // 🔹 Obtener vuelos por continente (América, Asia, Europa, Oceanía)
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
        WHERE c.nombre = @Continente";

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

        // 🔹 Obtener vuelos económicos (precio base entre 200 y 450 USD)
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
        WHERE v.precio_base BETWEEN @MinPrice AND @MaxPrice";

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

        // 🔹 Obtener vuelos cortos (duración menor o igual a 5 horas)
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
        WHERE DATEDIFF(MINUTE, v.fecha_salida, v.fecha_llegada) <= @MaxMinutes";

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

        // 🔹 Método para mapear el resultado SQL al modelo `Vuelo`
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
        WHERE v.idVuelo = @id";

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
                    }
                }
            }

            return vuelo;
        }

        public Vuelo GetVueloDetallesById(int id)
        {
            Vuelo vuelo = null;
            string query = @"
        SELECT 
            v.idVuelo, v.codigo_vuelo, 
            v.idAeropuertoOrigen, ao.nombre AS nombreAeropuertoOrigen, co.nombre AS ciudadOrigen, po.nombre AS paisOrigen,
            v.idAeropuertoDestino, ad.nombre AS nombreAeropuertoDestino, cd.nombre AS ciudadDestino, pd.nombre AS paisDestino,
            v.fecha_salida, v.fecha_llegada, 
            v.idAerolinea, a.nombre AS nombreAerolinea,
            v.precio_base, v.cantidad_asientos, v.asientos_disponibles, v.estado,
            
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
        LEFT JOIN VUELOS_ASIENTOS va ON v.idVuelo = va.idVuelo -- Relación con asientos

        WHERE v.idVuelo = @id
        GROUP BY v.idVuelo, v.codigo_vuelo, v.idAeropuertoOrigen, ao.nombre, co.nombre, po.nombre,
                 v.idAeropuertoDestino, ad.nombre, cd.nombre, pd.nombre, v.fecha_salida, v.fecha_llegada, 
                 v.idAerolinea, a.nombre, v.precio_base, v.cantidad_asientos, v.asientos_disponibles, v.estado";

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

                                // Cantidad total de asientos por tipo
                                AsientosBusiness = Convert.ToInt32(reader["totalBusiness"]),
                                AsientosTurista = Convert.ToInt32(reader["totalTurista"]),
                                AsientosPrimeraClase = Convert.ToInt32(reader["totalPrimeraClase"]),

                                // Cantidad de asientos disponibles por tipo
                                AsientosBusinessDisponibles = Convert.ToInt32(reader["disponiblesBusiness"]),
                                AsientosTuristaDisponibles = Convert.ToInt32(reader["disponiblesTurista"]),
                                AsientosPrimeraClaseDisponibles = Convert.ToInt32(reader["disponiblesPrimeraClase"])
                            };

                            // Agregar el código del país usando el diccionario
                            vuelo.CodigoPaisOrigen = CodigoPaises.ContainsKey(vuelo.PaisOrigen) ? CodigoPaises[vuelo.PaisOrigen] : "UN";
                            vuelo.CodigoPaisDestino = CodigoPaises.ContainsKey(vuelo.PaisDestino) ? CodigoPaises[vuelo.PaisDestino] : "UN";
                        }
                    }
                }
            }

            return vuelo;
        }

        public List<Vuelo> GetVuelos(string origen, string destino, DateTime? fechaIda, decimal? precioMin, decimal? precioMax, string aerolinea, string categoria)
        {
            List<Vuelo> vuelos = new List<Vuelo>();
            using (SqlConnection conn = GetConnection())
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
            ao.idAeropuerto AS IdAeropuertoOrigen, co.nombre AS CiudadOrigen, 
            ad.idAeropuerto AS IdAeropuertoDestino, cd.nombre AS CiudadDestino, 
            v.fecha_salida, v.fecha_llegada, v.precio_base, 
            a.nombre AS NombreAerolinea, 
            cv.nombre AS Categoria, v.estado,
            -- Obtener cantidad de asientos disponibles
            (SELECT COUNT(*) FROM VUELOS_ASIENTOS va WHERE va.idVuelo = v.idVuelo AND va.estado = 'Disponible') AS AsientosDisponibles
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
            }
            return vuelos;
        }
    }

