using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace Boletos_Avion.Services
{
    public class MonitorService : DbController
    {
        public MonitorService() : base() { }
        public async Task CrearAsync(Boletos_Avion.Models.Monitor monitor)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"INSERT INTO MONITOR 
                (nombre, correo, documento_identidad, contrasena, idAerolinea) 
                VALUES (@nombre, @correo, @documento_identidad, @contrasena, @idAerolinea)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", monitor.Nombre);
                    cmd.Parameters.AddWithValue("@correo", monitor.Correo);
                    cmd.Parameters.AddWithValue("@documento_identidad", monitor.DocumentoIdentidad); // <- corregido aquí
                    cmd.Parameters.AddWithValue("@contrasena", monitor.Contrasena);
                    cmd.Parameters.AddWithValue("@idAerolinea", monitor.IdAerolinea);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    conn.Close();
                }

            }
        }

        public async Task<List<Boletos_Avion.Models.Monitor>> ObtenerTodosAsync()
        {
            var lista = new List<Boletos_Avion.Models.Monitor>();

            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT m.idMonitor, m.nombre, m.correo, m.documento_identidad, m.idAerolinea,
                         a.nombre AS nombreAerolinea
                         FROM MONITOR m
                         INNER JOIN AEROLINEAS a ON m.idAerolinea = a.idAerolinea";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Boletos_Avion.Models.Monitor
                            {
                                IdMonitor = (int)reader["idMonitor"],
                                Nombre = reader["nombre"].ToString(),
                                Correo = reader["correo"].ToString(),
                                DocumentoIdentidad = reader["documento_identidad"].ToString(), 
                                IdAerolinea = (int)reader["idAerolinea"],
                                NombreAerolinea = reader["nombreAerolinea"].ToString()
                            });
                        }
                    }
                    conn.Close();
                }
            }

            return lista;
        }

        public string ObtenerNombreAerolinea(int idAerolinea)
        {
            string nombre = "";

            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT nombre FROM AEROLINEAS WHERE idAerolinea = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idAerolinea);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        nombre = result.ToString();
                }
            }

            return nombre;
        }

        public Models.Monitor ValidateMonitor(string correo, string contrasena)
        {
            Models.Monitor monitor = null;

            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT * FROM MONITOR WHERE correo = @correo AND contrasena = @contrasena";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.Parameters.AddWithValue("@contrasena", contrasena);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            monitor = new Models.Monitor
                            {
                                IdMonitor = (int)reader["idMonitor"],
                                Nombre = reader["nombre"].ToString(),
                                Correo = reader["correo"].ToString(),
                                DocumentoIdentidad = reader["documento_identidad"].ToString(),
                                Contrasena = reader["contrasena"].ToString(),
                                IdAerolinea = (int)reader["idAerolinea"]
                            };
                        }
                    }
                }
            }

            return monitor;
        }

        public void EditarMonitor(int id, string nombre, string documentoIdentidad, string contrasena)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"UPDATE MONITOR 
                         SET nombre = @nombre, 
                             documento_identidad = @documento_identidad, 
                             contrasena = @contrasena 
                         WHERE idMonitor = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@documento_identidad", documentoIdentidad);
                    cmd.Parameters.AddWithValue("@contrasena", contrasena);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public bool EditarMonitor(int idMonitor, string nombre, string documentoIdentidad, string contrasena = null, int? idAerolinea = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var query = "UPDATE MONITOR SET nombre = @nombre, documento_identidad = @documento_identidad";

                if (contrasena != null)
                    query += ", contrasena = @contrasena";

                if (idAerolinea != null)
                    query += ", idAerolinea = @idAerolinea";

                query += " WHERE idMonitor = @idMonitor";

                Console.WriteLine("🛠️ Ejecutando actualización de monitor:");
                Console.WriteLine($"📌 Query: {query}");

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nombre", nombre);
                    command.Parameters.AddWithValue("@documento_identidad", documentoIdentidad);
                    command.Parameters.AddWithValue("@idMonitor", idMonitor);

                    if (contrasena != null)
                        command.Parameters.AddWithValue("@contrasena", contrasena);

                    if (idAerolinea != null)
                        command.Parameters.AddWithValue("@idAerolinea", idAerolinea);

                    try
                    {
                        int rows = command.ExecuteNonQuery();
                        Console.WriteLine("✅ Filas afectadas: " + rows);
                        return rows > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ ERROR al ejecutar actualización de monitor:");
                        Console.WriteLine(ex.ToString());
                        return false;
                    }
                }
            }
        }



        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM MONITOR WHERE correo = @correo";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    await conn.OpenAsync();
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task<bool> ExisteDocumentoAsync(string documento)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM MONITOR WHERE documento_identidad = @documento";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@documento", documento);
                    await conn.OpenAsync();

                    int count = (int)await cmd.ExecuteScalarAsync();

                    return count > 0;
                }
            }
        }

        public async Task<bool> ActualizarCorreoMonitorAsync(int idMonitor, string nuevoCorreo)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "UPDATE MONITOR SET correo = @correo WHERE idMonitor = @idMonitor";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@correo", nuevoCorreo);
                    cmd.Parameters.AddWithValue("@idMonitor", idMonitor);

                    await conn.OpenAsync();
                    int affectedRows = await cmd.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public string ObtenerContrasenaPorCorreo(string correo)
        {
            string password = null;

            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT contrasena FROM MONITOR WHERE correo = @correo";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        password = result.ToString();
                    }
                }
            }

            return password;
        }

    }
}
