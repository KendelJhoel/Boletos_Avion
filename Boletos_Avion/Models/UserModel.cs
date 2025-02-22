namespace Boletos_Avion.Models
{
    public class UserModel
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }  // Nombre completo (Nombre + Apellido)
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string DocumentoIdentidad { get; set; }
        public string Contrasena { get; set; }
        public int IdRol { get; set; }
    }
}
