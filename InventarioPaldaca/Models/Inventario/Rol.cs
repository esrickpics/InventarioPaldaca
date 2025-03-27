using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Rol
{
    public int RolId { get; set; }

    public string RolNombre { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
