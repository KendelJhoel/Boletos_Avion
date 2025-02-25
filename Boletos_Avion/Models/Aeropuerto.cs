namespace Boletos_Avion.Models
{
    public class Aeropuerto
    {
        public int IdAeropuerto { get; set; }
        public string Nombre { get; set; }
        public string CodigoIATA { get; set; } // Código del aeropuerto (Ej: "LAX", "JFK")
        public string Ciudad { get; set; }
        public int IdPais { get; set; } // Relación con la tabla de países

        // Relación con vuelos (un aeropuerto puede tener muchos vuelos de origen o destino)
        public List<Vuelo> VuelosOrigen { get; set; }
        public List<Vuelo> VuelosDestino { get; set; }
    }
}

