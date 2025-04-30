using Boletos_Avion.Models;
using Boletos_Avion.Services;
using Microsoft.Data.SqlClient;

public class ReservaService : DbController
{
    public ReservaService() : base() { }

    public int CrearReserva(int idUsuario, int idVuelo, string numeroReserva, DateTime fechaReserva, decimal totalReserva)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
        DECLARE @InsertedIds TABLE (idReserva INT);

        INSERT INTO RESERVAS (numeroReserva, idUsuario, idVuelo, fechaReserva, total)
        OUTPUT INSERTED.idReserva INTO @InsertedIds
        VALUES (@numeroReserva, @idUsuario, @idVuelo, @fechaReserva, @totalReserva);

        SELECT idReserva FROM @InsertedIds;";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@numeroReserva", numeroReserva);
                command.Parameters.AddWithValue("@idUsuario", idUsuario);
                command.Parameters.AddWithValue("@idVuelo", idVuelo);
                command.Parameters.AddWithValue("@fechaReserva", fechaReserva);
                command.Parameters.AddWithValue("@totalReserva", totalReserva);

                return (int)command.ExecuteScalar();
            }
        }
    }

    public bool RegistrarAsientos(int idReserva, List<int> asientosIds)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    if (asientosIds == null || asientosIds.Count == 0)
                    {
                        Console.WriteLine("[ERROR] La lista de asientos está vacía. No se pueden registrar.");
                        transaction.Rollback();
                        return false;
                    }

                    Console.WriteLine($"[LOG] Insertando {asientosIds.Count} asientos en RESERVA_ASIENTOS para la reserva {idReserva}");

                    string query = @"
                INSERT INTO RESERVA_ASIENTOS (idReserva, idVueloAsiento) 
                VALUES (@idReserva, @idAsiento);";

                    foreach (var idAsiento in asientosIds)
                    {
                        Console.WriteLine($"[LOG] Insertando asiento ID: {idAsiento} en reserva {idReserva}");

                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@idReserva", idReserva);
                            command.Parameters.AddWithValue("@idAsiento", idAsiento);
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    Console.WriteLine("[LOG] Inserción exitosa en RESERVA_ASIENTOS.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Falló la inserción de asientos: {ex.Message}");
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }

    public List<Reservas> ObtenerReservasPorUsuario(int idUsuario)
    {
        List<Reservas> reservas = new List<Reservas>();

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
        SELECT r.idReserva, r.numeroReserva, r.fechaReserva, r.total,
               v.codigo_vuelo,
               r.estado
        FROM RESERVAS r
        INNER JOIN VUELOS v ON r.idVuelo = v.idVuelo
        WHERE r.idUsuario = @idUsuario";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idUsuario", idUsuario);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reservas.Add(new Reservas
                        {
                            IdReserva = reader.GetInt32(0),
                            NumeroReserva = reader.GetString(1),
                            FechaReserva = reader.GetDateTime(2),
                            Total = reader.GetDecimal(3),
                            Origen = "N/A",
                            Destino = reader.GetString(4),
                            FechaSalida = DateTime.MinValue,
                            FechaLlegada = DateTime.MinValue,
                            Estado = reader.GetString(5)
                        });
                    }
                }
            }
        }

        return reservas;
    }


    public List<Reservas> ObtenerReservasPorVuelo(int idVuelo)
    {
        List<Reservas> reservas = new List<Reservas>();

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
            SELECT idReserva, numeroReserva, idUsuario, idVuelo, fechaReserva, total, estado
            FROM RESERVAS
            WHERE idVuelo = @idVuelo AND estado = 'Activo'";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idVuelo", idVuelo);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reservas.Add(new Reservas
                        {
                            IdReserva = reader.GetInt32(0),
                            NumeroReserva = reader.GetString(1),
                            IdUsuario = reader.GetInt32(2),
                            IdVuelo = reader.GetInt32(3),
                            FechaReserva = reader.GetDateTime(4),
                            Total = reader.GetDecimal(5),
                            Estado = reader.GetString(6)
                        });
                    }
                }
            }
        }

        return reservas;
    }

    public Reservas ObtenerReservaPorId(int idReserva)
    {
        Reservas reserva = null;

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
        SELECT r.idReserva, r.numeroReserva, r.fechaReserva, r.total, r.estado,
               v.codigo_vuelo, r.idUsuario, r.idVuelo
        FROM RESERVAS r
        INNER JOIN VUELOS v ON r.idVuelo = v.idVuelo
        WHERE r.idReserva = @idReserva";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idReserva", idReserva);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        reserva = new Reservas
                        {
                            IdReserva = reader.GetInt32(0),
                            NumeroReserva = reader.GetString(1),
                            FechaReserva = reader.GetDateTime(2),
                            Total = reader.GetDecimal(3),
                            Estado = reader.GetString(4),
                            Destino = reader.GetString(5),
                            IdUsuario = reader.GetInt32(6),
                            IdVuelo = reader.GetInt32(7)
                        };
                    }
                }
            }
        }

        return reserva;
    }

    public bool CambiarEstadoReserva(int idReserva, string nuevoEstado)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = "UPDATE RESERVAS SET estado = @nuevoEstado WHERE idReserva = @idReserva";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);
                command.Parameters.AddWithValue("@idReserva", idReserva);

                int filasAfectadas = command.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
        }
    }

    public List<TransaccionViewModel> ObtenerHistorialTransacciones()
    {
        List<TransaccionViewModel> historial = new List<TransaccionViewModel>();

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
            SELECT 
                R.numeroReserva,
                U.nombre,
                U.documento_identidad,
                R.total
            FROM RESERVAS R
            INNER JOIN USUARIOS U ON R.idUsuario = U.idUsuario;";


            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        historial.Add(new TransaccionViewModel
                        {
                            NumeroReserva = reader.GetString(0),
                            NombreCliente = reader.GetString(1),
                            DocumentoIdentidad = reader.GetString(2),
                            TotalPagado = reader.GetDecimal(3)
                        });
                    }
                }
            }
        }

        return historial;
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
