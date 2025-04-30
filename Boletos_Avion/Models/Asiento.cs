namespace GestionBoletos.Models
{
    public class Asiento
    {
        public int IdVueloAsiento { get; set; }
        public int IdVuelo { get; set; }
        public string Numero { get; set; }  
        public int IdCategoria { get; set; }  
        public decimal Precio { get; set; } 
        public string Estado { get; set; }  
        public string NombreCategoria { get; set; }  



    }
}
