namespace InventarioPaldaca.Models.ViewModels
{
    public class MovimientoHistorialViewModel
    {
        public DateTime FechaMovimiento { get; set; }
        public string UsuarioAnterior { get; set; }
        public string UsuarioNuevo { get; set; }
        public string UbicacionAnterior { get; set; }
        public string UbicacionNueva { get; set; }
        public int CantidadMantenimientos { get; set; }
        public string Observaciones { get; set; }
    }
}
