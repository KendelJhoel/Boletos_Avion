namespace Boletos_Avion.Models
{
    public class Aerolinea
    {
        public int IdAerolinea { get; set; }
        public string Nombre { get; set; }
        public int CantidadVuelos { get; set; } 


        public List<Vuelo> Vuelos { get; set; }
    }
}
