using Inventario.Application.Abstractions;
using Inventario.Infrastructure.Archivos;
using Inventario.Infrastructure.Identidad;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Infrastructure;

/// <summary>Registro en el contenedor de DI de la capa de infraestructura.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra el acceso a datos (EF Core + SQL Server), el hash de contraseñas y
    /// la emisión de tokens. La cadena de conexión se lee de
    /// <c>ConnectionStrings:Default</c> y las opciones de JWT de la sección <c>Jwt</c>.
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

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();

        services.AddOptions<ArchivosOptions>().Bind(configuration.GetSection(ArchivosOptions.Seccion));
        services.AddSingleton<IAlmacenArchivos, AlmacenArchivosLocal>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.Seccion))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
