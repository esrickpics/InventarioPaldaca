using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using InventarioPaldaca.Utilidades.Filters;
using InventarioPaldaca.Utilidades;

namespace InventarioPaldaca.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly Dolar _dolar;
        private readonly InventarioPaldacaContext _context;

        public ProveedorController(Dolar dolar, InventarioPaldacaContext context)
        {
            _dolar = dolar;
            _context = context;
        }

        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> Index()
        {
            var precio = await _dolar.ObtenerPrecioDolarAsync();
            ViewBag.PrecioDolar = precio;
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
        
        public IActionResult Create()
        {
            var viewModel = new CreateProveedorViewModel
            {
                // Obtener todas las Categorías Maestras
                CategoriaMasters = _context.CategoriaMasters.ToList()
            };
            return View(viewModel);
        }

        // POST: Proveedor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> Create(CreateProveedorViewModel Model)
        {
            if (!ModelState.IsValid)
            {
                Model.CategoriaMasters = _context.CategoriaMasters.ToList();
                return View(Model);
            }

            // Crear el nuevo proveedor con los datos del ViewModel
            var proveedor = new Proveedor
            {
                ProveedorNombre = Model.Proveedor.ProveedorNombre,
                ProveedorRif = Model.Proveedor.ProveedorRif,
                ProveedorTelefono = Model.Proveedor.ProveedorTelefono,
                ProveedorEmail = Model.Proveedor.ProveedorEmail,
                ProveedorDireccion = Model.Proveedor.ProveedorDireccion,
                Origen = Model.Proveedor.Origen
            };

            // Llamar al método privado para asociar las categorías al proveedor
            AsociarCategoriasAlProveedor(proveedor, Model.CategoriaMasterIdSeleccionada);

            _context.Proveedors.Add(proveedor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Método privado para asociar las categorías al proveedor basado en la categoría maestra
        private void AsociarCategoriasAlProveedor(Proveedor proveedor, int? categoriaMasterId)
        {
            if (categoriaMasterId.HasValue)
            {
                var categoriasAsociadas = _context.Categoria
                    .Where(c => c.CategoriaMasterId == categoriaMasterId.Value)
                    .ToList();

                foreach (var categoria in categoriasAsociadas)
                {
                    proveedor.Categoria.Add(categoria);
                }
            }
        }
        private async Task<ProveedorViewModel> BuscarProveedoresAsync(string searchTerm, int? categoriaMasterId)
        {
            var query = _context.Proveedors.AsQueryable();

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
                    })
                    .ToListAsync()
            };
            return model;
        }
    }
}
