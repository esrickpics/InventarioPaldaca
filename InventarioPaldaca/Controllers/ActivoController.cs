using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioPaldaca.Models.ViewModels;
using InventarioPaldaca.Models.Inventario;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using InventarioPaldaca.Models;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using InventarioPaldaca.Utilidades.Filters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace InventarioPaldaca.Controllers
{
    public class ActivoController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public ActivoController(InventarioPaldacaContext context)
        {
            _context = context;
        }

        // ========================== VISTAS PRINCIPALES ==========================

        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> Index(int pagina = 1, int pageSize = 10)
        {
            ViewData["Usuarios"] = new SelectList(_context.Usuarios
                .Select(u => new { u.UsuarioId, NombreCompleto = u.UsuarioNombre + " " + u.UsuarioApellido }),
                "UsuarioId", "NombreCompleto");

            var query = _context.Activos
                .Include(a => a.Usuario)
                .Include(a => a.Ubicacion)
                .Include(a => a.Categoria)
                .ThenInclude(c => c.CategoriaMaster)
                .AsQueryable();

            int totalActivos = await query.CountAsync();

            var activosPaginados = await query
                .OrderBy(a => a.ActivoId)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var listaActivosCompleta = await _context.Activos
           .Include(a => a.Categoria)
           .Include(a => a.Usuario)
           .ToListAsync(); // Esto trae todos


            var model = new ListaActivosViewModel
            {
                ListaActivos = activosPaginados,
                Categorias = await _context.Categoria.ToListAsync(),
                Ubicaciones = await _context.Ubicacions.ToListAsync(),
                CategoriasMaster = await _context.CategoriaMasters.ToListAsync(),
                TotalActivos = totalActivos,
                TotalActivosDañados = await _context.Activos.CountAsync(a => a.Funcionabilidad == false),
                PaginaActual = pagina,
                PageSize = pageSize,
                TotalFiltrados = totalActivos,
                ListaActivosCompleta = listaActivosCompleta,  
                ActivosPorCategoria = await query
                    .GroupBy(a => a.Categoria.CategoriaNombre)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Key, g => g.Count),
                ActivosPorUbicacion = await query
                    .GroupBy(a => a.Ubicacion.UbicacionNombre)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Key, g => g.Count)
            };

            return View(model);
        }

        [AuthorizeRole("Administrador")]
        public IActionResult ActivosDañados()
        {
             var activosDañados = _context.Activos
            .Include(a => a.Usuario)
            .Include(a => a.Categoria)
            .Include(a => a.Ubicacion)
            .Include(a => a.Mantenimientos)
            .Where(a => a.Funcionabilidad == false)
            .ToList();

            var model = new ListaActivosViewModel
            {
                ListaActivos = activosDañados
            };

            return View("ActivosDañados", model);
        }

        [HttpPost]
        public IActionResult EditarMantenimiento(int MantenimientoId, string Tecnico, string NumeroTecnico, decimal Costo, string Descripcion, string Estado)
        {
            var mantenimiento = _context.Mantenimientos.FirstOrDefault(m => m.MantenimientoId == MantenimientoId);
            if (mantenimiento == null)
            {
                TempData["ErrorMessage"] = "Mantenimiento no encontrado.";
                return RedirectToAction("ActivosDañados");
            }

            mantenimiento.Tecnico = Tecnico;
            mantenimiento.NumeroTecnico = NumeroTecnico;
            mantenimiento.Costo = Costo;
            mantenimiento.Descripcion = Descripcion;
            mantenimiento.Estado = Estado;

            if (Estado == "Finalizado" && mantenimiento.FechaFin == null)
            {
                mantenimiento.FechaFin = DateOnly.FromDateTime(DateTime.Now);

                // Opcional: reactivar la funcionabilidad del activo
                var activo = _context.Activos.FirstOrDefault(a => a.ActivoId == mantenimiento.ActivoId);
                if (activo != null)
                {
                    activo.Funcionabilidad = true;
                }
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Mantenimiento actualizado correctamente.";
            return RedirectToAction("ActivosDañados");
        }

        // ========================== CREACIÓN ==========================

        public IActionResult Create()
        {
            var usuarios = _context.Usuarios
                .Select(u => new UsuarioSelectDTO
                {
                    UsuarioId = u.UsuarioId,
                    NombreCompleto = u.UsuarioNombre + " " + u.UsuarioApellido
                }).ToList();

            usuarios.Insert(0, new UsuarioSelectDTO { UsuarioId = null, NombreCompleto = "-- Disponible --" });

            ViewData["Usuario"] = new SelectList(usuarios, "UsuarioId", "NombreCompleto");
            ViewData["Categoria"] = new SelectList(_context.Categoria, "CategoriaId", "CategoriaNombre");
            ViewData["Ubicacion"] = new SelectList(_context.Ubicacions, "UbicacionId", "UbicacionNombre");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> Create(ActivoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var activo = new Activo
                {
                    Marca = model.Marca,
                    Modelo = model.Modelo,
                    NumeroSerial = model.NumeroSerial,
                    Funcionabilidad = model.Funcionabilidad,
                    Observaciones = model.Observaciones,
                    CodigoInventario = model.CodigoInventario,
                    CategoriaId = model.CategoriaId,
                    UbicacionId = model.UbicacionId,
                    UsuarioId = model.UsuarioId,
                };
                try
                {
                    _context.Add(activo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "No se pudo guardar el activo. Intente nuevamente.");
                    Console.WriteLine(ex.Message);
                }
            }

            var usuarios = _context.Usuarios
                .Select(u => new UsuarioSelectDTO
                {
                    UsuarioId = u.UsuarioId,
                    NombreCompleto = u.UsuarioNombre + " " + u.UsuarioApellido
                }).ToList();

            usuarios.Insert(0, new UsuarioSelectDTO { UsuarioId = null, NombreCompleto = "-- No asignado --" });

            ViewData["Usuario"] = new SelectList(usuarios, "UsuarioId", "NombreCompleto", model.UsuarioId);
            ViewData["Categoria"] = new SelectList(_context.Categoria, "CategoriaId", "CategoriaNombre");
            ViewData["Ubicacion"] = new SelectList(_context.Ubicacions, "UbicacionId", "UbicacionNombre");

            return View(model);
        }

        // ========================== FILTROS, ORDEN Y BÚSQUEDA ==========================
        public async Task<IActionResult> FiltrarActivos(string categoria, string CodigoInventario, string ubicacion, string categoriamaster, int pagina = 1, int pageSize = 10)
        {
            var activos = _context.Activos
                .Include(a => a.Usuario)
                .Include(a => a.Categoria)
                .Include(a => a.Ubicacion)
                .AsQueryable();

            // Aplicar los filtros
            if (!string.IsNullOrEmpty(CodigoInventario))
                activos = activos.Where(a => a.CodigoInventario.Contains(CodigoInventario));

            if (!string.IsNullOrEmpty(categoria))
                activos = activos.Where(a => a.Categoria.CategoriaNombre.Contains(categoria));

            if (!string.IsNullOrEmpty(ubicacion))
                activos = activos.Where(a => a.Ubicacion.UbicacionNombre.Contains(ubicacion));

            if (!string.IsNullOrEmpty(categoriamaster))
            {
                var subcategorias = _context.Categoria
                    .Where(c => c.CategoriaMaster.Nombre == categoriamaster)
                    .Select(c => c.CategoriaNombre)
                    .ToList();

                activos = activos.Where(a => subcategorias.Contains(a.Categoria.CategoriaNombre));
            }

            
            var totalActivos = await activos.CountAsync();

            // Paginación: Aplicar Skip y Take para la paginación
            var listaActivos = await activos
                .OrderBy(a => a.ActivoId)  
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Crear el ViewModel
            var model = new ListaActivosViewModel
            {
                ListaActivos = listaActivos,
                TotalFiltrados = totalActivos,
                PaginaActual = pagina,
                PageSize = pageSize
            };
            Console.WriteLine($"Pagina actual {pagina}");
            return PartialView("_PaginacionActivosPartial", model);
        }


        [HttpGet]
        public async Task<IActionResult> OrdenarPorNombre(string sortOrder)
        {
            var activos = _context.Activos
                .Include(a => a.Usuario)
                .Include(a => a.Ubicacion)
                .Include(a => a.Categoria)
                .AsQueryable();

            activos = sortOrder == "asc"
                ? activos.OrderBy(a => a.Usuario.UsuarioNombre)
                : activos.OrderByDescending(a => a.Usuario.UsuarioNombre);

            var model = new ListaActivosViewModel
            {
                ListaActivos = await activos.ToListAsync()
            };

            return PartialView("_ListaActivosPartial", model);
        }

        public IActionResult ObtenerSubcategorias(string categoriaMaster)
        {
            if (string.IsNullOrEmpty(categoriaMaster))
                return Json(new List<object>());

            var subcategorias = _context.Categoria
                .Where(c => c.CategoriaMaster.Nombre == categoriaMaster)
                .Select(c => new { c.CategoriaNombre })
                .ToList();

            return Json(subcategorias);
        }

        // ========================== ACCIONES SOBRE ACTIVOS ==========================

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var activo = await _context.Activos.FindAsync(id);
                if (activo == null) return NotFound("El activo no fue encontrado.");

                _context.Activos.Remove(activo);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult ActualizarFuncionabilidad(int id, string Tecnico, string NumeroTecnico, decimal? Costo, string Descripcion)
        {
            var activo = _context.Activos
                .Include(a => a.Mantenimientos)
                .FirstOrDefault(a => a.ActivoId == id);

            if (activo == null)
            {
                TempData["ErrorMessage"] = "Activo no encontrado.";
                return RedirectToAction("Index");
            }

            activo.Funcionabilidad = !(activo.Funcionabilidad ?? false);

            if (!(activo.Funcionabilidad ?? false))
            {
                // Se marcó como dañado, registrar nuevo mantenimiento con datos del modal
                var nuevoMantenimiento = new Mantenimiento
                {
                    ActivoId = activo.ActivoId,
                    FechaInicio = DateOnly.FromDateTime(DateTime.Now),
                    Estado = "En proceso",
                    Tecnico = string.IsNullOrWhiteSpace(Tecnico) ? "Por asignar" : Tecnico,
                    NumeroTecnico = string.IsNullOrWhiteSpace(NumeroTecnico) ? "No registrado" : NumeroTecnico,
                    Costo = Costo ?? 0,
                    Descripcion = Descripcion
                };

                _context.Mantenimientos.Add(nuevoMantenimiento);
            }
            else
            {
                // Se marcó como reparado, finalizar mantenimiento
                var mantenimientoEnProceso = activo.Mantenimientos
                    .Where(m => m.Estado == "En proceso")
                    .OrderByDescending(m => m.FechaInicio)
                    .FirstOrDefault();

                if (mantenimientoEnProceso != null)
                {
                    mantenimientoEnProceso.Estado = "Finalizado";
                    mantenimientoEnProceso.FechaFin = DateOnly.FromDateTime(DateTime.Now);
                }
            }

            try
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "El estado del activo y el mantenimiento se actualizaron correctamente.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al intentar actualizar el activo.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ReasignarActivo(int id, int usuarioId)
        {
            var activo = _context.Activos
                .Include(a => a.Usuario)
                .FirstOrDefault(a => a.ActivoId == id);

            if (activo == null) return NotFound();

            var usuarioAnteriorId = activo.UsuarioId;
            activo.UsuarioId = usuarioId;

            try
            {
                var cantidadMantenimientos = _context.Mantenimientos
                    .Count(m => m.ActivoId == id);

                var movimiento = new Movimiento
                {
                    ActivoId = id,
                    FechaMovimiento = DateTime.Now,
                    UsuarioAnteriorId = usuarioAnteriorId,
                    UsuarioNuevoId = usuarioId,
                    UbicacionAnteriorId = activo.UbicacionId,
                    UbicacionNuevaId = activo.UbicacionId, // no cambia en este caso
                    CantidadMantenimientos = cantidadMantenimientos,
                    Observaciones = $"Reasignación del usuario responsable del activo {id}"
                };

                _context.Movimientos.Add(movimiento);
                _context.SaveChanges();
            }
            catch
            {
                TempData["ErrorMessage"] = "Error al reasignar el activo.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RelocalizarActivo(int id, int ubicacionId)
        {
            var activo = _context.Activos
                .Include(a => a.Ubicacion)
                .FirstOrDefault(a => a.ActivoId == id);
            if (activo == null) return RedirectToAction("Index");

            var ubicacionAnteriorId = activo.UbicacionId;

            activo.UbicacionId = ubicacionId;

            try
            {
                var cantidadMantenimientos = _context.Mantenimientos
                    .Count(m => m.ActivoId == id);

                var movimiento = new Movimiento
                {
                    ActivoId = id,
                    FechaMovimiento = DateTime.Now,
                    UbicacionAnteriorId = ubicacionAnteriorId,
                    UbicacionNuevaId = ubicacionId,
                    CantidadMantenimientos = cantidadMantenimientos,
                    Observaciones = $"Cambio de ubicación del activo #{id}"
                };

                _context.Movimientos.Add(movimiento);
                _context.SaveChanges();
            }
            catch
            {
                TempData["ErrorMessage"] = "Error al relocalizar el activo.";
            }

            return RedirectToAction("Index");
        }
    }

    // ========================== DTOs ==========================

    public class UsuarioSelectDTO
    {
        public int? UsuarioId { get; set; }
        public string NombreCompleto { get; set; }
    }
}
