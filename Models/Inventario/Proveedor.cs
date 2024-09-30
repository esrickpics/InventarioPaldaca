using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Proveedor
{
    public int ProveedorId { get; set; }

    public string ProveedorNombre { get; set; } = null!;

    public string? ProveedorContacto { get; set; }

    public string? ProveedorTelefono { get; set; }

    public string? ProveedorEmail { get; set; }

    public string? ProveedorDireccion { get; set; }

    public virtual ICollection<Activo> Activos { get; set; } = new List<Activo>();
}
