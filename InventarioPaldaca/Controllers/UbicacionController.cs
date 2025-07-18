using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Controllers
{
    public class UbicacionController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public UbicacionController(InventarioPaldacaContext context)
        {
            _context = context;
        }
   
        public IActionResult Create()
        {
            return View();
        }
        [AuthorizeRole("Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UbicacionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ubicacion = new Ubicacion
                {
                    UbicacionNombre = model.UbicacionNombre,
                    UbicacionDireccion = model.UbicacionDireccion,
                    UbicacionDescripcion = model.UbicacionDescripcion
                };

                _context.Ubicacions.Add(ubicacion);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "¡Ubicación creada exitosamente!";
                return RedirectToAction("Create"); 
            }

            return View(model);
        }
        // Mostrar vista de edición
        public async Task<IActionResult> Editar()
        {
            var viewModel = new EditarUbicacionViewModel
            {
                UbicacionesDisponibles = await _context.Ubicacions
                    .Select(u => new SelectListItem
                    {
                        Value = u.UbicacionId.ToString(),
                        Text = u.UbicacionNombre
                    }).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerUbicacion(int id)
        {
            var ubicacion = await _context.Ubicacions.FindAsync(id);
            if (ubicacion == null) return NotFound();

            return Json(new
            {
                ubicacionId = ubicacion.UbicacionId,
                ubicacionNombre = ubicacion.UbicacionNombre ?? string.Empty,
                ubicacionDireccion = ubicacion.UbicacionDireccion ?? string.Empty,
                ubicacionDescripcion = ubicacion.UbicacionDescripcion ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCambiosUbicacion(EditarUbicacionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage).ToList();
                Console.WriteLine("ModelState no es válido. Errores:");
                Console.WriteLine(string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                

                TempData["Errores"] = errores;
                model.UbicacionesDisponibles = await _context.Ubicacions
                    .Select(u => new SelectListItem
                    {
                        Value = u.UbicacionId.ToString(),
                        Text = u.UbicacionNombre
                    }).ToListAsync();
                return View("Editar", model);
            }

            var ubicacion = await _context.Ubicacions.FindAsync(model.UbicacionSeleccionadaId);
            if (ubicacion == null) return NotFound();

            ubicacion.UbicacionNombre = model.UbicacionNombre;
            ubicacion.UbicacionDireccion = model.UbicacionDireccion;
            ubicacion.UbicacionDescripcion = model.UbicacionDescripcion;

            _context.Update(ubicacion);
            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "La ubicación se actualizó correctamente.";
            return RedirectToAction("Editar");
        }

        [HttpPost]
        public IActionResult EliminarUbicacion(int id)
        {
            var ubicacion = _context.Ubicacions.FirstOrDefault(u => u.UbicacionId == id);
            if (ubicacion == null)
            {
                return Json(new { exito = false, mensaje = "Ubicación no encontrada." });
            }

            bool tieneActivos = _context.Activos.Any(a => a.UbicacionId == id);
            if (tieneActivos)
            {
                return Json(new { exito = false, mensaje = "No se puede eliminar la ubicación porque hay activos asociados." });
            }

            _context.Ubicacions.Remove(ubicacion);
            _context.SaveChanges();

            return Json(new { exito = true, mensaje = "Ubicación eliminada correctamente." });
        }

    }
}
           