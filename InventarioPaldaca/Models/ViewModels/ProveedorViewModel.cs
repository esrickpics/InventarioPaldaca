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

        public List<ProveedorViewModel> ProveedoresEncontrados { get; set; } = new List<ProveedorViewModel>();
    }
}
