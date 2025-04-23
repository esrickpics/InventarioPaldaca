using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Mantenimiento
{
    public int MantenimientoId { get; set; }

    public int ActivoId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string Estado { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Tecnico { get; set; }

    public string? NumeroTecnico { get; set; }

    public decimal? Costo { get; set; }

    public virtual Activo Activo { get; set; } = null!;
}
