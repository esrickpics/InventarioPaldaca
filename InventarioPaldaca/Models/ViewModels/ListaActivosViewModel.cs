using InventarioPaldaca.Models.Inventario;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ListaActivosViewModel
    {
        // Datos principales
        public List<Activo> ListaActivos { get; set; }
        public List<Categorium> Categorias { get; set; }
        public List<Ubicacion> Ubicaciones { get; set; }
        public List<CategoriaMaster> CategoriasMaster { get; set; }

        public List<Proyecto> Proyectos { get; set; }

        
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Categoria { get; set; }
        public string NumeroSerial { get; set; }
        public string Ubicacion { get; set; }
        public string CodInventario { get; set; }
        public string Estado { get; set; }

        // Estadísticas
        public int TotalActivos { get; set; }
        public int TotalFiltrados { get; set; }
        public int TotalActivosDañados { get; set; }
        public int TotalReportes { get; set; }
        public List<Activo> ListaActivosCompleta { get; set; }

        public List<Activo> ActivosDisponiblesParaReasignar { get; set; }


        public Dictionary<string, int> ActivosPorCategoria { get; set; }
        public Dictionary<string, int> ActivosPorUbicacion { get; set; }

        public Dictionary<string, string> IconosPorCategoria => new Dictionary<string, string>
        {
            { "Laptop", "/img/Iconos/iconlaptop.svg" },
            { "Impresora", "/img/Iconos/Impresora.svg" },
            { "Proyector", "/img/Iconos/iconproyector.svg" },
            { "MiniPC", "/img/Iconos/iconminipc.svg" },
            { "Servidor", "/img/Iconos/Servericon.svg" },
            { "PC", "/img/Iconos/iconpc.svg" },
            { "All in One", "/img/Iconos/Monitor.svg" },
            { "Monitor", "/img/Iconos/Monitor.svg" },
            { "Vehículo", "/img/Iconos/iconcar.svg"},
            { "Carro", "/img/Iconos/iconcar.svg" },
            { "Autobús", "/img/Iconos/iconcar.svg" },
            { "Mini bus", "/img/Iconos/iconcar.svg" },
            { "Camioneta", "/img/Iconos/iconcar.svg" },
            { "Ambulancia", "/img/Iconos/ambulancia.svg" },
            { "Gandola", "/img/Iconos/iconcar.svg" },
            { "Grúa", "/img/Iconos/iconcar.svg" },
        };

        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPage => (int)Math.Ceiling(TotalFiltrados / (double)PageSize);

        // Estados disponibles para filtros o formularios
    }
}
