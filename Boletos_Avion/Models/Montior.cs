namespace Boletos_Avion.Models
{
    public class Monitor
    {
        public int IdMonitor { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string DocumentoIdentidad { get; set; }
        public string Contrasena { get; set; }
        public int IdAerolinea { get; set; }
        public string NombreAerolinea { get; set; }
    }

    public class ChangeEmailMoViewModel
    {
        public string Nombre { get; set; }
        public string Contrasena { get; set; }
        public string EmailActual { get; set; }
    }
}
