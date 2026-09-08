using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Application.Conteos;
using Inventario.Application.Movimientos;
using Inventario.Domain.Catalogo;
using Inventario.Domain.Conteos;
using Inventario.Domain.Identidad;
using Inventario.Domain.Partners;
using Inventario.Domain.Sucursales;
using Inventario.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Inventario.UnitTests.Conteos;

public sealed class ConteosServiceTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _articuloId = Guid.NewGuid();
    private readonly Guid _sucursalId = Guid.NewGuid();

    public ConteosServiceTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexion).Options;

        using var ctx = Crear();
        ctx.Database.EnsureCreated();
        ctx.Tenants.Add(new Tenant { Id = _tenant, Nombre = "T", Codigo = "T" });
        var unidad = new UnidadMedida { Id = Guid.NewGuid(), Codigo = "PZA", Nombre = "Pieza" };
        var categoria = new Categoria { Id = Guid.NewGuid(), TenantId = _tenant, Nombre = "Otros", EsSistema = true };
        ctx.UnidadesMedida.Add(unidad);
        ctx.Categorias.Add(categoria);
        ctx.Sucursales.Add(new Sucursal { Id = _sucursalId, TenantId = _tenant, Nombre = "Matriz", Codigo = "M" });
        ctx.Articulos.Add(new Articulo
        {
            Id = _articuloId,
            TenantId = _tenant,
            Sku = "ART-1",
            Nombre = "Balata",
            CategoriaId = categoria.Id,
            UnidadMedidaId = unidad.Id,
        });
        ctx.SaveChanges();

        using var seed = Crear();
        new MotorExistencias(seed).AplicarAsync(_articuloId, _sucursalId, 10, 100m, default)
            .GetAwaiter().GetResult();
        seed.SaveChanges();
    }

    [Fact]
    public async Task Conciliar_ajusta_la_existencia_a_lo_contado_y_genera_movimiento()
    {
        var servicio = Nuevo();
        var conteo = await servicio.IniciarAsync(new IniciarConteoRequest(_sucursalId, null), default);
        conteo.Detalles.ShouldHaveSingleItem().CantidadSistema.ShouldBe(10m);

        await Nuevo().CapturarAsync(
            conteo.Id,
            new CapturaConteoRequest([new CapturaRenglonRequest(_articuloId, 7)]),
            default);

        var conciliado = await Nuevo().ConciliarAsync(conteo.Id, default);
        conciliado.Estado.ShouldBe(EstadoConteo.Conciliado);
        conciliado.MovimientoAjusteId.ShouldNotBeNull();

        var existencia = await Crear().Existencias.SingleAsync();
        existencia.Cantidad.ShouldBe(7m);
        existencia.CostoPromedio.ShouldBe(100m); // el ajuste conserva el costo
    }

    [Fact]
    public async Task No_permite_dos_conteos_en_progreso_para_la_misma_sucursal()
    {
        await Nuevo().IniciarAsync(new IniciarConteoRequest(_sucursalId, null), default);

        await Should.ThrowAsync<ConflictoException>(
            () => Nuevo().IniciarAsync(new IniciarConteoRequest(_sucursalId, null), default));
    }

    [Fact]
    public async Task Conciliar_sin_diferencias_no_genera_movimiento()
    {
        var servicio = Nuevo();
        var conteo = await servicio.IniciarAsync(new IniciarConteoRequest(_sucursalId, null), default);
        await Nuevo().CapturarAsync(
            conteo.Id,
            new CapturaConteoRequest([new CapturaRenglonRequest(_articuloId, 10)]),
            default);

        var conciliado = await Nuevo().ConciliarAsync(conteo.Id, default);
        conciliado.MovimientoAjusteId.ShouldBeNull();
    }

    private ConteosService Nuevo()
    {
        var db = Crear();
        var user = new CurrentUserFake(_tenant);
        return new ConteosService(db, user, new MotorExistencias(db), new FoliosService(db, user));
    }

    private AppDbContext Crear() => new(_opciones, new CurrentUserFake(_tenant));

    public void Dispose() => _conexion.Dispose();

    private sealed class CurrentUserFake(Guid tenant) : ICurrentUser
    {
        public Guid? UsuarioId => null;

        public Guid? TenantId => tenant;

        public RolUsuario? Rol => RolUsuario.Administrador;

        public bool EsSuperadmin => false;

        public bool EstaAutenticado => true;
    }
}
