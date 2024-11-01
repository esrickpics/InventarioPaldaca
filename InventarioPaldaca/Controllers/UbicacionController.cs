using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InventarioPaldaca.Controllers
{
    public class UbicacionController : Controller
    {
        private readonly InventaryPaldacaContext _context;

        public UbicacionController(InventaryPaldacaContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

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
                return RedirectToAction("Create", "Activo"); // Redirige a la creación de un Activo o a donde lo necesites.
            }

            return View(model);
        }
    }
}
           