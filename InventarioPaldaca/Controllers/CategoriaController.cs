using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InventarioPaldaca.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly InventaryPaldacaContext _context;

        public CategoriaController(InventaryPaldacaContext context)
        {
            _context = context;
        }

        // GET: Categoria/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categoria/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaViewModel Model)
        {
            if (ModelState.IsValid)
            {
                var categoria = new Categorium
                {
                     CategoriaNombre = Model.CategoriaNombre,
                     CategoriaDescripcion = Model.CategoriaDescripcion
                };

                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create", "Activo"); // Redirige de nuevo a la vista de creación de Activo
            }
            return View(Model);
        }
    }
}