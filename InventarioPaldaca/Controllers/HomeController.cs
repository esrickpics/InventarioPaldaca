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

    
            IActionResult redireccion = rol switch
            {
                1 => RedirectToAction("UsuarioHome"),
                2 => RedirectToAction("Home"),
                3 => RedirectToAction("Home"), // rol administradorproyecto(3)
                _ => RedirectToAction("Login", "Acceso")
            };
            return redireccion;
        }

        public async Task<IActionResult> Home()
        {
            var rolString = HttpContext.Session.GetString("UsuarioRol");
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (!int.TryParse(usuarioIdStr, out int usuarioId))
            {
                return RedirectToAction("Login", "Acceso");
            }

            List<Activo> activosFiltrados;
            int totalReportes;

            if (rolString == "3" || rolString == "AdministradorProyecto")
            {
                // ?? Filtrar activos y reportes solo del proyecto del usuario
                var proyectosDelUsuario = _context.ProyectoAdministradors
                    .Where(pa => pa.UsuarioId == usuarioId)
                    .Select(pa => pa.ProyectoId)
                    .ToList();

                activosFiltrados = await _context.Activos
                    .Where(a => a.ProyectoId != null && proyectosDelUsuario.Contains(a.ProyectoId.Value))
                    .ToListAsync();

                totalReportes = await _context.Reportes
                    .Where(r => r.Activo != null && r.Activo.ProyectoId != null &&
                                proyectosDelUsuario.Contains(r.Activo.ProyectoId.Value))
                    .CountAsync();
            }
            else
            {
                // ?? Administrador normal ve todo
                activosFiltrados = await _context.Activos.ToListAsync();
                totalReportes = await _context.Reportes.CountAsync();
            }

            var model = new ListaActivosViewModel
            {
                ListaActivos = activosFiltrados,
                TotalActivos = activosFiltrados.Count,
                TotalActivosDañados = activosFiltrados.Count(a => a.Funcionabilidad == false),
                TotalReportes = totalReportes
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
