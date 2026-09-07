using Inventario.Api.Seguridad;
using Inventario.Application.Abstractions;
using Inventario.Application.Identidad;
using Microsoft.Extensions.Options;

namespace Inventario.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/auth").WithTags("Autenticación");

        grupo.MapPost("/login", async (
            LoginRequest request,
            AutenticacionService servicio,
            CancellationToken ct) =>
        {
            var token = await servicio.LoginAsync(request, ct);
            return Results.Ok(token);
        })
        .AllowAnonymous()
        .WithSummary("Inicia sesión y devuelve un token JWT.");

        grupo.MapGet("/me", async (AutenticacionService servicio, CancellationToken ct) =>
        {
            var actual = await servicio.ObtenerActualAsync(ct);
            return Results.Ok(actual);
        })
        .RequireAuthorization()
        .WithSummary("Devuelve los datos del usuario autenticado.");

        grupo.MapPost("/superadmin/login", (
            SuperadminLoginRequest request,
            IOptions<SuperadminOptions> opciones,
            IJwtTokenService jwt) =>
        {
            var cfg = opciones.Value;
            if (string.IsNullOrEmpty(cfg.Email)
                || !string.Equals(request.Email, cfg.Email, StringComparison.OrdinalIgnoreCase)
                || request.Contrasena != cfg.Contrasena)
            {
                return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Credenciales inválidas.");
            }

            var token = jwt.EmitirSuperadmin(cfg.Email);
            return Results.Ok(new TokenResponse(token.AccessToken, token.ExpiraEn));
        })
        .AllowAnonymous()
        .WithSummary("Inicia sesión como superadministrador de la plataforma (config).");

        return app;
    }
}

public sealed record SuperadminLoginRequest(string Email, string Contrasena);
