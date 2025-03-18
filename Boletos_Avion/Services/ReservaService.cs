using Microsoft.Data.SqlClient;

public class ReservaService : DbController
{
    public ReservaService() : base() { }

    // 🔹 Método para crear una reserva con el total de la compra
    public int CrearReserva(int idUsuario, int idVuelo, string numeroReserva, DateTime fechaReserva, decimal totalReserva)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
            INSERT INTO RESERVAS (numeroReserva, idUsuario, idVuelo, fechaReserva, total)
            OUTPUT INSERTED.idReserva
            VALUES (@numeroReserva, @idUsuario, @idVuelo, @fechaReserva, @totalReserva);";

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

    // 🔹 Método para registrar los asientos en la reserva
    public bool RegistrarAsientos(int idReserva, List<int> asientosIds)
    {
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // ✅ Verificar que la lista de asientos no está vacía
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
}
