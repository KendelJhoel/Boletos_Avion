using Boletos_Avion.Models;
using Microsoft.Data.SqlClient;

public class DbController
{
    private readonly string connectionString;
    private SqlConnection connection;

    public DbController()
    {
        List<string> connectionOptions = new List<string>
            {
                "Data Source=DESKTOP-IT9FVD5\\SQLEXPRESS;Initial Catalog=GestionBoletos;User ID=sa;Password=15012004;TrustServerCertificate=True;",
                "Data Source=DESKTOP-34DG23J\\SQLEXPRESS;Initial Catalog=GestionBoletos;User ID=sa;Password=Chiesafordel1+;TrustServerCertificate=True;",
               
                "Data Source=DESKTOP-MP89LU5;Initial Catalog=GestionBoletos;User ID=jona;Password=4321;TrustServerCertificate=True;",
                
            };

        connectionString = GetAvailableConnection(connectionOptions);
        connection = new SqlConnection(connectionString); // 🔹 Inicializa la conexión UNA SOLA VEZ
    }

    private string GetAvailableConnection(List<string> connectionOptions)
    {
        foreach (var conn in connectionOptions)
        {
            try
            {
                using (var testConnection = new SqlConnection(conn))
                {
                    testConnection.Open();
                    Console.WriteLine($"✅ Conectado exitosamente usando: {conn}");
                    return conn; // Retorna la primera conexión exitosa
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fallo en la conexión: {ex.Message}");
            }
        }
        throw new Exception("❌ No se pudo conectar con ninguna base de datos.");
    }

    public SqlConnection GetConnection()
    {
        if (connection == null || connection.State == System.Data.ConnectionState.Closed)
        {
            connection = new SqlConnection(connectionString); // 🔹 Si está cerrada, la reabre.
        }
        return connection;
    }

    public string TestConnection()
    {
        try
        {
            using (SqlConnection testConnection = GetConnection())
            {
                testConnection.Open();
                return "✅ Conectado a la base de datos: " + testConnection.Database;
            }
        }
        catch (Exception ex)
        {
            return "❌ No se ha podido conectar a la base de datos: " + ex.Message;
        }
    }

    private static readonly Dictionary<string, string> CodigoPaises = new Dictionary<string, string>{
        // América
        { "Argentina", "AR" },
        { "Belice", "BZ" },
        { "Brasil", "BR" },
        { "Canadá", "CA" },
        { "Chile", "CL" },
        { "Colombia", "CO" },
        { "Costa Rica", "CR" },
        { "El Salvador", "SV" },
        { "Estados Unidos", "US" },
        { "Guatemala", "GT" },
        { "Honduras", "HN" },
        { "México", "MX" },
        { "Panamá", "PA" },
        { "Perú", "PE" },
        { "Puerto Rico", "PR" },
        { "República Dominicana", "DO" },

        // Asia
        { "Japón", "JP" },

        // Europa
        { "Alemania", "DE" },
        { "España", "ES" },
        { "Francia", "FR" },
        { "Portugal", "PT" },
        { "Reino Unido", "GB" },
        { "Suecia", "SE" },

        // Oceanía
        { "Australia", "AU" },
        { "Nueva Zelanda", "NZ" }
    };

  


  
}