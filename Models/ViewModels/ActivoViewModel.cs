using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ActivoViewModel
    {
        [Required(ErrorMessage = "La marca es obligatoria.")]
        [DisplayName("Marca")]
        public string Marca { get; set; }

        [DisplayName("Modelo")]
        public string Modelo { get; set; }

        [DisplayName("Número Serial")]
        public string NumeroSerial { get; set; }

        [DisplayName("Funcionabilidad")]
        public bool Funcionabilidad { get; set; }

        [DisplayName("Observaciones")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "El codigo es obligatorio.")]
        [DisplayName("Código de Inventario")] 
        public string CodigoInventario { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [DisplayName("Categoria")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "Ingrese la Ubicacion")]
        [DisplayName("Ubicación")]
        public int UbicacionId { get; set; }

        [Required(ErrorMessage = "Ingrese un Usuario")]
        [DisplayName("Usuario")]
        public int UsuarioId { get; set; }

        public int AñoAdquirido { get; set; }

    }
}
