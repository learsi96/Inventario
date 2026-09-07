using System.Linq.Expressions;
using Inventario.Application.Abstractions;
using Inventario.Domain.Catalogo;
using Inventario.Domain.Common;
using Inventario.Domain.Existencias;
using Inventario.Domain.Identidad;
using Inventario.Domain.Movimientos;
using Inventario.Domain.Partners;
using Inventario.Domain.Sucursales;
using Inventario.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core del sistema. Aplica el aislamiento multi-tenant:
/// filtro global por <c>TenantId</c> en toda entidad <see cref="ITenantEntity"/>
/// y asignación automática del tenant actual al insertar. Además rellena las
/// marcas de tiempo de <see cref="EntidadBase"/>.
/// Ver <c>docs/decisions/0002-estrategia-multi-tenant.md</c>.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : DbContext(options), IAppDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<UsuarioSucursal> UsuariosSucursales => Set<UsuarioSucursal>();

    public DbSet<Sucursal> Sucursales => Set<Sucursal>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();

    public DbSet<Articulo> Articulos => Set<Articulo>();

    public DbSet<CodigoAlterno> CodigosAlternos => Set<CodigoAlterno>();

    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();

    public DbSet<Existencia> Existencias => Set<Existencia>();

    public DbSet<ExistenciaUbicacion> ExistenciasUbicacion => Set<ExistenciaUbicacion>();

    public DbSet<Movimiento> Movimientos => Set<Movimiento>();

    public DbSet<MovimientoRenglon> MovimientoRenglones => Set<MovimientoRenglon>();

    /// <summary>Tenant en contexto. Lo lee el filtro global de consultas en cada query.</summary>
    public Guid? TenantIdActual => currentUser.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConstruirFiltroTenant(entityType.ClrType));
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AplicarAuditoriaYTenant();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AplicarAuditoriaYTenant();
        return base.SaveChanges();
    }

    private void AplicarAuditoriaYTenant()
    {
        var ahora = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is EntidadBase entidad)
            {
                if (entry.State == EntityState.Added)
                {
                    entidad.CreadoEn = ahora;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entidad.ActualizadoEn = ahora;
                }
            }

            if (entry is { Entity: ITenantEntity tenantEntity, State: EntityState.Added }
                && tenantEntity.TenantId == Guid.Empty
                && currentUser.TenantId is { } tenantId)
            {
                tenantEntity.TenantId = tenantId;
            }
        }
    }

    /// <summary>
    /// Construye <c>e =&gt; e.TenantId == TenantIdActual</c>. El filtro referencia
    /// el miembro de instancia, por lo que se reevalúa en cada consulta.
    /// </summary>
    private LambdaExpression ConstruirFiltroTenant(Type tipoEntidad)
    {
        var e = Expression.Parameter(tipoEntidad, "e");
        var tenantIdEntidad = Expression.Convert(
            Expression.Property(e, nameof(ITenantEntity.TenantId)),
            typeof(Guid?));
        var tenantIdActual = Expression.Property(
            Expression.Constant(this),
            nameof(TenantIdActual));

        return Expression.Lambda(Expression.Equal(tenantIdEntidad, tenantIdActual), e);
    }
}
