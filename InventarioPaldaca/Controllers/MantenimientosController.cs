using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioPaldaca.Models;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Models.Inventario;

namespace InventarioPaldaca.Controllers
{
    public class MantenimientosController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public MantenimientosController(InventarioPaldacaContext context)
        {
            _context = context;
        }
        public IActionResult HistorialMantenimientos(string? codigo)
        {
            var viewModel = new MantenimientoViewModel
            {
                UltimosMantenimientos = _context.Mantenimientos
                    .Include(m => m.Activo)
                    .OrderByDescending(m => m.FechaInicio)
                    .ThenByDescending(m => m.FechaFin)
                    .Take(15)
                    .ToList()
            };

            if (!string.IsNullOrEmpty(codigo))
            {
                viewModel.HistorialDelActivo = _context.Mantenimientos
                    .Include(m => m.Activo)
                    .Where(m => m.Activo.CodigoInventario == codigo)
                    .OrderByDescending(m => m.FechaInicio)
                    .ThenByDescending(m => m.FechaFin)
                    .ToList();

                viewModel.CodigoInventarioBuscado = codigo;
                viewModel.MostrandoHistorial = true;
            }
            return View(viewModel);
        }
        public async Task<IActionResult> ActivosEnMantenimiento()
        {
            var activosDañados = await _context.Activos
                .Include(a => a.Mantenimientos)
                .Include(a => a.Categoria)
                .Include(a => a.Ubicacion)
                .Include(a => a.Usuario)
                .Where(a => a.Funcionabilidad == false)
                .ToListAsync();

            var model = new ActivosMantenimientoViewModel
            {
                Activos = activosDañados,
                TotalActivosEnMantenimiento = activosDañados.Count
            };

            return View(model);
        }

        public IActionResult EditarMantenimiento(int MantenimientoId, string Tecnico, string NumeroTecnico, decimal Costo, string Descripcion)
        {
            var mantenimiento = _context.Mantenimientos.FirstOrDefault(m => m.MantenimientoId == MantenimientoId);
            if (mantenimiento == null)
            {
                TempData["ErrorMessage"] = "Mantenimiento no encontrado.";
                return RedirectToAction("ActivosDañados");
            }

            mantenimiento.Tecnico = Tecnico;
            mantenimiento.NumeroTecnico = NumeroTecnico;
            mantenimiento.Costo = Costo;
            mantenimiento.Descripcion = Descripcion;
            

          

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Mantenimiento actualizado correctamente.";
            return RedirectToAction("ActivosEnMantenimiento");
        }
        [HttpPost]
        public async Task<IActionResult> FinalizarMantenimiento(int mantenimientoId)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(mantenimientoId);
            if (mantenimiento == null)
            {
                return NotFound();
            }

            // Actualizar estado
            mantenimiento.Estado = "Finalizado";
            mantenimiento.FechaFin = DateOnly.FromDateTime(DateTime.Now);


            // Si deseas, también puedes marcar el activo como funcional nuevamente:
            var activo = await _context.Activos.FindAsync(mantenimiento.ActivoId);
            if (activo != null)
            {
                activo.Funcionabilidad = true; // marcar como reparado
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ActivosEnMantenimiento"); // o la vista actual que estés usando
        }

    }
}
