namespace Boletos_Avion.Models
{
    public class VueloViewModel
    {
        //public int IdVuelo { get; set; }
        //public string CodigoVuelo { get; set; }
        //public string AeropuertoOrigen { get; set; }
        //public string CiudadOrigen { get; set; }
        //public string AeropuertoDestino { get; set; }
        //public string CiudadDestino { get; set; }
        //public DateTime FechaSalida { get; set; }
        //public DateTime FechaLlegada { get; set; }
        //public decimal PrecioBase { get; set; }

        public int IdVuelo { get; set; }
        public string CodigoVuelo { get; set; }
        public string Aerolinea { get; set; } // Nueva propiedad
        public string AeropuertoOrigen { get; set; }
        public string CiudadOrigen { get; set; }
        public string Origen => $"{CiudadOrigen} - {AeropuertoOrigen}"; // Generado dinámicamente
        public string AeropuertoDestino { get; set; }
        public string CiudadDestino { get; set; }
        public string Destino => $"{CiudadDestino} - {AeropuertoDestino}"; // Generado dinámicamente
        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }
        public string Duracion => (FechaLlegada - FechaSalida).ToString(@"hh\:mm"); // Calcula la duración del vuelo
        public decimal PrecioBase { get; set; }
        public decimal Precio => PrecioBase * 1.13m; // Aplicando impuesto (ejemplo)
        public string Categoria { get; set; } // Nueva propiedad

    }
}
