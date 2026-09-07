using Inventario.Api.Seguridad;
using Inventario.Application.Sucursales;

namespace Inventario.Api.Endpoints;

public static class SucursalesEndpoints
{
    public static IEndpointRouteBuilder MapSucursalesEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/sucursales")
            .WithTags("Sucursales")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/", async (
            bool? incluirInactivas,
            SucursalesService servicio,
            CancellationToken ct) =>
        {
            var sucursales = await servicio.ListarAsync(incluirInactivas ?? false, ct);
            return Results.Ok(sucursales);
        })
        .WithSummary("Lista las sucursales del partner.");

        grupo.MapGet("/{id:guid}", async (Guid id, SucursalesService servicio, CancellationToken ct) =>
        {
            var sucursal = await servicio.ObtenerAsync(id, ct);
            return Results.Ok(sucursal);
        })
        .WithSummary("Obtiene una sucursal por id.");

        grupo.MapPost("/", async (
            CrearSucursalRequest request,
            SucursalesService servicio,
            CancellationToken ct) =>
        {
            var creada = await servicio.CrearAsync(request, ct);
            return Results.Created($"/api/sucursales/{creada.Id}", creada);
        })
        .RequireAuthorization(Politicas.AdminPartner)
        .WithSummary("Crea una sucursal (solo Administrador).");

        grupo.MapPut("/{id:guid}", async (
            Guid id,
            ActualizarSucursalRequest request,
            SucursalesService servicio,
            CancellationToken ct) =>
        {
            var actualizada = await servicio.ActualizarAsync(id, request, ct);
            return Results.Ok(actualizada);
        })
        .RequireAuthorization(Politicas.AdminPartner)
        .WithSummary("Actualiza una sucursal (solo Administrador).");

        grupo.MapPatch("/{id:guid}/activacion", async (
            Guid id,
            CambiarActivacionRequest request,
            SucursalesService servicio,
            CancellationToken ct) =>
        {
            var resultado = await servicio.CambiarActivacionAsync(id, request.Activa, ct);
            return Results.Ok(resultado);
        })
        .RequireAuthorization(Politicas.AdminPartner)
        .WithSummary("Activa o desactiva una sucursal (solo Administrador).");

        return app;
    }
}
