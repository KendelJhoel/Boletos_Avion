namespace GestionBoletos.Models
{
    public class Asiento
    {
        public int IdVueloAsiento { get; set; }
        public int IdVuelo { get; set; }
        public string Numero { get; set; }  // Ejemplo: "3A"
        public int IdCategoria { get; set; }  // Cambiado de string a int
        public decimal Precio { get; set; } // Precio del asiento
        public string Estado { get; set; }  // "Disponible", "Reservado", "Ocupado"
        public string NombreCategoria { get; set; }  // "Disponible", "Reservado", "Ocupado"



    }
}
