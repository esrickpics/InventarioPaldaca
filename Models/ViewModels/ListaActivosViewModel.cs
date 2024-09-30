using InventarioPaldaca.Models.Inventario;
using System;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ListaActivosViewModel
    {
        public List<Activo> ListaActivos { get; set; }

        public List<Categorium> Categorias { get; set; }

        public List<Ubicacion> Ubicaciones { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Categoria { get; set; }
        public string NumeroSerial { get; set; }
        public string Ubicacion { get; set; }

        public string CodInventario { get; set; }
        public int TotalActivos { get; set; }
        public Dictionary<string, int> ActivosPorCategoria { get; set; }
        public Dictionary<string, int> ActivosPorUbicacion { get; set; }

    }
}
