using InventarioPaldaca.Models.Inventario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace InventarioPaldaca.Utilidades.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeRole : Attribute, IAuthorizationFilter
    {
        private readonly string[] _rolesPermitidos;

        public AuthorizeRole(params string[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Se obtiene el valor de la sesión; este valor puede ser el nombre del rol o el id ("Administrador" o "2", "Usuario" o "1")
            var rolUsuario = context.HttpContext.Session.GetString("UsuarioRol");

            if (string.IsNullOrEmpty(rolUsuario))
            {
                Console.WriteLine("Acceso denegado. No hay rol en sesión.");
                context.Result = new RedirectToActionResult("Login", "Acceso", null);
                return;
            }

            bool autorizado = false;

            // Se recorre cada rol permitido definido en el atributo
            foreach (var rolPermitido in _rolesPermitidos)
            {
                // Para el rol Administrador: se permite si el valor de sesión es "Administrador" o "2"
                if (rolPermitido.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    if (rolUsuario.Equals("Administrador", StringComparison.OrdinalIgnoreCase) || rolUsuario.Equals("2"))
                    {
                        autorizado = true;
                        break;
                    }
                }
                // Para el rol Usuario: se permite si el valor de sesión es "Usuario" o "1"
                else if (rolPermitido.Equals("Usuario", StringComparison.OrdinalIgnoreCase))
                {
                    if (rolUsuario.Equals("Usuario", StringComparison.OrdinalIgnoreCase) || rolUsuario.Equals("1"))
                    {
                        autorizado = true;
                        break;
                    }
                }
                // Si se usan otros valores, se hace comparación directa
                else if (rolUsuario.Equals(rolPermitido, StringComparison.OrdinalIgnoreCase))
                {
                    autorizado = true;
                    break;
                }
            }

            if (!autorizado)
            {
                Console.WriteLine("Acceso denegado. El rol de usuario no está permitido.");
                context.Result = new RedirectToActionResult("Login", "Acceso", null);
            }
        }
    }
}

