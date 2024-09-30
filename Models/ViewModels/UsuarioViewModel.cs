using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class UsuarioViewModel
    {

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")] 
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Display(Name = "Telefono Celular")]
        public string Telefono { get; set; }
        [Display(Name = "Cargo")]
        public string? Cargo { get; set; }

   
    }
}
