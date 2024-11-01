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

            // Validar si el usuario tiene asignado un PDF
            if (string.IsNullOrEmpty(model.AsignacionPdf))
            {
                ViewBag.ErrorMensaje = "Este usuario no tiene un PDF asignado.";
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
        public async Task<IActionResult> EditarUsuario(UsuarioPerfilViewModel model, IFormFile pdfFile, IFormFile imageFile)
        {
            // Remover la validación de SearchTerm y AsignacionPdf
            ModelState.Remove("SearchTerm");
            ModelState.Remove("AsignacionPdf");  // Remueve AsignacionPdf de la validación
            ModelState.Remove("ImagenUrl");  // Remueve ImagenUrl de la validación

            if (model.UsuarioId < 0)
            {
                TempData["Error"] = "ID de usuario no válido.";
                return RedirectToAction("PerfilUsuario", new { id = model.UsuarioId });
            }

            // Validaciones adicionales para el correo electrónico y teléfono
            if (!string.IsNullOrEmpty(model.UsuarioCorreo) && !new EmailAddressAttribute().IsValid(model.UsuarioCorreo))
            {
                ModelState.AddModelError("UsuarioCorreo", "Ingrese un correo electrónico válido.");
            }

            if (!string.IsNullOrEmpty(model.UsuarioTelefono) && !new PhoneAttribute().IsValid(model.UsuarioTelefono))
            {
                ModelState.AddModelError("UsuarioTelefono", "Ingrese un número de teléfono válido.");
            }

            var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("PerfilUsuario", new { id = model.UsuarioId });
            }

            model.UsuarioImagenUrl = usuario.ImagenUrl ?? "/img/default-profile.png";

            model.AsignacionPdf = usuario.AsignacionPdf ?? string.Empty;

            // Subida del archivo PDF
            if (pdfFile != null && pdfFile.Length > 0)
            {
                if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                {
                    ModelState.AddModelError("PdfFile", "Solo se permiten archivos PDF.");
                }
                else
                {
                    // Eliminar el PDF anterior si existe
                    if (!string.IsNullOrEmpty(usuario.AsignacionPdf))
                    {
                        var previousFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", usuario.AsignacionPdf.TrimStart('/'));
                        if (System.IO.File.Exists(previousFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(previousFilePath);  // Eliminar el archivo antiguo
                            }
                            catch (Exception ex)
                            {
                                TempData["Error"] = $"No se pudo eliminar el archivo anterior: {ex.Message}";
                            }
                        }
                    }

                    // Guardar el nuevo archivo PDF
                    var originalFileName = Path.GetFileNameWithoutExtension(pdfFile.FileName);
                    var yearMonth = DateTime.Now.ToString("yyyyMM");
                    var fileName = $"{originalFileName}_{yearMonth}{Path.GetExtension(pdfFile.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/AsignacionPDF", fileName);

                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await pdfFile.CopyToAsync(stream);
                        }

                        // Asignar la nueva ruta del PDF al modelo y usuario
                        model.AsignacionPdf = $"/img/AsignacionPDF/{fileName}";
                        usuario.AsignacionPdf = model.AsignacionPdf;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("PdfFile", $"Ocurrió un error al subir el archivo PDF: {ex.Message}");
                    }
                }
            }

            // Subida del archivo de imagen
            if (imageFile != null && imageFile.Length > 0)
            {
                var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!validExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Solo se permiten archivos JPG o PNG.");
                }
                else
                {
                    // Eliminar la imagen anterior si existe
                    if (!string.IsNullOrEmpty(usuario.ImagenUrl))
                    {
                        var previousImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", usuario.ImagenUrl.TrimStart('/'));
                        if (System.IO.File.Exists(previousImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(previousImagePath);  // Eliminar la imagen antigua
                            }
                            catch (Exception ex)
                            {
                                TempData["Error"] = $"No se pudo eliminar la imagen anterior: {ex.Message}";
                            }
                        }
                    }

                    // Guardar la nueva imagen
                    var originalFileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                    
                    var imageName = $"{originalFileName}{extension}";
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Usuarios", imageName);

                    try
                    {
                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Asignar la nueva ruta de la imagen al modelo y usuario
                        model.UsuarioImagenUrl = $"/img/Usuarios/{imageName}";
                        usuario.ImagenUrl = model.UsuarioImagenUrl;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ImageFile", $"Ocurrió un error al subir la imagen: {ex.Message}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                // Actualizar los campos del usuario
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
                AsignacionPdf= usuario.AsignacionPdf,
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
