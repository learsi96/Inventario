using Inventario.Domain.Conteos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

internal sealed class ConteoConfiguration : IEntityTypeConfiguration<Conteo>
{
    public void Configure(EntityTypeBuilder<Conteo> builder)
    {
        builder.ToTable("Conteos");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.UsuarioNombre).HasMaxLength(200);
        builder.HasIndex(c => new { c.TenantId, c.Folio }).IsUnique();
        builder.HasIndex(c => new { c.TenantId, c.SucursalId, c.Estado });

        builder.HasOne(c => c.Sucursal)
            .WithMany()
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Categoria)
            .WithMany()
            .HasForeignKey(c => c.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Detalles)
            .WithOne(d => d.Conteo)
            .HasForeignKey(d => d.ConteoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ConteoDetalleConfiguration : IEntityTypeConfiguration<ConteoDetalle>
{
    public void Configure(EntityTypeBuilder<ConteoDetalle> builder)
    {
        builder.ToTable("ConteoDetalles");
        builder.HasKey(d => d.Id);
        builder.Ignore(d => d.Diferencia);
        builder.Property(d => d.CantidadSistema).HasPrecision(18, 4);
        builder.Property(d => d.CantidadContada).HasPrecision(18, 4);
        builder.HasIndex(d => new { d.ConteoId, d.ArticuloId }).IsUnique();

        builder.HasOne(d => d.Articulo)
            .WithMany()
            .HasForeignKey(d => d.ArticuloId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
