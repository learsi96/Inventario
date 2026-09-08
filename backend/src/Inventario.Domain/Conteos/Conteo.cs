using Inventario.Domain.Catalogo;
using Inventario.Domain.Common;
using Inventario.Domain.Sucursales;
using Inventario.Domain.Tenancy;

namespace Inventario.Domain.Conteos;

public enum EstadoConteo
{
    EnProgreso = 1,
    Conciliado = 2,
    Cancelado = 3,
}

/// <summary>
/// Toma de inventario físico de una sucursal (opcionalmente acotada a una categoría).
/// Al iniciarse fotografía las existencias; al conciliarse ajusta las diferencias.
/// </summary>
public class Conteo : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public int Folio { get; set; }

    public Guid SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;

    public Guid? CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    public EstadoConteo Estado { get; set; } = EstadoConteo.EnProgreso;

    public DateTimeOffset? ConciliadoEn { get; set; }

    public Guid UsuarioId { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;

    /// <summary>Movimiento de ajuste generado al conciliar (si hubo diferencias).</summary>
    public Guid? MovimientoAjusteId { get; set; }

    public ICollection<ConteoDetalle> Detalles { get; } = [];
}

public class ConteoDetalle : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid ConteoId { get; set; }

    public Conteo Conteo { get; set; } = null!;

    public Guid ArticuloId { get; set; }

    public Articulo Articulo { get; set; } = null!;

    /// <summary>Existencia según el sistema al iniciar el conteo.</summary>
    public decimal CantidadSistema { get; set; }

    public decimal? CantidadContada { get; set; }

    public decimal Diferencia => (CantidadContada ?? CantidadSistema) - CantidadSistema;
}
