using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;

public class AuthService : DbController
{
    public AuthService() : base() { } 


    public UserModel ValidateUser(string email, string password)
    {
        UserModel user = null;
        string query = "SELECT idUsuario, nombre, correo, contrasena, idRol FROM USUARIOS WHERE correo = @Email AND contrasena = @Password";

        try
        {
            using (SqlConnection connection = GetConnection()) 
            {
                connection.Open(); 

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new UserModel
                            {
                                IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                                Nombre = reader["nombre"].ToString(),
                                Correo = reader["correo"].ToString(),
                                Contrasena = reader["contrasena"].ToString(),
                                IdRol = Convert.ToInt32(reader["idRol"])
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en ValidateUser: {ex.Message}");
        }

        return user;
    }

    public bool RegisterUser(UserModel user)
    {
        string query = @"
        INSERT INTO USUARIOS (nombre, correo, telefono, direccion, documento_identidad, contrasena, idRol, fecha_registro) 
        VALUES (@Nombre, @Correo, @Telefono, @Direccion, @Documento, @Contrasena, @IdRol, GETDATE())";

        try
        {
            using (SqlConnection connection = GetConnection()) 
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
            using (SqlConnection connection = GetConnection()) 
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
            using (SqlConnection connection = GetConnection()) 
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
            using (SqlConnection connection = GetConnection()) 
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

    public string GetUserPasswordById(int userId)
    {
        string password = null;
        string query = "SELECT contrasena FROM USUARIOS WHERE idUsuario = @UserId";

        try
        {
            using (SqlConnection connection = GetConnection()) 
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        password = result.ToString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en GetUserPasswordById: {ex.Message}");
        }

        return password;
    }

    public bool UpdateUserEmail(int idUsuario, string newEmail)
    {
        string query = "UPDATE USUARIOS SET correo = @Correo WHERE idUsuario = @IdUsuario";

        try
        {
            using (SqlConnection connection = GetConnection()) 
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Correo", newEmail);
                    command.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en UpdateUserEmail: {ex.Message}");
            return false;
        }
    }

    public string GetUserPasswordByEmail(string email)
    {
        string password = null;
        string query = "SELECT contrasena FROM USUARIOS WHERE correo = @Correo";

        try
        {
            using (SqlConnection connection = GetConnection()) 
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Correo", email);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        password = result.ToString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en GetUserPasswordByEmail: {ex.Message}");
        }

        return password;
    }

    public string GenerateUniquePassword()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@$!%*?&";
        string password;
        bool exists;

        do
        {
            Random random = new Random();
            password = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            exists = PasswordExistsInUsuarios(password) || PasswordExistsInMonitores(password);

        } while (exists);

        return password;
    }

    private bool PasswordExistsInUsuarios(string password)
    {
        string query = "SELECT COUNT(*) FROM USUARIOS WHERE contrasena = @Password";
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Password", password);
                return (int)command.ExecuteScalar() > 0;
            }
        }
    }

    private bool PasswordExistsInMonitores(string password)
    {
        string query = "SELECT COUNT(*) FROM MONITOR WHERE contrasena = @Password";
        using (SqlConnection connection = GetConnection())
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Password", password);
                return (int)command.ExecuteScalar() > 0;
            }
        }
    }

}
