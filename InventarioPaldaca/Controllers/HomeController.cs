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
            
            string usuarioId = HttpContext.Session.GetString("UsuarioId");
            string rolString = HttpContext.Session.GetString("UsuarioRol");

            Console.WriteLine($"UsuarioId: {usuarioId}, Rol: {rolString}");

            // Verificar si la sesión o el rol son inválidos, o si no se pudo convertir el rol a entero
            if (string.IsNullOrEmpty(usuarioId) ||
                string.IsNullOrEmpty(rolString) ||
                !int.TryParse(rolString, out int rol))
            {
                Console.WriteLine("Sesión no encontrada o rol no válido. Redirigiendo al login.");
                return RedirectToAction("Login", "Acceso");
            }

            // Utilizar un switch expression para determinar la redirección según el rol
            IActionResult redireccion = rol switch
            {
                1 => RedirectToAction("UsuarioHome"),
                2 => RedirectToAction("Home"),
                _ => RedirectToAction("Login", "Acceso")
            };

            // Opcional: imprimir la redirección que se realizará
            Console.WriteLine($"Redirigiendo a {(rol == 1 ? "UsuarioHome" : rol == 2 ? "Home" : "Login")}.");

            return redireccion;
        }

        public async Task<IActionResult> Home()
        {
            var Activos = await _context.Activos.ToListAsync();
            var TotalReportes = await _context.Reportes.CountAsync(); // Contar los reportes en la BD

            var model = new ListaActivosViewModel
            {
                TotalActivos = Activos.Count,
                ListaActivos = Activos,
                TotalActivosDañados = Activos.Count(a => a.Funcionabilidad == false),
                TotalReportes = TotalReportes // Asignar la cantidad de reportes
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
            // Si la excepción está disponible en el contexto, obtenemos detalles específicos
            var statusCode = HttpContext.Response.StatusCode; // Obtiene el código de estado (404, 500, etc.)
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Creamos el modelo de error
            var errorViewModel = new ErrorViewModel
            {
                RequestId = requestId
            };

            if (statusCode == 404)
            {
                errorViewModel.Message = "La página que estás buscando no existe.";
                errorViewModel.ErrorType = "Página no encontrada";
            }
            else
            {
                errorViewModel.Message = "Hubo un problema con la solicitud.";
                errorViewModel.ErrorType = "Error desconocido";
            }

            return View(errorViewModel); // Redirige a la vista de error con los detalles
        }
    }
}
