using GestionBoletos.Models;
using System.ComponentModel.DataAnnotations;

namespace Boletos_Avion.Models
{
    public class Reservas
    {
        [Key]
        public int IdReserva { get; set; }
        public string NumeroReserva { get; set; }
        public int IdUsuario { get; set; }
        public int IdVuelo { get; set; }
        public DateTime FechaReserva { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }

        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }

        public string NombreCliente { get; set; }
        public string CorreoCliente { get; set; }

    }

    public class DetalleReservaViewModel
    {
        public Reservas Reserva { get; set; }
        public Vuelo Vuelo { get; set; }
        public List<Asiento> Asientos { get; set; }
    }
}
