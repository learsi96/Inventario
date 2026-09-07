using Inventario.Domain.Sucursales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

internal sealed class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("Sucursales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Direccion).HasMaxLength(500);

        // Código único por partner.
        builder.HasIndex(s => new { s.TenantId, s.Codigo }).IsUnique();
    }
}
