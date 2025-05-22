using InventarioPaldaca.Models.Inventario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventarioPaldaca.Utilidades.Filters
{
    public class VerificarSession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var usuarioId = session.GetString("UsuarioId");
            var path = context.HttpContext.Request.Path.Value?.ToLower();

            Console.WriteLine($"Ruta solicitada: {path}, UsuarioId: {usuarioId}");

            // Rutas que no requieren sesión activa
            // 🔧 Si necesitas permitir acceso sin sesión a nuevas rutas, agrégalas aquí:
            var rutasPermitidas = new[]
            {
                "/",
                "/acceso/login",
                "/acceso/registro",
                "/acceso/restablecercontrasena",
                "/acceso/restablecerpassword",
                "/acceso/confirmarsolicitudrestablecimiento"
            };

            if (rutasPermitidas.Contains(path))
            {
                base.OnActionExecuting(context);
                return;
            }

            // Redirigir al login si no hay usuario en sesión
            if (string.IsNullOrEmpty(usuarioId))
            {
                context.Result = new RedirectToActionResult("Login", "Acceso", null);
            }

            base.OnActionExecuting(context);
        }
    }
}


