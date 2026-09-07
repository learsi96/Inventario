using Inventario.Domain.Common;
using Inventario.Domain.Sucursales;
using Inventario.Domain.Tenancy;

namespace Inventario.Domain.Existencias;

/// <summary>Posición física dentro de una sucursal (pasillo/anaquel/nivel), estructura plana.</summary>
public class Ubicacion : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;

    /// <summary>Código único dentro de la sucursal (p. ej. "P1-A3-N2").</summary>
    public required string Codigo { get; set; }

    public string? Descripcion { get; set; }

    public bool Activa { get; set; } = true;
}
