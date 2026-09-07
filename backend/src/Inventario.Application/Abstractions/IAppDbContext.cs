using Inventario.Domain.Catalogo;
using Inventario.Domain.Identidad;
using Inventario.Domain.Partners;
using Inventario.Domain.Sucursales;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Application.Abstractions;

/// <summary>
/// Superficie del contexto de datos que consume la capa de aplicación. La
/// implementación (<c>AppDbContext</c>) vive en Infrastructure y aplica el filtro
/// multi-tenant y la asignación automática de <c>TenantId</c>.
/// </summary>
public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }

    DbSet<Usuario> Usuarios { get; }

    DbSet<UsuarioSucursal> UsuariosSucursales { get; }

    DbSet<Sucursal> Sucursales { get; }

    DbSet<Categoria> Categorias { get; }

    DbSet<UnidadMedida> UnidadesMedida { get; }

    DbSet<Articulo> Articulos { get; }

    DbSet<CodigoAlterno> CodigosAlternos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
