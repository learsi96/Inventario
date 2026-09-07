using Inventario.Domain.Catalogo;
using Inventario.Domain.Common;
using Inventario.Domain.Sucursales;
using Inventario.Domain.Tenancy;

namespace Inventario.Domain.Existencias;

/// <summary>
/// Existencia de un artículo en una sucursal: cantidad, costo promedio ponderado
/// y parámetros de reorden. Una fila por (Articulo, Sucursal), creada bajo demanda.
/// Ver <c>docs/decisions/0006-modelo-de-existencias.md</c>.
/// </summary>
public class Existencia : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid ArticuloId { get; set; }

    public Articulo Articulo { get; set; } = null!;

    public Guid SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public decimal CostoPromedio { get; set; }

    public decimal Minimo { get; set; }

    public decimal Maximo { get; set; }

    public decimal PuntoReorden { get; set; }

    /// <summary>Desglose de <see cref="Cantidad"/> por ubicación (opcional).</summary>
    public ICollection<ExistenciaUbicacion> PorUbicacion { get; } = [];

    public decimal Valor => Cantidad * CostoPromedio;

    public bool BajoMinimo => Minimo > 0 && Cantidad <= Minimo;
}

/// <summary>Cantidad de una existencia asignada a una ubicación concreta.</summary>
public class ExistenciaUbicacion : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid ExistenciaId { get; set; }

    public Existencia Existencia { get; set; } = null!;

    public Guid UbicacionId { get; set; }

    public Ubicacion Ubicacion { get; set; } = null!;

    public decimal Cantidad { get; set; }
}
