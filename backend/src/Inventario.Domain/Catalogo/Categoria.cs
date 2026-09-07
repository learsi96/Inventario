using Inventario.Domain.Common;
using Inventario.Domain.Tenancy;

namespace Inventario.Domain.Catalogo;

/// <summary>
/// Categoría de artículos, jerárquica (lista de adyacencia). Cada partner tiene
/// una categoría "Otros" de sistema (<see cref="EsSistema"/>) que recibe los
/// artículos sin categoría explícita y que no se puede borrar ni desactivar.
/// </summary>
public class Categoria : EntidadBase, ITenantEntity
{
    public Guid TenantId { get; set; }

    public required string Nombre { get; set; }

    public Guid? CategoriaPadreId { get; set; }

    public Categoria? CategoriaPadre { get; set; }

    public ICollection<Categoria> Subcategorias { get; } = [];

    /// <summary>Categoría creada por el sistema (p. ej. "Otros"). No editable/borrable en su estado.</summary>
    public bool EsSistema { get; set; }

    public bool Activa { get; set; } = true;
}
