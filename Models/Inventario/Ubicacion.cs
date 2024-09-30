using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Ubicacion
{
    public int UbicacionId { get; set; }

    public string UbicacionNombre { get; set; } = null!;

    public string UbicacionDireccion { get; set; } = null!;

    public string? UbicacionDescripcion { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();
}
