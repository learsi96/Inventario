using Inventario.Application.Abstractions;
using Inventario.Application.Common;
using Inventario.Application.Movimientos;
using Inventario.Domain.Catalogo;
using Inventario.Domain.Identidad;
using Inventario.Domain.Sucursales;
using Inventario.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Inventario.UnitTests.Existencias;

public sealed class MotorExistenciasTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _articuloId = Guid.NewGuid();
    private readonly Guid _sucursalId = Guid.NewGuid();

    public MotorExistenciasTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexion).Options;

        using var ctx = Crear();
        ctx.Database.EnsureCreated();
        ctx.UnidadesMedida.Add(new UnidadMedida { Id = Guid.NewGuid(), Codigo = "PZA", Nombre = "Pieza" });
        var unidad = ctx.UnidadesMedida.Local.First();
        ctx.Categorias.Add(new Categoria { Id = Guid.NewGuid(), TenantId = _tenant, Nombre = "Otros", EsSistema = true });
        ctx.Sucursales.Add(new Sucursal { Id = _sucursalId, TenantId = _tenant, Nombre = "Matriz", Codigo = "M" });
        ctx.Articulos.Add(new Articulo
        {
            Id = _articuloId,
            TenantId = _tenant,
            Sku = "ART-1",
            Nombre = "Balata",
            CategoriaId = ctx.Categorias.Local.First().Id,
            UnidadMedidaId = unidad.Id,
        });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task Dos_entradas_a_distinto_costo_promedian_ponderadamente()
    {
        var db = Crear();
        var motor = new MotorExistencias(db);

        await motor.AplicarAsync(_articuloId, _sucursalId, 10, 100m, default); // 1000
        await motor.AplicarAsync(_articuloId, _sucursalId, 10, 200m, default); // 2000
        await db.SaveChangesAsync();

        var existencia = await Crear().Existencias.SingleAsync();
        existencia.Cantidad.ShouldBe(20m);
        existencia.CostoPromedio.ShouldBe(150m); // (1000 + 2000) / 20
    }

    [Fact]
    public async Task La_salida_no_cambia_el_costo_y_usa_el_promedio()
    {
        var db = Crear();
        var motor = new MotorExistencias(db);

        await motor.AplicarAsync(_articuloId, _sucursalId, 10, 100m, default);
        var salida = await motor.AplicarAsync(_articuloId, _sucursalId, -4, null, default);
        await db.SaveChangesAsync();

        salida.CostoUnitario.ShouldBe(100m);
        (await Crear().Existencias.SingleAsync()).Cantidad.ShouldBe(6m);
    }

    [Fact]
    public async Task No_permite_dejar_la_existencia_en_negativo()
    {
        var db = Crear();
        var motor = new MotorExistencias(db);

        await motor.AplicarAsync(_articuloId, _sucursalId, 5, 100m, default);
        await db.SaveChangesAsync();

        await Should.ThrowAsync<ValidacionException>(
            () => new MotorExistencias(Crear()).AplicarAsync(_articuloId, _sucursalId, -6, null, default));
    }

    private AppDbContext Crear() => new(_opciones, new CurrentUserFake(_tenant));

    public void Dispose() => _conexion.Dispose();

    private sealed class CurrentUserFake(Guid tenant) : ICurrentUser
    {
        public Guid? UsuarioId => Guid.NewGuid();

        public Guid? TenantId => tenant;

        public RolUsuario? Rol => RolUsuario.Administrador;

        public bool EsSuperadmin => false;

        public bool EstaAutenticado => true;
    }
}
