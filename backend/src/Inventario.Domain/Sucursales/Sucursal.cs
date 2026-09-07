using Inventario.Domain.Common;
using Inventario.Domain.Tenancy;

namespace Inventario.Domain.Sucursales;

/// <summary>Sucursal (tienda o punto de operación) de un partner.</summary>
public class Sucursal : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public required string Nombre { get; set; }

    /// <summary>Clave corta única de la sucursal dentro del partner (p. ej. "MATRIZ").</summary>
    public required string Codigo { get; set; }

    public string? Direccion { get; set; }

    public bool Activa { get; set; } = true;
}
