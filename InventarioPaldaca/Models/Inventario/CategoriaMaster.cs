using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class CategoriaMaster
{
    public int CategoriaMasterId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Categorium> Categoria { get; set; } = new List<Categorium>();
}
