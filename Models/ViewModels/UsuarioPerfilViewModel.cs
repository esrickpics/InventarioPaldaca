using InventarioPaldaca.Models.Inventario;
using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class UsuarioPerfilViewModel
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string UsuarioNombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string UsuarioApellido { get; set; }
       
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string UsuarioCorreo { get; set; }

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        public string UsuarioTelefono { get; set; }

        public string UsuarioCargo { get; set; }

        public string UsuarioImagenUrl { get; set; }

        public List<ActivosAsociadosViewModel> ActivosAsociados { get; set; } = new List<ActivosAsociadosViewModel>();
        // Datos para la búsqueda
        public string SearchTerm { get; set; }
        public List<Usuario> UsuariosEncontrados { get; set; } = new List<Usuario>();
    }
}
