using Inventario.Api.Seguridad;
using Inventario.Application.Partners;

namespace Inventario.Api.Endpoints;

public static class PartnersEndpoints
{
    public static IEndpointRouteBuilder MapPartnersEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/tenants")
            .WithTags("Partners")
            .RequireAuthorization(Politicas.Superadmin);

        grupo.MapPost("/", async (
            CrearPartnerRequest request,
            PartnersService servicio,
            CancellationToken ct) =>
        {
            var creado = await servicio.CrearAsync(request, ct);
            return Results.Created($"/api/tenants/{creado.TenantId}", creado);
        })
        .WithSummary("Da de alta un partner y su usuario administrador (solo superadmin).");

        return app;
    }
}
