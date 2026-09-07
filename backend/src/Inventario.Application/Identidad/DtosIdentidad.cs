using Inventario.Domain.Identidad;

namespace Inventario.Application.Identidad;

public sealed record LoginRequest(string Email, string Contrasena);

public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiraEn);

public sealed record UsuarioActualDto(
    Guid Id,
    string Email,
    string NombreCompleto,
    RolUsuario Rol,
    Guid TenantId,
    IReadOnlyList<Guid> SucursalIds);
