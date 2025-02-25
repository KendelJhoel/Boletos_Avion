using Boletos_Avion.Models;
using Microsoft.AspNetCore.Mvc;

namespace Boletos_Avion.Controllers
{
    public class CategoriasController:Controller
    {
        // Acción que muestra los vuelos con destino a Asia
        public IActionResult Asia()
        {
            DbController dbController = new DbController();
            List<Vuelo> vuelosAsia = dbController.GetVuelosByContinente("Asia") ?? new List<Vuelo>();
            return View(vuelosAsia);
        }



        public IActionResult America(int page = 1, int pageSize = 10)
        {
            DbController dbController = new DbController();
            List<Vuelo> vuelosAmerica = dbController.GetVuelosByContinente("America") ?? new List<Vuelo>();

            // Calcula la cantidad total de páginas
            int totalItems = vuelosAmerica.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Obtén los elementos para la página actual
            var vuelosPaginados = vuelosAmerica
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Puedes pasar la información de paginación a la vista a través de ViewData o un ViewModel específico
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(vuelosPaginados);
        }



        // Acción para vuelos hacia Europa
        public IActionResult Europa()
        {
            DbController dbController = new DbController();
            List<Vuelo> vuelosEuropa = dbController.GetVuelosByContinente("Europa") ?? new List<Vuelo>();
            return View(vuelosEuropa);
        }



        // Acción para vuelos hacia Oceania
        public IActionResult Oceania()
        {
            DbController dbController = new DbController();
            // Se pasa "Oceanía" con tilde para que coincida con el valor en la base de datos.
            List<Vuelo> vuelosOceania = dbController.GetVuelosByContinente("Oceanía") ?? new List<Vuelo>();
            return View(vuelosOceania);
        }



        public IActionResult VuelosEconomicos()
        {
            DbController dbController = new DbController();
            // Filtra vuelos cuyo precio base esté entre 250 y 500
            List<Vuelo> vuelosEconomicos = dbController.GetVuelosByPriceRange(200m, 450m) ?? new List<Vuelo>();
            return View(vuelosEconomicos);
        }


        public IActionResult VuelosCortos()
        {
            DbController dbController = new DbController();
            // Filtra vuelos cuya duración sea 300 minutos (5 horas) o menos
            List<Vuelo> vuelosCortos = dbController.GetVuelosByDuration(300) ?? new List<Vuelo>();
            return View(vuelosCortos);
        }





    }
}
