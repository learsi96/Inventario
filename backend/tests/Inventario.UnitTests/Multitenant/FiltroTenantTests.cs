using Inventario.Application.Abstractions;
using Inventario.Domain.Identidad;
using Inventario.Domain.Sucursales;
using Inventario.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Inventario.UnitTests.Multitenant;

/// <summary>
/// Verifica el aislamiento multi-tenant del <see cref="AppDbContext"/>: filtro
/// global por <c>TenantId</c> y asignación automática al insertar.
/// </summary>
public sealed class FiltroTenantTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private readonly Guid _tenantA = Guid.NewGuid();
    private readonly Guid _tenantB = Guid.NewGuid();

    public FiltroTenantTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexion)
            .Options;

        using var ctx = Crear(null);
        ctx.Database.EnsureCreated();
        SembrarSucursal(_tenantA, "A1");
        SembrarSucursal(_tenantB, "B1");
    }

    [Fact]
    public void Las_consultas_solo_ven_las_sucursales_del_tenant_actual()
    {
        using var ctx = Crear(_tenantA);

        var sucursales = ctx.Sucursales.ToList();

        sucursales.ShouldHaveSingleItem().Codigo.ShouldBe("A1");
    }

    [Fact]
    public void SaveChanges_asigna_el_tenant_actual_a_las_entidades_nuevas()
    {
        using (var ctx = Crear(_tenantB))
        {
            ctx.Sucursales.Add(new Sucursal { Nombre = "Nueva", Codigo = "B2" });
            ctx.SaveChanges();
        }

        using var verificacion = Crear(null);
        var creada = verificacion.Sucursales.IgnoreQueryFilters().Single(s => s.Codigo == "B2");
        creada.TenantId.ShouldBe(_tenantB);
    }

    [Fact]
    public void Sin_tenant_en_contexto_no_se_ve_ninguna_entidad_con_tenant()
    {
        using var ctx = Crear(null);

        ctx.Sucursales.ToList().ShouldBeEmpty();
    }

    private void SembrarSucursal(Guid tenantId, string codigo)
    {
        using var ctx = Crear(null);
        ctx.Sucursales.Add(new Sucursal { TenantId = tenantId, Nombre = codigo, Codigo = codigo });
        ctx.SaveChanges();
    }

    private AppDbContext Crear(Guid? tenantId) => new(_opciones, new CurrentUserFake(tenantId));

    public void Dispose() => _conexion.Dispose();

    private sealed class CurrentUserFake(Guid? tenantId) : ICurrentUser
    {
        public Guid? UsuarioId => tenantId is null ? null : Guid.NewGuid();

        public Guid? TenantId => tenantId;

        public RolUsuario? Rol => RolUsuario.Administrador;

        public bool EsSuperadmin => false;

        public bool EstaAutenticado => tenantId is not null;
    }
}
