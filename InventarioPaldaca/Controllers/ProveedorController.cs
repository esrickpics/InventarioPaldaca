using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            var viewModel = new ProveedorViewModel
            {
                CategoriasMaestras = _context.CategoriaMasters.ToList(),
                ProveedoresEncontrados = new List<ProveedorViewModel>() // Inicializa con una lista vacía
            };
            return View(viewModel);
        }

        public async Task<IActionResult> BuscarProveedores(string searchTerm = "", int? categoriaMasterId = null)
        {
            var model = await BuscarProveedoresAsync(searchTerm, categoriaMasterId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProveedoresEncontradosPartial", model.ProveedoresEncontrados);
            }

            return View("Index", model);
        }

        private async Task<ProveedorViewModel> BuscarProveedoresAsync(string searchTerm, int? categoriaMasterId)
        {
            var query = _context.Proveedors.AsQueryable();

            // Filtro por término de búsqueda si está presente
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.ProveedorNombre.Contains(searchTerm) ||
                                         p.ProveedorRif.Contains(searchTerm) ||
                                         p.ProveedorEmail.Contains(searchTerm));
            }

            if (categoriaMasterId.HasValue)
            {
                    query = query
                    .Where(p => p.Categoria
                                .Any(pc => pc.CategoriaMaster.CategoriaMasterId == categoriaMasterId));
            }


            var model = new ProveedorViewModel
            {
                Search = searchTerm,
                ProveedoresEncontrados = await query
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
