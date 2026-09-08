using Inventario.Domain.Conteos;

namespace Inventario.Application.Conteos;

public sealed record IniciarConteoRequest(Guid SucursalId, Guid? CategoriaId);

public sealed record CapturaConteoRequest(
    IReadOnlyList<CapturaRenglonRequest> Renglones);

public sealed record CapturaRenglonRequest(Guid ArticuloId, decimal CantidadContada);

public sealed record ConteoDetalleDto(
    Guid ArticuloId,
    string ArticuloSku,
    string ArticuloNombre,
    decimal CantidadSistema,
    decimal? CantidadContada,
    decimal Diferencia);

public sealed record ConteoDto(
    Guid Id,
    int Folio,
    EstadoConteo Estado,
    Guid SucursalId,
    string SucursalNombre,
    Guid? CategoriaId,
    string? CategoriaNombre,
    DateTimeOffset CreadoEn,
    DateTimeOffset? ConciliadoEn,
    string UsuarioNombre,
    Guid? MovimientoAjusteId,
    int Contados,
    int ConDiferencia,
    decimal DiferenciaNeta,
    IReadOnlyList<ConteoDetalleDto> Detalles);

public sealed record ConteoListaDto(
    Guid Id,
    int Folio,
    EstadoConteo Estado,
    string SucursalNombre,
    string? CategoriaNombre,
    DateTimeOffset CreadoEn,
    string UsuarioNombre,
    int Articulos,
    int Contados);
