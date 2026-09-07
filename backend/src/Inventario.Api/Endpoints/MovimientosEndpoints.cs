using Inventario.Api.Seguridad;
using Inventario.Application.Movimientos;
using Inventario.Domain.Movimientos;

namespace Inventario.Api.Endpoints;

public static class MovimientosEndpoints
{
    public static IEndpointRouteBuilder MapMovimientosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/movimientos")
            .WithTags("Movimientos")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/", async (
            Guid? sucursalId,
            TipoMovimiento? tipo,
            DateTimeOffset? desde,
            DateTimeOffset? hasta,
            string? texto,
            int? pagina,
            int? tamano,
            MovimientosService servicio,
            CancellationToken ct) =>
        {
            var filtro = new FiltroMovimientos(
                sucursalId, tipo, desde, hasta, texto, pagina ?? 1, tamano ?? 20);
            return Results.Ok(await servicio.ListarAsync(filtro, ct));
        })
        .WithSummary("Lista movimientos con filtros y paginación.");

        grupo.MapGet("/{id:guid}", async (Guid id, MovimientosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ObtenerAsync(id, ct)))
            .WithSummary("Detalle de un movimiento con sus renglones.");

        grupo.MapGet("/kardex/{articuloId:guid}", async (
            Guid articuloId,
            Guid? sucursalId,
            DateTimeOffset? desde,
            DateTimeOffset? hasta,
            MovimientosService servicio,
            CancellationToken ct) =>
            Results.Ok(await servicio.KardexAsync(articuloId, sucursalId, desde, hasta, ct)))
            .WithSummary("Kardex (histórico de movimientos) de un artículo.");

        grupo.MapPost("/entrada", async (
            RegistrarEntradaRequest request, MovimientosService servicio, CancellationToken ct) =>
        {
            var creado = await servicio.RegistrarEntradaAsync(request, ct);
            return Results.Created($"/api/movimientos/{creado.Id}", creado);
        })
        .RequireAuthorization(Politicas.OperadorAlmacen)
        .WithSummary("Registra una entrada de inventario (recalcula el costo promedio).");

        grupo.MapPost("/salida", async (
            RegistrarSalidaRequest request, MovimientosService servicio, CancellationToken ct) =>
        {
            var creado = await servicio.RegistrarSalidaAsync(request, ct);
            return Results.Created($"/api/movimientos/{creado.Id}", creado);
        })
        .RequireAuthorization(Politicas.OperadorAlmacen)
        .WithSummary("Registra una salida o merma de inventario.");

        return app;
    }
}
