using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;

namespace Boletos_Avion.Services
{
    public class AeropuertoService : DbController
    {
        public AeropuertoService() : base() { }


        public List<Aeropuerto> ObtenerTodos()
        {
            List<Aeropuerto> lista = new List<Aeropuerto>();
            DbController db = new DbController();
            var ciudades = new CiudadService().ObtenerTodas(); 

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = "SELECT idAeropuerto, nombre, idCiudad FROM Aeropuertos";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int idCiudad = Convert.ToInt32(reader["idCiudad"]);

                    var aeropuerto = new Aeropuerto
                    {
                        IdAeropuerto = Convert.ToInt32(reader["idAeropuerto"]),
                        Nombre = reader["nombre"].ToString(),
                        IdCiudad = idCiudad,
                        NombreCiudad = ciudades.FirstOrDefault(c => c.IdCiudad == idCiudad)?.Nombre
                    };

                    lista.Add(aeropuerto);
                }

                reader.Close();
            }

            return lista;
        }


        public bool Agregar(Aeropuerto aeropuerto)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Aeropuertos (nombre, idCiudad) VALUES (@nombre, @idCiudad)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nombre", aeropuerto.Nombre);
                    cmd.Parameters.AddWithValue("@idCiudad", aeropuerto.IdCiudad);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int idAeropuerto)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Aeropuertos WHERE idAeropuerto = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idAeropuerto);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<Aeropuerto> ObtenerPorCiudad(int idCiudad)
        {
            List<Aeropuerto> lista = new List<Aeropuerto>();

            string query = "SELECT idAeropuerto, nombre FROM AEROPUERTOS WHERE idCiudad = @idCiudad";

            using (SqlConnection conn = GetConnection())

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idCiudad", idCiudad);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Aeropuerto
                        {
                            IdAeropuerto = (int)reader["idAeropuerto"],
                            Nombre = reader["nombre"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        public Aeropuerto ObtenerPorId(int idAeropuerto)
        {
            Aeropuerto aeropuerto = null;

            string query = @"SELECT idAeropuerto, nombre, idCiudad 
                     FROM AEROPUERTOS 
                     WHERE idAeropuerto = @id";

            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idAeropuerto);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                aeropuerto = new Aeropuerto
                                {
                                    IdAeropuerto = Convert.ToInt32(reader["idAeropuerto"]),
                                    Nombre = reader["nombre"].ToString(),
                                    IdCiudad = Convert.ToInt32(reader["idCiudad"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al obtener aeropuerto por ID: " + ex.Message);
            }

            return aeropuerto;
        }

        public bool EditarAeropuerto(int idAeropuerto, string nuevoNombre)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE AEROPUERTOS SET nombre = @nombre WHERE idAeropuerto = @id";

                    Console.WriteLine("🛠️ Ejecutando UPDATE de aeropuerto:");
                    Console.WriteLine(query);

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nuevoNombre);
                        cmd.Parameters.AddWithValue("@id", idAeropuerto);

                        int filas = cmd.ExecuteNonQuery();
                        Console.WriteLine("✅ Filas afectadas: " + filas);
                        return filas > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error al actualizar aeropuerto:");
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

    }
}
