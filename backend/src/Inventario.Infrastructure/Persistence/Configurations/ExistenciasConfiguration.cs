using Inventario.Domain.Existencias;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

internal sealed class UbicacionConfiguration : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> builder)
    {
        builder.ToTable("Ubicaciones");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Codigo).HasMaxLength(40).IsRequired();
        builder.Property(u => u.Descripcion).HasMaxLength(200);
        builder.HasIndex(u => new { u.SucursalId, u.Codigo }).IsUnique();

        builder.HasOne(u => u.Sucursal)
            .WithMany()
            .HasForeignKey(u => u.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExistenciaConfiguration : IEntityTypeConfiguration<Existencia>
{
    public void Configure(EntityTypeBuilder<Existencia> builder)
    {
        builder.ToTable("Existencias");
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.Valor);
        builder.Ignore(e => e.BajoMinimo);

        foreach (var prop in new[] { "Cantidad", "CostoPromedio", "Minimo", "Maximo", "PuntoReorden" })
        {
            builder.Property(prop).HasPrecision(18, 4);
        }

        builder.HasIndex(e => new { e.ArticuloId, e.SucursalId }).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.SucursalId });

        builder.HasOne(e => e.Articulo)
            .WithMany()
            .HasForeignKey(e => e.ArticuloId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Sucursal)
            .WithMany()
            .HasForeignKey(e => e.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.PorUbicacion)
            .WithOne(pu => pu.Existencia)
            .HasForeignKey(pu => pu.ExistenciaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ExistenciaUbicacionConfiguration : IEntityTypeConfiguration<ExistenciaUbicacion>
{
    public void Configure(EntityTypeBuilder<ExistenciaUbicacion> builder)
    {
        builder.ToTable("ExistenciasUbicacion");
        builder.HasKey(pu => pu.Id);
        builder.Property(pu => pu.Cantidad).HasPrecision(18, 4);
        builder.HasIndex(pu => new { pu.ExistenciaId, pu.UbicacionId }).IsUnique();

        builder.HasOne(pu => pu.Ubicacion)
            .WithMany()
            .HasForeignKey(pu => pu.UbicacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
