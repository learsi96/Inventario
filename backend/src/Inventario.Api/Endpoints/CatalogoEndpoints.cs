using Inventario.Api.Seguridad;
using Inventario.Application.Catalogo;

namespace Inventario.Api.Endpoints;

public static class CatalogoEndpoints
{
    public static IEndpointRouteBuilder MapCatalogoEndpoints(this IEndpointRouteBuilder app)
    {
        MapCategorias(app);
        MapUnidadesMedida(app);
        return app;
    }

    private static void MapCategorias(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/categorias")
            .WithTags("Categorías")
            .RequireAuthorization(Politicas.UsuarioPartner);

        grupo.MapGet("/", async (
            bool? incluirInactivas, CategoriasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ArbolAsync(incluirInactivas ?? false, ct)))
            .WithSummary("Devuelve el árbol de categorías del partner.");

        grupo.MapPost("/", async (
            CrearCategoriaRequest request, CategoriasService servicio, CancellationToken ct) =>
        {
            var creada = await servicio.CrearAsync(request, ct);
            return Results.Created($"/api/categorias/{creada.Id}", creada);
        })
        .RequireAuthorization(Politicas.AdminPartner)
        .WithSummary("Crea una categoría (solo Administrador).");

        grupo.MapPut("/{id:guid}", async (
            Guid id, ActualizarCategoriaRequest request, CategoriasService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ActualizarAsync(id, request, ct)))
            .RequireAuthorization(Politicas.AdminPartner)
            .WithSummary("Actualiza una categoría (solo Administrador).");

        grupo.MapPatch("/{id:guid}/activacion", async (
            Guid id, CambiarActivacionCategoriaRequest request, CategoriasService servicio, CancellationToken ct) =>
        {
            await servicio.CambiarActivacionAsync(id, request.Activa, ct);
            return Results.NoContent();
        })
        .RequireAuthorization(Politicas.AdminPartner)
        .WithSummary("Activa o desactiva una categoría (solo Administrador).");
    }

    private static void MapUnidadesMedida(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/unidades-medida", async (UnidadesMedidaService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ListarAsync(ct)))
            .WithTags("Catálogo")
            .RequireAuthorization(Politicas.UsuarioPartner)
            .WithSummary("Lista el catálogo de unidades de medida.");
    }
}

public sealed record CambiarActivacionCategoriaRequest(bool Activa);
