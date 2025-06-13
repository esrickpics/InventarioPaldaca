using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Controllers
{
    public class ProyectosController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public ProyectosController(InventarioPaldacaContext context)
        {
            _context = context;
        }
        [HttpGet]
        [AuthorizeRole("Administrador")]
        public IActionResult Crear()
        {
            var fechaActual = DateOnly.FromDateTime(DateTime.Today);

            // Obtener los IDs de los usuarios que ya están asignados a proyectos activos
            var usuariosAsignadosAProyectosActivos = _context.ProyectoAdministradors
                .Where(pa => pa.Proyecto.FechaFin == null || pa.Proyecto.FechaFin >= fechaActual)
                .Select(pa => pa.UsuarioId)
                .Distinct()
                .ToList();

            var usuariosElegibles = _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Rol.RolNombre == "AdministradorProyecto" &&
                            !usuariosAsignadosAProyectosActivos.Contains(u.UsuarioId))
                .Select(u => new SelectListItem
                {
                    Value = u.UsuarioId.ToString(),
                    Text = $"{u.UsuarioNombre} {u.UsuarioApellido}"
                }).ToList();

          
            if (!usuariosElegibles.Any()) // Si no hay usuarios elegibles después de filtrar
            {
                // Agrega un elemento a la lista para mostrar el mensaje
                usuariosElegibles.Insert(0, new SelectListItem
                {
                    Value = "", // Valor vacío para que no se seleccione ningún usuario
                    Text = "-- No hay administradores de proyecto disponibles --",
                    Disabled = true, // Deshabilita esta opción
                    Selected = true // Selecciona esta opción por defecto
                });
            }
            else
            {
                // Si hay usuarios, agrega la opción por defecto normal
                usuariosElegibles.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Seleccione un administrador --"
                });
            }
            // --- FIN NUEVA LÓGICA ---

            var viewModel = new ProyectoViewModel
            {
                UsuariosDisponibles = usuariosElegibles,
                FechaInicio = fechaActual
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult Crear(ProyectoViewModel model)
        {
            ModelState.Remove("ProyectoId"); // Esto está bien si ProyectoId es auto-generado o no se envía desde la vista

            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, recarga la lista de administradores elegibles
                // ¡Usando la misma lógica de filtrado que en el GET!
                var fechaActual = DateOnly.FromDateTime(DateTime.Today);
                var usuariosAsignadosAProyectosActivos = _context.ProyectoAdministradors
                    .Where(pa => pa.Proyecto.FechaFin == null || pa.Proyecto.FechaFin >= fechaActual)
                    .Select(pa => pa.UsuarioId)
                    .Distinct()
                    .ToList();

                var usuariosElegibles = _context.Usuarios
                    .Include(u => u.Rol)
                    .Where(u => u.Rol.RolNombre == "AdministradorProyecto" &&
                                !usuariosAsignadosAProyectosActivos.Contains(u.UsuarioId))
                    .Select(u => new SelectListItem
                    {
                        Value = u.UsuarioId.ToString(),
                        Text = $"{u.UsuarioNombre} {u.UsuarioApellido}"
                    }).ToList();

                // --- LÓGICA DE MENSAJE PARA EL DROPDOWN (replicada del GET) ---
                if (!usuariosElegibles.Any()) // Si no hay usuarios elegibles después de filtrar
                {
                    usuariosElegibles.Insert(0, new SelectListItem
                    {
                        Value = "",
                        Text = "-- No hay administradores de proyecto disponibles --",
                        Disabled = true,
                        Selected = true
                    });
                }
                else
                {
                    usuariosElegibles.Insert(0, new SelectListItem
                    {
                        Value = "",
                        Text = "-- Seleccione un administrador --"
                    });
                }
                // --- FIN LÓGICA DE MENSAJE ---

                model.UsuariosDisponibles = usuariosElegibles; // Asigna la lista final al modelo

                return View(model); // Vuelve a la vista con el modelo y los errores de validación
            }

            var proyecto = new Proyecto
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                FechaInicio = model.FechaInicio,
                FechaFin = model.FechaFin,
                Estado = "Activo"
            };

            _context.Proyectos.Add(proyecto);
            _context.SaveChanges();

            if (model.UsuarioId.HasValue)
            {
                var asignacion = new ProyectoAdministrador
                {
                    UsuarioId = model.UsuarioId.Value,
                    ProyectoId = proyecto.ProyectoId
                };

                _context.ProyectoAdministradors.Add(asignacion);
                _context.SaveChanges();
            }

            return RedirectToAction("ListadoProyectos");
        }
        [AuthorizeRole("Administrador")]
        public IActionResult ListadoProyectos()
        {
            var proyectos = _context.ProyectoAdministradors
                .Include(pa => pa.Proyecto)
                .Include(pa => pa.Usuario)
                .Select(pa => new ListarProyectosViewModel
                {
                    ProyectoId = pa.Proyecto.ProyectoId,
                    NombreProyecto = pa.Proyecto.Nombre,
                    Descripcion = pa.Proyecto.Descripcion,
                    FechaInicio = pa.Proyecto.FechaInicio,
                    FechaFin = pa.Proyecto.FechaFin,
                    Estado = pa.Proyecto.Estado,
                    ImagenAdministrador = string.IsNullOrEmpty(pa.Usuario.ImagenUrl) ? "/img/Usuarios/Img.default.png" : pa.Usuario.ImagenUrl,
                    NombreAdministrador = pa.Usuario.UsuarioNombre + " " + pa.Usuario.UsuarioApellido,
                    CargoAdministrador = pa.Usuario.UsuarioCargo
                }).ToList();

            return View(proyectos);
        }
        [HttpGet]
        public IActionResult EditPartial(int id)
        {
            var proyecto = _context.Proyectos
                .Include(p => p.ProyectoAdministradors)
                .FirstOrDefault(p => p.ProyectoId == id);

            if (proyecto == null)
                return NotFound();

            var administradorActualId = proyecto.ProyectoAdministradors.FirstOrDefault()?.UsuarioId;

            // Usar DateOnly.FromDateTime(DateTime.Today) para asegurar que ambas son DateOnly
            var fechaActual = DateOnly.FromDateTime(DateTime.Today); // Asegura que es DateOnly

            // Obtener los IDs de los usuarios que ya están asignados a proyectos activos
            var usuariosAsignadosActivos = _context.ProyectoAdministradors
                .Where(pa => pa.Proyecto.FechaFin == null || // Si FechaFin es null (proyecto indefinido/activo)
                             pa.Proyecto.FechaFin >= fechaActual) // O si la fecha de fin es HOY o en el futuro
                .Select(pa => pa.UsuarioId)
                .Distinct()
                .ToList();

            // Filtrar los usuarios según las reglas:
            var usuariosElegibles = _context.Usuarios
                .Where(u => (u.RolId == 2 || u.RolId == 3) &&
                            (!usuariosAsignadosActivos.Contains(u.UsuarioId) ||
                             u.UsuarioId == administradorActualId))
                .Select(u => new SelectListItem
                {
                    Value = u.UsuarioId.ToString(),
                    Text = u.UsuarioNombre + " " + u.UsuarioApellido
                }).ToList();

            var viewModel = new ProyectoViewModel
            {
                ProyectoId = proyecto.ProyectoId,
                Nombre = proyecto.Nombre,
                Descripcion = proyecto.Descripcion,
                FechaInicio = proyecto.FechaInicio,
                FechaFin = proyecto.FechaFin,
                UsuarioId = administradorActualId,
                UsuariosDisponibles = usuariosElegibles
            };

            return PartialView("_EditProyectoPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditModal(ProyectoViewModel model)
        {
            // 1. **Validación de la lógica de negocio para UsuarioId (si es null)**
            // Agregamos este error *primero* si el UsuarioId es nulo.
            // Esto asegura que si el UsuarioId NO se selecciona, el error se captura.
            if (!model.UsuarioId.HasValue)
            {
                // Añade el error directamente a ModelState para que sea capturado por ModelState.IsValid
                ModelState.AddModelError(nameof(model.UsuarioId), "Debe seleccionar un administrador.");
            }

       
            if (!ModelState.IsValid)
            {
                // Recolecta todos los errores del ModelState.
                // Las claves del diccionario serán los nombres de las propiedades (ej. "Nombre", "FechaFin", "UsuarioId").
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                // Retorna un BadRequest con un mensaje general y los errores específicos.
                // El JavaScript del lado del cliente se encargará de mostrar estos errores.
                return BadRequest(new { mensaje = "Error de validación.", errores = errors });
            }

            var proyecto = _context.Proyectos
                .Include(p => p.ProyectoAdministradors)
                .FirstOrDefault(p => p.ProyectoId == model.ProyectoId);

            if (proyecto == null)
            {
                return NotFound();
            }

            // Actualizar datos del proyecto
            proyecto.Nombre = model.Nombre;
            proyecto.Descripcion = model.Descripcion;
            proyecto.FechaInicio = model.FechaInicio;
            proyecto.FechaFin = model.FechaFin;

            // Eliminar el administrador anterior (si existe)
            var adminAnterior = _context.ProyectoAdministradors
                .FirstOrDefault(pa => pa.ProyectoId == proyecto.ProyectoId);

            if (adminAnterior != null)
            {
                _context.ProyectoAdministradors.Remove(adminAnterior);
            }

            // Agregar el nuevo administrador (obligatorio)
            var nuevoAdmin = new ProyectoAdministrador
            {
                ProyectoId = proyecto.ProyectoId,
                UsuarioId = model.UsuarioId.Value
            };

            _context.ProyectoAdministradors.Add(nuevoAdmin);

            _context.SaveChanges();

            return Ok();
        }
    }
}

