namespace Boletos_Avion.Services;

using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;


public class AccountService : DbController
{
    public AccountService() : base() { }

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

    //ACTUALIZAR GMAIL DE USUARIO
    public bool UpdateUserEmail(int userId, string newEmail)
    {
        string query = "UPDATE USUARIOS SET correo = @Correo WHERE idUsuario = @UserId";

        using (SqlConnection connection = GetConnection())
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Correo", newEmail);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                
                Console.WriteLine($"Filas afectadas: {rowsAffected} para el usuario {userId}");

                return rowsAffected > 0; 
            }
        }
    }

    public Reservas GetReservaById(int idReserva)
    {
        Reservas reserva = null;

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
            SELECT r.idUsuario, r.idReserva, r.numeroReserva, r.fechaReserva, r.total, r.estado,
                   v.fecha_salida, v.fecha_llegada, a1.nombre AS origen, a2.nombre AS destino
            FROM RESERVAS r
            INNER JOIN VUELOS v ON r.idVuelo = v.idVuelo
            INNER JOIN AEROPUERTOS a1 ON v.idAeropuertoOrigen = a1.idAeropuerto
            INNER JOIN AEROPUERTOS a2 ON v.idAeropuertoDestino = a2.idAeropuerto
            WHERE r.idReserva = @IdReserva";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdReserva", idReserva);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        reserva = new Reservas
                        {
                            IdReserva = Convert.ToInt32(reader["idReserva"]),
                            NumeroReserva = reader["numeroReserva"].ToString(),
                            FechaReserva = Convert.ToDateTime(reader["fechaReserva"]),
                            Total = Convert.ToDecimal(reader["total"]),
                            Estado = reader["estado"].ToString(),
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            Origen = reader["origen"].ToString(),
                            Destino = reader["destino"].ToString()
                        };
                    }
                }
            }
        }

        return reserva;
    }

    public bool CancelarReserva(string numeroReserva, out string mensaje)
    {
        mensaje = string.Empty;

        using (var connection = GetConnection())
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Obtener la fecha de salida de la reserva
                    DateTime fechaSalida;
                    using (var command = new SqlCommand(@"
                    SELECT v.fecha_salida 
                    FROM VUELOS v
                    JOIN RESERVAS r ON v.idVuelo = r.idVuelo
                    WHERE r.numeroReserva = @NumeroReserva", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@NumeroReserva", numeroReserva);
                        var result = command.ExecuteScalar();
                        if (result == null)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        fechaSalida = (DateTime)result;
                    }

                    DateTime fechaHoy = DateTime.Today;
                    int diferenciaDias = (fechaSalida - fechaHoy).Days;

                    if (diferenciaDias >= 15)
                    {
                        mensaje = "Se le devolverá el 50% del monto total que pagó.";
                    }
                    else if (diferenciaDias >= 7)
                    {
                        mensaje = "No se le devolverá ningún monto.";
                    }
                    else
                    {
                        mensaje = "No se puede cancelar la reserva con menos de 7 días de anticipación.";
                        return false;
                    }

                    // Actualizar el estado de la reserva a 'Cancelada'
                    using (var command = new SqlCommand("UPDATE Reservas SET estado = 'Cancelada' WHERE NumeroReserva = @NumeroReserva", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@NumeroReserva", numeroReserva);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    // Obtener los IDs de los asientos asociados a la reserva
                    List<int> asientosIds = new List<int>();
                    using (var command = new SqlCommand(@"
                    SELECT va.idVueloAsiento 
                    FROM VUELOS_ASIENTOS va
                    JOIN RESERVA_ASIENTOS ra ON va.idVueloAsiento = ra.idVueloAsiento
                    JOIN RESERVAS r ON ra.idReserva = r.idReserva
                    WHERE r.numeroReserva = @NumeroReserva", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@NumeroReserva", numeroReserva);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                asientosIds.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    // Cambiar el estado de los asientos a 'Disponible'
                    if (asientosIds.Count > 0)
                    {
                        var asientoService = new AsientoService();
                        bool asientosActualizados = asientoService.CambiarEstadoAsientos(asientosIds, "Disponible");
                        if (!asientosActualizados)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }

    public List<Reservas> GetReservasActivas(int userId)
    {
        List<Reservas> reserva = new List<Reservas>();

        string query = @"
            SELECT r.idReserva, r.numeroReserva, r.fechaReserva, r.total, r.estado,
               v.fecha_salida, v.fecha_llegada, a1.nombre AS origen, a2.nombre AS destino
        FROM RESERVAS r
        INNER JOIN VUELOS v ON r.idVuelo = v.idVuelo
        INNER JOIN AEROPUERTOS a1 ON v.idAeropuertoOrigen = a1.idAeropuerto
        INNER JOIN AEROPUERTOS a2 ON v.idAeropuertoDestino = a2.idAeropuerto
        WHERE r.idUsuario = @UserId
        AND v.fecha_salida >= GETDATE()
        AND r.estado = 'Activo'
        ORDER BY v.fecha_salida ASC";

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Reservas reservas = new Reservas
                        {
                            IdReserva = Convert.ToInt32(reader["idReserva"]),
                            NumeroReserva = reader["numeroReserva"].ToString(),
                            FechaReserva = Convert.ToDateTime(reader["fechaReserva"]),
                            Total = Convert.ToDecimal(reader["total"]),
                            FechaSalida = Convert.ToDateTime(reader["fecha_salida"]),
                            FechaLlegada = Convert.ToDateTime(reader["fecha_llegada"]),
                            Estado = reader["estado"].ToString(),
                            Origen = reader["origen"].ToString(),
                            Destino = reader["destino"].ToString()
                        };

                        reserva.Add(reservas);
                    }
                }
            }
        }
        return reserva;
    }
}



