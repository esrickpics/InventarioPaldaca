using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class RestablecerPasswordViewModel
    {
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y su confirmación no coinciden.")]
        public string ConfirmarPassword { get; set; }

        // Podrías incluir el ID del usuario si no estás usando sesión
        public int UsuarioId { get; set; }
    }
}

