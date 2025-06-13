using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class ProyectoAdministrador
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int ProyectoId { get; set; }

    public virtual Proyecto Proyecto { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
