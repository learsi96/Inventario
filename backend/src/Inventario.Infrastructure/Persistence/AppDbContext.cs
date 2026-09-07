using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core del sistema. Aún sin entidades de negocio: el esqueleto de
/// la Fase 1 solo establece la infraestructura de acceso a datos y el punto donde
/// se aplicará el filtro multi-tenant.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        ApplyTenantFilters(modelBuilder);
    }

    /// <summary>
    /// TODO (Hito 1): aplicar un query filter global
    /// <c>e =&gt; e.TenantId == _tenant.TenantId</c> a toda entidad que implemente
    /// <c>Inventario.Domain.Tenancy.ITenantEntity</c>, usando <c>ITenantContext</c>.
    /// Ver <c>docs/decisions/0002-estrategia-multi-tenant.md</c>.
    /// </summary>
    private static void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        _ = modelBuilder;
    }
}
