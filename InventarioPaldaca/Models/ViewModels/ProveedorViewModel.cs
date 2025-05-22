using InventarioPaldaca.Models.Inventario;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ProveedorViewModel
    {
        public int ProveedorId { get; set; }

    
        public string ProveedorNombre { get; set; } = null!;
        public string ProveedorRif { get; set; } = string.Empty;
        public string ProveedorTelefono { get; set; } = string.Empty;
        public string ProveedorEmail { get; set; } = string.Empty;
        public string ProveedorDireccion { get; set; } = string.Empty;
        public string ProveedorOrigen { get; set; } = string.Empty;
        public int? Requisicion { get; set; }
        public string Search { get; set; } = string.Empty;

        // Lista para almacenar los proveedores encontrados
        public List<ProveedorViewModel> ProveedoresEncontrados { get; set; } = new List<ProveedorViewModel>();

        // Lista de Categorías Maestras para el filtro
        public List<CategoriaMaster> CategoriasMaestras { get; set; } = new List<CategoriaMaster>();

        public int? CategoriaMasterIdSeleccionada { get; set; }
    }
}
