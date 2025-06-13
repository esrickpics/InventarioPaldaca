using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Ubicacion
{
    public int UbicacionId { get; set; }

    public string UbicacionNombre { get; set; } = null!;

    public string? UbicacionDireccion { get; set; }

    public string? UbicacionDescripcion { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();

    public virtual ICollection<Movimiento> MovimientoUbicacionAnteriors { get; set; } = new List<Movimiento>();

    public virtual ICollection<Movimiento> MovimientoUbicacionNuevas { get; set; } = new List<Movimiento>();
}
