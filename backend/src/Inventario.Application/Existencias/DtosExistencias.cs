using Inventario.Application.Catalogo;

namespace Inventario.Application.Existencias;

// ---- Ubicaciones ----

public sealed record UbicacionDto(
    Guid Id, Guid SucursalId, string Codigo, string? Descripcion, bool Activa);

public sealed record CrearUbicacionRequest(Guid SucursalId, string Codigo, string? Descripcion);

public sealed record ActualizarUbicacionRequest(string Codigo, string? Descripcion, bool Activa);

// ---- Existencias ----

public sealed record ExistenciaListaDto(
    Guid ArticuloId,
    string Sku,
    string ArticuloNombre,
    Guid SucursalId,
    string SucursalNombre,
    decimal Cantidad,
    string UnidadCodigo,
    decimal CostoPromedio,
    decimal Valor,
    decimal Minimo,
    bool BajoMinimo);

public sealed record ExistenciaUbicacionDto(Guid UbicacionId, string UbicacionCodigo, decimal Cantidad);

public sealed record ExistenciaDto(
    Guid ArticuloId,
    string Sku,
    string ArticuloNombre,
    Guid SucursalId,
    string SucursalNombre,
    decimal Cantidad,
    decimal CostoPromedio,
    decimal Valor,
    decimal Minimo,
    decimal Maximo,
    decimal PuntoReorden,
    bool BajoMinimo,
    IReadOnlyList<ExistenciaUbicacionDto> PorUbicacion);

public sealed record AjusteExistenciaRequest(
    Guid ArticuloId,
    Guid SucursalId,
    decimal Cantidad,
    decimal CostoPromedio,
    string Motivo);

public sealed record ParametrosReordenRequest(
    Guid ArticuloId,
    Guid SucursalId,
    decimal Minimo,
    decimal Maximo,
    decimal PuntoReorden);

public sealed record AsignacionUbicacionRequest(Guid UbicacionId, decimal Cantidad);

public sealed record AsignarUbicacionesRequest(
    Guid ArticuloId,
    Guid SucursalId,
    IReadOnlyList<AsignacionUbicacionRequest> Asignaciones);

public sealed record FiltroExistencias(
    Guid? SucursalId,
    Guid? CategoriaId,
    string? Texto,
    bool SoloBajoMinimo = false,
    int Pagina = 1,
    int Tamano = 20);

public sealed record ValorizacionSucursalDto(
    Guid SucursalId, string SucursalNombre, int Articulos, decimal Unidades, decimal Valor);

public sealed record ValorizacionDto(
    decimal ValorTotal,
    decimal UnidadesTotal,
    int ArticulosConExistencia,
    IReadOnlyList<ValorizacionSucursalDto> PorSucursal);
