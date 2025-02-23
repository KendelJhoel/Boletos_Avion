namespace Boletos_Avion.Models
{
    public class VueloViewModel
    {
        public int IdVuelo { get; set; }
        public string CodigoVuelo { get; set; }
        public string AeropuertoOrigen { get; set; }
        public string CiudadOrigen { get; set; }
        public string AeropuertoDestino { get; set; }
        public string CiudadDestino { get; set; }
        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }
        public decimal PrecioBase { get; set; }
    }
}
