using Inventario.Domain.Catalogo;
using Inventario.Domain.Common;
using Inventario.Domain.Sucursales;
using Inventario.Domain.Tenancy;

namespace Inventario.Domain.Movimientos;

public enum TipoMovimiento
{
    AjusteInventario = 1,
    Entrada = 2,
    Salida = 3,
    Merma = 4,
    TransferenciaSalida = 5,
    TransferenciaEntrada = 6,
}

/// <summary>Estado de una transferencia entre sucursales (Hito 5).</summary>
public enum EstadoTransferencia
{
    Solicitada = 1,
    EnTransito = 2,
    Recibida = 3,
    Cancelada = 4,
}

/// <summary>
/// Encabezado de un movimiento de inventario. Fuente del kardex. Cada renglón
/// guarda la cantidad y el costo resultantes tras aplicarse, para reconstruir el
/// histórico sin recalcular.
/// </summary>
public class Movimiento : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>Folio consecutivo por partner.</summary>
    public int Folio { get; set; }

    public TipoMovimiento Tipo { get; set; }

    public DateTimeOffset Fecha { get; set; }

    public Guid SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;

    /// <summary>Sucursal destino (solo transferencias).</summary>
    public Guid? SucursalDestinoId { get; set; }

    public Sucursal? SucursalDestino { get; set; }

    public EstadoTransferencia? EstadoTransferencia { get; set; }

    /// <summary>Movimiento de transferencia hermano (salida ↔ entrada).</summary>
    public Guid? MovimientoRelacionadoId { get; set; }

    public string? Motivo { get; set; }

    public string? Referencia { get; set; }

    public Guid UsuarioId { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;

    public ICollection<MovimientoRenglon> Renglones { get; } = [];
}

/// <summary>Línea de un movimiento: un artículo, su cantidad y su costo.</summary>
public class MovimientoRenglon : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid MovimientoId { get; set; }

    public Movimiento Movimiento { get; set; } = null!;

    public Guid ArticuloId { get; set; }

    public Articulo Articulo { get; set; } = null!;

    /// <summary>Cantidad del renglón. Positiva entra, negativa sale.</summary>
    public decimal Cantidad { get; set; }

    public decimal CostoUnitario { get; set; }

    /// <summary>Existencia de la sucursal tras aplicar este renglón (snapshot para el kardex).</summary>
    public decimal CantidadResultante { get; set; }

    public decimal CostoPromedioResultante { get; set; }
}
