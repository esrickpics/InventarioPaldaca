using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public CategoriaController(InventarioPaldacaContext context)
        {
            _context = context;
        }

        // GET: Crear categoría
        public IActionResult Create()
        {
            var model = new CategoriaViewModel
            {
                CategoriasMasterDisponibles = _context.CategoriaMasters
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoriaMasterId.ToString(),
                        Text = c.Nombre
                    }).ToList()
            };

            return View(model);
        }

        

        // POST: Crear categoría
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var categoria = new Categorium
                {
                    CategoriaNombre = model.CategoriaNombre,
                    CategoriaDescripcion = model.CategoriaDescripcion,
                    CategoriaMasterId = model.CategoriaMasterId
                };

                _context.Add(categoria);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Activo");
            }

            // Si falla la validación, recargar el combo de categorías master
            model.CategoriasMasterDisponibles = _context.CategoriaMasters
                .Select(c => new SelectListItem
                {
                    Value = c.CategoriaMasterId.ToString(),
                    Text = c.Nombre
                }).ToList();

            return View(model);
        }
        public async Task<IActionResult> Editar()
        {
            var viewModel = new EditarCategoriaViewModel
            {
                CategoriasDisponibles = await _context.Categoria
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoriaId.ToString(),
                        Text = c.CategoriaNombre
                    }).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult EliminarCategoria(int id)
        {
            var categoria = _context.Categoria.FirstOrDefault(c => c.CategoriaId == id);
            if (categoria == null)
            {
                return Json(new { exito = false, mensaje = "Categoría no encontrada." });
            }

            bool tieneActivos = _context.Activos.Any(a => a.CategoriaId == id);
            if (tieneActivos)
            {
                return Json(new { exito = false, mensaje = "No se puede eliminar la categoría porque hay activos asociados." });
            }

            _context.Categoria.Remove(categoria);
            _context.SaveChanges();

            return Json(new { exito = true, mensaje = "Categoría eliminada correctamente." });
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerCategoria(int id)
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null) return NotFound();

            return Json(new
            {
                categoriaId = categoria.CategoriaId,
                categoriaNombre = categoria.CategoriaNombre,
                categoriaDescripcion = categoria.CategoriaDescripcion
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCambiosCategoria(EditarCategoriaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar categorías si es necesario
                model.CategoriasDisponibles = await _context.Categoria
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoriaId.ToString(),
                        Text = c.CategoriaNombre
                    }).ToListAsync();
                return View("Editar", model);
            }

            var categoria = await _context.Categoria.FindAsync(model.CategoriaSeleccionadaId);
            if (categoria == null) return NotFound();

            categoria.CategoriaNombre = model.CategoriaNombre;
            categoria.CategoriaDescripcion = model.CategoriaDescripcion;

            _context.Update(categoria);
            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "La categoría se actualizó correctamente.";
            return RedirectToAction("Editar");

        }
    }
}