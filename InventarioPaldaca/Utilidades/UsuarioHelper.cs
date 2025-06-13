using InventarioPaldaca.Models.Inventario;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventarioPaldaca.Utilidades
{
    public class UsuarioHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly InventarioPaldacaContext _context;

        public UsuarioHelper(IHttpContextAccessor httpContextAccessor, InventarioPaldacaContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Usuario? ObtenerUsuarioActual()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdString, out int userId))
            {
                return _context.Usuarios
                    .Include(u => u.ProyectoAdministradors)
                        .ThenInclude(pa => pa.Proyecto)
                    .FirstOrDefault(u => u.UsuarioId == userId);
            }

            return null;
        }

        public List<int> ObtenerProyectoIdsDelUsuario()
        {
            var usuario = ObtenerUsuarioActual();
            return usuario?.ProyectoAdministradors.Select(pa => pa.ProyectoId).ToList() ?? new List<int>();
        }
    }

}
