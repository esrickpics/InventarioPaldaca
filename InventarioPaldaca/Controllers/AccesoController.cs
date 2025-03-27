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

        // Vista del Login
        public IActionResult Login()
        {
            return View();
        }

        // Procesar Login
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {

            if (model.Password.IsNullOrEmpty())
            {
                ViewBag.Error = "ingrese la contraseña";
                return View();
            }

            if (model.Email.IsNullOrEmpty())
            {
                ViewBag.Error = "Ingrese su correo";
                return View();
            }
            // Encriptar la clave ingresada
            string claveEncriptada = Encrypt.GetSHA256(model.Password);

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.UsuarioEmail == model.Email && u.UsuarioPassword == claveEncriptada);

            if (usuario != null)
            {
                HttpContext.Session.Clear();

                HttpContext.Session.SetString("UsuarioId", usuario.UsuarioId.ToString());
                HttpContext.Session.SetString("UsuarioNombre", usuario.UsuarioNombre);
                HttpContext.Session.SetString("UsuarioRol", usuario.RolId.ToString());
                return RedirectToAction("Index", "Home"); // Redirigir al panel principal
            }
            // Autenticación fallida
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
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
                Console.WriteLine("Validación del modelo exitosa.");

                if (_context.Usuarios.Any(u => u.UsuarioEmail == model.Email))
                {
                    Console.WriteLine("El correo ya está registrado.");
                    ModelState.AddModelError(string.Empty, "El correo ya está registrado.");
                    return View("Registro", model);
                }

                if (model.Password != model.ConfirmarPassword)
                {
                    Console.WriteLine("Las contraseñas no coinciden.");
                    ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                    return View("Registro", model);
                }

                var rolUsuario = _context.Rols.FirstOrDefault(r => r.RolNombre == "Usuario");


                if (rolUsuario == null)
                {
                    Console.WriteLine("Error: El rol 'Usuario' no está configurado en la base de datos.");
                    ModelState.AddModelError(string.Empty, "El rol 'Usuario' no está configurado en el sistema.");
                    return View("Registro", model);
                }

                var nuevoUsuario = new Usuario
                {
                    UsuarioNombre = model.Nombre,
                    UsuarioApellido = model.Apellido,
                    UsuarioEmail = model.Email,
                    UsuarioPassword = Encrypt.GetSHA256(model.Password),
                    UsuarioRol = rolUsuario
                };

                Console.WriteLine("Intentando guardar usuario en la base de datos.");
                try
                {
                    _context.Usuarios.Add(nuevoUsuario);
                    _context.SaveChanges();
                    Console.WriteLine("Usuario registrado correctamente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al guardar en la base de datos: {ex.Message}");
                    throw;
                }

                return RedirectToAction("Login");
            }
            Console.WriteLine("Validación del modelo fallida.");
            return View("Registro", model);
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
