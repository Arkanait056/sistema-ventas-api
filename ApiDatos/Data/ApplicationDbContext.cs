using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApiDatos.Models;

namespace ApiDatos.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) 
    {
    }

    public DbSet<Producto> Productos { get; set; } 
    public DbSet<Venta> Ventas { get; set; } 
    public DbSet<DetalleVenta> DetallesVentas { get; set; } 

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Producto>()
            .Property(p => p.PrecioCosto)
            .HasPrecision(18, 2);

        builder.Entity<Producto>()
            .Property(p => p.PrecioVenta)
            .HasPrecision(18, 2);

        builder.Entity<Venta>()
            .Property(v => v.Total)
            .HasPrecision(18, 2);

        builder.Entity<DetalleVenta>()
            .Property(d => d.PrecioUnitario)
            .HasPrecision(18, 2);
            
        builder.Entity<Venta>()
                .HasIndex(v => v.Fecha);

        builder.Entity<DetalleVenta>()
                .HasIndex(d => d.ProductoId);


        builder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.UsuarioId);

        builder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.VentaId);

        builder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId);
        
        builder.Entity<Producto>()
                .HasIndex(p => p.Sku)
                .IsUnique();
            
    }
}