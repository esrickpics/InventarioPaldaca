using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class EditarCategoriaViewModel
    {
        [Required]
        public int CategoriaSeleccionadaId { get; set; }

        public List<SelectListItem> CategoriasDisponibles { get; set; } = new();

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string CategoriaNombre { get; set; } = null!;

        public string? CategoriaDescripcion { get; set; }
    }
}
