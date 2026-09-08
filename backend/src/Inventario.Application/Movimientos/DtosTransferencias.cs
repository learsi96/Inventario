using Inventario.Domain.Movimientos;

namespace Inventario.Application.Movimientos;

public sealed record RenglonTransferenciaRequest(Guid ArticuloId, decimal Cantidad);

public sealed record SolicitarTransferenciaRequest(
    Guid SucursalOrigenId,
    Guid SucursalDestinoId,
    string? Motivo,
    IReadOnlyList<RenglonTransferenciaRequest> Renglones);

public sealed record RenglonRecepcionRequest(Guid ArticuloId, decimal CantidadRecibida);

public sealed record RecibirTransferenciaRequest(
    IReadOnlyList<RenglonRecepcionRequest> Renglones);

public sealed record RenglonTransferenciaDto(
    Guid ArticuloId,
    string ArticuloSku,
    string ArticuloNombre,
    decimal CantidadEnviada,
    decimal? CantidadRecibida,
    decimal CostoUnitario);

public sealed record TransferenciaDto(
    Guid Id,
    int Folio,
    EstadoTransferencia Estado,
    DateTimeOffset Fecha,
    Guid SucursalOrigenId,
    string SucursalOrigenNombre,
    Guid SucursalDestinoId,
    string SucursalDestinoNombre,
    string? Motivo,
    string UsuarioNombre,
    Guid? MovimientoEntradaId,
    IReadOnlyList<RenglonTransferenciaDto> Renglones);

public sealed record TransferenciaListaDto(
    Guid Id,
    int Folio,
    EstadoTransferencia Estado,
    DateTimeOffset Fecha,
    string SucursalOrigenNombre,
    string SucursalDestinoNombre,
    int Renglones,
    string UsuarioNombre);
