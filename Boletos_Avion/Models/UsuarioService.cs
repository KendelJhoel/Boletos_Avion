using Microsoft.Data.SqlClient;

namespace Boletos_Avion.Services
{
    public class UsuarioService : DbController
    {
        public UsuarioService() : base() { }

        public async Task<bool> ExisteDocumentoAsync(string documento)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM USUARIOS WHERE documento_identidad = @documento";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@documento", documento);
                    await conn.OpenAsync();

                    int count = (int)await cmd.ExecuteScalarAsync();

                    return count > 0;
                }
            }
        }
        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM USUARIOS WHERE correo = @correo";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    await conn.OpenAsync();
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

    }
}
