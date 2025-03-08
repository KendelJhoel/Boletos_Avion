using System;

namespace Boletos_Avion.Models
{
    public class Vuelo
    {
        public int IdVuelo { get; set; }
        public string CodigoVuelo { get; set; }
        public string Categoria { get; set; } // Nueva propiedad

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


        public string PaisOrigen { get; set; }
        public string PaisDestino { get; set; }

        public string CodigoPaisOrigen { get; set; }
        public string CodigoPaisDestino { get; set; }

        // Cantidad de asientos totales por categoría
        public int AsientosBusiness { get; set; }
        public int AsientosPrimeraClase { get; set; }
        public int AsientosTurista { get; set; }

        // Cantidad de asientos disponibles por categoría
        public int AsientosBusinessDisponibles { get; set; }
        public int AsientosPrimeraClaseDisponibles { get; set; }
        public int AsientosTuristaDisponibles { get; set; }

        // Propiedades calculadas extraidos del antiguo VueloViewModel.cs
        public string Origen => $"{CiudadOrigen} - {NombreAeropuertoOrigen}";
        public string Destino => $"{CiudadDestino} - {NombreAeropuertoDestino}";
        public string Duracion => (FechaLlegada - FechaSalida).ToString(@"hh\:mm");
        public decimal Precio => PrecioBase * 1.13m; // Aplicando impuesto

    }
}
