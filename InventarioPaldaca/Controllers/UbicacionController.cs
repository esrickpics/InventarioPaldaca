using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.AspNetCore.Mvc;

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
                return RedirectToAction("Create", "Activo"); 
            }

            return View(model);
        }
    }
}
           