using InventarioPaldaca.Models;
using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace InventarioPaldaca.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly InventarioPaldacaContext _context;

        // Constructor que acepta ambos servicios
        public HomeController(ILogger<HomeController> logger, InventarioPaldacaContext context)
        {
            _logger = logger;
            _context = context;
        }
       
        public IActionResult Index()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            var rol = HttpContext.Session.GetString("UsuarioRol");

            Console.WriteLine($"UsuarioId: {usuarioId}, Rol: {rol}");

            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(rol))
            {
                Console.WriteLine("Sesión no encontrada. Redirigiendo al login.");
                return RedirectToAction("Login", "Acceso");
            }

            if (rol == "Usuario")
            {
                Console.WriteLine("Redirigiendo a UsuarioHome.");
                return RedirectToAction("UsuarioHome");
            }

            Console.WriteLine("Redirigiendo a AdminHome.");
            return RedirectToAction("AdminHome");
        }

        public async Task<IActionResult> AdminHome()
        {
            var Activos = await _context.Activos.ToListAsync();

            var model = new ListaActivosViewModel
            {
                TotalActivos = Activos.Count,
                ListaActivos = Activos,
                TotalActivosDañados = Activos.Count(a => a.Funcionabilidad == false)
            };

            return View(model);
        }
        public IActionResult UsuarioHome()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
