namespace Inventario.Application.Catalogo;

// ---- Categorías ----

/// <summary>Categoría con sus hijas anidadas (para pintar el árbol).</summary>
public sealed record CategoriaNodoDto(
    Guid Id,
    string Nombre,
    Guid? CategoriaPadreId,
    bool EsSistema,
    bool Activa,
    IReadOnlyList<CategoriaNodoDto> Subcategorias);

public sealed record CrearCategoriaRequest(string Nombre, Guid? CategoriaPadreId);

public sealed record ActualizarCategoriaRequest(string Nombre, Guid? CategoriaPadreId);

// ---- Unidades de medida ----

public sealed record UnidadMedidaDto(Guid Id, string Codigo, string Nombre, bool Activa);
