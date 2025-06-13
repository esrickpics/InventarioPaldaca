using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Proyecto
{
    public int ProyectoId { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();

    public virtual ICollection<Movimiento> MovimientoProyectoAnteriors { get; set; } = new List<Movimiento>();

    public virtual ICollection<Movimiento> MovimientoProyectoNuevos { get; set; } = new List<Movimiento>();

    public virtual ICollection<ProyectoAdministrador> ProyectoAdministradors { get; set; } = new List<ProyectoAdministrador>();
}
