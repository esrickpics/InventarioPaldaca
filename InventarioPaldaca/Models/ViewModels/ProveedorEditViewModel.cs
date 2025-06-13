using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ProveedorEditViewModel
    {
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        public string ProveedorNombre { get; set; } = null!;

        public string? ProveedorRif { get; set; } = string.Empty;

        public string ProveedorTelefono { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El email no es válido.")]
        public string? ProveedorEmail { get; set; } = string.Empty;

        public string? ProveedorDireccion { get; set; } = string.Empty;

        public string? ProveedorOrigen { get; set; } = string.Empty;
    }
}
