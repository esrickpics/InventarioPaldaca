using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Controllers
{
    public class CategoriaMasterController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public CategoriaMasterController(InventarioPaldacaContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaMasterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var nueva = new CategoriaMaster { Nombre = model.Nombre };
                _context.Add(nueva);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create", "Categoria");

            }

            return View(model);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await _context.CategoriaMasters.FindAsync(id);
            if (categoria == null) return NotFound();

            var model = new CategoriaMasterViewModel { Nombre = categoria.Nombre };
            ViewBag.Id = categoria.CategoriaMasterId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CategoriaMasterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var categoria = await _context.CategoriaMasters.FindAsync(id);
                if (categoria == null) return NotFound();

                categoria.Nombre = model.Nombre;
                await _context.SaveChangesAsync();
                return RedirectToAction("Create", "Categoria");
            }

            ViewBag.Id = id;
            return View(model);
        }
    }
}
