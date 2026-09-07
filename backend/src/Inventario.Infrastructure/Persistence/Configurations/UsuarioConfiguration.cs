using Inventario.Domain.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.NombreCompleto).HasMaxLength(200).IsRequired();
        builder.Property(u => u.HashContrasena).HasMaxLength(512).IsRequired();
        builder.Property(u => u.Rol).HasConversion<string>().HasMaxLength(32);

        // Email único a nivel global (el login no conoce el tenant todavía).
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.TenantId);

        builder.HasMany(u => u.Sucursales)
            .WithOne(us => us.Usuario)
            .HasForeignKey(us => us.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class UsuarioSucursalConfiguration : IEntityTypeConfiguration<UsuarioSucursal>
{
    public void Configure(EntityTypeBuilder<UsuarioSucursal> builder)
    {
        builder.ToTable("UsuariosSucursales");

        builder.HasKey(us => new { us.UsuarioId, us.SucursalId });
        builder.HasIndex(us => us.TenantId);

        builder.HasOne(us => us.Sucursal)
            .WithMany()
            .HasForeignKey(us => us.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
