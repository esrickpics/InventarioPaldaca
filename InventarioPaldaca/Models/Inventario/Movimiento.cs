using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Movimiento
{
    public int MovimientoId { get; set; }

    public int ActivoId { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public int? UsuarioAnteriorId { get; set; }

    public int? UsuarioNuevoId { get; set; }

    public int? UbicacionAnteriorId { get; set; }

    public int? UbicacionNuevaId { get; set; }

    public string? Observaciones { get; set; }

    public int CantidadMantenimientos { get; set; }

    public virtual Activo Activo { get; set; } = null!;

    public virtual Ubicacion? UbicacionAnterior { get; set; }

    public virtual Ubicacion? UbicacionNueva { get; set; }

    public virtual Usuario? UsuarioAnterior { get; set; }

    public virtual Usuario? UsuarioNuevo { get; set; }
}
