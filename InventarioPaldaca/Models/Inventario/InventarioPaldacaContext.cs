using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InventarioPaldaca.Models.Inventario;

public partial class InventarioPaldacaContext : DbContext
{
    public InventarioPaldacaContext()
    {
    }

    public InventarioPaldacaContext(DbContextOptions<InventarioPaldacaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activo> Activos { get; set; }

    public virtual DbSet<CategoriaMaster> CategoriaMasters { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Mantenimiento> Mantenimientos { get; set; }

    public virtual DbSet<Movimiento> Movimientos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Proyecto> Proyectos { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Ubicacion> Ubicacions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=RAG\\SQLEXPRESS; Database=InventarioPaldaca; Trusted_Connection=true; TrustServerCertificate=True ");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activo>(entity =>
        {
            entity.HasKey(e => e.ActivoId).HasName("PK__activo__D2F3F09055EC7B0D");

            entity.ToTable("activo");

            entity.Property(e => e.ActivoId).HasColumnName("activo_id");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.CodigoInventario)
                .HasMaxLength(50)
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
                .HasMaxLength(150)
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
                .HasConstraintName("FK__activo__categori__5535A963");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.Activos)
                .HasForeignKey(d => d.ProveedorId)
                .HasConstraintName("FK__activo__proveedo__5629CD9C");

            entity.HasOne(d => d.Ubicacion).WithMany(p => p.Activos)
                .HasForeignKey(d => d.UbicacionId)
                .HasConstraintName("FK__activo__ubicacio__571DF1D5");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Activos)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__activo__usuario___5812160E");
        });

        modelBuilder.Entity<CategoriaMaster>(entity =>
        {
            entity.HasKey(e => e.CategoriaMasterId).HasName("PK__Categori__4695212E31C5E5F0");

            entity.ToTable("CategoriaMaster");

            entity.Property(e => e.CategoriaMasterId).HasColumnName("CategoriaMasterID");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__categori__DB875A4FA166AFA7");

            entity.ToTable("categoria");

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.CategoriaDescripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("categoria_descripcion");
            entity.Property(e => e.CategoriaMasterId).HasColumnName("CategoriaMasterID");
            entity.Property(e => e.CategoriaNombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("categoria_nombre");

            entity.HasOne(d => d.CategoriaMaster).WithMany(p => p.Categoria)
                .HasForeignKey(d => d.CategoriaMasterId)
                .HasConstraintName("FK_CategoriaMaster");
        });

        modelBuilder.Entity<Mantenimiento>(entity =>
        {
            entity.HasKey(e => e.MantenimientoId).HasName("PK__Mantenim__A62E61A23CD93147");

            entity.Property(e => e.Costo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NumeroTecnico).HasMaxLength(20);
            entity.Property(e => e.Tecnico).HasMaxLength(100);

            entity.HasOne(d => d.Activo).WithMany(p => p.Mantenimientos)
                .HasForeignKey(d => d.ActivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mantenimientos_Activos");
        });

        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.HasKey(e => e.MovimientoId).HasName("PK__Movimien__BF923C2C2630DD66");

            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Activo).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.ActivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Movimient__Activ__634EBE90");

            entity.HasOne(d => d.UbicacionAnterior).WithMany(p => p.MovimientoUbicacionAnteriors)
                .HasForeignKey(d => d.UbicacionAnteriorId)
                .HasConstraintName("FK__Movimient__Ubica__662B2B3B");

            entity.HasOne(d => d.UbicacionNueva).WithMany(p => p.MovimientoUbicacionNuevas)
                .HasForeignKey(d => d.UbicacionNuevaId)
                .HasConstraintName("FK__Movimient__Ubica__671F4F74");

            entity.HasOne(d => d.UsuarioAnterior).WithMany(p => p.MovimientoUsuarioAnteriors)
                .HasForeignKey(d => d.UsuarioAnteriorId)
                .HasConstraintName("FK__Movimient__Usuar__6442E2C9");

            entity.HasOne(d => d.UsuarioNuevo).WithMany(p => p.MovimientoUsuarioNuevos)
                .HasForeignKey(d => d.UsuarioNuevoId)
                .HasConstraintName("FK__Movimient__Usuar__65370702");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.ProveedorId).HasName("PK__proveedo__88BBADA4AA901FB7");

            entity.ToTable("proveedor");

            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.Origen)
                .HasMaxLength(50)
                .IsUnicode(false);
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
            entity.Property(e => e.ProveedorRif)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("proveedor_rif");
            entity.Property(e => e.ProveedorTelefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("proveedor_telefono");

            entity.HasMany(d => d.Categoria).WithMany(p => p.Proveedors)
                .UsingEntity<Dictionary<string, object>>(
                    "ProveedorCategorium",
                    r => r.HasOne<Categorium>().WithMany()
                        .HasForeignKey("CategoriaId")
                        .HasConstraintName("FK__proveedor__categ__59FA5E80"),
                    l => l.HasOne<Proveedor>().WithMany()
                        .HasForeignKey("ProveedorId")
                        .HasConstraintName("FK__proveedor__prove__5AEE82B9"),
                    j =>
                    {
                        j.HasKey("ProveedorId", "CategoriaId").HasName("PK__proveedo__6503D800F137C9DD");
                        j.ToTable("proveedor_categoria");
                        j.IndexerProperty<int>("ProveedorId").HasColumnName("proveedor_id");
                        j.IndexerProperty<int>("CategoriaId").HasColumnName("categoria_id");
                    });
        });

        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.HasKey(e => e.ProyectoId).HasName("PK__Proyecto__CF241D65A2125893");

            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.ReporteId).HasName("PK__Reporte__0B29EA6E59A0A6C3");

            entity.ToTable("Reporte");

            entity.Property(e => e.ActivoId).HasColumnName("Activo_Id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.Activo).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.ActivoId)
                .HasConstraintName("FK_Reporte_Activo");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reporte__Usuario__2A164134");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Rol__CF32E443050FE306");

            entity.ToTable("Rol");

            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.RolNombre)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("rol_nombre");
        });

        modelBuilder.Entity<Ubicacion>(entity =>
        {
            entity.HasKey(e => e.UbicacionId).HasName("PK__ubicacio__5451282938C63BC6");

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
            entity.HasKey(e => e.UsuarioId).HasName("PK__usuario__2ED7D2AFDE44D96C");

            entity.ToTable("usuario");

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.AsignacionPdf)
                .IsUnicode(false)
                .HasColumnName("AsignacionPDF");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RolId).HasColumnName("Rol_Id");
            entity.Property(e => e.UsuarioApellido)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("usuario_apellido");
            entity.Property(e => e.UsuarioCargo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioEmail)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("usuario_email");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("usuario_nombre");
            entity.Property(e => e.UsuarioPassword)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("usuario_password");
            entity.Property(e => e.UsuarioTelefono)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
