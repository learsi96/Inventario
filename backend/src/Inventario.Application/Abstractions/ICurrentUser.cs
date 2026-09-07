using Inventario.Domain.Identidad;

namespace Inventario.Application.Abstractions;

/// <summary>
/// Datos del usuario autenticado en la petición en curso, resueltos desde el JWT.
/// La implementación vive en la capa de API (usa <c>IHttpContextAccessor</c>).
/// </summary>
public interface ICurrentUser
{
    /// <summary>Id del usuario, o <c>null</c> si la petición no está autenticada.</summary>
    Guid? UsuarioId { get; }

    /// <summary>Partner (tenant) del usuario, o <c>null</c> si no aplica (p. ej. superadmin).</summary>
    Guid? TenantId { get; }

    RolUsuario? Rol { get; }

    /// <summary>El usuario es superadministrador de la plataforma (fuera de un partner).</summary>
    bool EsSuperadmin { get; }

    bool EstaAutenticado { get; }
}
