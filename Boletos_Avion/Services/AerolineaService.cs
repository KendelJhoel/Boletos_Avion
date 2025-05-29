using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Boletos_Avion.Services
{
    public class AerolineaService
    {
        private readonly DbController _db;

        public AerolineaService()
        {
            _db = new DbController();
        }

        public async Task<List<Aerolinea>> ObtenerTodasAsync()
        {
            var lista = new List<Aerolinea>();

            using (SqlConnection conn = _db.GetConnection())
            {
                string query = "SELECT idAerolinea, nombre FROM AEROLINEAS";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Aerolinea
                            {
                                IdAerolinea = (int)reader["idAerolinea"],
                                Nombre = reader["nombre"].ToString()
                            });
                        }
                    }
                    conn.Close();
                }
            }

            return lista;
        }

        public async Task CrearAsync(Aerolinea aerolinea)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                string query = "INSERT INTO AEROLINEAS (nombre) VALUES (@nombre)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", aerolinea.Nombre);
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    conn.Close();
                }
            }
        }

        public async Task ActualizarAsync(Aerolinea aerolinea)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                string query = "UPDATE AEROLINEAS SET nombre = @nombre WHERE idAerolinea = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", aerolinea.Nombre);
                    cmd.Parameters.AddWithValue("@id", aerolinea.IdAerolinea);
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    conn.Close();
                }
            }
        }

        public async Task<List<Aerolinea>> ObtenerConCantidadVuelosAsync()
        {
            var lista = new List<Aerolinea>();

            using (SqlConnection conn = _db.GetConnection())
            {
                string query = @"
            SELECT 
                a.idAerolinea,
                a.nombre,
                COUNT(v.idVuelo) AS CantidadVuelos
            FROM AEROLINEAS a
            LEFT JOIN VUELOS v ON a.idAerolinea = v.idAerolinea
            GROUP BY a.idAerolinea, a.nombre
            ORDER BY a.nombre";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Aerolinea
                            {
                                IdAerolinea = (int)reader["idAerolinea"],
                                Nombre = reader["nombre"].ToString(),
                                CantidadVuelos = (int)reader["CantidadVuelos"]
                            });
                        }
                    }
                    conn.Close();
                }
            }

            return lista;
        }

    }
}
