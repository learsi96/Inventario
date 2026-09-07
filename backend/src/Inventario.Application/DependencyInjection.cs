using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Application;

/// <summary>Registro en el contenedor de DI de la capa de aplicación.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa de aplicación (casos de uso, validadores,
    /// comportamientos). Se poblará a partir del Hito 1.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
