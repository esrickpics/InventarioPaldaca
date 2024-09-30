using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Models.Inventario;

public partial class InventaryPaldacaContext : DbContext
{
    public InventaryPaldacaContext()
    {
    }

    public InventaryPaldacaContext(DbContextOptions<InventaryPaldacaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activo> Activos { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Ubicacion> Ubicacions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-85DQ6DB; Database=InventaryPaldaca; Trusted_Connection=true; TrustServerCertificate=True ");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activo>(entity =>
        {
            entity.HasKey(e => e.ActivoId).HasName("PK__activo__D2F3F0903D6062D4");

            entity.ToTable("activo");

            entity.Property(e => e.ActivoId).HasColumnName("activo_id");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.CodigoInventario)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("codigo_inventario");
            entity.Property(e => e.Funcionabilidad).HasColumnName("funcionabilidad");
            entity.Property(e => e.Marca)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("marca");
            entity.Property(e => e.Modelo)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("modelo");
            entity.Property(e => e.NumeroSerial)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("numero_serial");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.UbicacionId).HasColumnName("ubicacion_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Activos)
                .HasForeignKey(d => d.CategoriaId)
                .HasConstraintName("FK__activo__categori__5165187F");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.Activos)
                .HasForeignKey(d => d.ProveedorId)
                .HasConstraintName("FK__activo__proveedo__5441852A");

            entity.HasOne(d => d.Ubicacion).WithMany(p => p.Activos)
                .HasForeignKey(d => d.UbicacionId)
                .HasConstraintName("FK__activo__ubicacio__52593CB8");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Activos)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__activo__usuario___534D60F1");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__categori__DB875A4FC215485D");

            entity.ToTable("categoria");

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.CategoriaDescripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("categoria_descripcion");
            entity.Property(e => e.CategoriaNombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("categoria_nombre");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.ProveedorId).HasName("PK__proveedo__88BBADA4E3F5BDD6");

            entity.ToTable("proveedor");

            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.ProveedorContacto)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("proveedor_contacto");
            entity.Property(e => e.ProveedorDireccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("proveedor_direccion");
            entity.Property(e => e.ProveedorEmail)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("proveedor_email");
            entity.Property(e => e.ProveedorNombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("proveedor_nombre");
            entity.Property(e => e.ProveedorTelefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("proveedor_telefono");
        });

        modelBuilder.Entity<Ubicacion>(entity =>
        {
            entity.HasKey(e => e.UbicacionId).HasName("PK__ubicacio__545128291D1E5E8C");

            entity.ToTable("ubicacion");

            entity.Property(e => e.UbicacionId).HasColumnName("ubicacion_id");
            entity.Property(e => e.UbicacionDescripcion)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("ubicacion_descripcion");
            entity.Property(e => e.UbicacionDireccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ubicacion_direccion");
            entity.Property(e => e.UbicacionNombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ubicacion_nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__usuario__2ED7D2AFDDF23721");

            entity.ToTable("usuario");

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.UsuarioCargo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cargo");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioTelefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.UsuarioApellido)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("usuario_apellido");
            entity.Property(e => e.UsuarioEmail)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("usuario_email");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("usuario_nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
