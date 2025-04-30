using System;

namespace Boletos_Avion.Models
{
    public class Vuelo
    {
        public int IdVuelo { get; set; }
        public string CodigoVuelo { get; set; }
        public string Categoria { get; set; } 
        public int IdCategoriaVuelo { get; set; }


        public int IdAeropuertoOrigen { get; set; }
        public int IdAeropuertoDestino { get; set; }
        public Aeropuerto AeropuertoOrigen { get; set; }
        public Aeropuerto AeropuertoDestino { get; set; }

        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }

        public int IdAerolinea { get; set; }
        public Aerolinea Aerolinea { get; set; }

        public decimal PrecioBase { get; set; }
        public int Capacidad { get; set; }
        public int CantidadAsientos { get; set; }
        public int AsientosDisponibles { get; set; }

        public string Estado { get; set; }

        public string NombreAeropuertoOrigen { get; set; }
        public string CiudadOrigen { get; set; }
        public string NombreAeropuertoDestino { get; set; }
        public string CiudadDestino { get; set; }

        public string NombreAerolinea { get; set; }


        public string PaisOrigen { get; set; }
        public string PaisDestino { get; set; }

        public string CodigoPaisOrigen { get; set; }
        public string CodigoPaisDestino { get; set; }

        public int AsientosBusiness { get; set; }
        public int AsientosPrimeraClase { get; set; }
        public int AsientosTurista { get; set; }

        public int AsientosBusinessDisponibles { get; set; }
        public int AsientosPrimeraClaseDisponibles { get; set; }
        public int AsientosTuristaDisponibles { get; set; }

        public string Origen => $"{CiudadOrigen} - {NombreAeropuertoOrigen}";
        public string Destino => $"{CiudadDestino} - {NombreAeropuertoDestino}";
        public string Duracion => (FechaLlegada - FechaSalida).ToString(@"hh\:mm");
        public decimal Precio => PrecioBase * 1.13m; 

        public int IdCiudadOrigen { get; set; }
        public int IdCiudadDestino { get; set; }
    }
}
