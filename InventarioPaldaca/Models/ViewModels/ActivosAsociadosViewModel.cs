using System.ComponentModel.DataAnnotations;

namespace InventarioPaldaca.Models.ViewModels
{
    public class ActivosAsociadosViewModel
    {
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string CodigoInventario { get; set; }

        [Display(Name = "Categoría")]
        public string CategoriaName { get; set; }

        [Display(Name = "Ubicación")]
        public string UbicacionName { get; set; }
    }
}
