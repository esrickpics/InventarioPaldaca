using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Categorium
{
    public int CategoriaId { get; set; }

    public string CategoriaNombre { get; set; } = null!;

    public string? CategoriaDescripcion { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();
}
