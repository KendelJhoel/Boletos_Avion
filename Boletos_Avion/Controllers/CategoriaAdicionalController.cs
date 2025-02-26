using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Boletos_Avion.Models;

namespace Boletos_Avion.Controllers
{
    public class CategoriaAdicionalController : Controller
    {
        private readonly DbController _dbController;

        public CategoriaAdicionalController()
        {
            _dbController = new DbController();
        }

        public IActionResult Index(string filtro)
        {
            if (string.IsNullOrEmpty(filtro))
            {
                return RedirectToAction("Index", "Home");
            }

            List<Vuelo> vuelosFiltrados = new List<Vuelo>();

            // Aplicar filtros dinámicamente usando DbController
            switch (filtro.ToLower())
            {
                case "america":
                    vuelosFiltrados = _dbController.GetVuelosByContinente("America") ?? new List<Vuelo>();
                    break;
                case "asia":
                    vuelosFiltrados = _dbController.GetVuelosByContinente("Asia") ?? new List<Vuelo>();
                    break;
                case "europa":
                    vuelosFiltrados = _dbController.GetVuelosByContinente("Europa") ?? new List<Vuelo>();
                    break;
                case "oceania":
                    vuelosFiltrados = _dbController.GetVuelosByContinente("Oceanía") ?? new List<Vuelo>();
                    break;
                case "vuelos cortos":
                    vuelosFiltrados = _dbController.GetVuelosByDuration(300) ?? new List<Vuelo>(); // 300 minutos = 5 horas
                    break;
                case "vuelos economicos":
                    vuelosFiltrados = _dbController.GetVuelosByPriceRange(200m, 450m) ?? new List<Vuelo>();
                    break;
                default:
                    return RedirectToAction("Index", "Home");
            }

            ViewData["Filtro"] = filtro;
            return View("CategoriaAdicional", vuelosFiltrados);
        }
    }
}
