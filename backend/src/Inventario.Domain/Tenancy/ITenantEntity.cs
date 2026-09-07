namespace Inventario.Domain.Tenancy;

/// <summary>
/// Marca una entidad de negocio que pertenece a un partner (tenant).
/// Toda entidad que implemente esta interfaz recibe automáticamente el filtro
/// global por <see cref="TenantId"/> en el <c>AppDbContext</c>, y <c>SaveChanges</c>
/// le asigna el tenant actual al insertarla.
/// Ver <c>docs/decisions/0002-estrategia-multi-tenant.md</c>.
/// </summary>
public interface ITenantEntity
{
    /// <summary>Identificador del partner dueño del registro.</summary>
    Guid TenantId { get; set; }
}
