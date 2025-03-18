namespace Boletos_Avion.Models
{
    public class ConfirmarPagoRequest
    {
        public int idVuelo { get; set; }
        public List<int> asientos { get; set; }
    }
}
