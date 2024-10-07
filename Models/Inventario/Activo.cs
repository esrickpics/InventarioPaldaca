using System;
using System.Collections.Generic;

namespace InventarioPaldaca.Models.Inventario;

public partial class Activo
{
    public int ActivoId { get; set; }

    public string? Marca { get; set; }

    public string Modelo { get; set; } = null!;

    public string? NumeroSerial { get; set; }

    public bool? Funcionabilidad { get; set; }

    public string? Observaciones { get; set; }

    public string? CodigoInventario { get; set; }

    public int? CategoriaId { get; set; }

    public int? UbicacionId { get; set; }

    public int? UsuarioId { get; set; }

    public int? ProveedorId { get; set; }

    public int? Adquisicion { get; set; }

    public virtual Categorium? Categoria { get; set; }

    public virtual Proveedor? Proveedor { get; set; }

    public virtual Ubicacion? Ubicacion { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
