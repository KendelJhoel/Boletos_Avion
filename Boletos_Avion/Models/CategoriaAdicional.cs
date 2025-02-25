namespace Boletos_Avion.Models
{
    public class CategoriaAdicional
    {
        public int Id { get; set; }
        public string Nombre { get; set; } // Nombre de la categoría (Ej: "Vuelos Económicos", "Asia")
        public string Filtro { get; set; } // Criterio de filtrado (Ej: "Precio", "Continente")
    }
}
