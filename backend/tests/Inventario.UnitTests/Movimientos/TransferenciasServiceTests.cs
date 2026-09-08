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

public sealed class TransferenciasServiceTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _articuloId = Guid.NewGuid();
    private readonly Guid _origenId = Guid.NewGuid();
    private readonly Guid _destinoId = Guid.NewGuid();

    public TransferenciasServiceTests()
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
        ctx.Sucursales.Add(new Sucursal { Id = _origenId, TenantId = _tenant, Nombre = "Origen", Codigo = "O" });
        ctx.Sucursales.Add(new Sucursal { Id = _destinoId, TenantId = _tenant, Nombre = "Destino", Codigo = "D" });
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

        // Existencia inicial en el origen: 10 @ 50.
        using var ctxSeed = Crear();
        new MotorExistencias(ctxSeed).AplicarAsync(_articuloId, _origenId, 10, 50m, default)
            .GetAwaiter().GetResult();
        ctxSeed.SaveChanges();
    }

    [Fact]
    public async Task El_flujo_completo_mueve_la_existencia_y_registra_diferencias()
    {
        var servicio = NuevoServicio();

        var t = await servicio.SolicitarAsync(
            new SolicitarTransferenciaRequest(_origenId, _destinoId, "test",
                [new RenglonTransferenciaRequest(_articuloId, 4)]),
            default);
        t.Estado.ShouldBe(EstadoTransferencia.Solicitada);

        await NuevoServicio().EnviarAsync(t.Id, default);
        (await Existencia(_origenId)).Cantidad.ShouldBe(6m); // 10 - 4

        var recibida = await NuevoServicio().RecibirAsync(
            t.Id, new RecibirTransferenciaRequest([new RenglonRecepcionRequest(_articuloId, 3)]), default);

        recibida.Estado.ShouldBe(EstadoTransferencia.Recibida);
        recibida.Renglones[0].CantidadEnviada.ShouldBe(4m);
        recibida.Renglones[0].CantidadRecibida.ShouldBe(3m);
        (await Existencia(_destinoId)).Cantidad.ShouldBe(3m); // recibió 3 (1 se perdió)
    }

    [Fact]
    public async Task Cancelar_en_transito_devuelve_la_existencia_al_origen()
    {
        var servicio = NuevoServicio();
        var t = await servicio.SolicitarAsync(
            new SolicitarTransferenciaRequest(_origenId, _destinoId, null,
                [new RenglonTransferenciaRequest(_articuloId, 4)]),
            default);
        await NuevoServicio().EnviarAsync(t.Id, default);
        (await Existencia(_origenId)).Cantidad.ShouldBe(6m);

        await NuevoServicio().CancelarAsync(t.Id, "me equivoqué", default);

        (await Existencia(_origenId)).Cantidad.ShouldBe(10m);
    }

    [Fact]
    public async Task No_permite_transferir_a_la_misma_sucursal()
    {
        await Should.ThrowAsync<ValidacionException>(() => NuevoServicio().SolicitarAsync(
            new SolicitarTransferenciaRequest(_origenId, _origenId, null,
                [new RenglonTransferenciaRequest(_articuloId, 1)]),
            default));
    }

    private async Task<Inventario.Domain.Existencias.Existencia> Existencia(Guid sucursalId) =>
        await Crear().Existencias.SingleAsync(e => e.SucursalId == sucursalId);

    private TransferenciasService NuevoServicio()
    {
        var db = Crear();
        var user = new CurrentUserFake(_tenant);
        return new TransferenciasService(db, user, new MotorExistencias(db), new FoliosService(db, user));
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
