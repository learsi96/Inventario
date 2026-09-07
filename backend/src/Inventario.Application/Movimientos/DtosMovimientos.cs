using Inventario.Domain.Movimientos;

namespace Inventario.Application.Movimientos;

public sealed record RenglonEntradaRequest(Guid ArticuloId, decimal Cantidad, decimal CostoUnitario);

public sealed record RenglonSalidaRequest(Guid ArticuloId, decimal Cantidad);

public sealed record RegistrarEntradaRequest(
    Guid SucursalId,
    string? Referencia,
    string? Motivo,
    IReadOnlyList<RenglonEntradaRequest> Renglones);

public sealed record RegistrarSalidaRequest(
    Guid SucursalId,
    TipoMovimiento Tipo,
    string Motivo,
    IReadOnlyList<RenglonSalidaRequest> Renglones);

public sealed record RenglonMovimientoDto(
    Guid ArticuloId,
    string ArticuloSku,
    string ArticuloNombre,
    decimal Cantidad,
    decimal CostoUnitario,
    decimal CantidadResultante,
    decimal CostoPromedioResultante);

public sealed record MovimientoDto(
    Guid Id,
    int Folio,
    TipoMovimiento Tipo,
    DateTimeOffset Fecha,
    Guid SucursalId,
    string SucursalNombre,
    Guid? SucursalDestinoId,
    string? SucursalDestinoNombre,
    EstadoTransferencia? EstadoTransferencia,
    string? Motivo,
    string? Referencia,
    string UsuarioNombre,
    IReadOnlyList<RenglonMovimientoDto> Renglones);

public sealed record MovimientoListaDto(
    Guid Id,
    int Folio,
    TipoMovimiento Tipo,
    DateTimeOffset Fecha,
    string SucursalNombre,
    string? Motivo,
    string? Referencia,
    string UsuarioNombre,
    int Renglones,
    decimal Total);

public sealed record FiltroMovimientos(
    Guid? SucursalId,
    TipoMovimiento? Tipo,
    DateTimeOffset? Desde,
    DateTimeOffset? Hasta,
    string? Texto,
    int Pagina = 1,
    int Tamano = 20);

public sealed record KardexRenglonDto(
    Guid MovimientoId,
    int Folio,
    TipoMovimiento Tipo,
    DateTimeOffset Fecha,
    string SucursalNombre,
    string? Motivo,
    decimal Cantidad,
    decimal CostoUnitario,
    decimal CantidadResultante,
    decimal CostoPromedioResultante);
