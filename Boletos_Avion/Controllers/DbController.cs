using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

public class DbController
{
    private readonly string connectionString;

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

    public DbController()
    {
        //connectionString = "Data Source=DESKTOP-34DG23J\\SQLEXPRESS;Initial Catalog=GestionBoletos;User ID=sa;Password=Chiesafordel1+;TrustServerCertificate=True;";
        //connectionString = "Data Source=DESKTOP-MP89LU5;Initial Catalog=GestionBoletos;User ID=jona;Password=4321;TrustServerCertificate=True;";
        connectionString = "Data Source=DESKTOP-IT9FVD5\\SQLEXPRESS;Initial Catalog=GestionBoletos46;User ID=sa;Password=15012004;TrustServerCertificate=True;";
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(connectionString);
    }

    // Método para probar la conexión a la base de datos
    public string TestConnection()
    {
        try
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                return "Conectado a la base de datos: " + connection.Database;
            }
        }
        catch (Exception ex)
        {
            return "No se ha podido conectar a la base de datos: " + ex.Message;
        }
    }

    // Validar usuario en la base de datos
    public UserModel ValidateUser(string email, string password)
    {
        UserModel user = null;

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = "SELECT idUsuario, nombre, correo, contrasena, idRol FROM USUARIOS WHERE correo = @Email AND contrasena = @Password";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password); // 🔥 Sin hashing

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel
                        {
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                            Nombre = reader["nombre"].ToString(),
                            Correo = reader["correo"].ToString(),
                            Contrasena = reader["contrasena"].ToString(),
                            IdRol = Convert.ToInt32(reader["idRol"])
                        };
                    }
                }
            }
        }
        return user;
    }

    // Registrar usuario en la base de datos
    public bool RegisterUser(UserModel user)
    {
        string query = @"INSERT INTO USUARIOS (nombre, correo, telefono, direccion, documento_identidad, contrasena, idRol, fecha_registro) 
                     VALUES (@Nombre, @Correo, @Telefono, @Direccion, @Documento, @Contrasena, @IdRol, GETDATE())";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nombre", user.Nombre);
                command.Parameters.AddWithValue("@Correo", user.Correo);
                command.Parameters.AddWithValue("@Telefono", user.Telefono ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Direccion", user.Direccion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Documento", user.DocumentoIdentidad);
                command.Parameters.AddWithValue("@Contrasena", user.Contrasena);
                command.Parameters.AddWithValue("@IdRol", user.IdRol);

                try
                {
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
                catch (SqlException ex)
                {
                    // Log de error en consola y retorno de falso para manejo en el controlador
                    Console.WriteLine($"Error SQL: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error general: {ex.Message}");
                    return false;
                }
            }
        }
    }

    // Verificar si un usuario ya existe en la base de datos por su correo
    public bool CheckUserExists(string email)
    {
        bool exists = false;
        string query = "SELECT COUNT(*) FROM USUARIOS WHERE correo = @Correo";

        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Correo", email);
                connection.Open();
                int count = (int)command.ExecuteScalar();
                exists = count > 0;
            }
        }
        return exists;
    }

    // Verificar si un número de teléfono ya está registrado
    public bool CheckPhoneExists(string phone)
    {
        bool exists = false;
        string query = "SELECT COUNT(*) FROM USUARIOS WHERE telefono = @Telefono";

        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Telefono", phone);
                connection.Open();
                int count = (int)command.ExecuteScalar();
                exists = count > 0;
            }
        }
        return exists;
    }

    // Verificar si un documento de identidad ya está registrado
    public bool CheckDocumentExists(string document)
    {
        bool exists = false;
        string query = "SELECT COUNT(*) FROM USUARIOS WHERE documento_identidad = @Documento";

        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Documento", document);
                connection.Open();
                int count = (int)command.ExecuteScalar();
                exists = count > 0;
            }
        }
        return exists;
    }

    // Método para eliminar un usuario
    public bool DeleteUser(int idUsuario)
    {
        string query = "DELETE FROM USUARIOS WHERE idUsuario = @IdUsuario";

        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdUsuario", idUsuario);

                connection.Open();
                int result = command.ExecuteNonQuery();
                return result > 0;
            }
        }
    }

    // Obtener la contraseña actual por correo
    public string GetUserPasswordByEmail(string correo)
    {
        string password = null;
        string query = "SELECT contrasena FROM USUARIOS WHERE correo = @Correo";

        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Correo", correo);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        password = reader["contrasena"].ToString();
                    }
                }
            }
        }
        return password;
    }

    // Obtener un usuario por su ID (incluye contraseña)
    public UserModel GetUserById(int idUsuario)
    {
        UserModel user = null;

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
                SELECT 
                    idUsuario,
                    nombre,
                    correo,
                    telefono,
                    direccion,
                    documento_identidad,
                    contrasena,
                    idRol
                FROM USUARIOS
                WHERE idUsuario = @Id
            ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", idUsuario);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel
                        {
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                            Nombre = reader["nombre"].ToString(),
                            Correo = reader["correo"].ToString(),
                            Telefono = reader["telefono"].ToString(),
                            Direccion = reader["direccion"].ToString(),
                            DocumentoIdentidad = reader["documento_identidad"].ToString(),
                            Contrasena = reader["contrasena"].ToString(),
                            IdRol = Convert.ToInt32(reader["idRol"])
                        };
                    }
                }
            }
        }
        return user;
    }

    // Actualizar la información de un usuario (incluyendo la contraseña)
    public bool UpdateUser(UserModel user)
    {
        string query = @"
            UPDATE USUARIOS 
               SET nombre = @Nombre,
                   correo = @Correo,
                   telefono = @Telefono,
                   direccion = @Direccion,
                   documento_identidad = @Documento,
                   contrasena = @Contrasena,
                   idRol = @IdRol
             WHERE idUsuario = @IdUsuario
        ";

        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nombre", user.Nombre);
                command.Parameters.AddWithValue("@Correo", user.Correo);
                command.Parameters.AddWithValue("@Telefono", user.Telefono ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Direccion", user.Direccion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Documento", user.DocumentoIdentidad);
                command.Parameters.AddWithValue("@Contrasena", user.Contrasena);
                command.Parameters.AddWithValue("@IdRol", user.IdRol);
                command.Parameters.AddWithValue("@IdUsuario", user.IdUsuario);

                connection.Open();
                int result = command.ExecuteNonQuery();
                return result > 0;
            }
        }
    }

    //===========================================
    // MÉTODOS ADICIONALES PARA GESTIÓN DE VUELOS
    //===========================================


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


}