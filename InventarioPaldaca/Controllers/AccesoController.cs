using Microsoft.AspNetCore.Mvc;
using InventarioPaldaca.Models;
using InventarioPaldaca.Models.Inventario;
using InventarioPaldaca.Utilidades;
using InventarioPaldaca.Models.ViewModels;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;

namespace InventarioPaldaca.Controllers
{
    public class AccesoController : Controller
    {
        private readonly InventarioPaldacaContext _context;

        public AccesoController(InventarioPaldacaContext context)
        {
            _context = context;
        }
            public IActionResult Login()
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.Error = TempData["Error"];
                return View();
            }

            [HttpPost]
            public IActionResult Login(LoginViewModel model)
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    ViewBag.Error = "Ingrese la contraseña";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    ViewBag.Error = "Ingrese su correo";
                    return View(model);
                }

                string claveEncriptada = Encrypt.GetSHA256(model.Password);

                var usuario = _context.Usuarios
                    .FirstOrDefault(u => u.UsuarioEmail == model.Email && u.UsuarioPassword == claveEncriptada);

                if (usuario != null)
                {
                    HttpContext.Session.Clear();
                    HttpContext.Session.SetString("UsuarioId", usuario.UsuarioId.ToString());
                    HttpContext.Session.SetString("UsuarioNombre", usuario.UsuarioNombre);
                    HttpContext.Session.SetString("UsuarioRol", usuario.RolId.ToString());
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View(model);
            }

            public IActionResult Registro()
            {
                return View();
            }

            [HttpPost]
            public IActionResult Registro(RegistroViewModel model)
            {
                if (ModelState.IsValid)
                {
                    if (_context.Usuarios.Any(u => u.UsuarioEmail == model.Email))
                    {
                        ModelState.AddModelError(string.Empty, "El correo ya está registrado.");
                        return View("Registro", model);
                    }

                    if (model.Password != model.ConfirmarPassword)
                    {
                        ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                        return View("Registro", model);
                    }

                    var rolUsuario = _context.Rols.FirstOrDefault(r => r.RolNombre == "Usuario");

                    if (rolUsuario == null)
                    {
                        ModelState.AddModelError(string.Empty, "El rol 'Usuario' no está configurado en el sistema.");
                        return View("Registro", model);
                    }

                    var nuevoUsuario = new Usuario
                    {
                        UsuarioNombre = model.Nombre,
                        UsuarioApellido = model.Apellido,
                        UsuarioEmail = model.Email,
                        UsuarioPassword = Encrypt.GetSHA256(model.Password),
                        RolId = rolUsuario.RolId,
                    };

                    _context.Usuarios.Add(nuevoUsuario);
                    _context.SaveChanges();

                    return RedirectToAction("Login");
                }

                return View("Registro", model);
            }

            public IActionResult AccesoDenegado()
            {
                return View();
            }

            public IActionResult Logout()
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Acceso");
            }

        // ✔️ Acción de restablecimiento: verifica y marca la solicitud
        [HttpGet]
        public IActionResult RestablecerContrasena(string email)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioEmail == email);

            if (usuario == null)
            {
                TempData["Error"] = "El correo no está registrado.";
                return RedirectToAction("Login");
            }

            if (usuario.PuedeRestablecer)
            {
                // Ya está autorizado -> redirigir al formulario para cambiar la clave
                return RedirectToAction("RestablecerPassword", new { usuarioId = usuario.UsuarioId });
            }

            if (usuario.SolicitoRestablecer)
            {
                Console.WriteLine("Ya solicitaste un restablecimiento. Espera autorización del administrador.");
                TempData["Error"] = "Ya solicitaste un restablecimiento. Espera autorización del administrador.";
                return RedirectToAction("Login");
            }

            // Primera solicitud: marcar como solicitada
            usuario.SolicitoRestablecer = true;
            _context.Update(usuario);
            _context.SaveChanges();

            return View("ConfirmarSolicitudRestablecimiento", usuario);
        }


        [HttpGet]
        public IActionResult RestablecerPassword(int usuarioId)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == usuarioId);
            if (usuario == null || !usuario.PuedeRestablecer)
            {
                TempData["Error"] = "No tienes autorización para restablecer la contraseña.";
                return RedirectToAction("Login");
            }

            var model = new RestablecerPasswordViewModel { UsuarioId = usuarioId };
            return View(model);
        }

        [HttpPost]
        public IActionResult RestablecerPassword(RestablecerPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == model.UsuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.UsuarioPassword = Encrypt.GetSHA256(model.Password);
            usuario.SolicitoRestablecer = false;
            usuario.PuedeRestablecer = false; // 🔧 Se desactiva la posibilidad de restablecer

            _context.SaveChanges();

            TempData["Mensaje"] = "Tu contraseña fue restablecida con éxito.";
            return RedirectToAction("Login");
        }

        // 👇 Método de ayuda (puedes eliminarlo si ya no lo usas)
        private bool PuedeRestablecer(string email, out Usuario usuario)
            {
                usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioEmail == email);
                return usuario != null && usuario.SolicitoRestablecer && usuario.PuedeRestablecer;
            }
        }
}
