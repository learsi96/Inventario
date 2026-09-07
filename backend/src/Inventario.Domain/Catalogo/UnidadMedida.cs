using Inventario.Domain.Common;

namespace Inventario.Domain.Catalogo;

/// <summary>
/// Unidad de medida de los artículos. Catálogo global (no multi-tenant): las
/// unidades son universales. Si en el futuro un partner necesita una unidad
/// propia, se añadirá un <c>TenantId</c> opcional (null = global).
/// </summary>
public class UnidadMedida : EntidadBase
{
    /// <summary>Clave corta (p. ej. "PZA", "JGO", "LT").</summary>
    public required string Codigo { get; set; }

    public required string Nombre { get; set; }

    public bool Activa { get; set; } = true;
}
