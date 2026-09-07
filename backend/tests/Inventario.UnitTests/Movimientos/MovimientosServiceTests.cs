using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Application.Movimientos;
using Inventario.Domain.Catalogo;
using Inventario.Domain.Identidad;
using Inventario.Domain.Movimientos;
using Inventario.Domain.Partners;
using Inventario.Domain.Sucursales;
using Inventario.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Inventario.UnitTests.Movimientos;

public sealed class MovimientosServiceTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _articuloId = Guid.NewGuid();
    private readonly Guid _sucursalId = Guid.NewGuid();

    public MovimientosServiceTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexion).Options;

        using var ctx = Crear();
        ctx.Database.EnsureCreated();
        ctx.Tenants.Add(new Tenant { Id = _tenant, Nombre = "T", Codigo = "T" });
        var unidad = new UnidadMedida { Id = Guid.NewGuid(), Codigo = "PZA", Nombre = "Pieza" };
        ctx.UnidadesMedida.Add(unidad);
        var categoria = new Categoria { Id = Guid.NewGuid(), TenantId = _tenant, Nombre = "Otros", EsSistema = true };
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
    }

    [Fact]
    public async Task Entrada_recalcula_el_costo_promedio_y_el_kardex_lo_refleja()
    {
        var servicio = NuevoServicio();

        await servicio.RegistrarEntradaAsync(
            new RegistrarEntradaRequest(_sucursalId, null, null,
                [new RenglonEntradaRequest(_articuloId, 10, 100)]),
            default);
        await servicio.RegistrarEntradaAsync(
            new RegistrarEntradaRequest(_sucursalId, null, null,
                [new RenglonEntradaRequest(_articuloId, 10, 200)]),
            default);

        var kardex = await servicio.KardexAsync(_articuloId, _sucursalId, null, null, default);
        kardex.Count.ShouldBe(2);
        kardex[^1].CostoPromedioResultante.ShouldBe(150m);
        kardex[^1].CantidadResultante.ShouldBe(20m);
    }

    [Fact]
    public async Task Salida_de_mas_de_lo_que_hay_es_rechazada()
    {
        var servicio = NuevoServicio();
        await servicio.RegistrarEntradaAsync(
            new RegistrarEntradaRequest(_sucursalId, null, null,
                [new RenglonEntradaRequest(_articuloId, 5, 100)]),
            default);

        await Should.ThrowAsync<ValidacionException>(() => NuevoServicio().RegistrarSalidaAsync(
            new RegistrarSalidaRequest(_sucursalId, TipoMovimiento.Salida, "test",
                [new RenglonSalidaRequest(_articuloId, 6)]),
            default));
    }

    [Fact]
    public async Task Los_folios_de_movimiento_son_consecutivos()
    {
        var servicio = NuevoServicio();
        var a = await servicio.RegistrarEntradaAsync(
            new RegistrarEntradaRequest(_sucursalId, null, null,
                [new RenglonEntradaRequest(_articuloId, 1, 10)]),
            default);
        var b = await NuevoServicio().RegistrarEntradaAsync(
            new RegistrarEntradaRequest(_sucursalId, null, null,
                [new RenglonEntradaRequest(_articuloId, 1, 10)]),
            default);

        (b.Folio - a.Folio).ShouldBe(1);
    }

    private MovimientosService NuevoServicio()
    {
        var db = Crear();
        var currentUser = new CurrentUserFake(_tenant);
        return new MovimientosService(
            db, currentUser, new MotorExistencias(db), new FoliosService(db, currentUser));
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
