namespace Boletos_Avion.Models
{
    public class EditarCliente
    {
        public UserModel Client { get; set; }
        public List<Reservas> Reservas { get; set; }
    }
}
