using Inventario.Domain.Catalogo;

namespace Inventario.Application.Catalogo;

public sealed record CodigoAlternoDto(string Codigo, TipoCodigoAlterno Tipo);

/// <summary>Fila de la lista de artículos (ligera).</summary>
public sealed record ArticuloListaDto(
    Guid Id,
    string Sku,
    string Nombre,
    string? Marca,
    string CategoriaNombre,
    string UnidadCodigo,
    decimal PrecioVenta,
    EstadoArticulo Estado,
    bool TieneImagen);

/// <summary>Ficha completa del artículo.</summary>
public sealed record ArticuloDto(
    Guid Id,
    string Sku,
    string? CodigoBarras,
    string Nombre,
    string? Descripcion,
    string? Marca,
    string? NumeroParteOem,
    Guid CategoriaId,
    string CategoriaNombre,
    Guid UnidadMedidaId,
    string UnidadCodigo,
    decimal Costo,
    decimal PrecioVenta,
    decimal IvaPorcentaje,
    EstadoArticulo Estado,
    bool TieneImagen,
    IReadOnlyList<CodigoAlternoDto> CodigosAlternos,
    DateTimeOffset CreadoEn);

public sealed record CrearArticuloRequest(
    string? Sku,
    string? CodigoBarras,
    string Nombre,
    string? Descripcion,
    string? Marca,
    string? NumeroParteOem,
    Guid? CategoriaId,
    Guid UnidadMedidaId,
    decimal Costo,
    decimal PrecioVenta,
    decimal? IvaPorcentaje,
    IReadOnlyList<CodigoAlternoDto>? CodigosAlternos);

public sealed record ActualizarArticuloRequest(
    string Sku,
    string? CodigoBarras,
    string Nombre,
    string? Descripcion,
    string? Marca,
    string? NumeroParteOem,
    Guid? CategoriaId,
    Guid UnidadMedidaId,
    decimal Costo,
    decimal PrecioVenta,
    decimal? IvaPorcentaje,
    IReadOnlyList<CodigoAlternoDto>? CodigosAlternos);

public sealed record CambiarEstadoArticuloRequest(EstadoArticulo Estado);

/// <summary>Filtros y paginación del listado de artículos.</summary>
public sealed record FiltroArticulos(
    string? Texto,
    Guid? CategoriaId,
    EstadoArticulo? Estado,
    int Pagina = 1,
    int Tamano = 20);
