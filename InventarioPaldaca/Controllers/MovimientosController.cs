using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Controllers
{
    public class MovimientosController : Controller
    {
    
        private readonly InventarioPaldacaContext _context;

    public MovimientosController(InventarioPaldacaContext context)
    {
        _context = context;
    }

        public async Task<IActionResult> HistorialMovimientos(int id)
        {
            var movimientos = await _context.Movimientos
                .Include(m => m.UsuarioAnterior)
                .Include(m => m.UsuarioNuevo)
                .Include(m => m.UbicacionAnterior)
                .Include(m => m.UbicacionNueva)
                .Where(m => m.ActivoId == id)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();

            var historial = movimientos.Select(m =>
            {
                // Convertir FechaMovimiento a DateOnly para comparar
                var fechaMovimiento = DateOnly.FromDateTime(m.FechaMovimiento);

                return new MovimientoHistorialViewModel
                {
                    FechaMovimiento = m.FechaMovimiento,
                    UsuarioAnterior = m.UsuarioAnterior != null
                        ? $"{m.UsuarioAnterior.UsuarioNombre} {m.UsuarioAnterior.UsuarioApellido}"
                        : "No asignado",
                    UsuarioNuevo = m.UsuarioNuevo != null
                        ? $"{m.UsuarioNuevo.UsuarioNombre} {m.UsuarioNuevo.UsuarioApellido}"
                        : "No asignado",
                    UbicacionAnterior = m.UbicacionAnterior?.UbicacionNombre ?? "No asignada",
                    UbicacionNueva = m.UbicacionNueva?.UbicacionNombre ?? "No asignada",
                    Observaciones = m.Observaciones ?? "Sin observaciones",
                    CantidadMantenimientos = _context.Mantenimientos
                        .Count(mt => mt.ActivoId == m.ActivoId && mt.FechaInicio <= fechaMovimiento)
                };
            }).ToList();

            return View(historial);
        }
    }
}
