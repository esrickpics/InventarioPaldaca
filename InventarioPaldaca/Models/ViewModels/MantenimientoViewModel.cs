using InventarioPaldaca.Models.Inventario;

namespace InventarioPaldaca.Models.ViewModels
{
    public class MantenimientoViewModel
    {

        public string CodigoInventarioBuscado { get; set; } = string.Empty;

        public List<Mantenimiento> UltimosMantenimientos { get; set; } = new();

        public List<Mantenimiento> HistorialDelActivo { get; set; } = new();

        public bool MostrandoHistorial { get; set; } = false;


    }
    
}
