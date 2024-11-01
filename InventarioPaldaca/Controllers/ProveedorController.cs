using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Controllers
{
    public class ProveedorController : Controller
    {

        private readonly InventaryPaldacaContext _context;

        public ProveedorController(InventaryPaldacaContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> BuscarProveedores(string searchTerm = "")
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // Devuelve la vista parcial sin resultados si no hay término de búsqueda
                return PartialView("_ProveedoresEncontradosPartial", null);
            }

            var model = await BuscarProveedoresAsync(searchTerm);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProveedoresEncontradosPartial", model);
            }

            return View("Index", model);
        }

        private async Task<ProveedorViewModel> BuscarProveedoresAsync(string searchTerm)
        {
            var model = new ProveedorViewModel
            {
                Search = searchTerm,
                ProveedoresEncontrados = await _context.Proveedors
                    .Where(p => p.ProveedorNombre.Contains(searchTerm) ||
                                p.ProveedorRif.Contains(searchTerm) ||
                                p.ProveedorEmail.Contains(searchTerm))
                    .Select(p => new ProveedorViewModel
                    {
                        ProveedorId = p.ProveedorId,
                        ProveedorNombre = p.ProveedorNombre,
                        ProveedorRif = p.ProveedorRif,
                        ProveedorTelefono = p.ProveedorTelefono,
                        ProveedorEmail = p.ProveedorEmail,
                        ProveedorDireccion = p.ProveedorDireccion,
                        ProveedorOrigen = p.Origen,
                        Requisicion = p.Requisicion
                    })
                    .ToListAsync()
            };
            return model;
        }

    }
}
