namespace Boletos_Avion.Models
{
    public class TransaccionViewModel
    {
        public string NumeroReserva { get; set; }
        public string NombreCliente { get; set; }
        public string DocumentoIdentidad { get; set; }
        public decimal TotalPagado { get; set; }
    }
}
