using Inventario.Api.Seguridad;
using Inventario.Application.Conteos;
using Inventario.Domain.Conteos;

namespace Inventario.Api.Endpoints;

public static class ConteosEndpoints
{
    public static IEndpointRouteBuilder MapConteosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/conteos")
            .WithTags("Conteos")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/", async (
            Guid? sucursalId, EstadoConteo? estado, ConteosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ListarAsync(sucursalId, estado, ct)))
            .WithSummary("Lista conteos físicos.");

        grupo.MapGet("/{id:guid}", async (Guid id, ConteosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ObtenerAsync(id, ct)))
            .WithSummary("Detalle de un conteo con sus diferencias.");

        var escritura = grupo.MapGroup("").RequireAuthorization(Politicas.OperadorAlmacen);

        escritura.MapPost("/", async (
            IniciarConteoRequest request, ConteosService servicio, CancellationToken ct) =>
        {
            var creado = await servicio.IniciarAsync(request, ct);
            return Results.Created($"/api/conteos/{creado.Id}", creado);
        })
        .WithSummary("Inicia un conteo (fotografía las existencias de la sucursal).");

        escritura.MapPut("/{id:guid}/captura", async (
            Guid id, CapturaConteoRequest request, ConteosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.CapturarAsync(id, request, ct)))
            .WithSummary("Captura cantidades contadas (incremental).");

        escritura.MapPost("/{id:guid}/conciliar", async (
            Guid id, ConteosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ConciliarAsync(id, ct)))
            .WithSummary("Concilia: ajusta las existencias a lo contado y genera el movimiento.");

        escritura.MapPost("/{id:guid}/cancelar", async (
            Guid id, ConteosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.CancelarAsync(id, ct)))
            .WithSummary("Cancela un conteo en progreso.");

        return app;
    }
}
