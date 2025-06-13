using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ProyectoViewModel : IValidatableObject
    {
        public int ProyectoId { get; set; } // Necesario para la edición

        [Required]
        [Display(Name = "Nombre del Proyecto")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateOnly? FechaInicio { get; set; }

        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateOnly? FechaFin { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un administrador de proyecto")]
        [Display(Name = "Administrador del Proyecto")]
        public int? UsuarioId { get; set; }

        public List<SelectListItem>? UsuariosDisponibles { get; set; }

        // ✅ Validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin.HasValue && FechaInicio.HasValue && FechaInicio > FechaFin)
            {
                yield return new ValidationResult(
                    "La fecha de fin no puede ser anterior a la fecha de inicio.",
                    new[] { nameof(FechaFin) }
                );
            }
        }
    }
}
