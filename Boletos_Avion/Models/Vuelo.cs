using System;

namespace Boletos_Avion.Models
{
    public class Vuelo
    {
        public int IdVuelo { get; set; }
        public string CodigoVuelo { get; set; }

        // Relaciones con Aeropuertos
        public int IdAeropuertoOrigen { get; set; }
        public int IdAeropuertoDestino { get; set; }
        public Aeropuerto AeropuertoOrigen { get; set; }
        public Aeropuerto AeropuertoDestino { get; set; }

        // Fechas del vuelo
        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }

        // Relación con Aerolínea
        public int IdAerolinea { get; set; }
        public Aerolinea Aerolinea { get; set; }

        // Información de precios y disponibilidad
        public decimal PrecioBase { get; set; }
        public int Capacidad { get; set; }
        public int CantidadAsientos { get; set; }
        public int AsientosDisponibles { get; set; }

        // Estado del vuelo (Disponible, Lleno, Cancelado)
        public string Estado { get; set; }

        // Datos extra (opcional, usados para mostrar en la vista)
        public string NombreAeropuertoOrigen { get; set; }
        public string CiudadOrigen { get; set; }
        public string NombreAeropuertoDestino { get; set; }
        public string CiudadDestino { get; set; }

        // 🔹 Propiedades adicionales para mostrar nombres en las vistas
        public string NombreAerolinea { get; set; }

    }
}
