using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioPaldaca.Models;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Utilidades.Filters;
using System.Globalization;

namespace InventarioPaldaca.Controllers
{
    public class MantenimientosController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public MantenimientosController(InventarioPaldacaContext context)
        {
            _context = context;
        }

        [AuthorizeRole("Administrador", "AdministradorProyecto")]
        public IActionResult HistorialMantenimientos(string? codigo)
        {
            var viewModel = new MantenimientoViewModel();

            var rolUsuario = HttpContext.Session.GetString("UsuarioRol");
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            int.TryParse(usuarioIdStr, out int usuarioId);

            // ✅ Declaración corregida:
            IQueryable<Mantenimiento> mantenimientosQuery = _context.Mantenimientos
                .Include(m => m.Activo);

            if (rolUsuario == "3" || rolUsuario == "AdministradorProyecto")
            {
                var proyectosDelUsuario = _context.ProyectoAdministradors
                    .Where(pa => pa.UsuarioId == usuarioId)
                    .Select(pa => pa.ProyectoId)
                    .ToList();

                mantenimientosQuery = mantenimientosQuery
                    .Where(m => m.Activo.ProyectoId != null && proyectosDelUsuario.Contains(m.Activo.ProyectoId.Value));
            }

            viewModel.UltimosMantenimientos = mantenimientosQuery
                .OrderByDescending(m => m.FechaInicio)
                .ThenByDescending(m => m.FechaFin)
                .Take(15)
                .ToList();

            if (!string.IsNullOrEmpty(codigo))
            {
                viewModel.HistorialDelActivo = mantenimientosQuery
                    .Where(m => m.Activo.CodigoInventario == codigo)
                    .OrderByDescending(m => m.FechaInicio)
                    .ThenByDescending(m => m.FechaFin)
                    .ToList();

                viewModel.CodigoInventarioBuscado = codigo;
                viewModel.MostrandoHistorial = true;
            }

            return View(viewModel);
        }



        [AuthorizeRole("Administrador", "AdministradorProyecto")]
        public async Task<IActionResult> ActivosEnMantenimiento()
        {
            var rolUsuario = HttpContext.Session.GetString("UsuarioRol");
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            int.TryParse(usuarioIdStr, out int usuarioId);

            var query = _context.Activos
                .Include(a => a.Mantenimientos)
                .Include(a => a.Categoria)
                .Include(a => a.Ubicacion)
                .Include(a => a.Usuario)
                .Where(a => a.Funcionabilidad == false)
                .AsQueryable();

            if (rolUsuario == "3" || rolUsuario == "AdministradorProyecto")
            {
                var proyectosDelUsuario = _context.ProyectoAdministradors
                    .Where(pa => pa.UsuarioId == usuarioId)
                    .Select(pa => pa.ProyectoId)
                    .ToList();

                query = query.Where(a => a.ProyectoId != null && proyectosDelUsuario.Contains(a.ProyectoId.Value));
            }

            var activosDañados = await query.ToListAsync();

            var model = new ActivosMantenimientoViewModel
            {
                Activos = activosDañados,
                TotalActivosEnMantenimiento = activosDañados.Count
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditarMantenimiento(int MantenimientoId, string Tecnico, string NumeroTecnico, string Costo, string Descripcion)
        {
            var mantenimiento = _context.Mantenimientos.FirstOrDefault(m => m.MantenimientoId == MantenimientoId);
            if (mantenimiento == null)
            {
                TempData["ErrorMessage"] = "Mantenimiento no encontrado.";
                return RedirectToAction("ActivosEnMantenimiento");
            }

            mantenimiento.Tecnico = Tecnico;
            mantenimiento.NumeroTecnico = NumeroTecnico;
            mantenimiento.Descripcion = Descripcion;

            if (decimal.TryParse(Costo, NumberStyles.Any, CultureInfo.InvariantCulture, out var costoDecimal))
            {
                mantenimiento.Costo = costoDecimal;
            }
            else
            {
                TempData["ErrorMessage"] = "El costo ingresado no es válido.";
                return RedirectToAction("ActivosEnMantenimiento");
            }

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
