namespace Boletos_Avion.Models
{
    public class reservaViewModel
    {
        public UserModel Cliente { get; set; }
        public List<Vuelo> Vuelos { get; set; }
    }
}
