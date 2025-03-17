namespace Boletos_Avion.Services;
using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;

public class AgentService : DbController
{
    public AgentService() : base() { }

    public bool RegisterUser(UserModel user)
    {
        string query = @"
        INSERT INTO USUARIOS (nombre, correo, telefono, direccion, documento_identidad, contrasena, idRol, fecha_registro) 
        VALUES (@Nombre, @Correo, @Telefono, @Direccion, @Documento, @Contrasena, @IdRol, GETDATE())";

        try
        {
            using (SqlConnection connection = GetConnection()) // ✅ Usa GetConnection()
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", user.Nombre);
                    command.Parameters.AddWithValue("@Correo", user.Correo);
                    command.Parameters.AddWithValue("@Telefono", user.Telefono ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Direccion", user.Direccion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Documento", user.DocumentoIdentidad);
                    command.Parameters.AddWithValue("@Contrasena", user.Contrasena);
                    command.Parameters.AddWithValue("@IdRol", user.IdRol);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en RegisterUser: {ex.Message}");
            return false;
        }
    }
    public bool CheckUserExists(string email)
    {
        bool exists = false;
        string query = "SELECT COUNT(*) FROM USUARIOS WHERE correo = @Correo";

        try
        {
            using (SqlConnection connection = GetConnection()) // ✅ Usa GetConnection()
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Correo", email);
                    exists = (int)command.ExecuteScalar() > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en CheckUserExists: {ex.Message}");
        }

        return exists;
    }
    public bool CheckPhoneExists(string phone)
    {
        bool exists = false;
        string query = "SELECT COUNT(*) FROM USUARIOS WHERE telefono = @Telefono";

        try
        {
            using (SqlConnection connection = GetConnection()) // ✅ Usa GetConnection()
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Telefono", phone);
                    exists = (int)command.ExecuteScalar() > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en CheckPhoneExists: {ex.Message}");
        }

        return exists;
    }

    public bool CheckDocumentExists(string document)
    {
        bool exists = false;
        string query = "SELECT COUNT(*) FROM USUARIOS WHERE documento_identidad = @Documento";

        try
        {
            using (SqlConnection connection = GetConnection()) // ✅ Usa GetConnection()
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Documento", document);
                    exists = (int)command.ExecuteScalar() > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en CheckDocumentExists: {ex.Message}");
        }

        return exists;
    }
    public List<UserModel> GetAllClients()
    {
        List<UserModel> clients = new List<UserModel>();
        string query = "SELECT idUsuario, nombre, correo, telefono, direccion, documento_identidad FROM USUARIOS WHERE idRol = 3";

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    clients.Add(new UserModel
                    {
                        IdUsuario = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Correo = reader.GetString(2),
                        Telefono = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Direccion = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        DocumentoIdentidad = reader.GetString(5)
                    });
                }
            }
        }
        return clients;
    }

    public UserModel GetUserById(int idUsuario)
    {
        UserModel user = null;

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            string query = @"
                SELECT 
                    idUsuario,
                    nombre,
                    correo,
                    telefono,
                    direccion,
                    documento_identidad,
                    contrasena,
                    idRol
                FROM USUARIOS
                WHERE idUsuario = @Id
            ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", idUsuario);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel
                        {
                            IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                            Nombre = reader["nombre"].ToString(),
                            Correo = reader["correo"].ToString(),
                            Telefono = reader["telefono"].ToString(),
                            Direccion = reader["direccion"].ToString(),
                            DocumentoIdentidad = reader["documento_identidad"].ToString(),
                            Contrasena = reader["contrasena"].ToString(),
                            IdRol = Convert.ToInt32(reader["idRol"])
                        };
                    }
                }
            }
        }
        return user;
    }

    // Actualizar la información de un usuario (incluyendo la contraseña)
    public bool UpdateClient(UserModel client)
    {
        string query = @"UPDATE USUARIOS 
                     SET nombre = @Nombre, telefono = @Telefono, direccion = @Direccion, documento_identidad = @Documento 
                     WHERE idUsuario = @IdUsuario";

        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdUsuario", client.IdUsuario);
                command.Parameters.AddWithValue("@Nombre", client.Nombre);
                command.Parameters.AddWithValue("@Telefono", client.Telefono ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Direccion", client.Direccion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Documento", client.DocumentoIdentidad);

                int result = command.ExecuteNonQuery();
                return result > 0;
            }
        }
    }

}

