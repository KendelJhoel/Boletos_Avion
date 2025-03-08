namespace Boletos_Avion.Models
{
    public class Aerolinea
    {
        public int IdAerolinea { get; set; }
        public string Nombre { get; set; }

        // Relación con vuelos (una aerolínea puede tener muchos vuelos)
        public List<Vuelo> Vuelos { get; set; }
    }
}
