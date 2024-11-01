using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Categorium
{
    public int CategoriaId { get; set; }

    public string CategoriaNombre { get; set; } = null!;

    public string? CategoriaDescripcion { get; set; }

    public int? CategoriaMasterId { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();

    public virtual CategoriaMaster? CategoriaMaster { get; set; }

    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();
}
