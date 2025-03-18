using Boletos_Avion.Models;
using GestionBoletos.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

public class AsientoService : DbController
{
    public AsientoService() : base() { }

    public List<Asiento> ObtenerAsientosPorVuelo(int idVuelo)
    {
        List<Asiento> asientos = new List<Asiento>();

        string query = @"
        SELECT va.idVueloAsiento, va.idVuelo, va.numero, c.idCategoria, 
               c.nombre AS nombreCategoria, c.precio, va.estado
        FROM VUELOS_ASIENTOS va
        JOIN CATEGORIAS_ASIENTOS c ON va.idCategoria = c.idCategoria
        WHERE va.idVuelo = @idVuelo";

        using (SqlConnection connection = GetConnection())
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@idVuelo", idVuelo);
            connection.Open();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    asientos.Add(new Asiento
                    {
                        IdVueloAsiento = reader.GetInt32(0),
                        IdVuelo = reader.GetInt32(1),
                        Numero = reader.GetString(2),
                        IdCategoria = reader.GetInt32(3),
                        NombreCategoria = reader.GetString(4), // ✅ Ahora se obtiene el nombre de la categoría
                        Precio = reader.GetDecimal(5), // ✅ Ahora usa el precio real de la BD
                        Estado = reader.GetString(6)
                    });
                }
            }
        }
        return asientos;
    }


    // 🔹 Reservar asientos
    public bool ReservarAsientos(List<int> asientosIds)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var idAsiento in asientosIds)
                    {
                        string query = @"
                            UPDATE VUELOS_ASIENTOS
                            SET estado = 'Reservado'
                            WHERE idVueloAsiento = @idVueloAsiento AND estado = 'Disponible'";

                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@idVueloAsiento", idAsiento);
                            int affectedRows = command.ExecuteNonQuery();
                            if (affectedRows == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
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

    public bool VerificarDisponibilidad(List<int> asientosIds)
    {
        if (asientosIds == null || asientosIds.Count == 0)
            return false;

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();

            // 🔹 Crear una consulta con parámetros dinámicos para evitar SQL Injection
            string query = @"
        SELECT COUNT(*) 
        FROM VUELOS_ASIENTOS 
        WHERE idVueloAsiento IN (" + string.Join(",", asientosIds.Select((id, index) => $"@id{index}")) + @") 
        AND estado = 'Reservado'";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // 🔹 Agregar parámetros dinámicos para cada ID de asiento
                for (int i = 0; i < asientosIds.Count; i++)
                {
                    command.Parameters.AddWithValue($"@id{i}", asientosIds[i]);
                }

                int count = (int)command.ExecuteScalar();
                return count == 0; // 🔹 Devuelve `true` si no hay asientos reservados
            }
        }
    }

    public List<Asiento> ObtenerDetallesAsientos(List<int> asientosIds)
    {
        List<Asiento> detalles = new List<Asiento>();

        if (asientosIds == null || asientosIds.Count == 0)
            return detalles;

        string query = @"
        SELECT va.idVueloAsiento, va.numero, c.idCategoria, c.nombre AS nombreCategoria, 
               c.precio, va.estado
        FROM VUELOS_ASIENTOS va
        JOIN CATEGORIAS_ASIENTOS c ON va.idCategoria = c.idCategoria
        WHERE va.idVueloAsiento IN (" + string.Join(",", asientosIds) + @")";

        using (SqlConnection connection = GetConnection())
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            connection.Open();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    detalles.Add(new Asiento
                    {
                        IdVueloAsiento = reader.GetInt32(0),
                        Numero = reader.GetString(1),
                        IdCategoria = reader.GetInt32(2),
                        NombreCategoria = reader.GetString(3), // Ahora obtenemos el nombre de la categoría
                        Precio = reader.GetDecimal(4), // ✅ Precio real desde la BD
                        Estado = reader.GetString(5)
                    });
                }
            }
        }
        return detalles;
    }


    public List<dynamic> ObtenerDetallesAsientosPorVuelo(int idVuelo)
    {
        var detalles = new List<dynamic>();

        string query = @"
        SELECT va.idVueloAsiento, va.numero, c.idCategoria, c.nombre AS nombreCategoria, 
               c.precio, va.estado
        FROM VUELOS_ASIENTOS va
        JOIN CATEGORIAS_ASIENTOS c ON va.idCategoria = c.idCategoria
        WHERE va.idVuelo = @idVuelo;
    ";

        using (var connection = GetConnection())
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@idVuelo", idVuelo);
            connection.Open();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    detalles.Add(new
                    {
                        idVueloAsiento = reader.GetInt32(0), // ID del asiento en el vuelo
                        numero = reader.GetString(1), // Número del asiento
                        idCategoria = reader.GetInt32(2), // ID de la categoría
                        nombreCategoria = reader.GetString(3), // Nombre de la categoría
                        precio = reader.GetDecimal(4), // Precio del asiento
                        estado = reader.GetString(5) // Estado del asiento (Disponible, Reservado, etc.)
                    });
                }
            }
        }
        return detalles;
    }


    public bool GuardarReserva(int userId, int idVuelo, List<int> asientos, string numeroReserva)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // 🔹 Insertar la reserva y obtener el ID generado
                    string queryReserva = @"
                INSERT INTO RESERVAS (NumeroReserva, IdUsuario, IdVuelo, FechaReserva) 
                VALUES (@NumeroReserva, @IdUsuario, @IdVuelo, GETDATE());
                SELECT SCOPE_IDENTITY();";

                    int reservaId;
                    using (SqlCommand command = new SqlCommand(queryReserva, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@NumeroReserva", numeroReserva);
                        command.Parameters.AddWithValue("@IdUsuario", userId);
                        command.Parameters.AddWithValue("@IdVuelo", idVuelo);
                        reservaId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // 🔹 Asociar los asientos a la reserva
                    foreach (var idAsiento in asientos)
                    {
                        string queryReservaAsientos = @"
                    INSERT INTO BOLETOS_DETALLE (idBoleto, idVuelo, idVueloAsiento)
                    VALUES (@IdReserva, @IdVuelo, @IdVueloAsiento);";

                        using (SqlCommand command = new SqlCommand(queryReservaAsientos, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdReserva", reservaId);
                            command.Parameters.AddWithValue("@IdVuelo", idVuelo);
                            command.Parameters.AddWithValue("@IdVueloAsiento", idAsiento);
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit(); // 🔹 Confirmar la transacción si todo salió bien
                    return true;
                }
                catch
                {
                    transaction.Rollback(); // 🔹 Si hay un error, se deshacen los cambios
                    return false;
                }
            }
        }
    }


    public bool CambiarEstadoAsientos(List<int> asientosIds, string nuevoEstado)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string query = $@"
                    UPDATE VUELOS_ASIENTOS
                    SET estado = @nuevoEstado
                    WHERE idVueloAsiento IN ({string.Join(",", asientosIds)})";

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);
                        int affectedRows = command.ExecuteNonQuery();

                        if (affectedRows == 0)
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



}
