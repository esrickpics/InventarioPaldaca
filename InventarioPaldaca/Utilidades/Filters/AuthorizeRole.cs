using InventarioPaldaca.Models.Inventario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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
            var rolUsuario = context.HttpContext.Session.GetString("UsuarioRol");

            if (string.IsNullOrEmpty(rolUsuario) || !_rolesPermitidos.Contains(rolUsuario))
            {
                // Redirigir al login o prohibir acceso
                context.Result = new RedirectToActionResult("Login", "Acceso", null);
            }
        }
    }

}
