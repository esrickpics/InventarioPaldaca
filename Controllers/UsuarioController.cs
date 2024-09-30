using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioPaldaca.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly InventaryPaldacaContext _context;

       public UsuarioController(InventaryPaldacaContext context)
            {
                _context = context;
            }
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            var model = new UsuarioPerfilViewModel
            {
                SearchTerm = searchTerm
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> BuscarUsuariosIndex(string searchTerm = "")
        {
            var model = await BuscarUsuariosAsync(searchTerm);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UsuariosEncontradosPartial", model);
            }
            return View("PerfilUsuario", model); // Devuelve a la vista Index con el modelo actualizado
        }

        public IActionResult Create()
            {
                return View();
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Usuario
                {
                    UsuarioNombre = model.Nombre,
                    UsuarioApellido = model.Apellido,
                    UsuarioEmail = model.Email,
                    UsuarioTelefono = model.Telefono,
                    UsuarioCargo = model.Cargo
                };

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create", "Activo"); // Redirige a la acción deseada
            }
            return View(model);
        }

        // Acción para mostrar el perfil de un usuario
        public async Task<IActionResult> PerfilUsuario(int? id, string searchTerm = "")
        {
            if (id == null)
            {
                return NotFound();
            }
            // Obtener perfil del usuario
            var model = await ObtenerPerfilUsuarioAsync(id.Value);

            if (model == null)
            {
                return NotFound();
            }
            
            if (string.IsNullOrEmpty(model.UsuarioImagenUrl))
               {
                 model.UsuarioImagenUrl = Url.Content("~/img/Usuarios/Img.default.png");
               }


            // Realizar búsqueda si se proporcionó un término de búsqueda
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                model = await BuscarUsuariosAsync(searchTerm);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_UsuariosEncontradosPartial", model);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(UsuarioPerfilViewModel model)
        {
            ModelState.Remove("SearchTerm"); // Quitar SearchTerm de la validación

            if (model.UsuarioId < 0)
            {
                TempData["Error"] = "ID de usuario no válido.";
                return RedirectToAction("PerfilUsuario", new { id = model.UsuarioId });
            }

            if (!string.IsNullOrEmpty(model.UsuarioCorreo) && !new EmailAddressAttribute().IsValid(model.UsuarioCorreo))
            {
                ModelState.AddModelError("UsuarioCorreo", "Ingrese un correo electrónico válido.");
            }

            if (!string.IsNullOrEmpty(model.UsuarioTelefono) && !new PhoneAttribute().IsValid(model.UsuarioTelefono))
            {
                ModelState.AddModelError("UsuarioTelefono", "Ingrese un número de teléfono válido.");
            }

            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("PerfilUsuario", new { id = model.UsuarioId });
                }

                usuario.UsuarioNombre = model.UsuarioNombre;
                usuario.UsuarioApellido = model.UsuarioApellido;
                usuario.UsuarioEmail = model.UsuarioCorreo;
                usuario.UsuarioTelefono = model.UsuarioTelefono;
                usuario.UsuarioCargo = model.UsuarioCargo;

                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Datos actualizados correctamente.";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Ocurrió un error al actualizar los datos.";
                }

                return RedirectToAction("PerfilUsuario", new { id = model.UsuarioId });
            }
            else
            {
                TempData["Error"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                // Recargar datos del usuario en caso de error
                model = await ObtenerPerfilUsuarioAsync(model.UsuarioId);
                return View("PerfilUsuario", model);
            }
        }

        // Acción para buscar usuarios
        public async Task<IActionResult> BuscarUsuarios(string searchTerm = "")
        {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    // Devuelve la vista principal sin ningún cambio
                    return PartialView("_UsuariosEncontradosPartial", null);
                }

                var model = await BuscarUsuariosAsync(searchTerm);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_UsuariosEncontradosPartial", model);
                }

                return View("PerfilUsuario", model);
        }

        // Método privado para obtener el perfil del usuario
        private async Task<UsuarioPerfilViewModel> ObtenerPerfilUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Activos)
                    .ThenInclude(a => a.Categoria)
                .Include(u => u.Activos)
                    .ThenInclude(a => a.Ubicacion)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
            {
                return null;
            }

            return new UsuarioPerfilViewModel
            {
                UsuarioId = usuario.UsuarioId,
                UsuarioNombre = usuario.UsuarioNombre,
                UsuarioApellido = usuario.UsuarioApellido,
                UsuarioCorreo = usuario.UsuarioEmail,
                UsuarioTelefono = usuario.UsuarioTelefono,
                UsuarioCargo = usuario.UsuarioCargo,
                UsuarioImagenUrl = usuario.ImagenUrl,
                ActivosAsociados = usuario.Activos.Select(a => new ActivosAsociadosViewModel
                {
                    Marca = a.Marca,
                    Modelo = a.Modelo,
                    CodigoInventario = a.CodigoInventario,
                    CategoriaName = a.Categoria?.CategoriaNombre ?? "Sin Categoría",
                    UbicacionName = a.Ubicacion?.UbicacionNombre ?? "Sin Ubicación"
                }).ToList()
            };
        }

        // Método privado para buscar usuarios
        private async Task<UsuarioPerfilViewModel> BuscarUsuariosAsync(string searchTerm)
        {
            var model = new UsuarioPerfilViewModel
            {
                SearchTerm = searchTerm,
                UsuariosEncontrados = await _context.Usuarios
                  .Where(u => u.UsuarioNombre.Contains(searchTerm) || u.UsuarioApellido.Contains(searchTerm) || u.UsuarioEmail.Contains(searchTerm))
                  .ToListAsync()
            };

            return model;
        }
    }
}
