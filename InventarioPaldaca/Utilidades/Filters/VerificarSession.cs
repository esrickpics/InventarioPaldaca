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

            var rutasPermitidas = new[] { "/", "/acceso/login", "/acceso/registro" };

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

