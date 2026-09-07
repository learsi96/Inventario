using Inventario.Domain.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

internal sealed class UnidadMedidaConfiguration : IEntityTypeConfiguration<UnidadMedida>
{
    public void Configure(EntityTypeBuilder<UnidadMedida> builder)
    {
        builder.ToTable("UnidadesMedida");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Codigo).HasMaxLength(10).IsRequired();
        builder.Property(u => u.Nombre).HasMaxLength(60).IsRequired();
        builder.HasIndex(u => u.Codigo).IsUnique();
    }
}
