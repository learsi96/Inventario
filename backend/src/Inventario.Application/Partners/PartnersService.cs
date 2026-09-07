using Inventario.Application.Abstractions;
using Inventario.Application.Catalogo;
using Inventario.Application.Common;
using Inventario.Domain.Catalogo;
using Inventario.Domain.Identidad;
using Inventario.Domain.Partners;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Partners;

/// <summary>
/// Alta de partners. Solo la ejecuta un superadministrador. Crea el tenant y su
/// primer usuario administrador en una sola operación.
/// </summary>
public sealed class PartnersService(IAppDbContext db, IPasswordHasher passwordHasher)
{
    public async Task<PartnerCreadoDto> CrearAsync(CrearPartnerRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Codigo))
        {
            throw new ValidacionException("Nombre y código del partner son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(request.AdminEmail) || string.IsNullOrWhiteSpace(request.AdminContrasena))
        {
            throw new ValidacionException("Email y contraseña del administrador son obligatorios.");
        }

        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var adminEmail = request.AdminEmail.Trim().ToLowerInvariant();

        if (await db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Codigo == codigo, ct))
        {
            throw new ConflictoException($"Ya existe un partner con el código '{codigo}'.");
        }

        if (await db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == adminEmail, ct))
        {
            throw new ConflictoException($"Ya existe un usuario con el email '{adminEmail}'.");
        }

        var tenant = new Tenant { Nombre = request.Nombre.Trim(), Codigo = codigo };
        db.Tenants.Add(tenant);

        db.Categorias.Add(new Categoria
        {
            TenantId = tenant.Id,
            Nombre = CategoriasService.NombreCategoriaSistema,
            EsSistema = true,
            Activa = true,
        });

        var admin = new Usuario
        {
            TenantId = tenant.Id,
            Email = adminEmail,
            NombreCompleto = request.AdminNombreCompleto.Trim(),
            HashContrasena = passwordHasher.Hash(request.AdminContrasena),
            Rol = RolUsuario.Administrador,
            Activo = true,
        };
        db.Usuarios.Add(admin);

        await db.SaveChangesAsync(ct);

        return new PartnerCreadoDto(tenant.Id, admin.Id, codigo);
    }
}
