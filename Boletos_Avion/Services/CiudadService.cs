using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;

namespace Boletos_Avion.Services
{
    public class CiudadService
    {
        private readonly SqlConnection _connection;

        public CiudadService()
        {
            DbController db = new DbController();
            _connection = db.GetConnection();
        }

        public List<Ciudad> ObtenerTodas()
        {
            List<Ciudad> lista = new List<Ciudad>();

            try
            {
                using (SqlConnection conn = _connection)
                {
                    conn.Open();
                    string query = "SELECT idCiudad, nombre FROM Ciudades";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Ciudad
                        {
                            IdCiudad = Convert.ToInt32(reader["idCiudad"]),
                            Nombre = reader["nombre"].ToString()
                        });
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al obtener ciudades: " + ex.Message);
            }

            return lista;
        }
    }
}
