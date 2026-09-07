using Inventario.Api.Seguridad;
using Inventario.Application.Existencias;

namespace Inventario.Api.Endpoints;

public static class ExistenciasEndpoints
{
    public static IEndpointRouteBuilder MapExistenciasEndpoints(this IEndpointRouteBuilder app)
    {
        MapUbicaciones(app);
        MapExistencias(app);
        return app;
    }

    private static void MapUbicaciones(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/ubicaciones")
            .WithTags("Ubicaciones")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/", async (
            Guid? sucursalId, bool? incluirInactivas, UbicacionesService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ListarAsync(sucursalId, incluirInactivas ?? false, ct)))
            .WithSummary("Lista ubicaciones (opcionalmente por sucursal).");

        grupo.MapPost("/", async (
            CrearUbicacionRequest request, UbicacionesService servicio, CancellationToken ct) =>
        {
            var creada = await servicio.CrearAsync(request, ct);
            return Results.Created($"/api/ubicaciones/{creada.Id}", creada);
        })
        .RequireAuthorization(Politicas.AdminPartner)
        .WithSummary("Crea una ubicación (solo Administrador).");

        grupo.MapPut("/{id:guid}", async (
            Guid id, ActualizarUbicacionRequest request, UbicacionesService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ActualizarAsync(id, request, ct)))
            .RequireAuthorization(Politicas.AdminPartner)
            .WithSummary("Actualiza una ubicación (solo Administrador).");
    }

    private static void MapExistencias(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/existencias")
            .WithTags("Existencias")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/", async (
            Guid? sucursalId,
            Guid? categoriaId,
            string? texto,
            bool? soloBajoMinimo,
            int? pagina,
            int? tamano,
            ExistenciasService servicio,
            CancellationToken ct) =>
        {
            var filtro = new FiltroExistencias(
                sucursalId, categoriaId, texto, soloBajoMinimo ?? false, pagina ?? 1, tamano ?? 20);
            return Results.Ok(await servicio.ListarAsync(filtro, ct));
        })
        .WithSummary("Lista existencias con filtros (sucursal, categoría, bajo mínimo).");

        grupo.MapGet("/valorizacion", async (
            Guid? sucursalId, ExistenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ValorizacionAsync(sucursalId, ct)))
            .WithSummary("Valorización del inventario, total y por sucursal.");

        grupo.MapGet("/{articuloId:guid}/{sucursalId:guid}", async (
            Guid articuloId, Guid sucursalId, ExistenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ObtenerAsync(articuloId, sucursalId, ct)))
            .WithSummary("Existencia de un artículo en una sucursal, con desglose por ubicación.");

        grupo.MapPost("/ajuste", async (
            AjusteExistenciaRequest request, ExistenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.AjustarAsync(request, ct)))
            .RequireAuthorization(Politicas.OperadorAlmacen)
            .WithSummary("Fija cantidad y costo de un artículo en una sucursal (carga inicial / ajuste).");

        grupo.MapPut("/parametros", async (
            ParametrosReordenRequest request, ExistenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.GuardarParametrosAsync(request, ct)))
            .RequireAuthorization(Politicas.AdminPartner)
            .WithSummary("Guarda mínimo, máximo y punto de reorden (solo Administrador).");

        grupo.MapPut("/ubicaciones", async (
            AsignarUbicacionesRequest request, ExistenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.AsignarUbicacionesAsync(request, ct)))
            .RequireAuthorization(Politicas.OperadorAlmacen)
            .WithSummary("Distribuye la existencia de la sucursal entre ubicaciones.");
    }
}
