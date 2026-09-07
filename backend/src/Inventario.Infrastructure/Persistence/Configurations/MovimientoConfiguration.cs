using Inventario.Domain.Movimientos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

internal sealed class MovimientoConfiguration : IEntityTypeConfiguration<Movimiento>
{
    public void Configure(EntityTypeBuilder<Movimiento> builder)
    {
        builder.ToTable("Movimientos");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.EstadoTransferencia).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Motivo).HasMaxLength(300);
        builder.Property(m => m.Referencia).HasMaxLength(100);
        builder.Property(m => m.UsuarioNombre).HasMaxLength(200);

        builder.HasIndex(m => new { m.TenantId, m.Folio }).IsUnique();
        builder.HasIndex(m => new { m.TenantId, m.Fecha });

        builder.HasOne(m => m.Sucursal)
            .WithMany()
            .HasForeignKey(m => m.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.SucursalDestino)
            .WithMany()
            .HasForeignKey(m => m.SucursalDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Renglones)
            .WithOne(r => r.Movimiento)
            .HasForeignKey(r => r.MovimientoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class MovimientoRenglonConfiguration : IEntityTypeConfiguration<MovimientoRenglon>
{
    public void Configure(EntityTypeBuilder<MovimientoRenglon> builder)
    {
        builder.ToTable("MovimientoRenglones");
        builder.HasKey(r => r.Id);

        foreach (var prop in new[]
                 {
                     "Cantidad", "CostoUnitario", "CantidadResultante", "CostoPromedioResultante",
                 })
        {
            builder.Property(prop).HasPrecision(18, 4);
        }

        builder.HasIndex(r => new { r.TenantId, r.ArticuloId });

        builder.HasOne(r => r.Articulo)
            .WithMany()
            .HasForeignKey(r => r.ArticuloId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
