using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Infrastructure;

/// <summary>Registro en el contenedor de DI de la capa de infraestructura.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra el acceso a datos (EF Core + SQL Server) y los servicios de
    /// infraestructura. La cadena de conexión se lee de <c>ConnectionStrings:Default</c>.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'ConnectionStrings:Default'.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

        return services;
    }
}
