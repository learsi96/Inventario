using Inventario.Domain.Partners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Codigo).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.Codigo).IsUnique();
        builder.Property(t => t.Moneda).HasMaxLength(3).IsRequired();
        builder.Property(t => t.IvaPorcentaje).HasPrecision(5, 2);
        builder.Property(t => t.ZonaHoraria).HasMaxLength(64).IsRequired();
    }
}
