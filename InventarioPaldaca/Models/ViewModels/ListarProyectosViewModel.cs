namespace InventarioPaldaca.Models.ViewModels
{
    public class ListarProyectosViewModel
    {
        public int ProyectoId { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;

        public string NombreAdministrador { get; set; } = string.Empty;
        public string? ImagenAdministrador { get; set; }
        public string? CargoAdministrador { get; set; }
    }
}
