using InventarioPaldaca.Models;
using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace InventarioPaldaca.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InventaryPaldacaContext _context;

        // Constructor que acepta ambos servicios
        public HomeController(ILogger<HomeController> logger, InventaryPaldacaContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener todos los activos incluyendo sus relaciones
            var Activos = await _context.Activos.ToListAsync();

            // Calcular el total de activos
            int totalActivos = Activos?.Count ?? 0;

            // Crear el ViewModel
            var model = new ListaActivosViewModel
            {
                TotalActivos = totalActivos,
                ListaActivos = Activos ?? new List<Activo>()
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
