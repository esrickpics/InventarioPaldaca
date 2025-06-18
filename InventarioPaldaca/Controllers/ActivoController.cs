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
using InventarioPaldaca.Utilidades;
using System;
namespace InventarioPaldaca.Controllers
{
    public class ActivoController : Controller
    {
        private readonly InventarioPaldacaContext _context;
        private readonly FiltroActivosService _filtroActivosService;

        public ActivoController(InventarioPaldacaContext context, FiltroActivosService filtroActivosService)
        {
            _context = context;
            _filtroActivosService = filtroActivosService;
        }

        // ========================== VISTAS PRINCIPALES ==========================

        [AuthorizeRole("Administrador", "AdministradorProyecto")]
        public async Task<IActionResult> Index(int pagina = 1, int pageSize = 10)
        {
            var rolUsuario = HttpContext.Session.GetString("UsuarioRol");
            var usuarioIdString = HttpContext.Session.GetString("UsuarioId");
            
            if (!int.TryParse(usuarioIdString, out int usuarioId))
            {
                // Si no se puede obtener o convertir, redirige al login o maneja el error
                return RedirectToAction("Login", "Acceso");
            }


            var usuarios = _context.Usuarios
                .Select(u => new SelectListItem
                {
                    Value = u.UsuarioId.ToString(),
                    Text = u.UsuarioNombre + " " + u.UsuarioApellido
                }).ToList();

            usuarios.Insert(0, new SelectListItem
            {
                Value = "", // o "0" si usas un valor entero especial
                Text = "-- Disponible --"
            });

            ViewBag.Usuarios = usuarios;

            ViewBag.Proyectos = _context.Proyectos
                .Select(p => new SelectListItem
                {
                    Value = p.ProyectoId.ToString(),
                    Text = p.Nombre ?? "-- Sin Proyecto --"
                }).ToList();

            // Consulta base para activos con includes necesarios
            var query = _context.Activos
                .Include(a => a.Usuario)
                .Include(a => a.Ubicacion)
                .Include(a => a.Categoria)
                    .ThenInclude(c => c.CategoriaMaster)
                .AsQueryable();

            List<int> proyectosDelUsuario = null;

            // Si el usuario es AdministradorProyecto (rol "3" o string "AdministradorProyecto"), filtrar por sus proyectos
            if (rolUsuario == "3" || string.Equals(rolUsuario, "AdministradorProyecto", StringComparison.OrdinalIgnoreCase))
            {
                proyectosDelUsuario = _context.ProyectoAdministradors
                    .Where(pa => pa.UsuarioId == usuarioId)
                    .Select(pa => pa.ProyectoId)
                    .ToList();

                query = query.Where(a => a.ProyectoId != null && proyectosDelUsuario.Contains(a.ProyectoId.Value));
            }

            int totalActivos = await query.CountAsync();

            var activosPaginados = await query
                .OrderBy(a => a.ActivoId)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Traemos solo los activos completos filtrados por proyectos si aplica
            var listaActivosCompletaQuery = _context.Activos
                .Include(a => a.Categoria)
                .Include(a => a.Usuario)
                .AsQueryable();

            if (proyectosDelUsuario != null)
            {
                listaActivosCompletaQuery = listaActivosCompletaQuery
                    .Where(a => a.ProyectoId != null && proyectosDelUsuario.Contains(a.ProyectoId.Value));
            }

            var listaActivosCompleta = await listaActivosCompletaQuery.ToListAsync();

            // Activos disponibles para reasignar, también filtrados si aplica
            IQueryable<Activo> activosDisponiblesQuery = _context.Activos
                .Include(a => a.Categoria)
                .Include(a => a.Usuario)
                .Where(a => a.Funcionabilidad == true || a.Funcionabilidad == null);

            if (proyectosDelUsuario != null)
            {
                activosDisponiblesQuery = activosDisponiblesQuery
                    .Where(a => a.ProyectoId != null && proyectosDelUsuario.Contains(a.ProyectoId.Value));
            }

            var model = new ListaActivosViewModel
            {
                ListaActivos = activosPaginados,
                Categorias = await _context.Categoria.ToListAsync(),
                Ubicaciones = await _context.Ubicacions.ToListAsync(),
                CategoriasMaster = await _context.CategoriaMasters.ToListAsync(),
                Proyectos = await _context.Proyectos.ToListAsync(),
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
                    .ToDictionaryAsync(g => g.Key, g => g.Count),
                ActivosDisponiblesParaReasignar = await activosDisponiblesQuery.ToListAsync()
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
        [HttpGet]
        [AuthorizeRole("Administrador", "AdministradorProyecto")]
        public IActionResult Create()
        {
            try
            {
                var rolUsuario = HttpContext.Session.GetString("UsuarioRol");
                var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
                int.TryParse(usuarioIdStr, out int usuarioId);

                // Cargar usuarios
                var usuarios = _context.Usuarios
                    .Select(u => new UsuarioSelectDTO
                    {
                        UsuarioId = u.UsuarioId,
                        NombreCompleto = (u.UsuarioNombre ?? "") + " " + (u.UsuarioApellido ?? "")
                    }).ToList();

                usuarios.Insert(0, new UsuarioSelectDTO { UsuarioId = null, NombreCompleto = "-- Disponible --" });

                // Cargar proyectos filtrados
                List<ProyectoSelectDTO> proyectos;

                if (rolUsuario == "3" || rolUsuario == "AdministradorProyecto")
                {
                    proyectos = _context.ProyectoAdministradors
                        .Where(pa => pa.UsuarioId == usuarioId)
                        .Select(pa => new ProyectoSelectDTO
                        {
                            ProyectoId = pa.Proyecto.ProyectoId,
                            Nombre = pa.Proyecto.Nombre ?? "-- Proyecto sin nombre --"
                        }).ToList();
                }
                else
                {
                    proyectos = _context.Proyectos
                        .Select(p => new ProyectoSelectDTO
                        {
                            ProyectoId = p.ProyectoId,
                            Nombre = p.Nombre ?? "-- Proyecto sin nombre --"
                        }).ToList();
                }

                proyectos.Insert(0, new ProyectoSelectDTO { ProyectoId = null, Nombre = "-- Sin Proyecto --" });

                // Categorías y ubicaciones
                var categorias = _context.Categoria
                    .Where(c => c.CategoriaNombre != null)
                    .ToList();

                var ubicaciones = _context.Ubicacions
                    .Where(u => u.UbicacionNombre != null)
                    .ToList();

                ViewData["Usuario"] = new SelectList(usuarios, "UsuarioId", "NombreCompleto");
                ViewData["Proyecto"] = new SelectList(proyectos, "ProyectoId", "Nombre");
                ViewData["Categoria"] = new SelectList(categorias, "CategoriaId", "CategoriaNombre");
                ViewData["Ubicacion"] = new SelectList(ubicaciones, "UbicacionId", "UbicacionNombre");

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR EN EL MÉTODO GET CREATE: " + ex.Message);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "AdministradorProyecto")]
        public async Task<IActionResult> Create(ActivoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var rolUsuario = HttpContext.Session.GetString("UsuarioRol");
                var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
                int.TryParse(usuarioIdStr, out int usuarioId);

                // Si es administrador de proyecto, verificar que solo pueda asignar proyectos propios
                if ((rolUsuario == "3" || rolUsuario == "AdministradorProyecto") && model.ProyectoId != null)
                {
                    var proyectoEsValido = _context.ProyectoAdministradors
                        .Any(pa => pa.UsuarioId == usuarioId && pa.ProyectoId == model.ProyectoId);

                    if (!proyectoEsValido)
                    {
                        ModelState.AddModelError("ProyectoId", "No puedes asignar este proyecto.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var activo = new Activo
                    {
                        Marca = model.Marca,
                        Modelo = model.Modelo,
                        NumeroSerial = model.NumeroSerial,
                        Funcionabilidad = true,
                        Observaciones = model.Observaciones,
                        CodigoInventario = model.CodigoInventario,
                        CategoriaId = model.CategoriaId,
                        UbicacionId = model.UbicacionId,
                        UsuarioId = model.UsuarioId,
                        ProyectoId = model.ProyectoId
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
            }
            var usuarios = _context.Usuarios
                .Select(u => new UsuarioSelectDTO
                {
                    UsuarioId = u.UsuarioId,
                    NombreCompleto = (u.UsuarioNombre ?? "") + " " + (u.UsuarioApellido ?? "")
                })
                .OrderBy(u => u.NombreCompleto)
                .ToList();

            usuarios.Insert(0, new UsuarioSelectDTO { UsuarioId = null, NombreCompleto = "-- Disponible --" });


            var rolUsuarioRecarga = HttpContext.Session.GetString("UsuarioRol");
            var usuarioIdStrRecarga = HttpContext.Session.GetString("UsuarioId");
            int.TryParse(usuarioIdStrRecarga, out int usuarioIdRecarga);

            List<ProyectoSelectDTO> proyectosRecarga;
            if (rolUsuarioRecarga == "3" || rolUsuarioRecarga == "AdministradorProyecto")
            {
                proyectosRecarga = _context.ProyectoAdministradors
                    .Where(pa => pa.UsuarioId == usuarioIdRecarga)
                    .Select(pa => new ProyectoSelectDTO
                    {
                        ProyectoId = pa.Proyecto.ProyectoId,
                        Nombre = pa.Proyecto.Nombre ?? "-- Proyecto sin nombre --"
                    }).ToList();
            }
            else
            {
                proyectosRecarga = _context.Proyectos
                    .Select(p => new ProyectoSelectDTO
                    {
                        ProyectoId = p.ProyectoId,
                        Nombre = p.Nombre ?? "-- Proyecto sin nombre --"
                    }).ToList();
            }
            proyectosRecarga.Insert(0, new ProyectoSelectDTO { ProyectoId = null, Nombre = "-- Sin Proyecto --" });

            ViewData["Usuario"] = new SelectList(usuarios, "UsuarioId", "NombreCompleto", model.UsuarioId);
            ViewData["Proyecto"] = new SelectList(proyectosRecarga, "ProyectoId", "Nombre", model.ProyectoId);
            ViewData["Categoria"] = new SelectList(_context.Categoria, "CategoriaId", "CategoriaNombre", model.CategoriaId);
            ViewData["Ubicacion"] = new SelectList(_context.Ubicacions, "UbicacionId", "UbicacionNombre", model.UbicacionId);

            return View(model);
        }


        // ========================== FILTROS, ORDEN Y BÚSQUEDA ==========================
        public async Task<IActionResult> FiltrarActivos(string categoria, string CodigoInventario, string ubicacion, string categoriamaster,int? proyectoId,int pagina = 1, int pageSize = 10)
        {
       
            var activos = _filtroActivosService.ObtenerActivosFiltrados();

            // 🔍 Filtros adicionales
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

            if (proyectoId.HasValue && proyectoId.Value != 0)
            {
                activos = activos.Where(a => a.ProyectoId == proyectoId.Value);
            }

            // 📦 Paginación
            var totalActivos = await activos.CountAsync();

            var listaActivos = await activos
                .OrderBy(a => a.ActivoId)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 📊 ViewModel parcial
            var model = new ListaActivosViewModel
            {
                ListaActivos = listaActivos,
                TotalFiltrados = totalActivos,
                PaginaActual = pagina,
                PageSize = pageSize
            };

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
        public async Task<IActionResult> EditarActivo(int ActivoId, string Marca, string Modelo, string NumeroSerial, string CodigoInventario)
        {
            var activo = await _context.Activos.FindAsync(ActivoId);
            if (activo == null)
            {
                return NotFound();
            }

            // Actualizamos los campos permitidos
            activo.Marca = Marca;
            activo.Modelo = Modelo;
            activo.NumeroSerial = NumeroSerial;
            activo.CodigoInventario = CodigoInventario;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Activo editado correctamente.";
            }
            catch (DbUpdateException ex)
            {
                // Manejo básico de errores
                ModelState.AddModelError("", "Error al guardar los cambios: " + ex.Message);
            }

            return RedirectToAction("Index"); // O donde estés listando los activos
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
        public IActionResult ReasignarActivo(int id)
        {
            var activo = _filtroActivosService.ObtenerActivosFiltrados()
                .Include(a => a.Usuario)
                .Include(a => a.Proyecto)
                .FirstOrDefault(a => a.ActivoId == id);

            if (activo == null) return NotFound();

            var form = Request.Form;
            var usuarioIdStr = form["usuarioId"];
            var proyectoIdStr = form["proyectoId"];

            var usuarioAnteriorId = activo.UsuarioId;
            var proyectoAnteriorId = activo.ProyectoId;

            bool huboCambio = false;

            if (usuarioIdStr != "default")
            {
                int? nuevoUsuarioId = string.IsNullOrEmpty(usuarioIdStr) ? null : int.Parse(usuarioIdStr);
                if (nuevoUsuarioId != activo.UsuarioId)
                {
                    activo.UsuarioId = nuevoUsuarioId;
                    huboCambio = true;
                }
            }

            if (proyectoIdStr != "default")
            {
                int? nuevoProyectoId = string.IsNullOrEmpty(proyectoIdStr) ? null : int.Parse(proyectoIdStr);
                if (nuevoProyectoId != activo.ProyectoId)
                {
                    activo.ProyectoId = nuevoProyectoId;
                    huboCambio = true;
                }
            }

            if (huboCambio)
            {
                try
                {
                    var cantidadMantenimientos = _context.Mantenimientos
                        .Count(m => m.ActivoId == id);

                    var movimiento = new Movimiento
                    {
                        ActivoId = id,
                        FechaMovimiento = DateTime.Now,
                        UsuarioAnteriorId = usuarioAnteriorId,
                        UsuarioNuevoId = activo.UsuarioId,
                        ProyectoAnteriorId = proyectoAnteriorId,
                        ProyectoNuevoId = activo.ProyectoId,
                        UbicacionAnteriorId = activo.UbicacionId,
                        UbicacionNuevaId = activo.UbicacionId,
                        CantidadMantenimientos = cantidadMantenimientos,
                        Observaciones = $"Reasignación del activo {activo.CodigoInventario}."
                    };

                    _context.Movimientos.Add(movimiento);
                    _context.SaveChanges();
                    TempData["Success"] = "Reasignación realizada correctamente.";
                }
                catch
                {
                    TempData["ErrorMessage"] = "Error al reasignar el activo.";
                }
            }
            else
            {
                TempData["InfoMessage"] = "No se realizaron cambios.";
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult RelocalizarActivo(int id, int ubicacionId)
        {
            var activo = _filtroActivosService.ObtenerActivosFiltrados()
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
                    Observaciones = $"Cambio de ubicación del activo de código {activo.CodigoInventario}"
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


        // ========================== DTOs ==========================

        public class UsuarioSelectDTO
        {
            public int? UsuarioId { get; set; }
            public string NombreCompleto { get; set; }
        }

        public class ProyectoSelectDTO
        {
            public int? ProyectoId { get; set; }
            public string Nombre { get; set; }
        }
    }
}
