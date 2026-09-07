namespace Inventario.Infrastructure.Tenancy;

/// <summary>
/// Provee el identificador del partner (tenant) de la petición en curso.
/// La implementación real —resolución desde el <c>tenant</c> claim del JWT—
/// llega en el Hito 1.
/// </summary>
public interface ITenantContext
{
    /// <summary>Tenant actual, o <c>null</c> si la petición no está autenticada.</summary>
    Guid? TenantId { get; }
}
