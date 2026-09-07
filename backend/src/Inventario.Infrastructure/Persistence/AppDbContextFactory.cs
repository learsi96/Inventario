using Inventario.Application.Abstractions;
using Inventario.Domain.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Inventario.Infrastructure.Persistence;

/// <summary>
/// Permite a <c>dotnet ef</c> crear el contexto sin arrancar la aplicación (para
/// generar y aplicar migraciones). Lee <c>ConnectionStrings:Default</c> de la
/// configuración del proyecto de API. No se usa en tiempo de ejecución.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var entorno = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var rutaApi = Path.Combine(Directory.GetCurrentDirectory(), "..", "Inventario.Api");
        var basePath = Directory.Exists(rutaApi) ? rutaApi : Directory.GetCurrentDirectory();

        var configuracion = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{entorno}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuracion.GetConnectionString("Default")
            ?? "Server=localhost,1433;Database=InventarioDev;User Id=sa;Password=Local_Dev_P4ssw0rd!;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                connectionString,
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
