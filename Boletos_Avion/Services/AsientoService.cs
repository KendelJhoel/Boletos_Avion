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
                        NombreCategoria = reader.GetString(4), 
                        Precio = reader.GetDecimal(5), 
                        Estado = reader.GetString(6)
                    });
                }
            }
        }
        return asientos;
    }


    //  Reervar asientos
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

            string query = @"
        SELECT COUNT(*) 
        FROM VUELOS_ASIENTOS 
        WHERE idVueloAsiento IN (" + string.Join(",", asientosIds.Select((id, index) => $"@id{index}")) + @") 
        AND estado = 'Reservado'";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                for (int i = 0; i < asientosIds.Count; i++)
                {
                    command.Parameters.AddWithValue($"@id{i}", asientosIds[i]);
                }

                int count = (int)command.ExecuteScalar();
                return count == 0; 
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
                        NombreCategoria = reader.GetString(3), 
                        Precio = reader.GetDecimal(4),
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
                        idVueloAsiento = reader.GetInt32(0), 
                        numero = reader.GetString(1), 
                        idCategoria = reader.GetInt32(2), 
                        nombreCategoria = reader.GetString(3), 
                        precio = reader.GetDecimal(4), 
                        estado = reader.GetString(5) 
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

    public void InsertarAsientosPorTipo(int idVuelo, int cantPrimera, int cantBusiness, int cantTurista, SqlConnection conn, SqlTransaction transaction)
    {
        Dictionary<string, int> categorias = new Dictionary<string, int>();

        string queryCategorias = "SELECT idCategoria, nombre FROM CATEGORIAS_ASIENTOS";
        using (SqlCommand cmd = new SqlCommand(queryCategorias, conn, transaction))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                string nombre = reader["nombre"].ToString().Trim().ToLower();
                int idCategoria = Convert.ToInt32(reader["idCategoria"]);
                categorias[nombre] = idCategoria;
            }
        }

        void insertar(string tipoNombre, int cantidad, char prefijo)
        {
            if (!categorias.ContainsKey(tipoNombre.ToLower())) return;

            int idCategoria = categorias[tipoNombre.ToLower()];

            for (int i = 1; i <= cantidad; i++)
            {
                string numAsiento = $"{prefijo}{i}";
                string insert = @"
                INSERT INTO VUELOS_ASIENTOS (idVuelo, numero, idCategoria, estado)
                VALUES (@idVuelo, @numero, @idCategoria, 'disponible')";

                using (SqlCommand cmd = new SqlCommand(insert, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@idVuelo", idVuelo);
                    cmd.Parameters.AddWithValue("@numero", numAsiento);
                    cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        insertar("Primera Clase", cantPrimera, 'P');
        insertar("Business", cantBusiness, 'B');
        insertar("Turista", cantTurista, 'T');
    }

    public List<Asiento> ObtenerAsientosPorReserva(int idReserva)
    {
        List<Asiento> asientos = new List<Asiento>();

        string query = @"
        SELECT va.idVueloAsiento, va.numero, va.idCategoria, c.nombre AS nombreCategoria,
               c.precio, va.estado
        FROM RESERVA_ASIENTOS ra
        JOIN VUELOS_ASIENTOS va ON ra.idVueloAsiento = va.idVueloAsiento
        JOIN CATEGORIAS_ASIENTOS c ON va.idCategoria = c.idCategoria
        WHERE ra.idReserva = @idReserva";

        using (SqlConnection connection = GetConnection())
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@idReserva", idReserva);
            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    asientos.Add(new Asiento
                    {
                        IdVueloAsiento = reader.GetInt32(0),
                        Numero = reader.GetString(1),
                        IdCategoria = reader.GetInt32(2),
                        NombreCategoria = reader.GetString(3),
                        Precio = reader.GetDecimal(4),
                        Estado = reader.GetString(5)
                    });
                }
            }
        }

        return asientos;
    }

    public bool LiberarAsientosPorReserva(int idReserva)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();

            string query = @"
            UPDATE va
            SET va.estado = 'Disponible'
            FROM VUELOS_ASIENTOS va
            INNER JOIN RESERVA_ASIENTOS ra ON ra.idVueloAsiento = va.idVueloAsiento
            WHERE ra.idReserva = @idReserva";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idReserva", idReserva);
                int filas = command.ExecuteNonQuery();
                return filas > 0;
            }
        }
    }

}
