using Inventario.Application.Abstractions;
using Inventario.Application.Catalogo;
using Inventario.Application.Common;
using Inventario.Domain.Catalogo;
using Inventario.Domain.Identidad;
using Inventario.Domain.Partners;
using Inventario.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Inventario.UnitTests.Catalogo;

public sealed class ArticulosServiceTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _unidadId = Guid.NewGuid();

    public ArticulosServiceTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexion).Options;

        using var ctx = Crear();
        ctx.Database.EnsureCreated();
        ctx.Tenants.Add(new Tenant { Id = _tenant, Nombre = "T", Codigo = "T" });
        ctx.Categorias.Add(new Categoria
        {
            TenantId = _tenant,
            Nombre = "Otros",
            EsSistema = true,
            Activa = true,
        });
        ctx.UnidadesMedida.Add(new UnidadMedida { Id = _unidadId, Codigo = "PZA", Nombre = "Pieza" });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task Sin_sku_lo_autogenera_de_forma_secuencial()
    {
        var servicio = NuevoServicio();

        var a = await servicio.CrearAsync(Nuevo("Aceite"), default);
        var b = await servicio.CrearAsync(Nuevo("Filtro"), default);

        a.Sku.ShouldBe("ART-000001");
        b.Sku.ShouldBe("ART-000002");
    }

    [Fact]
    public async Task Sin_categoria_lo_asigna_a_Otros()
    {
        var a = await NuevoServicio().CrearAsync(Nuevo("Aceite"), default);

        a.CategoriaNombre.ShouldBe("Otros");
    }

    [Fact]
    public async Task No_permite_dos_articulos_con_el_mismo_codigo_de_barras()
    {
        var servicio = NuevoServicio();
        await servicio.CrearAsync(Nuevo("Aceite") with { CodigoBarras = "750123" }, default);

        await Should.ThrowAsync<ConflictoException>(
            () => servicio.CrearAsync(Nuevo("Filtro") with { CodigoBarras = "750123" }, default));
    }

    [Fact]
    public async Task La_busqueda_encuentra_por_codigo_alterno()
    {
        var servicio = NuevoServicio();
        await servicio.CrearAsync(
            Nuevo("Balata") with
            {
                CodigosAlternos = [new CodigoAlternoDto("TRW-999", TipoCodigoAlterno.Equivalencia)],
            },
            default);

        var r = await servicio.ListarAsync(new FiltroArticulos("TRW-999", null, null), default);

        r.Items.ShouldHaveSingleItem().Nombre.ShouldBe("Balata");
    }

    private CrearArticuloRequest Nuevo(string nombre) => new(
        Sku: null,
        CodigoBarras: null,
        Nombre: nombre,
        Descripcion: null,
        Marca: null,
        NumeroParteOem: null,
        CategoriaId: null,
        UnidadMedidaId: _unidadId,
        Costo: 10,
        PrecioVenta: 20,
        IvaPorcentaje: null,
        CodigosAlternos: null);

    private ArticulosService NuevoServicio()
    {
        var db = Crear();
        return new ArticulosService(
            db, new CurrentUserFake(_tenant), new CategoriasService(db), new AlmacenNulo());
    }

    private sealed class AlmacenNulo : IAlmacenArchivos
    {
        public Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct) =>
            Task.FromResult($"fake{extension}");

        public Task<ArchivoAlmacenado?> ObtenerAsync(string nombre, CancellationToken ct) =>
            Task.FromResult<ArchivoAlmacenado?>(null);

        public Task EliminarAsync(string nombre, CancellationToken ct) => Task.CompletedTask;
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
