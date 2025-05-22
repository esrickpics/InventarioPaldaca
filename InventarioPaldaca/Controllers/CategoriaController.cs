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

                return RedirectToAction("Create", "Activo"); // O a donde necesites redirigir
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

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null) return NotFound();

            ViewData["CategoriaMasterId"] = new SelectList(_context.CategoriaMasters, "CategoriaMasterId", "Nombre", categoria.CategoriaMasterId);

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("CategoriaId,CategoriaNombre,CategoriaDescripcion,CategoriaMasterId")] Categorium categoria)
        {
            if (id != categoria.CategoriaId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categoria.Any(e => e.CategoriaId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["CategoriaMasterId"] = new SelectList(_context.CategoriaMasters, "CategoriaMasterId", "Nombre", categoria.CategoriaMasterId);
            return View(categoria);
        }


    }
}