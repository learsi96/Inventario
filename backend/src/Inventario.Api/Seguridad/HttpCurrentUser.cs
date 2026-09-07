using System.Security.Claims;
using Inventario.Application.Abstractions;
using Inventario.Domain.Identidad;

namespace Inventario.Api.Seguridad;

/// <summary>Resuelve <see cref="ICurrentUser"/> desde los claims del JWT de la petición.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? UsuarioId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Principal?.FindFirstValue("sub"), out var id)
            ? id
            : null;

    public Guid? TenantId =>
        Guid.TryParse(Principal?.FindFirstValue("tenant"), out var id) ? id : null;

    public RolUsuario? Rol =>
        Enum.TryParse<RolUsuario>(Principal?.FindFirstValue("role"), out var rol) ? rol : null;

    public bool EsSuperadmin =>
        string.Equals(Principal?.FindFirstValue("superadmin"), "true", StringComparison.OrdinalIgnoreCase);

    public bool EstaAutenticado => Principal?.Identity?.IsAuthenticated ?? false;
}
