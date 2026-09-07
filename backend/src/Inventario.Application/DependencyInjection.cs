using Inventario.Application.Catalogo;
using Inventario.Application.Identidad;
using Inventario.Application.Partners;
using Inventario.Application.Sucursales;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Application;

/// <summary>Registro en el contenedor de DI de la capa de aplicación.</summary>
public static class DependencyInjection
{
    /// <summary>Registra los casos de uso de la capa de aplicación.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AutenticacionService>();
        services.AddScoped<SucursalesService>();
        services.AddScoped<PartnersService>();
        services.AddScoped<CategoriasService>();
        services.AddScoped<UnidadesMedidaService>();
        return services;
    }
}
