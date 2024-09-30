using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class UbicacionViewModel
    {
        [Required(ErrorMessage = "El nombre de la ubicación es obligatorio.")]
        [Display(Name = "Nombre de la Ubicación")]
        public string UbicacionNombre { get; set; } = null!;

        [Required(ErrorMessage = "La dirección de la ubicación es obligatoria.")]
        [Display(Name = "Dirección de la Ubicación")]
        public string UbicacionDireccion { get; set; } = null!;

        [Display(Name = "Descripción de la Ubicación")]
        public string? UbicacionDescripcion { get; set; }
    }
}
