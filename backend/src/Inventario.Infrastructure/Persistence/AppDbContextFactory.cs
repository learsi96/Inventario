using Inventario.Application.Abstractions;
using Inventario.Domain.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventario.Infrastructure.Persistence;

/// <summary>
/// Permite a <c>dotnet ef</c> crear el contexto sin arrancar la aplicación
/// (para generar y comparar migraciones). No se usa en tiempo de ejecución.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=InventarioDesignTime;Trusted_Connection=True;TrustServerCertificate=True",
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name))
            .Options;

        return new AppDbContext(options, new CurrentUserDesignTime());
    }

    private sealed class CurrentUserDesignTime : ICurrentUser
    {
        public Guid? UsuarioId => null;

        public Guid? TenantId => null;

        public RolUsuario? Rol => null;

        public bool EsSuperadmin => false;

        public bool EstaAutenticado => false;
    }
}
