using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels

{
    public class CategoriaMasterViewModel
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        public string Nombre { get; set; } = null!;
    }

}
