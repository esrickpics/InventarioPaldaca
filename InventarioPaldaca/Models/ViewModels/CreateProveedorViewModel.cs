using InventarioPaldaca.Models.Inventario;

namespace InventarioPaldaca.Models.ViewModels
{
    public class CreateProveedorViewModel
    {
        // Proveedor que se va a crear
        public Proveedor Proveedor { get; set; } = new Proveedor();

        // Identificador de la Categoría Maestra seleccionada
        public int? CategoriaMasterIdSeleccionada { get; set; }

        // Lista de Categorías Maestras disponibles para seleccionar
        public List<CategoriaMaster> CategoriaMasters { get; set; } = new();

        // Lista de categorías filtradas que pertenecen a la categoría maestra seleccionada
        public List<Categorium> CategoriasFiltradas { get; set; } = new();

        // Lista de categorías asociadas al proveedor
        public List<int> CategoriaSeleccionadas { get; set; } = new();
    }
}
