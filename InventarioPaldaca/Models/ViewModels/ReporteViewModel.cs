using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ReporteViewModel
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; }
        public int? ActivoIdSeleccionado { get; set; }
        public List<SelectListItem> ActivosAsociados { get; set; } = new();
    }
}

