using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class EditarUbicacionViewModel
    {
        [Required]
        public int UbicacionSeleccionadaId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string UbicacionNombre { get; set; }
        public string? UbicacionDireccion { get; set; }
        public string? UbicacionDescripcion { get; set; }

        public List<SelectListItem> UbicacionesDisponibles { get; set; } = new List<SelectListItem>();
    }
}
