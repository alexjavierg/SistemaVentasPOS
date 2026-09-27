using Ventas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using System.Linq;

namespace Ventas.Domain.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Modelo> Modelos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Sucursal> Sucursales { get; set; }
            public DbSet<InventarioSucursal> InventariosSucursal { get; set; }
        public DbSet<Talla> Tallas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<CajaTurno> CajasTurnos { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<DetalleVenta> DetallesVentas { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -- CONFIGURACIÃ“N FLUENT API REQUERIDA --

        // 1. PrevenciÃ³n de Cascada (DeleteBehavior.Restrict) Global
        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

                foreach (var fk in cascadeFKs)
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // --- OVERRIDES PARA BORRADO DE MODELO Y SNAPSHOTS ---
        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Modelo)
            .WithMany()
            .HasForeignKey(d => d.ModeloId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.Modelo)
            .WithMany()
            .HasForeignKey(m => m.ModeloId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<InventarioSucursal>()
            .HasOne(i => i.Modelo)
            .WithMany(m => m.Inventarios)
            .HasForeignKey(i => i.ModeloId)
            .OnDelete(DeleteBehavior.Cascade);

        // 2. ConfiguraciÃ³n de Tipos de Datos y TamaÃ±os

        // CategorÃ­a
        modelBuilder.Entity<Categoria>().Property(c => c.Nombre).HasMaxLength(100);

        // Sucursal
        modelBuilder.Entity<Sucursal>().Property(s => s.Nombre).HasMaxLength(100);
        modelBuilder.Entity<Sucursal>().Property(s => s.Direccion).HasMaxLength(255);
        modelBuilder.Entity<Sucursal>().Property(s => s.Telefono).HasMaxLength(20);

        // Usuario
        modelBuilder.Entity<Usuario>().Property(u => u.Username).HasMaxLength(100);
        modelBuilder.Entity<Usuario>().Property(u => u.PasswordHash).HasMaxLength(500);
        modelBuilder.Entity<Usuario>().Property(u => u.Rol).HasMaxLength(50);

        // Modelo
        modelBuilder.Entity<Modelo>().Property(m => m.Nombre).HasMaxLength(100);
        modelBuilder.Entity<Modelo>().Property(m => m.NombreCorto).HasMaxLength(100);
        modelBuilder.Entity<Modelo>().Property(m => m.Foto).HasMaxLength(500);
        modelBuilder.Entity<Modelo>().Property(m => m.Talla).HasMaxLength(50);
        modelBuilder.Entity<Modelo>().Property(m => m.CodigoQR).HasMaxLength(500);
        modelBuilder.Entity<Modelo>().Property(m => m.PrecioPorDocena).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Modelo>().Property(m => m.CostoPorDocena).HasColumnType("decimal(18,2)");
        
        // 3. Ãndice en Nombre de Modelo
        modelBuilder.Entity<Modelo>().HasIndex(m => m.Nombre);

        // CajaTurno
        modelBuilder.Entity<CajaTurno>().Property(c => c.MontoInicial).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<CajaTurno>().Property(c => c.MontoFinal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<CajaTurno>().Property(c => c.Estado).HasMaxLength(50);

        // Venta
        modelBuilder.Entity<Venta>().Property(v => v.Total).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Venta>().Property(v => v.Descuento).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Venta>().Property(v => v.Recargo).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Venta>().Property(v => v.ClienteNombre).HasMaxLength(100);
        modelBuilder.Entity<Venta>().Property(v => v.ClienteDocumento).HasMaxLength(50);
        modelBuilder.Entity<Venta>().Property(v => v.MedioPago).HasMaxLength(50);
        modelBuilder.Entity<Venta>().Property(v => v.Estado).HasMaxLength(50);
        modelBuilder.Entity<Venta>().Property(v => v.MotivoAnulacion).HasMaxLength(255);
        modelBuilder.Entity<Venta>().Property(v => v.Serie).HasMaxLength(10);
        modelBuilder.Entity<Venta>().Property(v => v.TipoComprobante).HasMaxLength(50);

        // DetalleVenta
        modelBuilder.Entity<DetalleVenta>().Property(d => d.PrecioPorDocena).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<DetalleVenta>().Property(d => d.Subtotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<DetalleVenta>().Property(d => d.CostoHistoricoPorDocena).HasColumnType("decimal(18,2)");

        // MovimientoInventario
        modelBuilder.Entity<MovimientoInventario>().Property(m => m.TipoMovimiento).HasMaxLength(20);
        modelBuilder.Entity<MovimientoInventario>().Property(m => m.Motivo).HasMaxLength(255);

        // ConfiguraciÃ³n adicional de relaciones explÃ­citas (opcional si ya usamos convenciones, pero mantenemos las tuyas)
        modelBuilder.Entity<Modelo>()
            .HasOne(m => m.Categoria)
            .WithMany(c => c.Modelos)
            .HasForeignKey(m => m.CategoriaId);

        modelBuilder.Entity<InventarioSucursal>()
            .HasOne(i => i.Modelo)
            .WithMany(m => m.Inventarios)
            .HasForeignKey(i => i.ModeloId);

        modelBuilder.Entity<InventarioSucursal>()
            .HasOne(i => i.Sucursal)
            .WithMany(s => s.Inventarios)
            .HasForeignKey(i => i.SucursalId);

        // -- SEED DATA --
        modelBuilder.Entity<Sucursal>().HasData(
            new Sucursal { Id = 1, Nombre = "MUJER BONITA", Direccion = "Lima" }
        );

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, Username = "admin", PasswordHash = "$2a$11$PxJVDpSwx9mjYGSxORIPeOl2VP5kIf/CLkkNOeSBLFF0FtxRP28om", Rol = "Admin" }
        );

                    modelBuilder.Entity<Talla>().HasData(
                new Talla { Id = 1, Nombre = "ESTÁNDAR" },
                new Talla { Id = 2, Nombre = "S" },
                new Talla { Id = 3, Nombre = "M" },
                new Talla { Id = 4, Nombre = "L" },
                new Talla { Id = 5, Nombre = "XL" }
            );

            modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "BODYS MANGA CERO" },
            new Categoria { Id = 2, Nombre = "BODYS MANGA CORTA" },
            new Categoria { Id = 3, Nombre = "BODYS MANGA LARGA" },
            new Categoria { Id = 4, Nombre = "POLOS" }
        );

        modelBuilder.Entity<InventarioSucursal>().HasData(
            new InventarioSucursal { Id = 1, ModeloId = 1, SucursalId = 1, CantidadDocenas = 10 },
            new InventarioSucursal { Id = 2, ModeloId = 2, SucursalId = 1, CantidadDocenas = 3 },
            new InventarioSucursal { Id = 3, ModeloId = 3, SucursalId = 1, CantidadDocenas = 0 },
            new InventarioSucursal { Id = 4, ModeloId = 4, SucursalId = 1, CantidadDocenas = 5 },
            new InventarioSucursal { Id = 5, ModeloId = 5, SucursalId = 1, CantidadDocenas = 15 }
        );

        modelBuilder.Entity<Modelo>().HasData(
            new Modelo { Id = 1, CategoriaId = 4, Nombre = "Polo Manga Corta Cuello Cuadrado", PrecioPorDocena = 10.00m },
            new Modelo { Id = 2, CategoriaId = 2, Nombre = "Body Manga Corta Cuello Cuadrado", PrecioPorDocena = 14.00m },
            new Modelo { Id = 3, CategoriaId = 2, Nombre = "Body Manga Corta Espalda Descubierta", PrecioPorDocena = 12.50m },
            new Modelo { Id = 4, CategoriaId = 3, Nombre = "Body Manga Larga Cuello Cuadrado", PrecioPorDocena = 14.00m },
            new Modelo { Id = 5, CategoriaId = 1, Nombre = "Body Manga Cero Escote Espejo", PrecioPorDocena = 10.50m },
            new Modelo { Id = 6, CategoriaId = 4, Nombre = "Polo Manga Corta Cuello CorazÃ³n", PrecioPorDocena = 10.00m },
            new Modelo { Id = 7, CategoriaId = 1, Nombre = "Body Manga Cero Tirita", PrecioPorDocena = 10.00m },
            new Modelo { Id = 8, CategoriaId = 4, Nombre = "Polo Lazo", PrecioPorDocena = 11.50m },
            new Modelo { Id = 9, CategoriaId = 4, Nombre = "Polo Manga Corta Cuello Redondo", PrecioPorDocena = 9.50m },
            new Modelo { Id = 10, CategoriaId = 2, Nombre = "Body Manga Corta Cuello CorazÃ³n", PrecioPorDocena = 12.50m }
        );
    }
}





