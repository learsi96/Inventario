using Inventario.Api.Seguridad;
using Inventario.Application.Catalogo;
using Inventario.Domain.Catalogo;

namespace Inventario.Api.Endpoints;

public static class ArticulosEndpoints
{
    public static IEndpointRouteBuilder MapArticulosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/articulos")
            .WithTags("Artículos")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/", async (
            string? texto,
            Guid? categoriaId,
            EstadoArticulo? estado,
            int? pagina,
            int? tamano,
            ArticulosService servicio,
            CancellationToken ct) =>
        {
            var filtro = new FiltroArticulos(
                texto, categoriaId, estado, pagina ?? 1, tamano ?? 20);
            return Results.Ok(await servicio.ListarAsync(filtro, ct));
        })
        .WithSummary("Lista artículos con búsqueda, filtros y paginación.");

        grupo.MapGet("/{id:guid}", async (Guid id, ArticulosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ObtenerAsync(id, ct)))
            .WithSummary("Ficha completa de un artículo.");

        grupo.MapPost("/", async (
            CrearArticuloRequest request, ArticulosService servicio, CancellationToken ct) =>
        {
            var creado = await servicio.CrearAsync(request, ct);
            return Results.Created($"/api/articulos/{creado.Id}", creado);
        })
        .RequireAuthorization(Politicas.AdminPartner)
        .WithSummary("Crea un artículo (solo Administrador).");

        grupo.MapPut("/{id:guid}", async (
            Guid id, ActualizarArticuloRequest request, ArticulosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ActualizarAsync(id, request, ct)))
            .RequireAuthorization(Politicas.AdminPartner)
            .WithSummary("Actualiza un artículo (solo Administrador).");

        grupo.MapPatch("/{id:guid}/estado", async (
            Guid id, CambiarEstadoArticuloRequest request, ArticulosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.CambiarEstadoAsync(id, request.Estado, ct)))
            .RequireAuthorization(Politicas.AdminPartner)
            .WithSummary("Cambia el estado (Activo/Descontinuado) de un artículo.");

        return app;
    }
}
