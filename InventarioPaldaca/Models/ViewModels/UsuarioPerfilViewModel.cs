using InventarioPaldaca.Models.Inventario;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Policy;

namespace InventarioPaldaca.Models.ViewModels
{
    public class UsuarioPerfilViewModel
    {
        // Identificador único del usuario
        public int UsuarioId { get; set; }

        // Nombre del usuario (obligatorio)
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string UsuarioNombre { get; set; }

        // Apellido del usuario (obligatorio)
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string UsuarioApellido { get; set; }

        // Correo electrónico (opcional, con validación de formato)
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string? UsuarioCorreo { get; set; }

        // Teléfono del usuario (opcional, con validación de formato)
        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        public string? UsuarioTelefono { get; set; }

        // Cargo del usuario (opcional, con valor predeterminado)
        public string? UsuarioCargo { get; set; } = string.Empty;

        // URL de la imagen del usuario (opcional, con valor predeterminado)
        public string? UsuarioImagenUrl { get; set; } = "~/img/Usuarios/Img.default.png";

        // Ruta del archivo PDF asociado (opcional)
        public string? AsignacionPdf { get; set; }

        // Archivo PDF cargado (opcional)
        public IFormFile? PdfFile { get; set; }

        // Lista de activos asociados al usuario
        public List<ActivosAsociadosViewModel> ActivosAsociados { get; set; } = new List<ActivosAsociadosViewModel>();

        // Término de búsqueda (opcional)
        public string? SearchTerm { get; set; }

        // Lista de usuarios encontrados por el término de búsqueda
        public List<Usuario> UsuariosEncontrados { get; set; } = new List<Usuario>();
    }
}
