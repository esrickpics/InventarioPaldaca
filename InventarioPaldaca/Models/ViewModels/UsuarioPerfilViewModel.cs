using InventarioPaldaca.Models.Inventario;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Policy;

namespace InventarioPaldaca.Models.ViewModels
{
    public class UsuarioPerfilViewModel
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string UsuarioNombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string UsuarioApellido { get; set; }
        [AllowNull]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string UsuarioCorreo { get; set; } // No es obligatorio ahora

        [AllowNull]
        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        public string UsuarioTelefono { get; set; } // No es obligatorio ahora

        public string UsuarioCargo { get; set; } = string.Empty;

        // Imagen no obligatoria, con un valor predeterminado
        public string UsuarioImagenUrl { get; set; } = "~/img/Usuarios/Img.default.png";
        [AllowNull]
        public string AsignacionPdf { get; set; }

        public IFormFile PdfFile { get; set; }

        public List<ActivosAsociadosViewModel> ActivosAsociados { get; set; } = new List<ActivosAsociadosViewModel>();

        // Datos para la búsqueda
        public string SearchTerm { get; set; }
        public List<Usuario> UsuariosEncontrados { get; set; } = new List<Usuario>();
    }
}

