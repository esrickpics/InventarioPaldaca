using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Utilidades;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioPaldaca.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public UsuarioController(InventarioPaldacaContext context)
        {
            _context = context;
        }
        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            var model = new UsuarioPerfilViewModel
            {
                SearchTerm = searchTerm
            };
            return View(model);
        }
        [AuthorizeRole("Administrador")]
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

        // Acción para mostrar el perfil de un usuario
        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> PerfilUsuario(int? id, string searchTerm = "", bool mostrarBotonRestablecer = false)
        {
            if (id == null)
            {
                Console.WriteLine("ID de usuario no proporcionado.");
                return NotFound();
            }

            // Obtener perfil del usuario
            var model = await ObtenerPerfilUsuarioAsync(id.Value);

            if (model == null)
            {
          
                return NotFound();

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
            // Ya lo haces bien aquí:
            ViewBag.MostrarBotonRestablecer = model.SolicitarRestablecer;

            return View(model);
        }


        [HttpPost]
        public IActionResult AprobarRestablecimiento(int usuarioId)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == usuarioId);
            if (usuario == null)
                return NotFound();

            usuario.PuedeRestablecer = true;
            _context.Update(usuario);
            _context.SaveChanges();

            TempData["Mensaje"] = "Restablecimiento aprobado.";

            // Redirige a la acción que vuelve a cargar el usuario desde la DB
            return RedirectToAction("PerfilUsuario", new { id = usuarioId });
        }


        [AuthorizeRole("Administrador")]
        public IActionResult Create()
        {
            return View();
        }
        [AuthorizeRole("Administrador")]
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
                    UsuarioCargo = model.Cargo,
                    UsuarioPassword = Encrypt.GetSHA256(model.Password),
                    RolId = model.Rol
                };
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create", "Activo"); // Redirige a la acción deseada
            }
            return View(model);
        }

        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> EditarUsuario(UsuarioPerfilViewModel model, IFormFile pdfFile, IFormFile imageFile)
        {
            ModelState.Remove("SearchTerm");
            ModelState.Remove("imageFile");
            ModelState.Remove("AsignacionPdf");
            ModelState.Remove("pdfFile");

            if (model.UsuarioId < 0)
            {
                TempData["Error"] = "ID de usuario no válido.";
                return RedirectToAction("PerfilUsuario", new { id = model.UsuarioId });
            }

            // Validar correo electrónico y teléfono
            if (!string.IsNullOrEmpty(model.UsuarioCorreo) && !new EmailAddressAttribute().IsValid(model.UsuarioCorreo))
                ModelState.AddModelError("UsuarioCorreo", "Ingrese un correo electrónico válido.");

            if (!string.IsNullOrEmpty(model.UsuarioTelefono) && !new PhoneAttribute().IsValid(model.UsuarioTelefono))
                ModelState.AddModelError("UsuarioTelefono", "Ingrese un número de teléfono válido.");

            var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("PerfilUsuario", new { id = model.UsuarioId });
            }

            // Valores predeterminados para imagen y PDF
            model.AsignacionPdf ??= usuario.AsignacionPdf;
            model.UsuarioImagenUrl ??= usuario.ImagenUrl ?? "~/img/Usuarios/Img.default.png";

            // Manejar archivo PDF
            if (pdfFile != null && pdfFile.Length > 0)
            {
                var pdfResult = await ManejarArchivo(pdfFile, "wwwroot/img/AsignacionPDF", ".pdf");
                if (!pdfResult.Success)
                {
                    ModelState.AddModelError("PdfFile", pdfResult.ErrorMessage);
                }
                else
                {
                    EliminarArchivo(usuario.AsignacionPdf); // Eliminar PDF anterior
                    usuario.AsignacionPdf = pdfResult.FilePath;
                }
            }

            // Manejar archivo de imagen
            if (imageFile != null && imageFile.Length > 0)
            {
                var imageResult = await ManejarArchivo(imageFile, "wwwroot/img/Usuarios", ".jpg,.jpeg,.png");
                if (!imageResult.Success)
                {
                    ModelState.AddModelError("ImageFile", imageResult.ErrorMessage);
                }
                else
                {
                    EliminarArchivo(usuario.ImagenUrl); // Eliminar imagen anterior
                    usuario.ImagenUrl = imageResult.FilePath;
                }
            }

            if (ModelState.IsValid)
            {
                // Actualizar los datos del usuario
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
                catch (Exception ex)
                {
                    TempData["Error"] = "Ocurrió un error al actualizar los datos: " + ex.Message;
                }

                return RedirectToAction("PerfilUsuario", new { id = model.UsuarioId });
            }

            TempData["Error"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            model = await ObtenerPerfilUsuarioAsync(model.UsuarioId); // Recargar datos en caso de error
            return View("PerfilUsuario", model);
        }


        // Método reutilizable para manejar la subida de archivos
        private async Task<(bool Success, string FilePath, string ErrorMessage)> ManejarArchivo(IFormFile file, string uploadPath, string allowedExtensions)
        {
            try
            {
                var validExtensions = allowedExtensions.Split(',');
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!validExtensions.Contains(extension))
                    return (false, null, $"Solo se permiten archivos de tipo {string.Join(", ", validExtensions)}.");

                var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Crear directorio si no existe
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), uploadPath));

                // Guardar el archivo
                using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), filePath), FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return (true, $"/{filePath.Substring("wwwroot/".Length)}", null);

            }
            catch (Exception ex)
            {
                return (false, null, $"Error al subir el archivo: {ex.Message}");
            }
        }

        // Método para eliminar archivos
      
        [AuthorizeRole("Administrador")]
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
        // por cada campo nuevo en la base de datos se debe actualizar este metodo
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
                Console.WriteLine($"Usuario con ID {id} no encontrado.");
                return null;
            }

            Console.WriteLine($"Usuario encontrado: {usuario.UsuarioNombre} {usuario.UsuarioApellido}");
            return new UsuarioPerfilViewModel
            {
                UsuarioId = usuario.UsuarioId,
                UsuarioNombre = usuario.UsuarioNombre,
                UsuarioApellido = usuario.UsuarioApellido,
                UsuarioCorreo = usuario.UsuarioEmail,
                UsuarioTelefono = usuario.UsuarioTelefono,
                UsuarioCargo = usuario.UsuarioCargo,
                UsuarioImagenUrl = usuario.ImagenUrl,
                AsignacionPdf = usuario.AsignacionPdf,
                SolicitarRestablecer = usuario.SolicitoRestablecer,
                PuedeRestablecer = usuario.PuedeRestablecer,

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
            Console.WriteLine($"Buscando usuarios con término: '{searchTerm}'");
            var model = new UsuarioPerfilViewModel
            {
                SearchTerm = searchTerm,
                UsuariosEncontrados = await _context.Usuarios
                  .Where(u => u.UsuarioNombre.Contains(searchTerm) || u.UsuarioApellido.Contains(searchTerm) || u.UsuarioEmail.Contains(searchTerm))
                  .ToListAsync()
            };
            Console.WriteLine($"Se encontraron {model.UsuariosEncontrados.Count} usuarios con el término de búsqueda '{searchTerm}'.");
            return model;
        } 
        private void EliminarArchivo(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    try
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al eliminar archivo: {ex.Message}");
                    }
                }
            }
        }
    }
}
