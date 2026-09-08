using Inventario.Api.Seguridad;
using Inventario.Application.Movimientos;
using Inventario.Domain.Movimientos;

namespace Inventario.Api.Endpoints;

public static class TransferenciasEndpoints
{
    public static IEndpointRouteBuilder MapTransferenciasEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/transferencias")
            .WithTags("Transferencias")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/", async (
            Guid? sucursalId,
            EstadoTransferencia? estado,
            int? pagina,
            int? tamano,
            TransferenciasService servicio,
            CancellationToken ct) =>
            Results.Ok(await servicio.ListarAsync(
                sucursalId, estado, pagina ?? 1, tamano ?? 20, ct)))
            .WithSummary("Lista transferencias con filtros.");

        grupo.MapGet("/{id:guid}", async (Guid id, TransferenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ObtenerAsync(id, ct)))
            .WithSummary("Detalle de una transferencia (enviado vs recibido).");

        var escritura = grupo.MapGroup("").RequireAuthorization(Politicas.OperadorAlmacen);

        escritura.MapPost("/", async (
            SolicitarTransferenciaRequest request, TransferenciasService servicio, CancellationToken ct) =>
        {
            var creada = await servicio.SolicitarAsync(request, ct);
            return Results.Created($"/api/transferencias/{creada.Id}", creada);
        })
        .WithSummary("Solicita una transferencia (no mueve existencia todavía).");

        escritura.MapPost("/{id:guid}/enviar", async (
            Guid id, TransferenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.EnviarAsync(id, ct)))
            .WithSummary("Marca en tránsito: descuenta la existencia del origen.");

        escritura.MapPost("/{id:guid}/recibir", async (
            Guid id, RecibirTransferenciaRequest request, TransferenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.RecibirAsync(id, request, ct)))
            .WithSummary("Recibe en destino (con cantidades recibidas por renglón).");

        escritura.MapPost("/{id:guid}/cancelar", async (
            Guid id, CancelarTransferenciaRequest request, TransferenciasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.CancelarAsync(id, request.Motivo, ct)))
            .WithSummary("Cancela una transferencia solicitada o en tránsito.");

        return app;
    }
}

public sealed record CancelarTransferenciaRequest(string? Motivo);
