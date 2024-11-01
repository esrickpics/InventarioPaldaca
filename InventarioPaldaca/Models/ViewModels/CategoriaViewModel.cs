using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class CategoriaViewModel
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [Display(Name = "Nombre de la Categoría")]
        public string CategoriaNombre { get; set; } = null!;

        [Display(Name = "Descripción de la Categoría")]
        public string? CategoriaDescripcion { get; set; }
    }
}
