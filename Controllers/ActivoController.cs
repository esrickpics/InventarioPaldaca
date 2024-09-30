using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Models.Inventario;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using InventarioPaldaca.Models;
using System.Diagnostics;

namespace InventarioPaldaca.Controllers
{
    public class ActivoController : Controller
    {
        private readonly InventaryPaldacaContext _context;

        public ActivoController(InventaryPaldacaContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Obtener todos los activos incluyendo sus relaciones
            var Activos = await _context.Activos
                               .Include(u => u.Usuario)
                               .Include(u => u.Ubicacion)
                               .Include(u => u.Categoria)
                               .ToListAsync();

            // Calcular el total de activos
            int totalActivos = Activos?.Count ?? 0;

            // Obtener todas las categorías disponibles
            var categorias = await _context.Categoria.ToListAsync();

            // Obtener todas las ubicaciones disponibles
            var ubicaciones = await _context.Ubicacions.ToListAsync();

            // Contar los activos por categoría
            var activosPorCategoria = Activos?
                .GroupBy(a => a.Categoria.CategoriaNombre)
                .ToDictionary(g => g.Key, g => g.Count())
                ?? new Dictionary<string, int>();

            // Contar los activos por ubicación
            var activosPorUbicacion = Activos?
                .GroupBy(a => a.Ubicacion.UbicacionNombre)
                .ToDictionary(g => g.Key, g => g.Count())
                ?? new Dictionary<string, int>();

            // Crear el ViewModel
            var model = new ListaActivosViewModel
            {
                TotalActivos = totalActivos,
                ActivosPorCategoria = activosPorCategoria,
                ActivosPorUbicacion = activosPorUbicacion,
                ListaActivos = Activos ?? new List<Activo>(),
                Categorias = categorias ?? new List<Categorium>(),
                Ubicaciones = ubicaciones ?? new List<Ubicacion>()
            };

            return View(model);
        }

        public IActionResult Create()
        {
            ViewData["Categoria"] = new SelectList(_context.Categoria, "CategoriaId", "CategoriaNombre");
            ViewData["Ubicacion"] = new SelectList(_context.Ubicacions, "UbicacionId", "UbicacionNombre");
            ViewData["Usuario"] = new SelectList(_context.Usuarios
                                                        .Select(u => new {
                                                           u.UsuarioId,
                                                           NombreCompleto = u.UsuarioNombre + " " + u.UsuarioApellido
                                                         }), "UsuarioId", "NombreCompleto");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var activo = new Activo()
                {
                    Marca = model.Marca,
                    Modelo = model.Modelo,
                    NumeroSerial = model.NumeroSerial,
                    Funcionabilidad = model.Funcionabilidad,
                    Observaciones = model.Observaciones,
                    CodigoInventario = model.CodigoInventario,
                    CategoriaId = model.CategoriaId,
                    UbicacionId = model.UbicacionId,
                    UsuarioId = model.UsuarioId
                };

                try
                {
                    _context.Add(activo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Manejar excepción o registrar el error
                    Console.WriteLine(ex.Message);
                    ModelState.AddModelError("", "No se pudo guardar el activo. Intente nuevamente.");
                }
            }
            else
            {
                // Mostrar los errores en el ModelState
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // Revisa qué error aparece
                }
            }
            ViewData["Categoria"] = new SelectList(_context.Categoria, "CategoriaId", "CategoriaNombre", model.CategoriaId);
            ViewData["Ubicacion"] = new SelectList(_context.Ubicacions, "UbicacionId", "UbicacionNombre", model.UbicacionId);
            ViewData["Usuario"] = new SelectList(_context.Usuarios
                                                                  .Select(u => new {
                                                                      u.UsuarioId,
                                                                      NombreCompleto = u.UsuarioNombre + " " + u.UsuarioApellido
                                                                  }), "UsuarioId", "NombreCompleto", model.UsuarioId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var activo = await _context.Activos.FindAsync(id);
                if (activo == null)
                {
                    return NotFound("El activo no fue encontrado.");
                }

                _context.Activos.Remove(activo);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Manejo de cualquier otra excepción inesperada
                ModelState.AddModelError("", "Ocurrió un error inesperado al intentar eliminar el activo.");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Message = ex.Message });
            }
        }
        public IActionResult FiltrarActivos(string categoria, string CodigoInventario, string ubicacion)
        {
            var activos = _context.Activos.Include(a => a.Usuario).Include(a => a.Categoria).Include(a => a.Ubicacion).AsQueryable();

            if (!string.IsNullOrEmpty(CodigoInventario))
            {
                activos = activos.Where(a => a.CodigoInventario.Contains(CodigoInventario));
            }


            if (!string.IsNullOrEmpty(categoria))
            {
                activos = activos.Where(a => a.Categoria.CategoriaNombre.Contains(categoria));
            }

           
            if (!string.IsNullOrEmpty(ubicacion))
            {
                activos = activos.Where(a => a.Ubicacion.UbicacionNombre.Contains(ubicacion));
            }

            var listaActivos = activos.ToList();

            // Si no hay activos que coincidan, devolvemos un JSON con el mensaje de "No se encontraron activos"
            if (!listaActivos.Any())
            {
                return Json(new { success = false, message = "No se encontraron activos que coincidan con los criterios." });
            }

            var model = new ListaActivosViewModel
            {
                ListaActivos = activos.ToList()
            };
          

            return PartialView("_ListaActivosPartial", model);
        }
    }
}
