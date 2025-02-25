namespace Boletos_Avion.Models
{
    public class Aerolinea
    {
        public int IdAerolinea { get; set; }
        public string Nombre { get; set; }
        public string CodigoICAO { get; set; } // Código de la aerolínea (Ej: "AA" para American Airlines)
        public string Pais { get; set; } // País de origen de la aerolínea

        // Relación con vuelos (una aerolínea puede tener muchos vuelos)
        public List<Vuelo> Vuelos { get; set; }
    }
}
