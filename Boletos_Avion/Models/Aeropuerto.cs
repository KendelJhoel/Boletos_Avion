namespace Boletos_Avion.Models
{
    public class Aeropuerto
    {
        public int IdAeropuerto { get; set; }
        public string Nombre { get; set; }
        public string CodigoIATA { get; set; } 
        public int IdCiudad { get; set; }
        public int IdPais { get; set; } 

        public string NombreCiudad { get; set; } 


        public List<Vuelo> VuelosOrigen { get; set; }
        public List<Vuelo> VuelosDestino { get; set; }
    }
}

