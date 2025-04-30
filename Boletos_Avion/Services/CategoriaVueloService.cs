using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace Boletos_Avion.Services
{
    public class CategoriaVueloService : DbController
    {
        public CategoriaVueloService() : base() { }

        public List<CategoriaVuelo> ObtenerTodas()
        {
            List<CategoriaVuelo> lista = new List<CategoriaVuelo>();

            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string query = "SELECT idCategoriaVuelo, nombre FROM CATEGORIAS_VUELOS";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new CategoriaVuelo
                        {
                            IdCategoriaVuelo = (int)reader["idCategoriaVuelo"],
                            Nombre = reader["nombre"].ToString()
                        });
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al obtener categorías de vuelo: " + ex.Message);
            }

            return lista;
        }
    }
}
