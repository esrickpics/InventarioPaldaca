using InventarioPaldaca.Models.Inventario;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventarioPaldaca.Utilidades.Filters
{
    public class FiltroActivosService
    {
        private readonly InventarioPaldacaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FiltroActivosService(InventarioPaldacaContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IQueryable<Activo> ObtenerActivosFiltrados()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var rol = httpContext?.Session.GetString("UsuarioRol");
            var usuarioIdStr = httpContext?.Session.GetString("UsuarioId");

            var query = _context.Activos
                .Include(a => a.Categoria)
                .Include(a => a.Ubicacion)
                .Include(a => a.Proyecto)
                .Include(a => a.Usuario)
                .AsQueryable();

            if (rol == "3" || rol == "AdministradorProyecto")
            {
                if (int.TryParse(usuarioIdStr, out int usuarioId))
                {
                    var proyectos = _context.ProyectoAdministradors
                        .Where(pa => pa.UsuarioId == usuarioId)
                        .Select(pa => pa.ProyectoId)
                        .ToList();

                    query = query.Where(a => a.ProyectoId != null && proyectos.Contains(a.ProyectoId.Value));
                }
            }

            return query;
        }
    }
}
