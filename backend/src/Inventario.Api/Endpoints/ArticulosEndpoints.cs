using Inventario.Api.Seguridad;
using Inventario.Application.Catalogo;
using Inventario.Domain.Catalogo;

namespace Inventario.Api.Endpoints;

public static class ArticulosEndpoints
{
    private static readonly string[] ExtensionesImagen = [".jpg", ".jpeg", ".png", ".webp"];
    private const long TamanoMaximoImagen = 5 * 1024 * 1024;

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

        grupo.MapGet("/{id:guid}/imagen", async (
            Guid id, ArticulosService servicio, CancellationToken ct) =>
        {
            var imagen = await servicio.ObtenerImagenAsync(id, ct);
            return imagen is null
                ? Results.NotFound()
                : Results.Stream(imagen.Contenido, imagen.ContentType);
        })
        .WithSummary("Devuelve la imagen del artículo.");

        grupo.MapPost("/{id:guid}/imagen", async (
            Guid id, IFormFile archivo, ArticulosService servicio, CancellationToken ct) =>
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesImagen.Contains(extension))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Formato no permitido. Usa JPG, PNG o WEBP.");
            }
            if (archivo.Length is 0 or > TamanoMaximoImagen)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "La imagen debe pesar entre 1 byte y 5 MB.");
            }

            await using var contenido = archivo.OpenReadStream();
            return Results.Ok(await servicio.SubirImagenAsync(id, contenido, extension, ct));
        })
        .RequireAuthorization(Politicas.AdminPartner)
        .DisableAntiforgery()
        .WithSummary("Sube o reemplaza la imagen del artículo (solo Administrador).");

        grupo.MapDelete("/{id:guid}/imagen", async (
            Guid id, ArticulosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.EliminarImagenAsync(id, ct)))
            .RequireAuthorization(Politicas.AdminPartner)
            .WithSummary("Elimina la imagen del artículo (solo Administrador).");

        return app;
    }
}
