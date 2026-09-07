using Inventario.Application.Abstractions;
using Inventario.Domain.Identidad;
using Inventario.Domain.Partners;
using Inventario.Domain.Sucursales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventario.Infrastructure.Persistence;

/// <summary>
/// Datos de arranque para desarrollo: un partner demo con su administrador y una
/// sucursal, para poder iniciar sesión de inmediato. Solo se ejecuta si no hay
/// ningún tenant.
/// </summary>
public static class SeedData
{
    public const string DemoAdminEmail = "admin@demo.com";
    public const string DemoAdminContrasena = "Demo1234!";
    public const string DemoTenantCodigo = "DEMO";

    public static async Task InicializarAsync(IServiceProvider services, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SeedData));

        await db.Database.MigrateAsync(ct);

        if (await db.Tenants.IgnoreQueryFilters().AnyAsync(ct))
        {
            return;
        }

        var tenant = new Tenant { Nombre = "Refaccionaria Demo", Codigo = DemoTenantCodigo };
        db.Tenants.Add(tenant);

        var matriz = new Sucursal
        {
            TenantId = tenant.Id,
            Nombre = "Matriz",
            Codigo = "MATRIZ",
            Direccion = "Av. Siempre Viva 123",
            Activa = true,
        };
        db.Sucursales.Add(matriz);

        var admin = new Usuario
        {
            TenantId = tenant.Id,
            Email = DemoAdminEmail,
            NombreCompleto = "Administrador Demo",
            HashContrasena = hasher.Hash(DemoAdminContrasena),
            Rol = RolUsuario.Administrador,
            Activo = true,
        };
        admin.Sucursales.Add(new UsuarioSucursal
        {
            TenantId = tenant.Id,
            Usuario = admin,
            Sucursal = matriz,
        });
        db.Usuarios.Add(admin);

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "Seed de desarrollo creado: tenant {Codigo}, admin {Email}",
            DemoTenantCodigo,
            DemoAdminEmail);
    }
}
