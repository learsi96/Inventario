using Inventario.Domain.Common;
using Inventario.Domain.Tenancy;

namespace Inventario.Domain.Catalogo;

public enum EstadoArticulo
{
    Activo = 1,
    Descontinuado = 2,
}

public enum TipoCodigoAlterno
{
    Oem = 1,
    Proveedor = 2,
    Equivalencia = 3,
    Interno = 4,
}

/// <summary>Refacción o producto del catálogo de un partner.</summary>
public class Articulo : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    /// <summary>Clave interna única por partner (autogenerada, editable).</summary>
    public required string Sku { get; set; }

    /// <summary>Código de barras del fabricante (opcional, único por partner).</summary>
    public string? CodigoBarras { get; set; }

    public required string Nombre { get; set; }

    public string? Descripcion { get; set; }

    public string? Marca { get; set; }

    /// <summary>Número de parte OEM (el identificador cruzado más habitual).</summary>
    public string? NumeroParteOem { get; set; }

    public Guid CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public Guid UnidadMedidaId { get; set; }

    public UnidadMedida UnidadMedida { get; set; } = null!;

    /// <summary>Costo de referencia (último conocido). El costo real por promedio ponderado
    /// se mantiene por sucursal en el Hito 3.</summary>
    public decimal Costo { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal IvaPorcentaje { get; set; } = 16m;

    public EstadoArticulo Estado { get; set; } = EstadoArticulo.Activo;

    /// <summary>Nombre del archivo de imagen en el almacén (se resuelve en 2c).</summary>
    public string? ImagenNombre { get; set; }

    public ICollection<CodigoAlterno> CodigosAlternos { get; } = [];
}

/// <summary>Código adicional por el que también se identifica un artículo.</summary>
public class CodigoAlterno : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid ArticuloId { get; set; }

    public Articulo Articulo { get; set; } = null!;

    public required string Codigo { get; set; }

    public TipoCodigoAlterno Tipo { get; set; } = TipoCodigoAlterno.Equivalencia;
}
