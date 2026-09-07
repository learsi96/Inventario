namespace Inventario.Application.Partners;

public sealed record CrearPartnerRequest(
    string Nombre,
    string Codigo,
    string AdminEmail,
    string AdminNombreCompleto,
    string AdminContrasena);

public sealed record PartnerCreadoDto(Guid TenantId, Guid AdminUsuarioId, string Codigo);
