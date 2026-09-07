using Inventario.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

internal sealed class ArticuloConfiguration : IEntityTypeConfiguration<Articulo>
{
    public void Configure(EntityTypeBuilder<Articulo> builder)
    {
        builder.ToTable("Articulos");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Sku).HasMaxLength(40).IsRequired();
        builder.Property(a => a.CodigoBarras).HasMaxLength(64);
        builder.Property(a => a.Nombre).HasMaxLength(250).IsRequired();
        builder.Property(a => a.Descripcion).HasMaxLength(1000);
        builder.Property(a => a.Marca).HasMaxLength(120);
        builder.Property(a => a.NumeroParteOem).HasMaxLength(80);
        builder.Property(a => a.ImagenNombre).HasMaxLength(200);
        builder.Property(a => a.Costo).HasPrecision(18, 4);
        builder.Property(a => a.PrecioVenta).HasPrecision(18, 4);
        builder.Property(a => a.IvaPorcentaje).HasPrecision(5, 2);
        builder.Property(a => a.Estado).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => new { a.TenantId, a.Sku }).IsUnique();
        builder.HasIndex(a => new { a.TenantId, a.CodigoBarras })
            .IsUnique()
            .HasFilter("[CodigoBarras] IS NOT NULL");
        builder.HasIndex(a => new { a.TenantId, a.CategoriaId });

        builder.HasOne(a => a.Categoria)
            .WithMany()
            .HasForeignKey(a => a.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UnidadMedida)
            .WithMany()
            .HasForeignKey(a => a.UnidadMedidaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.CodigosAlternos)
            .WithOne(c => c.Articulo)
            .HasForeignKey(c => c.ArticuloId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CodigoAlternoConfiguration : IEntityTypeConfiguration<CodigoAlterno>
{
    public void Configure(EntityTypeBuilder<CodigoAlterno> builder)
    {
        builder.ToTable("CodigosAlternos");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Codigo).HasMaxLength(80).IsRequired();
        builder.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(c => new { c.TenantId, c.Codigo });
    }
}
