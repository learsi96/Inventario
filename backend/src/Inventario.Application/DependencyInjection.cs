using Inventario.Application.Catalogo;
using Inventario.Application.Conteos;
using Inventario.Application.Existencias;
using Inventario.Application.Identidad;
using Inventario.Application.Movimientos;
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
        services.AddScoped<UsuariosService>();
        services.AddScoped<SucursalesService>();
        services.AddScoped<PartnersService>();
        services.AddScoped<CategoriasService>();
        services.AddScoped<UnidadesMedidaService>();
        services.AddScoped<ArticulosService>();
        services.AddScoped<UbicacionesService>();
        services.AddScoped<ExistenciasService>();
        services.AddScoped<MotorExistencias>();
        services.AddScoped<FoliosService>();
        services.AddScoped<MovimientosService>();
        services.AddScoped<TransferenciasService>();
        services.AddScoped<ConteosService>();
        return services;
    }
}
