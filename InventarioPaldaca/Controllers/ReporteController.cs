using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Controllers
{
    public class ReporteController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public ReporteController(InventarioPaldacaContext context)
        {
            _context = context;
        }

        public IActionResult Reporte()
        {
            return View(new ReporteViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Generar(ReporteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

                var nuevoReporte = new Reporte
                {
                    UsuarioId = usuarioId,
                    FechaGeneracion = DateTime.Now,
                    Descripcion = model.Descripcion,
                    RutaArchivo = model.RutaArchivo
                };

                _context.Reportes.Add(nuevoReporte);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Reporte generado exitosamente.";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> Index()
        {
            var reportes = await _context.Reportes
                .Include(r => r.Usuario) // Asegura que se carga el nombre del usuario
                .ToListAsync();
            return View(reportes);
        }
    }
}

