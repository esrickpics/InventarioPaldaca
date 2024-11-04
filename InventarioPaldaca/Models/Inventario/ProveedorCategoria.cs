namespace InventarioPaldaca.Models.Inventario
{
    public class ProveedorCategoria
    {
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        public int CategoriaId { get; set; }
        public Categorium Categoria { get; set; } = null!;
    }
}
