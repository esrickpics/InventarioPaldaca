using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Reporte
{
    public int ReporteId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public string? Descripcion { get; set; }

    public string? RutaArchivo { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
