namespace Inventario.Application.Common;

/// <summary>Página de resultados de un listado.</summary>
public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Pagina,
    int Tamano);
