using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string UsuarioNombre { get; set; } = null!;

    public string UsuarioApellido { get; set; } = null!;

    public string? UsuarioEmail { get; set; }

    public string? UsuarioTelefono { get; set; }

    public string? UsuarioCargo { get; set; }

    public string? ImagenUrl { get; set; }

    public string? AsignacionPdf { get; set; }

    public string UsuarioPassword { get; set; } = null!;

    public int RolId { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public virtual Rol UsuarioRol { get; set; } = null!;
}
