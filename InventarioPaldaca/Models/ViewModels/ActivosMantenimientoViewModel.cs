using InventarioPaldaca.Models.Inventario;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ActivosMantenimientoViewModel
    {
        public List<Activo> Activos { get; set; }

        public int TotalActivosEnMantenimiento { get; set; }

        public List<SelectListItem> EstadosDisponibles { get; set; } = new List<SelectListItem>
        {
        new SelectListItem { Text = "En proceso", Value = "En proceso" },
        new SelectListItem { Text = "Finalizado", Value = "Finalizado" }
        };
    }
}
