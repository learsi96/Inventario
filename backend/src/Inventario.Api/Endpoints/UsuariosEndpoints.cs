using Inventario.Api.Seguridad;
using Inventario.Application.Identidad;

namespace Inventario.Api.Endpoints;

public static class UsuariosEndpoints
{
    public static IEndpointRouteBuilder MapUsuariosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization(Politicas.AdminPartner);

        grupo.MapGet("/", async (UsuariosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ListarAsync(ct)))
            .WithSummary("Lista los usuarios del partner.");

        grupo.MapGet("/{id:guid}", async (Guid id, UsuariosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ObtenerAsync(id, ct)))
            .WithSummary("Ficha de un usuario.");

        grupo.MapPost("/", async (
            CrearUsuarioRequest request, UsuariosService servicio, CancellationToken ct) =>
        {
            var creado = await servicio.CrearAsync(request, ct);
            return Results.Created($"/api/usuarios/{creado.Id}", creado);
        })
        .WithSummary("Crea un usuario del partner.");

        grupo.MapPut("/{id:guid}", async (
            Guid id, ActualizarUsuarioRequest request, UsuariosService servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ActualizarAsync(id, request, ct)))
            .WithSummary("Actualiza nombre, rol, estado y sucursales de un usuario.");

        grupo.MapPatch("/{id:guid}/activacion", async (
            Guid id, CambiarActivacionUsuarioRequest request, UsuariosService servicio, CancellationToken ct) =>
        {
            await servicio.CambiarActivacionAsync(id, request.Activo, ct);
            return Results.NoContent();
        })
        .WithSummary("Activa o desactiva un usuario.");

        grupo.MapPost("/{id:guid}/contrasena", async (
            Guid id, ResetContrasenaRequest request, UsuariosService servicio, CancellationToken ct) =>
        {
            await servicio.ResetContrasenaAsync(id, request.Contrasena, ct);
            return Results.NoContent();
        })
        .WithSummary("Restablece la contraseña de un usuario.");

        return app;
    }
}

public sealed record CambiarActivacionUsuarioRequest(bool Activo);
