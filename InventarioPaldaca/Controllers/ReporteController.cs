using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace InventarioPaldaca.Controllers
{
    public class ReporteController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public ReporteController(InventarioPaldacaContext context)
        {
            _context = context;
        }

        // Acción para mostrar el formulario de generación
        public IActionResult Reporte()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                return RedirectToAction("Login", "Cuenta");

            var activos = _context.Activos
                .Where(a => a.UsuarioId == usuarioId)
                .Select(a => new SelectListItem
                {
                    Value = a.ActivoId.ToString(),
                    Text = $"{a.CodigoInventario} - {a.Marca} {a.Modelo}"
                }).ToList();

            var viewModel = new ReporteViewModel
            {
                ActivosAsociados = activos
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Generar(ReporteViewModel model)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                return RedirectToAction("Login", "Cuenta");

            if (ModelState.IsValid)
            {
                var nuevoReporte = new Reporte
                {
                    UsuarioId = usuarioId,
                    FechaGeneracion = DateTime.Now,
                    Descripcion = model.Descripcion,
                    RutaArchivo = model.RutaArchivo,
                    ActivoId = model.ActivoIdSeleccionado
                };

                _context.Reportes.Add(nuevoReporte);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Reporte generado exitosamente.";
                return RedirectToAction("Index", "Home");
            }

            // Recargar lista de activos si hay errores
            model.ActivosAsociados = _context.Activos
                .Where(a => a.UsuarioId == usuarioId)
                .Select(a => new SelectListItem
                {
                    Value = a.ActivoId.ToString(),
                    Text = $"{a.CodigoInventario} - {a.Marca} {a.Modelo}"
                }).ToList();

            return View("Reporte", model);
        }

        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> Index()
        {
            var reportes = await _context.Reportes
                .Include(r => r.Usuario)
                .Include(r => r.Activo)
                .ToListAsync();

            return View(reportes);
        }
    }
}
