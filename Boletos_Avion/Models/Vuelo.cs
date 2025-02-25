namespace Boletos_Avion.Models
{
    public class Vuelo
    {
        public int IdVuelo { get; set; }
        public string CodigoVuelo { get; set; }
        public string IdAeropuertoOrigen { get; set; }
        public string IdAeropuertoDestino { get; set; }
        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }
        public int IdAerolinea { get; set; }
        public decimal PrecioBase { get; set; }
        public int Capacidad { get; set; }
        public string Estado { get; set; }
        public int CantidadAsientos { get; set; }
        public int AsientosDisponibles { get; set; }

        public string NombreAeropuertoOrigen { get; set; }
        public string CiudadOrigen { get; set; }
        public string NombreAeropuertoDestino { get; set; }
        public string CiudadDestino { get; set; }

    }
}
