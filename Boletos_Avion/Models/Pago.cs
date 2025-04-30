using Boletos_Avion.Models;

public class Pago
{
    public int IdVuelo { get; set; } 
    public List<AsientoSeleccionado> Asientos { get; set; } = new List<AsientoSeleccionado>();
    public string NumeroReserva { get; set; }
}

public class AsientoSeleccionado
{
    public int Id { get; set; }
    public string Numero { get; set; }
    public decimal Precio { get; set; }
    public int IdVueloAsiento { get; set; }
}

public class PagoViewModel
{
    public Pago Pago { get; set; }
    public Vuelo Vuelo { get; set; }
}

