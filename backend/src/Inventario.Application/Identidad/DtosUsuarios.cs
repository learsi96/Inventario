using Inventario.Domain.Identidad;

namespace Inventario.Application.Identidad;

public sealed record UsuarioDto(
    Guid Id,
    string Email,
    string NombreCompleto,
    RolUsuario Rol,
    bool Activo,
    IReadOnlyList<Guid> SucursalIds,
    bool EsUsuarioActual);

public sealed record CrearUsuarioRequest(
    string Email,
    string NombreCompleto,
    RolUsuario Rol,
    string Contrasena,
    IReadOnlyList<Guid>? SucursalIds);

public sealed record ActualizarUsuarioRequest(
    string NombreCompleto,
    RolUsuario Rol,
    bool Activo,
    IReadOnlyList<Guid>? SucursalIds);

public sealed record ResetContrasenaRequest(string Contrasena);
