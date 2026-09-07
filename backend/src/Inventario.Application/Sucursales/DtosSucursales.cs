namespace Inventario.Application.Sucursales;

public sealed record SucursalDto(
    Guid Id,
    string Nombre,
    string Codigo,
    string? Direccion,
    bool Activa,
    DateTimeOffset CreadoEn);

public sealed record CrearSucursalRequest(string Nombre, string Codigo, string? Direccion);

public sealed record ActualizarSucursalRequest(string Nombre, string Codigo, string? Direccion);

public sealed record CambiarActivacionRequest(bool Activa);
